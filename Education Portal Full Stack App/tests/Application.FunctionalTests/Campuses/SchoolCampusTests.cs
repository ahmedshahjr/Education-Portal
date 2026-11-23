using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Campuses;

/// <summary>
/// A school manages its own campuses (the class keeps its name), as its administrator, when its Campuses module is on. The
/// school is the one the host resolves to: one school never sees or changes another school's campuses. Each campus gets a
/// four-digit code, unique in its school. The master tenant sees only its own.
/// </summary>
public class SchoolCampusTests : TestBase
{
    private const string SchoolDomain = "school1.localhost";
    private const string OtherSchoolDomain = "school2.localhost";

    [Test]
    public async Task ShouldListTheMainCampusAndLetTheSchoolAddAnotherWithAGeneratedCode()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await SchoolWithLocationAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");

        (await ReadAsync(admin)).ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            c => c.Kind.ShouldBe(CampusKind.Main),
            c => c.Code.ShouldBe("0001"));

        await AddCampusAsync(admin, "Karachi");

        var after = await ReadAsync(admin);
        after.Count.ShouldBe(2);
        var karachi = after.Single(c => c.Kind == CampusKind.Other);
        karachi.Name.ShouldBe("Karachi");
        karachi.Code.ShouldMatch("^[0-9]{4}$");
        karachi.DisplayCode.ShouldBe(karachi.Code);
    }

    [Test]
    public async Task ShouldGiveEveryCampusOfASchoolADifferentFourDigitCode()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await SchoolWithLocationAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");

        for (var i = 0; i < 6; i++)
        {
            await AddCampusAsync(admin, $"Campus {i}");
        }

        var codes = (await ReadAsync(admin)).Select(c => c.Code).ToList();
        codes.Count.ShouldBe(7);
        codes.Distinct().Count().ShouldBe(7);
        codes.ShouldAllBe(code => code.Length == 4 && code.All(char.IsDigit));
    }

    [Test]
    public async Task ShouldLetTheSchoolRenameAndDeleteItsOwnCampusButNotChangeItsCode()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await SchoolWithLocationAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        var campus = await AddCampusAsync(admin, "Karachi");
        var code = (await ReadAsync(admin)).Single(c => c.Id == campus).Code;

        using var rename = await admin.PutAsync($"/api/Campuses/{campus}", new { name = "Karachi North" });
        rename.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadAsync(admin)).Single(c => c.Id == campus).ShouldSatisfyAllConditions(c => c.Name.ShouldBe("Karachi North"), c => c.Code.ShouldBe(code));

        using var delete = await admin.DeleteAsync($"/api/Campuses/{campus}");
        delete.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadAsync(admin)).ShouldNotContain(c => c.Id == campus);
    }

    [Test]
    public async Task ShouldNeverLetOneSchoolReachAnothersCampuses()
    {
        await TestApp.RunAsAdministratorAsync();
        var one = await SchoolWithLocationAsync("School One", SchoolDomain);
        var two = await SchoolWithLocationAsync("School Two", OtherSchoolDomain);
        using var adminOne = await SignInToSchoolAsync(one.Id, SchoolDomain, "admin@school1.local");
        using var adminTwo = await SignInToSchoolAsync(two.Id, OtherSchoolDomain, "admin@school2.local");
        var campusOfOne = await AddCampusAsync(adminOne, "Karachi");

        (await ReadAsync(adminTwo)).ShouldHaveSingleItem().Kind.ShouldBe(CampusKind.Main);

        using var edit = await adminTwo.PutAsync($"/api/Campuses/{campusOfOne}", new { name = "Stolen" });
        edit.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        using var delete = await adminTwo.DeleteAsync($"/api/Campuses/{campusOfOne}");
        delete.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ShouldKeepTheMainCampusItCannotBeDeleted()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await SchoolWithLocationAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        var main = (await ReadAsync(admin)).Single(c => c.Kind == CampusKind.Main).Id;

        using var delete = await admin.DeleteAsync($"/api/Campuses/{main}");

        delete.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadAsync(admin)).ShouldContain(c => c.Id == main);
    }

    [Test]
    public async Task ShouldRefuseToDeleteACampusThatStillHasModules()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await SchoolWithLocationAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        var campus = await AddCampusAsync(admin, "Karachi");
        var (schoolId, campusId) = await IdsAsync(school.PublicId, campus);
        await TestApp.SubscribeAsync(schoolId, [ModuleCode.Fees], campusId: campusId);

        using var delete = await admin.DeleteAsync($"/api/Campuses/{campus}");

        delete.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadAsync(admin)).ShouldContain(c => c.Id == campus);
    }

    [Test]
    public async Task ShouldShowTheCodeWithItsPrefixOnlyWhileThePrefixIsOn()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await SchoolWithLocationAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        await AddCampusAsync(admin, "Karachi");

        using var on = await admin.PutAsync("/api/Campuses/settings", new { prefixEnabled = true, prefix = "khi" });
        on.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadAsync(admin)).ShouldAllBe(c => c.DisplayCode == "KHI" + c.Code);

        using var off = await admin.PutAsync("/api/Campuses/settings", new { prefixEnabled = false, prefix = "KHI" });
        off.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadAsync(admin)).ShouldAllBe(c => c.DisplayCode == c.Code);
    }

    [Test]
    public async Task ShouldReadTheSettingsItSaved()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await SchoolWithLocationAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        using var save = await admin.PutAsync("/api/Campuses/settings", new { prefixEnabled = true, prefix = "KHI" });
        save.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var read = await admin.GetAsync("/api/Campuses/settings");

        read.StatusCode.ShouldBe(HttpStatusCode.OK);
        var settings = await read.Content.ReadFromJsonAsync<SettingsReadModel>(Json);
        settings.ShouldSatisfyAllConditions(s => s!.PrefixEnabled.ShouldBeTrue(), s => s!.Prefix.ShouldBe("KHI"));
    }

    [TestCase(true, "")]
    [TestCase(true, null)]
    [TestCase(true, "AB C")]
    [TestCase(true, "ABCDEFG")]
    public async Task ShouldRefuseAPrefixThatIsMissingOrNotLettersAndDigits(bool enabled, string? prefix)
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await SchoolWithLocationAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");

        using var response = await admin.PutAsync("/api/Campuses/settings", new { prefixEnabled = enabled, prefix });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseCampusesToAnonymousVisitors()
    {
        await TestApp.RunAsAdministratorAsync();
        await SchoolWithLocationAsync("School One", SchoolDomain);
        var anonymous = TestHttp.CreateDevelopmentSession(SchoolDomain);

        using var response = await anonymous.GetAsync("/api/Campuses");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldShowTheMasterTenantOnlyItsOwnCampuses()
    {
        await TestApp.RunAsAdministratorAsync();
        await SchoolWithLocationAsync("School One", SchoolDomain);
        using var master = TestHttp.CreateDevelopmentSession();
        using var signIn = await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword);
        signIn.StatusCode.ShouldBe(HttpStatusCode.OK);

        (await ReadAsync(master)).ShouldHaveSingleItem().Kind.ShouldBe(CampusKind.Main);
    }

    private static async Task<HttpSession> SignInToSchoolAsync(int schoolId, string domain, string email)
    {
        await TestApp.RunAsUserAsync(schoolId, email, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(domain);
        using var response = await session.SignInAsync(email, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return session;
    }

    /// <summary>A school in Pakistan, the country that has the one state and city the tests use.</summary>
    private static async Task<EntityIds> SchoolWithLocationAsync(string name, string domain)
    {
        using (var existingScope = FunctionalTestSetup.ScopeFactory.CreateScope())
        {
            // The second school of a test uses the country, state and city the first one created.
            var existing = existingScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            if (await existing.Countries.AnyAsync(c => c.Code == "PK"))
            {
                var countryId = await existing.Countries.Where(c => c.Code == "PK").Select(c => c.Id).SingleAsync();
                return await TestApp.AddSchoolAsync(name, domain, countryId);
            }
        }

        var country = await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");
        using (var scope = FunctionalTestSetup.ScopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var state = State.CreateCustom(country.Id, "Sindh", null, StateType.Province);
            context.States.Add(state);
            await context.SaveChangesAsync();
            context.Cities.Add(City.CreateCustom(state.Id, "Karachi", null, null, null));
            await context.SaveChangesAsync();
        }

        return await TestApp.AddSchoolAsync(name, domain, country.Id);
    }

    /// <summary>A complete, valid set of campus details: the required fields, one phone number, the state and city above.</summary>
    private static async Task<object> DetailsAsync()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var state = await context.States.AsNoTracking().FirstAsync();
        var city = await context.Cities.AsNoTracking().FirstAsync();

        return new
        {
            address = "12 Main Road",
            stateId = state.PublicId,
            cityId = city.PublicId,
            postalCode = "75500",
            phones = new[] { "+92 21 1234567" },
            schoolNumber = "SN-1",
            shortName = "KHI",
            latitude = 24.8607,
            longitude = 67.0011,
        };
    }
    private static async Task<Guid> AddCampusAsync(HttpSession session, string name)
    {
        using var response = await session.PostAsync("/api/Campuses", new { name, details = await DetailsAsync() });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static async Task<List<CampusReadModel>> ReadAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/Campuses");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<CampusReadModel>>(Json))!;
    }

    private static async Task<(int SchoolId, int CampusId)> IdsAsync(Guid schoolPublicId, Guid campusPublicId)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        // Campuses are tenant-filtered: look them up as the master tenant, which sees every school's rows.
        TestApp.SetTenant(scope.ServiceProvider, TestApp.Resolve(TestApp.MasterSchoolId, isMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var schoolId = await context.Schools.Where(s => s.PublicId == schoolPublicId).Select(s => s.Id).SingleAsync();
        var campusId = await context.Campuses.Where(b => b.PublicId == campusPublicId).Select(b => b.Id).SingleAsync();
        return (schoolId, campusId);
    }

    // The API sends enums by name (ENG-ENUM-01).
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private sealed record CampusReadModel(Guid Id, string Name, string Code, string DisplayCode, CampusKind Kind);

    private sealed record SettingsReadModel(bool PrefixEnabled, string? Prefix);
}
