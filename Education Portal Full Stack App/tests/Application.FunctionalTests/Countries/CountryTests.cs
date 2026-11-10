using System.Net;
using System.Net.Http.Json;
using EducationPortal.Application.Countries.Common;
using EducationPortal.Application.Localization.Common;
using EducationPortal.Application.Localization.Queries.GetLocalization;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.FunctionalTests.Countries;

/// <summary>
/// Countries and the school-to-country assignment are master-portal settings: only master tenant administrators see or
/// change them, every change is audited, and input is checked (SEC-AUTHZ-07, SEC-AUTHZ-09, SEC-INPUT-01, SEC-LOG-03).
/// </summary>
public class CountryTests : TestBase
{
    private const string SchoolDomain = "school1.localhost";
    private const string SchoolAdminEmail = "admin@school1.local";

    private static readonly object Pakistan = new
    {
        code = "PK",
        name = "Pakistan",
        languages = new[] { "en", "ur" },
        defaultLanguage = "en",
        timeZoneId = "Asia/Karachi",
        currencyCode = "PKR",
    };

    [Test]
    public async Task ShouldLetTheMasterTenantAddACountryAndAssignASchoolToIt()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var userId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var created = await master.PostAsync("/api/Countries", Pakistan);
        created.StatusCode.ShouldBe(HttpStatusCode.OK);
        var countryId = await created.Content.ReadFromJsonAsync<int>();

        using var assigned = await master.PutAsync($"/api/Schools/{schoolId}/country", new { countryId });
        assigned.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var countries = await ReadCountriesAsync(master);
        var country = countries.Items.ShouldHaveSingleItem();
        country.Id.ShouldBe(countryId);
        country.Code.ShouldBe("PK");
        country.Languages.ShouldBe(["en", "ur"]);
        country.TimeZoneId.ShouldBe("Asia/Karachi");
        country.CurrencyCode.ShouldBe("PKR");

        using var school = TestHttp.CreateDevelopmentSession(SchoolDomain);
        using var response = await school.GetAsync("/api/Localization");
        var localization = (await response.Content.ReadFromJsonAsync<LocalizationDto>())!;
        localization.Languages.Select(l => l.Code).ShouldBe(["en", "ur"]);

