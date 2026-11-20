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
/// A campus holds its contact and registration details. The address, state, city, postal code, at least one phone number,
/// the school number and the short name are required; the website and principal are optional. The state and city must
/// belong to the school's country, and the principal must be a principal of the same school.
/// </summary>
public class CampusDetailsTests : TestBase
{
    private const string SchoolDomain = "campus-details.localhost";

    [Test]
    public async Task ShouldSaveTheCampusDetailsWithTheirPhoneNumbersInOrder()
    {
        var (session, country) = await SetUpSchoolAsync("Campus Details School", SchoolDomain);
        var state = await AddStateAsync(country.Id, "Sindh");
        var city = await AddCityAsync(state.Id, "Karachi");
        var principal = await AddPersonAsync(session, "Ayesha", "Noor", "Khan", "Principal");

        using var create = await session.PostAsync("/api/Campuses", new
        {
            name = "Karachi",
            details = new
            {
                address = "12 Main Road",
                stateId = state.PublicId,
                cityId = city.PublicId,
                postalCode = "75500",
                phones = new[] { "+92 21 1234567", "0300-1234567" },
                website = "https://karachi.example.com",
                schoolNumber = "SN-1",
                shortName = "KHI",
                principalId = principal,
                latitude = 24.8607,
                longitude = 67.0011,
            },
        });
        create.StatusCode.ShouldBe(HttpStatusCode.OK);

        var campus = (await ReadAsync(session)).Single(c => c.Name == "Karachi");
        campus.Details.ShouldSatisfyAllConditions(
            d => d.Address.ShouldBe("12 Main Road"),
            d => d.StateName.ShouldBe("Sindh"),
            d => d.CityName.ShouldBe("Karachi"),
            d => d.PostalCode.ShouldBe("75500"),
            d => d.Phones.ShouldBe(["+92 21 1234567", "0300-1234567"]),
            d => d.Website.ShouldBe("https://karachi.example.com"),
            d => d.SchoolNumber.ShouldBe("SN-1"),
            d => d.ShortName.ShouldBe("KHI"),
            d => d.Latitude.ShouldBe(24.8607m),
            d => d.Longitude.ShouldBe(67.0011m),
            d => d.PrincipalName.ShouldBe("Ayesha Noor Khan"));
    }

