using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Tenancy;

/// <summary>
/// A school without a domain of its own signs in on the master tenant's domain with its email and password: the server finds
/// the school from the account. Its session then acts as that school (its rows, its modules, no master power), and it ends
/// when the school gets a domain or is switched off.
/// </summary>
public class SchoolWithoutDomainTests : TestBase
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private sealed record SignInAnswer(List<ModuleCode> Modules);

    private const string OwnDomain = "own-domain.localhost";
    private const string Password = "Local-Dev-Only-2026!";

    private static async Task<HttpSession> SignInOnMasterDomainAsync(EntityIds school, string email)
    {
        await TestApp.RunAsUserAsync(school.Id, email, Password, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(); // the master tenant's domain
        using var response = await session.SignInAsync(email, Password);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return session;
    }

    private static async Task ChangeSchoolAsync(int schoolId, Action<School> change)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var school = await context.Schools.SingleAsync(s => s.Id == schoolId);
        change(school);
        await context.SaveChangesAsync();
    }

    [Test]
    public async Task ShouldSignInOnTheMasterDomainAndActAsTheSchoolNotTheMaster()
    {
        var school = await TestApp.AddSchoolAsync("No Domain School", null);
        using var session = await SignInOnMasterDomainAsync(school, "admin@nodomain.local");

        using var me = await session.GetCurrentUserAsync();
        me.StatusCode.ShouldBe(HttpStatusCode.OK);

        // A master-only use case is refused: the request is the school's, never the master's.
        using var schools = await session.GetAsync("/api/Schools");
        schools.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // The master tenant has every module; this school has only its core modules.
        using var enabled = await session.GetAsync("/api/Modules/enabled");
        var modules = (await enabled.Content.ReadFromJsonAsync<List<string>>(Json))!;
        modules.ShouldContain(nameof(ModuleCode.Students));
        modules.ShouldNotContain(nameof(ModuleCode.Fees));
        modules.Count.ShouldBeLessThan(Enum.GetValues<ModuleCode>().Length - 1);
    }

    [Test]
    public async Task ShouldAnswerSignInWithTheModulesOfTheSchoolNotTheMasterTenant()
    {
        var school = await TestApp.AddSchoolAsync("Sign In Modules School", null);
        await TestApp.RunAsUserAsync(school.Id, "admin@signinmodules.local", Password, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(); // the master tenant's domain

        using var response = await session.SignInAsync("admin@signinmodules.local", Password);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var answer = (await response.Content.ReadFromJsonAsync<SignInAnswer>(Json))!;
        // The master tenant has every module; the sign-in answer is for the school the account belongs to.
        answer.Modules.ShouldContain(ModuleCode.Students);
        answer.Modules.ShouldNotContain(ModuleCode.Fees);
    }

    [Test]
    public async Task ShouldKeepTheSessionOfASchoolOnTheMasterDomainWhenItChangesItsPassword()
    {
        var school = await TestApp.AddSchoolAsync("Password School", null);
        using var session = await SignInOnMasterDomainAsync(school, "admin@password.local");

        using var changed = await session.PostAsync("/api/Users/me/password", new
        {
            currentPassword = Password,
            newPassword = "a brand new passphrase 2026",
        });
        changed.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Still signed in and still acting as the school: its modules answer, and the master portal stays closed to it.
        using var modules = await session.GetAsync("/api/Modules/enabled");
        modules.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var schools = await session.GetAsync("/api/Schools");
        schools.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldKeepTwoSchoolsWithoutDomainsApart()
    {
        var first = await TestApp.AddSchoolAsync("First School", null);
        var second = await TestApp.AddSchoolAsync("Second School", null);
        using var firstSession = await SignInOnMasterDomainAsync(first, "admin@first.local");
        using var secondSession = await SignInOnMasterDomainAsync(second, "admin@second.local");

        using var firstCampuses = await firstSession.GetAsync("/api/Campuses");
        using var secondCampuses = await secondSession.GetAsync("/api/Campuses");

        // Each school has only its own main campus, whichever session asks on the same host.
        (await CampusCountAsync(firstCampuses)).ShouldBe(1);
        (await CampusCountAsync(secondCampuses)).ShouldBe(1);
        // Both main campuses exist; read across schools as the master tenant, because the test's own scope is the last
        // school signed in, whose filter would show only its own campus.
        (await CountAllCampusesAsync(first.Id, second.Id)).ShouldBe(2);
    }

    private static async Task<int> CountAllCampusesAsync(params int[] schoolIds)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(schoolIds[0], IsMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await context.Campuses.CountAsync(c => schoolIds.Contains(c.SchoolId));
    }

    private static async Task<int> CampusCountAsync(HttpResponseMessage response)
    {
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return document.RootElement.GetArrayLength();
    }

    [Test]
    public async Task ShouldNotLetASchoolWithItsOwnDomainSignInOnTheMasterDomain()
    {
        var school = await TestApp.AddSchoolAsync("Own Domain School", OwnDomain);
        await TestApp.RunAsUserAsync(school.Id, "admin@owndomain.local", Password, [Roles.Administrator]);

        var onMaster = TestHttp.CreateDevelopmentSession();
        using var refused = await onMaster.SignInAsync("admin@owndomain.local", Password);

        refused.StatusCode.ShouldNotBe(HttpStatusCode.OK);

        // It still signs in on its own domain.
        var onOwnDomain = TestHttp.CreateDevelopmentSession(OwnDomain);
        using var accepted = await onOwnDomain.SignInAsync("admin@owndomain.local", Password);
        accepted.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task ShouldNotLetASchoolWithoutADomainSignInOnAnotherSchoolsDomain()
    {
        var school = await TestApp.AddSchoolAsync("No Domain School", null);
        await TestApp.AddSchoolAsync("Other School", OwnDomain);
        await TestApp.RunAsUserAsync(school.Id, "admin@nodomain.local", Password, [Roles.Administrator]);

        var onOtherSchool = TestHttp.CreateDevelopmentSession(OwnDomain);
        using var refused = await onOtherSchool.SignInAsync("admin@nodomain.local", Password);

        refused.StatusCode.ShouldNotBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task ShouldEndTheSessionOnTheMasterDomainWhenTheSchoolGetsADomain()
    {
        var school = await TestApp.AddSchoolAsync("Growing School", null);
        using var session = await SignInOnMasterDomainAsync(school, "admin@growing.local");
        (await session.GetCurrentUserAsync()).StatusCode.ShouldBe(HttpStatusCode.OK);

        await ChangeSchoolAsync(school.Id, s => s.UpdateProfile(s.Name, OwnDomain));

        using var after = await session.GetCurrentUserAsync();
        after.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldEndTheSessionOnTheMasterDomainWhenTheSchoolIsSwitchedOff()
    {
        var school = await TestApp.AddSchoolAsync("Paused School", null);
        using var session = await SignInOnMasterDomainAsync(school, "admin@paused.local");

        await ChangeSchoolAsync(school.Id, s => s.Deactivate());

        using var after = await session.GetCurrentUserAsync();
        after.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldNotSignInASchoolThatIsSwitchedOffOnTheMasterDomain()
    {
        var school = await TestApp.AddSchoolAsync("Paused School", null);
        await TestApp.RunAsUserAsync(school.Id, "admin@paused.local", Password, [Roles.Administrator]);
        await ChangeSchoolAsync(school.Id, s => s.Deactivate());

        var session = TestHttp.CreateDevelopmentSession();
        using var refused = await session.SignInAsync("admin@paused.local", Password);

        refused.StatusCode.ShouldNotBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task ShouldKeepTheMasterAdministratorAsTheMaster()
    {
        await TestApp.AddSchoolAsync("No Domain School", null);
        await TestApp.RunAsAdministratorAsync();
        using var master = await CommercialHttp.SignInAsMasterAsync();

        using var schools = await master.GetAsync("/api/Schools");

        schools.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task ShouldRefuseAWrongPasswordLikeAnyOtherSignIn()
    {
        var school = await TestApp.AddSchoolAsync("No Domain School", null);
        await TestApp.RunAsUserAsync(school.Id, "admin@nodomain.local", Password, [Roles.Administrator]);

        var session = TestHttp.CreateDevelopmentSession();
        using var refused = await session.SignInAsync("admin@nodomain.local", "not-the-password-0000");

        refused.StatusCode.ShouldNotBe(HttpStatusCode.OK);
    }
}
