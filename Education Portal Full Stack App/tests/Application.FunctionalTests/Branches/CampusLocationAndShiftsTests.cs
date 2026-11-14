using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Branches;

/// <summary>
/// A school records where each campus is (its province and city from the location data, its address and its map pin) and its
/// daily shifts (Morning, Evening). Only the school's administrator can change them, and only for the school's own campuses:
/// another school's campus is not found.
/// </summary>
public class CampusLocationAndShiftsTests : TestBase
{
    private const string SchoolDomain = "school1.localhost";
    private const string OtherSchoolDomain = "school2.localhost";

    [Test]
    public async Task ShouldSaveTheLocationOfACampusAndShowItBack()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        var campus = await AddCampusAsync(admin, "Lahore");
        var geography = await SeedGeographyAsync();

        using var save = await admin.PutAsync($"/api/Branches/{campus}/location", new
        {
            stateId = geography.Punjab,
            cityId = geography.Lahore,
            addressLine = "  Main Boulevard, Gulberg  ",
            latitude = 31.520370m,
            longitude = 74.358749m,
            googlePlaceId = "ChIJ-example",
            googleMapsUrl = "https://maps.google.com/?q=31.520370,74.358749",
        });

        save.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var location = (await ReadAsync(admin)).Single(c => c.Id == campus).Location;
        location.ShouldSatisfyAllConditions(
            l => l.StateId.ShouldBe(geography.Punjab),
            l => l.CityId.ShouldBe(geography.Lahore),
            l => l.AddressLine.ShouldBe("Main Boulevard, Gulberg"),
            l => l.Latitude.ShouldBe(31.520370m),
            l => l.Longitude.ShouldBe(74.358749m),
            l => l.GooglePlaceId.ShouldBe("ChIJ-example"),
            l => l.GoogleMapsUrl.ShouldBe("https://maps.google.com/?q=31.520370,74.358749"));
    }

    [Test]
    public async Task ShouldRefuseACityThatIsNotInTheChosenProvince()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        var campus = await AddCampusAsync(admin, "Karachi");
        var geography = await SeedGeographyAsync();

        using var save = await admin.PutAsync($"/api/Branches/{campus}/location", new { stateId = geography.Sindh, cityId = geography.Lahore });

        save.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseAProvinceThatIsNotInTheLocationData()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        var campus = await AddCampusAsync(admin, "Karachi");

        using var save = await admin.PutAsync($"/api/Branches/{campus}/location", new { stateId = Guid.NewGuid() });

        save.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ShouldSaveTheShiftsOfACampusInStartOrder()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        var campus = await AddCampusAsync(admin, "Lahore");

        using var save = await admin.PutAsync($"/api/Branches/{campus}/shifts", new
        {
            shifts = new object[]
            {
                new { name = "Evening", startTime = "14:00:00", endTime = "17:30:00" },
                new { name = "Morning", startTime = "08:00:00", endTime = "13:30:00" },
            },
        });

        save.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadAsync(admin)).Single(c => c.Id == campus).Shifts
            .Select(s => s.Name).ShouldBe(["Morning", "Evening"]);
    }

    [Test]
    public async Task ShouldReplaceTheShiftsAndHideTheOldOnes()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        var campus = await AddCampusAsync(admin, "Lahore");
        using var first = await admin.PutAsync($"/api/Branches/{campus}/shifts", new
        {
            shifts = new[] { new { name = "Morning", startTime = "08:00:00", endTime = "13:30:00" }, new { name = "Evening", startTime = "14:00:00", endTime = "17:30:00" } },
        });
        first.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var second = await admin.PutAsync($"/api/Branches/{campus}/shifts", new
        {
            shifts = new[] { new { name = "Morning", startTime = "07:30:00", endTime = "12:30:00" } },
        });

        second.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadAsync(admin)).Single(c => c.Id == campus).Shifts.ShouldHaveSingleItem()
            .ShouldSatisfyAllConditions(s => s.Name.ShouldBe("Morning"), s => s.StartTime.ShouldBe(new TimeOnly(7, 30)));
    }

    [Test]
    public async Task ShouldRefuseMoreThanFourShifts()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        var campus = await AddCampusAsync(admin, "Lahore");

        using var save = await admin.PutAsync($"/api/Branches/{campus}/shifts", new
        {
            shifts = Enumerable.Range(1, 5).Select(i => new { name = $"Shift {i}", startTime = $"{5 + i:00}:00:00", endTime = $"{5 + i:00}:30:00" }).ToArray(),
        });

        save.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseAShiftThatEndsBeforeItStarts()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        var campus = await AddCampusAsync(admin, "Lahore");

        using var save = await admin.PutAsync($"/api/Branches/{campus}/shifts", new
        {
            shifts = new[] { new { name = "Evening", startTime = "17:30:00", endTime = "14:00:00" } },
        });

        save.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldNeverLetOneSchoolChangeAnothersCampusLocationOrShifts()
    {
        await TestApp.RunAsAdministratorAsync();
        var one = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var two = await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        using var adminOne = await SignInToSchoolAsync(one.Id, SchoolDomain, "admin@school1.local");
        using var adminTwo = await SignInToSchoolAsync(two.Id, OtherSchoolDomain, "admin@school2.local");
        var campusOfOne = await AddCampusAsync(adminOne, "Karachi");

        using var location = await adminTwo.PutAsync($"/api/Branches/{campusOfOne}/location", new { addressLine = "Stolen" });
        location.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        using var shifts = await adminTwo.PutAsync($"/api/Branches/{campusOfOne}/shifts", new
        {
            shifts = new[] { new { name = "Morning", startTime = "08:00:00", endTime = "13:30:00" } },
        });
        shifts.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        (await ReadAsync(adminOne)).Single(c => c.Id == campusOfOne).ShouldSatisfyAllConditions(
            c => c.Location.AddressLine.ShouldBeNull(),
            c => c.Shifts.ShouldBeEmpty());
    }

    [Test]
    public async Task ShouldRefuseTheLocationAndShiftsToAnonymousVisitors()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var admin = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        var campus = await AddCampusAsync(admin, "Karachi");
        var anonymous = TestHttp.CreateDevelopmentSession(SchoolDomain);

        using var location = await anonymous.PutAsync($"/api/Branches/{campus}/location", new { addressLine = "x" });
        location.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static async Task<HttpSession> SignInToSchoolAsync(int schoolId, string domain, string email)
    {
        await TestApp.RunAsUserAsync(schoolId, email, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(domain);
        using var response = await session.SignInAsync(email, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return session;
    }

    private static async Task<Guid> AddCampusAsync(HttpSession session, string name)
    {
        using var response = await session.PostAsync("/api/Branches", new { name });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static async Task<List<CampusReadModel>> ReadAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/Branches");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<CampusReadModel>>(Json))!;
    }

    /// <summary>Pakistan with a province, a second province and a city in the first (the location data is one tree).</summary>
    private static async Task<(Guid Punjab, Guid Sindh, Guid Lahore)> SeedGeographyAsync()
    {
        var country = await TestApp.AddCountryAsync("PK", "Pakistan", ["en"], "en", "Asia/Karachi", "PKR");
        var punjab = State.CreateCustom(country.Id, "Punjab", null, StateType.Region);
        var sindh = State.CreateCustom(country.Id, "Sindh", null, StateType.Region);
        await TestApp.AddAsync(punjab);
        await TestApp.AddAsync(sindh);
        var lahore = City.CreateCustom(punjab.Id, "Lahore", null, 31.5204m, 74.3587m);
        await TestApp.AddAsync(lahore);

        return (punjab.PublicId, sindh.PublicId, lahore.PublicId);
    }

    // The API sends enums by name (ENG-ENUM-01).
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private sealed record CampusReadModel(Guid Id, string Name, string Code, string DisplayCode, BranchKind Kind, LocationReadModel Location, List<ShiftReadModel> Shifts);

    private sealed record LocationReadModel(Guid? StateId, Guid? CityId, string? AddressLine, decimal? Latitude, decimal? Longitude, string? GooglePlaceId, string? GoogleMapsUrl);

    private sealed record ShiftReadModel(string Name, TimeOnly StartTime, TimeOnly EndTime);
}