    [Test]
    public async Task ShouldAcceptACampusWithoutAWebsiteOrPrincipal()
    {
        var (session, country) = await SetUpSchoolAsync("Optional Fields School", SchoolDomain);
        var state = await AddStateAsync(country.Id, "Punjab");
        var city = await AddCityAsync(state.Id, "Lahore");

        using var create = await session.PostAsync("/api/Campuses", new { name = "Lahore", details = Details(state.PublicId, city.PublicId) });

        create.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task ShouldSaveTheLocalNameAndGenderPolicyOfANewCampus()
    {
        var (session, country) = await SetUpSchoolAsync("Profile School", SchoolDomain);
        var state = await AddStateAsync(country.Id, "Sindh");
        var city = await AddCityAsync(state.Id, "Karachi");

        using var create = await session.PostAsync("/api/Campuses", new
        {
            name = "Girls campus",
            localName = "  گرلز کیمپس  ",
            genderPolicy = "GirlsOnly",
            details = Details(state.PublicId, city.PublicId),
        });
        create.StatusCode.ShouldBe(HttpStatusCode.OK);

        var campus = (await ReadAsync(session)).Single(c => c.Name == "Girls campus");
        campus.LocalName.ShouldBe("گرلز کیمپس");
        campus.GenderPolicy.ShouldBe(SchoolGenderPolicy.GirlsOnly);
    }

    [TestCase("address")]
    [TestCase("postalCode")]
    [TestCase("schoolNumber")]
    [TestCase("shortName")]
    [TestCase("latitude")]
    [TestCase("longitude")]
    public async Task ShouldRefuseACampusMissingARequiredField(string field)
    {
        var (session, country) = await SetUpSchoolAsync("Required School", SchoolDomain);
        var state = await AddStateAsync(country.Id, "Sindh");
        var city = await AddCityAsync(state.Id, "Karachi");
        var details = Details(state.PublicId, city.PublicId);
        details.Remove(field);

        using var response = await session.PostAsync("/api/Campuses", new { name = "Campus", details });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseACampusWithoutAStateOrCity()
    {
        var (session, country) = await SetUpSchoolAsync("No Location School", SchoolDomain);
        var state = await AddStateAsync(country.Id, "Sindh");
        var city = await AddCityAsync(state.Id, "Karachi");

        using var noState = await session.PostAsync("/api/Campuses", new { name = "Campus", details = Details(null, city.PublicId) });
        using var noCity = await session.PostAsync("/api/Campuses", new { name = "Campus", details = Details(state.PublicId, null) });

        noState.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        noCity.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseACampusWithMoreThanFivePhoneNumbers()
    {
        var (session, country) = await SetUpSchoolAsync("Many Phones School", SchoolDomain);
        var state = await AddStateAsync(country.Id, "Sindh");
        var city = await AddCityAsync(state.Id, "Karachi");
        var phones = Enumerable.Range(0, 6).Select(i => $"+92 21 000000{i}").ToArray();

        using var response = await session.PostAsync("/api/Campuses", new { name = "Campus", details = Details(state.PublicId, city.PublicId, phones: phones) });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldKeepTheDetailsWhenAnUpdateLeavesThemOut()
    {
        var (session, country) = await SetUpSchoolAsync("Keep Details School", SchoolDomain);
        var state = await AddStateAsync(country.Id, "Punjab");
        var city = await AddCityAsync(state.Id, "Lahore");
        var campusId = await CreateCampusAsync(session, "Lahore", Details(state.PublicId, city.PublicId));

        using var rename = await session.PutAsync($"/api/Campuses/{campusId}", new { name = "Lahore Main" });
        rename.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var campus = (await ReadAsync(session)).Single(c => c.Id == campusId);
        campus.Name.ShouldBe("Lahore Main");
        campus.Details.Address.ShouldBe("12 Main Road");
        campus.Details.StateName.ShouldBe("Punjab");
    }

    [Test]
    public async Task ShouldRefuseAnUpdateThatEmptiesTheRequiredDetails()
    {
        var (session, country) = await SetUpSchoolAsync("Empty Update School", SchoolDomain);
        var state = await AddStateAsync(country.Id, "Punjab");
        var city = await AddCityAsync(state.Id, "Multan");
        var campusId = await CreateCampusAsync(session, "Multan", Details(state.PublicId, city.PublicId));

        using var update = await session.PutAsync($"/api/Campuses/{campusId}", new { name = "Multan", details = new { } });

        update.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadAsync(session)).Single(c => c.Id == campusId).Details.Address.ShouldBe("12 Main Road");
    }

    [Test]
    public async Task ShouldRefuseAStateOutsideTheSchoolsCountry()
    {
        var (session, _) = await SetUpSchoolAsync("Country Check School", SchoolDomain);
        var otherCountry = await TestApp.AddCountryAsync("US", "United States", ["en"], "en", "America/New_York", "USD");
        var foreignState = await AddStateAsync(otherCountry.Id, "Texas");
        var foreignCity = await AddCityAsync(foreignState.Id, "Austin");

        using var response = await session.PostAsync("/api/Campuses", new { name = "Austin", details = Details(foreignState.PublicId, foreignCity.PublicId) });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseACityThatIsNotInTheChosenState()
    {
        var (session, country) = await SetUpSchoolAsync("City Check School", SchoolDomain);
        var sindh = await AddStateAsync(country.Id, "Sindh");
        var punjab = await AddStateAsync(country.Id, "Punjab");
        var lahore = await AddCityAsync(punjab.Id, "Lahore");

        using var response = await session.PostAsync("/api/Campuses", new { name = "Mismatch", details = Details(sindh.PublicId, lahore.PublicId) });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseAPrincipalWhoIsNotAPrincipal()
    {
        var (session, country) = await SetUpSchoolAsync("Principal Type School", SchoolDomain);
        var state = await AddStateAsync(country.Id, "Sindh");
        var city = await AddCityAsync(state.Id, "Karachi");
        var teacher = await AddPersonAsync(session, "Bilal", null, "Ahmed", "Teacher");

        using var response = await session.PostAsync("/api/Campuses", new { name = "Campus", details = Details(state.PublicId, city.PublicId, principalId: teacher) });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseAPrincipalOfAnotherSchool()
    {
        var (session, country) = await SetUpSchoolAsync("Own Principal School", SchoolDomain);
        var state = await AddStateAsync(country.Id, "Sindh");
        var city = await AddCityAsync(state.Id, "Karachi");
        var other = await TestApp.AddSchoolAsync("Other Principal School", "other-principal.localhost");
        var foreignPrincipal = await AddPersonInSchoolAsync(other.Id, "Sara", null, "Ali", UserType.Principal);

        using var response = await session.PostAsync("/api/Campuses", new { name = "Campus", details = Details(state.PublicId, city.PublicId, principalId: foreignPrincipal) });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseAPhoneNumberThatIsNotValid()
    {
        var (session, country) = await SetUpSchoolAsync("Phone School", SchoolDomain);
        var state = await AddStateAsync(country.Id, "Sindh");
        var city = await AddCityAsync(state.Id, "Karachi");

        using var response = await session.PostAsync("/api/Campuses", new { name = "Campus", details = Details(state.PublicId, city.PublicId, phones: ["123"]) });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    /// <summary>A complete set of details with the given location and the required fields, as the API takes them.</summary>
    [Test]
    public async Task ShouldRefuseACampusOutsideTheMapLimits()
    {
        var (session, country) = await SetUpSchoolAsync("Map Limit School", SchoolDomain);
        var state = await AddStateAsync(country.Id, "Sindh");
        var city = await AddCityAsync(state.Id, "Karachi");
        var details = Details(state.PublicId, city.PublicId);
        details["latitude"] = 91;

        using var response = await session.PostAsync("/api/Campuses", new { name = "Campus", details });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldListTheSchoolsCountryWithItsStatesAndCitiesForTheCampusFilters()
    {
        var (session, country) = await SetUpSchoolAsync("Locations School", SchoolDomain);
        var state = await AddStateAsync(country.Id, "Punjab");
        await AddCityAsync(state.Id, "Lahore");

        using var response = await session.GetAsync("/api/Campuses/locations");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var locations = await response.Content.ReadFromJsonAsync<LocationsReadModel>(Json);
        locations.ShouldNotBeNull();
        locations.CountryName.ShouldBe("Pakistan");
        locations.States.Single().Name.ShouldBe("Punjab");
        locations.States.Single().Cities.Single().Name.ShouldBe("Lahore");
    }

    private static Dictionary<string, object?> Details(Guid? stateId, Guid? cityId, string[]? phones = null, Guid? principalId = null) => new()
    {
        ["address"] = "12 Main Road",
        ["stateId"] = stateId,
        ["cityId"] = cityId,
        ["postalCode"] = "75500",
        ["phones"] = phones ?? ["+92 21 1234567"],
        ["schoolNumber"] = "SN-1",
        ["shortName"] = "KHI",
        ["principalId"] = principalId,
        ["latitude"] = 24.8607,
        ["longitude"] = 67.0011,
    };

    private static async Task<(HttpSession Session, EntityIds Country)> SetUpSchoolAsync(string name, string domain)
    {
        await TestApp.RunAsAdministratorAsync();
        var country = await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");
        var school = await TestApp.AddSchoolAsync(name, domain, country.Id);
        var email = $"admin@{school.Id}.campus.local";
        await TestApp.RunAsUserAsync(school.Id, email, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(domain);
        using var response = await session.SignInAsync(email, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (session, country);
    }

    private static async Task<Guid> CreateCampusAsync(HttpSession session, string name, object details)
    {
        using var response = await session.PostAsync("/api/Campuses", new { name, details });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static async Task<Guid> AddPersonAsync(HttpSession session, string firstName, string? middleName, string lastName, string userType)
    {
        using var response = await session.PostAsync("/api/UserDetails", new { firstName, middleName, lastName, userType });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static async Task<Guid> AddPersonInSchoolAsync(int schoolId, string firstName, string? middleName, string lastName, UserType userType)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var person = UserDetails.Create(schoolId, firstName, middleName, lastName, userType, null);
        context.UserDetails.Add(person);
        await context.SaveChangesAsync();
        return person.PublicId;
    }

    private static async Task<State> AddStateAsync(int countryId, string name)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var state = State.CreateCustom(countryId, name, null, StateType.Province);
        context.States.Add(state);
        await context.SaveChangesAsync();
        return state;
    }

    private static async Task<City> AddCityAsync(int stateId, string name)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var city = City.CreateCustom(stateId, name, null, null, null);
        context.Cities.Add(city);
        await context.SaveChangesAsync();
        return city;
    }

    private static async Task<List<CampusReadModel>> ReadAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/Campuses");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<CampusReadModel>>(Json))!;
    }

    // The API sends enums by name (ENG-ENUM-01).
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private sealed record CampusReadModel(Guid Id, string Name, CampusDetailsReadModel Details, string? LocalName = null, SchoolGenderPolicy GenderPolicy = SchoolGenderPolicy.CoEducation);

    private sealed record LocationsReadModel(string? CountryName, List<StateReadModel> States);

    private sealed record StateReadModel(Guid Id, string Name, List<CityReadModel> Cities);

    private sealed record CityReadModel(Guid Id, string Name);

    private sealed record CampusDetailsReadModel(
        string? Address,
        Guid? StateId,
        string? StateName,
        Guid? CityId,
        string? CityName,
        string? PostalCode,
        IReadOnlyList<string> Phones,
        string? Website,
        string? SchoolNumber,
        string? ShortName,
        Guid? PrincipalId,
        string? PrincipalName,
        decimal? Latitude,
        decimal? Longitude);
}
