using System.Net;
using System.Text.Json;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Application.FunctionalTests.AcademicCalendar;

/// <summary>
/// A school's registration and contact details (School module), a campus's local name and gender policy, and a country's
/// regional settings (master tenant only). One school never reads or changes another's details.
/// </summary>
public class RegionalDetailsTests : TestBase
{
    private const string SchoolDomain = "school1.localhost";
    private const string OtherSchoolDomain = "school2.localhost";

    [Test]
    public async Task ShouldLetASchoolSaveItsDetailsWithACityOfItsCountryOnly()
    {
        await TestApp.RunAsAdministratorAsync();
        var pakistan = await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");
        var usa = await TestApp.AddCountryAsync("US", "United States", ["en"], "en", "America/New_York", "USD");
        var karachi = await AddCityAsync(pakistan.Id, "Sindh", "Karachi");
        var austin = await AddCityAsync(usa.Id, "Texas", "Austin");
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain, pakistan.Id);
        using var admin = await SignInAsync(school.Id, SchoolDomain, "admin@school1.local", [Roles.Administrator]);

        (await admin.PutAsync("/api/SchoolDetails", Details(karachi.City))).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var saved = await ReadJsonAsync(admin, "/api/SchoolDetails");
        saved.GetProperty("localName").GetString().ShouldBe("مدرسہ");
        saved.GetProperty("cityId").GetGuid().ShouldBe(karachi.City);
        saved.GetProperty("stateId").GetGuid().ShouldBe(karachi.State);
        saved.GetProperty("academicYearStartMonth").GetInt32().ShouldBe(4);

        (await admin.PutAsync("/api/SchoolDetails", Details(austin.City))).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await admin.PutAsync("/api/SchoolDetails", Details(null) with { AcademicYearStartMonth = 13 })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldNeverLetAnotherSchoolReadOrChangeTheDetails()
    {
        await TestApp.RunAsAdministratorAsync();
        var one = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var two = await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        using var adminTwo = await SignInAsync(two.Id, OtherSchoolDomain, "admin@school2.local", [Roles.Administrator]);

        (await adminTwo.GetAsync($"/api/SchoolDetails?schoolId={one.PublicId}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await adminTwo.PutAsync($"/api/SchoolDetails?schoolId={one.PublicId}", Details(null))).StatusCode.ShouldBe(HttpStatusCode.NotFound);

        using var staff = await SignInAsync(one.Id, SchoolDomain, "staff@school1.local", []);
        (await staff.GetAsync("/api/SchoolDetails")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldSaveACampusLocalNameAndGenderPolicy()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInAsync(school.Id, SchoolDomain, "admin@school1.local", [Roles.Administrator]);
        var campus = (await ReadJsonAsync(admin, "/api/Campuses")).EnumerateArray().Single().GetProperty("id").GetGuid();

        (await admin.PutAsync($"/api/Campuses/{campus}", new { name = "Main campus", localName = "مرکزی کیمپس", genderPolicy = "GirlsOnly" })).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var saved = (await ReadJsonAsync(admin, "/api/Campuses")).EnumerateArray().Single();
        saved.GetProperty("localName").GetString().ShouldBe("مرکزی کیمپس");
        saved.GetProperty("genderPolicy").GetString().ShouldBe("GirlsOnly");
    }

    [Test]
    public async Task ShouldLetOnlyTheMasterTenantChangeACountrysRegionalSettings()
    {
        await TestApp.RunAsAdministratorAsync();
        var saudi = await TestApp.AddCountryAsync("SA", "Saudi Arabia", ["en", "ar"], "ar", "Asia/Riyadh", "SAR");
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain, saudi.Id);
        var settings = new { currencyDecimals = 2, phonePattern = @"^05\d{8}$", weekStartDay = "Sunday", weekendDays = new[] { "Saturday", "Friday" } };

        using var schoolAdmin = await SignInAsync(school.Id, SchoolDomain, "admin@school1.local", [Roles.Administrator]);
        (await schoolAdmin.PutAsync($"/api/Countries/{saudi.PublicId}/regional-settings", settings)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        using var master = TestHttp.CreateDevelopmentSession();
        (await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await master.PutAsync($"/api/Countries/{saudi.PublicId}/regional-settings", settings)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await master.PutAsync($"/api/Countries/{saudi.PublicId}/regional-settings", settings with { phonePattern = "[bad" })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var country = (await ReadJsonAsync(master, "/api/Countries")).GetProperty("items").EnumerateArray().Single(c => c.GetProperty("code").GetString() == "SA");
        country.GetProperty("weekStartDay").GetString().ShouldBe("Sunday");
        country.GetProperty("weekendDays").EnumerateArray().Select(d => d.GetString()).ShouldBe(["Friday", "Saturday"]);
    }

    private static DetailsBody Details(Guid? cityId) =>
        new("مدرسہ", "REG-1", "Sindh board", "NTN-9", "1 Main road", cityId, "+92 21 1234567", "office@school.example", "https://school.example", 4);

    private sealed record DetailsBody(
        string? LocalName,
        string? RegistrationNumber,
        string? RegistrationAuthority,
        string? TaxNumber,
        string? Address,
        Guid? CityId,
        string? Phone,
        string? Email,
        string? Website,
        int AcademicYearStartMonth);

    private static async Task<(Guid State, Guid City)> AddCityAsync(int countryId, string stateName, string cityName)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var state = State.CreateCustom(countryId, stateName, null, StateType.Province);
        context.States.Add(state);
        await context.SaveChangesAsync();
        var city = City.CreateCustom(state.Id, cityName, null, null, null);
        context.Cities.Add(city);
        await context.SaveChangesAsync();
        return (state.PublicId, city.PublicId);
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpSession session, string path)
    {
        using var response = await session.GetAsync(path);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, path);
        return JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement.Clone();
    }

    private static async Task<HttpSession> SignInAsync(int schoolId, string domain, string email, string[] roles)
    {
        await TestApp.RunAsUserAsync(schoolId, email, TestApp.AdministratorPassword, roles);
        var session = TestHttp.CreateDevelopmentSession(domain);
        using var response = await session.SignInAsync(email, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return session;
    }
}
