using System.Net;
using System.Net.Http.Json;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Localization.Queries.GetLocalization;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Identity;

namespace EducationPortal.Application.FunctionalTests.Schools;

/// <summary>
/// Only master tenant administrators add schools. A new school is reachable on its domain at once, takes its country's
/// languages, never becomes a second master tenant, and every addition is audited; input is checked and a domain can
/// belong to one school only (SEC-AUTHZ-07, SEC-AUTHZ-10, SEC-INPUT-01, SEC-LOG-03).
/// </summary>
public class CreateSchoolTests : TestBase
{
    private const string NewDomain = "new-school.localhost";
    private const string SchoolDomain = "school1.localhost";
    private const string SchoolAdminEmail = "admin@school1.local";
    private const string NewAdminEmail = "head@new-school.local";
    private const string OneTimePassword = "violet harbour lantern 47";
    private const string NewPassword = "quiet orchard ladder 93";

    [Test]
    public async Task ShouldLetTheMasterTenantAddASchoolOnItsOwnDomain()
    {
        var countryId = await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "ur", "Asia/Karachi", "PKR");
        var userId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var response = await master.PostAsync("/api/Schools", new
        {
            name = "  New School  ",
            domain = "New-School.Localhost.",
            countryId = countryId.PublicId,
            dataRegion = "SouthAsia",
            administratorEmail = NewAdminEmail,
            administratorPassword = OneTimePassword,
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var schoolId = await response.Content.ReadFromJsonAsync<Guid>();
        var id = await TestApp.FindIdAsync<School>(schoolId);
        var school = (await TestApp.FindAsync<School>(id))!;
        school.Name.ShouldBe("New School");
        school.Domain.ShouldBe(NewDomain);
        school.IsMaster.ShouldBeFalse();
        school.CountryId.ShouldBe(countryId.Id);
        school.DataRegion.ShouldBe(DataRegion.SouthAsia);
        school.AccentColor.ShouldBe(School.DefaultAccentColor);

        // The domain resolves to the new school straight away, with its country's default language.
        using var visitor = TestHttp.CreateDevelopmentSession(NewDomain);
        using var localization = await visitor.GetAsync("/api/Localization");
        localization.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await localization.Content.ReadFromJsonAsync<LocalizationDto>())!.Language.ShouldBe("ur");

        // The audit log is internal, so it records the internal key.
        var audit = await TestApp.FindAuditEventsAsync(userId);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.SchoolCreated && e.TargetId == id.ToString());
    }

    [Test]
    public async Task ShouldAddASchoolWithoutCountryOrDataRegion()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var response = await master.PostAsync("/api/Schools", Body("New School", NewDomain));

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var school = (await TestApp.FindAsync<School>(await TestApp.FindIdAsync<School>(await response.Content.ReadFromJsonAsync<Guid>())))!;
        school.CountryId.ShouldBeNull();
        school.DataRegion.ShouldBeNull();
    }

    [TestCase("name", "")]
    [TestCase("domain", "")]
    [TestCase("domain", "https://school.com")]
    [TestCase("domain", "school.com:8080")]
    [TestCase("domain", "school.com/path")]
    [TestCase("domain", "-school.com")]
    [TestCase("domain", "school..com")]
    [TestCase("dataRegion", "Mars")]
    [TestCase("administratorEmail", "")]
    [TestCase("administratorEmail", "not-an-email")]
    [TestCase("administratorPassword", "")]
    [TestCase("administratorPassword", "too-short")]
    [TestCase("administratorPassword", "educationportal-2026")] // contains the product name, which the policy refuses
    public async Task ShouldRefuseAnInvalidSchool(string field, string value)
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var schoolsBefore = await TestApp.CountAsync<School>();
        var body = new Dictionary<string, object>
        {
            ["name"] = "New School",
            ["domain"] = NewDomain,
            ["administratorEmail"] = NewAdminEmail,
            ["administratorPassword"] = OneTimePassword,
        };
        body[field] = value;

        using var response = await master.PostAsync("/api/Schools", body);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await TestApp.CountAsync<School>()).ShouldBe(schoolsBefore);
        (await TestApp.CountAsync<ApplicationUser>()).ShouldBe(1); // only the master administrator
    }

    [Test]
    public async Task ShouldRefuseANameThatIsTooLong()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var response = await master.PostAsync("/api/Schools", Body(new string('a', School.NameMaxLength + 1), NewDomain));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [TestCase(SchoolDomain)]
    [TestCase("SCHOOL1.localhost")]
    [TestCase("localhost")] // the master tenant's own domain
    public async Task ShouldRefuseADomainAnotherSchoolUses(string domain)
    {
        await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var schoolsBefore = await TestApp.CountAsync<School>();

        using var response = await master.PostAsync("/api/Schools", Body("Copy", domain));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await TestApp.CountAsync<School>()).ShouldBe(schoolsBefore);
    }

    [Test]
    public async Task ShouldRefuseAnUnknownCountry()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var response = await master.PostAsync("/api/Schools", new { name = "New School", domain = NewDomain, countryId = Guid.NewGuid(), administratorEmail = NewAdminEmail, administratorPassword = OneTimePassword });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldNeverCreateASecondMasterTenant()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var response = await master.PostAsync("/api/Schools", new { name = "New School", domain = NewDomain, isMaster = true, administratorEmail = NewAdminEmail, administratorPassword = OneTimePassword });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var school = (await TestApp.FindAsync<School>(await TestApp.FindIdAsync<School>(await response.Content.ReadFromJsonAsync<Guid>())))!;
        school.IsMaster.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldKeepSchoolCreationAwayFromSchoolAdministrators()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsUserAsync(schoolId.Id, SchoolAdminEmail, TestApp.AdministratorPassword, [Roles.Administrator]);
        using var session = TestHttp.CreateDevelopmentSession(SchoolDomain);
        (await session.SignInAsync(SchoolAdminEmail, TestApp.AdministratorPassword)).Dispose();
        var schoolsBefore = await TestApp.CountAsync<School>();

        using var response = await session.PostAsync("/api/Schools", Body("New School", NewDomain));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await TestApp.CountAsync<School>()).ShouldBe(schoolsBefore);
    }

    [Test]
    public async Task ShouldRequireTheAdministratorRoleOnTheMasterTenant()
    {
        await TestApp.RunAsDefaultUserAsync();
        using var session = TestHttp.CreateDevelopmentSession();
        (await session.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword)).Dispose();

        using var response = await session.PostAsync("/api/Schools", Body("New School", NewDomain));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldRequireSignIn()
    {
        using var session = TestHttp.CreateDevelopmentSession();

        using var response = await session.PostAsync("/api/Schools", Body("New School", NewDomain));

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldRefuseAnAdministratorEmailThatAnotherAccountUses()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsUserAsync(schoolId.Id, SchoolAdminEmail, TestApp.AdministratorPassword, [Roles.Administrator]);
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var schoolsBefore = await TestApp.CountAsync<School>();

        using var response = await master.PostAsync("/api/Schools", new
        {
            name = "New School",
            domain = NewDomain,
            administratorEmail = SchoolAdminEmail.ToUpperInvariant(),
            administratorPassword = OneTimePassword,
        });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await TestApp.CountAsync<School>()).ShouldBe(schoolsBefore);
    }

    [Test]
    public async Task ShouldRollTheSchoolBackWhenTheAdministratorCannotBeCreated()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var schoolsBefore = await TestApp.CountAsync<School>();

        // Long enough for the validator, but it contains a word of the account (its email), which Identity's policy refuses.
        using var response = await master.PostAsync("/api/Schools", new
        {
            name = "New School",
            domain = NewDomain,
            administratorEmail = NewAdminEmail,
            administratorPassword = "head head head head",
        });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await TestApp.CountAsync<School>()).ShouldBe(schoolsBefore);
        (await TestApp.CountAsync<ApplicationUser>()).ShouldBe(1);
    }

    [Test]
    public async Task ShouldMakeTheNewAdministratorReplaceTheOneTimePasswordBeforeAnythingElse()
    {
        var userId = await TestApp.RunAsAdministratorAsync();
        using (var master = await SignInAsMasterAsync())
        {
            using var created = await master.PostAsync("/api/Schools", Body("New School", NewDomain));
            created.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        var audit = await TestApp.FindAuditEventsAsync(userId);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.UserCreated);

        // The administrator signs in on the school's own domain only.
        using (var wrongHost = TestHttp.CreateDevelopmentSession())
        using (var refused = await wrongHost.SignInAsync(NewAdminEmail, OneTimePassword))
        {
            refused.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        using var admin = TestHttp.CreateDevelopmentSession(NewDomain);
        (await admin.SignInAsync(NewAdminEmail, OneTimePassword)).StatusCode.ShouldBe(HttpStatusCode.OK);

        using (var me = await admin.GetCurrentUserAsync())
        {
            me.StatusCode.ShouldBe(HttpStatusCode.OK);
            var user = (await me.Content.ReadFromJsonAsync<CurrentUserDto>())!;
            user.MustChangePassword.ShouldBeTrue();
            user.Roles.ShouldBe([Roles.Administrator]);
        }

        // Everything else is refused with a code the app recognises.
        using (var blocked = await admin.GetAsync("/api/SchoolRoles"))
        {
            blocked.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            (await blocked.Content.ReadAsStringAsync()).ShouldContain("PASSWORD_CHANGE_REQUIRED");
        }

        using (var wrongCurrent = await admin.PostAsync("/api/Users/me/password", new { currentPassword = "not the right one at all", newPassword = NewPassword }))
        {
            wrongCurrent.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        using (var same = await admin.PostAsync("/api/Users/me/password", new { currentPassword = OneTimePassword, newPassword = OneTimePassword }))
        {
            same.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        using (var changed = await admin.PostAsync("/api/Users/me/password", new { currentPassword = OneTimePassword, newPassword = NewPassword }))
        {
            changed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }

        // The same session carries on, now unblocked.
        using (var me = await admin.GetCurrentUserAsync())
        {
            (await me.Content.ReadFromJsonAsync<CurrentUserDto>())!.MustChangePassword.ShouldBeFalse();
        }

        using (var allowed = await admin.GetAsync("/api/SchoolRoles"))
        {
            allowed.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        // The one-time password no longer works.
        using var again = TestHttp.CreateDevelopmentSession(NewDomain);
        using (var old = await again.SignInAsync(NewAdminEmail, OneTimePassword))
        {
            old.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }
    }

    private static object Body(string name, string domain) => new
    {
        name,
        domain,
        administratorEmail = NewAdminEmail,
        administratorPassword = OneTimePassword,
    };

    private static async Task<HttpSession> SignInAsMasterAsync()
    {
        var master = TestHttp.CreateDevelopmentSession();
        using var response = await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return master;
    }
}