        var audit = await TestApp.FindAuditEventsAsync(userId);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.CountryChanged && e.TargetId == countryId.ToString());
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.SchoolCountryChanged && e.TargetId == schoolId.ToString());
    }

    [Test]
    public async Task ShouldChangeACountrysLanguages()
    {
        var countryId = await TestApp.AddCountryAsync("AE", "United Arab Emirates", ["en"], "en", "Asia/Dubai", "AED");
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var response = await master.PutAsync($"/api/Countries/{countryId}", new
        {
            id = 0, // the route wins
            name = "United Arab Emirates",
            languages = new[] { "ar", "en" },
            defaultLanguage = "ar",
            timeZoneId = "Asia/Dubai",
            currencyCode = "AED",
        });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var country = (await ReadCountriesAsync(master)).Items.Single();
        country.Languages.ShouldBe(["ar", "en"]);
        country.DefaultLanguage.ShouldBe("ar");
    }

    [Test]
    public async Task ShouldListTheSupportedLanguages()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var response = await master.GetAsync("/api/Countries/languages");

        var languages = (await response.Content.ReadFromJsonAsync<List<LanguageDto>>())!;
        languages.Select(l => l.Code).ShouldBe(["en", "ar", "ur"]);
        languages.Single(l => l.Code == "ur").IsRightToLeft.ShouldBeTrue();
    }

    [TestCase("languages", "fr")]
    [TestCase("defaultLanguage", "ar")]
    [TestCase("timeZoneId", "Pakistan Standard Time")]
    [TestCase("timeZoneId", "Mars/Olympus")]
    [TestCase("currencyCode", "rs")]
    [TestCase("code", "PAK")]
    public async Task ShouldRefuseAnInvalidCountry(string field, string value)
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var body = new Dictionary<string, object>
        {
            ["code"] = "PK",
            ["name"] = "Pakistan",
            ["languages"] = new[] { "en", "ur" },
            ["defaultLanguage"] = "en",
            ["timeZoneId"] = "Asia/Karachi",
            ["currencyCode"] = "PKR",
        };
        body[field] = field == "languages" ? new[] { "en", value } : value;

        using var response = await master.PostAsync("/api/Countries", body);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await TestApp.CountAsync<Country>()).ShouldBe(0);
    }

    [Test]
    public async Task ShouldRefuseADuplicateCountryCode()
    {
        await TestApp.AddCountryAsync("PK", "Pakistan", ["en"], "en", "Asia/Karachi", "PKR");
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var response = await master.PostAsync("/api/Countries", new
        {
            code = "pk",
            name = "Pakistan again",
            languages = new[] { "en" },
            defaultLanguage = "en",
            timeZoneId = "Asia/Karachi",
            currencyCode = "PKR",
        });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseAnUnknownCountryForASchool()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var response = await master.PutAsync($"/api/Schools/{schoolId}/country", new { countryId = 999999 });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldAnswer404ForAnUnknownSchoolOrCountry()
    {
        var countryId = await TestApp.AddCountryAsync("PK", "Pakistan", ["en"], "en", "Asia/Karachi", "PKR");
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var school = await master.PutAsync("/api/Schools/999999/country", new { countryId });
        using var country = await master.PutAsync("/api/Countries/999999", Pakistan);

        school.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        country.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ShouldLetASchoolBackToNoCountry()
    {
        var countryId = await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "ur", "Asia/Karachi", "PKR");
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain, countryId);
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var response = await master.PutAsync($"/api/Schools/{schoolId}/country", new { countryId = (int?)null });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        using var school = TestHttp.CreateDevelopmentSession(SchoolDomain);
        using var localization = await school.GetAsync("/api/Localization");
        (await localization.Content.ReadFromJsonAsync<LocalizationDto>())!.Language.ShouldBe("en");
    }

    [Test]
    public async Task ShouldKeepCountrySettingsAwayFromSchoolAdministrators()
    {
        var countryId = await TestApp.AddCountryAsync("PK", "Pakistan", ["en"], "en", "Asia/Karachi", "PKR");
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsUserAsync(schoolId, SchoolAdminEmail, TestApp.AdministratorPassword, [Roles.Administrator]);
        using var session = TestHttp.CreateDevelopmentSession(SchoolDomain);
        (await session.SignInAsync(SchoolAdminEmail, TestApp.AdministratorPassword)).Dispose();

        using var list = await session.GetAsync("/api/Countries");
        using var languages = await session.GetAsync("/api/Countries/languages");
        using var create = await session.PostAsync("/api/Countries", Pakistan);
        using var update = await session.PutAsync($"/api/Countries/{countryId}", Pakistan);
        using var assign = await session.PutAsync($"/api/Schools/{schoolId}/country", new { countryId });

        list.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        languages.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        create.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        update.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        assign.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await TestApp.FindAsync<School>(schoolId))!.CountryId.ShouldBeNull();
    }

    [Test]
    public async Task ShouldRequireTheAdministratorRoleOnTheMasterTenant()
    {
        await TestApp.RunAsDefaultUserAsync();
        using var session = TestHttp.CreateDevelopmentSession();
        (await session.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword)).Dispose();

        using var list = await session.GetAsync("/api/Countries");
        using var create = await session.PostAsync("/api/Countries", Pakistan);

        list.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        create.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldRequireSignIn()
    {
        using var session = TestHttp.CreateDevelopmentSession();

        using var list = await session.GetAsync("/api/Countries");
        using var create = await session.PostAsync("/api/Countries", Pakistan);

        list.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        create.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static async Task<HttpSession> SignInAsMasterAsync()
    {
        var master = TestHttp.CreateDevelopmentSession();
        using var response = await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return master;
    }

    private static async Task<CountryPage> ReadCountriesAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/Countries");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<CountryPage>())!;
    }

    /// <summary>The JSON shape of one page of countries.</summary>
    private sealed record CountryPage(List<CountryDto> Items, int TotalCount);
}
