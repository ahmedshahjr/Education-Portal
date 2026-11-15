using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Branches;

/// <summary>
/// A campus holds contact and registration details. Its state and city must belong to the school's country, and its
/// principal must be a principal of the same school, so a campus never links to another school's or country's data.
/// </summary>
public class CampusDetailsTests : TestBase
{
    private const string SchoolDomain = "campus-details.localhost";

    [Test]
    public async Task ShouldSaveTheCampusDetailsAndShowTheirNames()
    {
        var (session, _, country) = await SetUpSchoolAsync("Campus Details School", SchoolDomain);
        var state = await AddStateAsync(country.Id, "Sindh");
        var city = await AddCityAsync(state.Id, "Karachi");
        var principal = await AddPersonAsync(session, "Ayesha", "Noor", "Khan", "Principal");

        using var create = await session.PostAsync("/api/Branches", new
        {
            name = "Karachi",
            details = new
            {
                address = "12 Main Road",
                stateId = state.PublicId,
                cityId = city.PublicId,
                postalCode = "75500",
                phone = "+92 21 1234567",
                website = "https://karachi.example.com",
                schoolNumber = "SN-1",
                shortName = "KHI",
                principalId = principal,
            },
        });
        create.StatusCode.ShouldBe(HttpStatusCode.OK);

        var campus = (await ReadAsync(session)).Single(c => c.Name == "Karachi");
        campus.Details.ShouldSatisfyAllConditions(
            d => d.Address.ShouldBe("12 Main Road"),
            d => d.StateName.ShouldBe("Sindh"),
            d => d.CityName.ShouldBe("Karachi"),
            d => d.PostalCode.ShouldBe("75500"),
            d => d.Phone.ShouldBe("+92 21 1234567"),
            d => d.Website.ShouldBe("https://karachi.example.com"),
            d => d.SchoolNumber.ShouldBe("SN-1"),
            d => d.ShortName.ShouldBe("KHI"),
            d => d.PrincipalName.ShouldBe("Ayesha Noor Khan"));
    }

    [Test]
    public async Task ShouldKeepTheDetailsWhenAnUpdateLeavesThemOut()
    {
        var (session, _, country) = await SetUpSchoolAsync("Keep Details School", SchoolDomain);
        var state = await AddStateAsync(country.Id, "Punjab");
        var campusId = await CreateCampusAsync(session, "Lahore", new { address = "5 Canal Road", stateId = state.PublicId });

        using var rename = await session.PutAsync($"/api/Branches/{campusId}", new { name = "Lahore Main" });
        rename.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var campus = (await ReadAsync(session)).Single(c => c.Id == campusId);
        campus.Name.ShouldBe("Lahore Main");
        campus.Details.Address.ShouldBe("5 Canal Road");
        campus.Details.StateName.ShouldBe("Punjab");
    }

    [Test]
    public async Task ShouldClearTheDetailsWhenAnUpdateSendsEmptyOnes()
    {
        var (session, _, country) = await SetUpSchoolAsync("Clear Details School", SchoolDomain);
        var state = await AddStateAsync(country.Id, "Punjab");
        var campusId = await CreateCampusAsync(session, "Multan", new { address = "1 Old Road", stateId = state.PublicId });

        using var update = await session.PutAsync($"/api/Branches/{campusId}", new { name = "Multan", details = new { } });
        update.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await ReadAsync(session)).Single(c => c.Id == campusId).Details.ShouldSatisfyAllConditions(
            d => d.Address.ShouldBeNull(),
            d => d.StateId.ShouldBeNull());
    }

    [Test]
    public async Task ShouldRefuseAStateOutsideTheSchoolsCountry()
    {
        var (session, _, _) = await SetUpSchoolAsync("Country Check School", SchoolDomain);
        var otherCountry = await TestApp.AddCountryAsync("US", "United States", ["en"], "en", "America/New_York", "USD");
        var foreignState = await AddStateAsync(otherCountry.Id, "Texas");

        using var response = await session.PostAsync("/api/Branches", new { name = "Austin", details = new { stateId = foreignState.PublicId } });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseACityThatIsNotInTheChosenState()
    {
        var (session, _, country) = await SetUpSchoolAsync("City Check School", SchoolDomain);
        var sindh = await AddStateAsync(country.Id, "Sindh");
        var punjab = await AddStateAsync(country.Id, "Punjab");
        var lahore = await AddCityAsync(punjab.Id, "Lahore");

        using var response = await session.PostAsync("/api/Branches", new { name = "Mismatch", details = new { stateId = sindh.PublicId, cityId = lahore.PublicId } });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseAPrincipalWhoIsNotAPrincipal()
    {
        var (session, _, _) = await SetUpSchoolAsync("Principal Type School", SchoolDomain);
        var teacher = await AddPersonAsync(session, "Bilal", null, "Ahmed", "Teacher");

        using var response = await session.PostAsync("/api/Branches", new { name = "Campus", details = new { principalId = teacher } });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseAPrincipalOfAnotherSchool()
    {
        var (session, _, _) = await SetUpSchoolAsync("Own Principal School", SchoolDomain);
        var other = await TestApp.AddSchoolAsync("Other Principal School", "other-principal.localhost");
        var foreignPrincipal = await AddPersonInSchoolAsync(other.Id, "Sara", null, "Ali", UserType.Principal);

        using var response = await session.PostAsync("/api/Branches", new { name = "Campus", details = new { principalId = foreignPrincipal } });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseAPhoneThatIsNotValid()
    {
        var (session, _, _) = await SetUpSchoolAsync("Phone School", SchoolDomain);

        using var response = await session.PostAsync("/api/Branches", new { name = "Campus", details = new { phone = "123" } });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static async Task<(HttpSession Session, int SchoolId, EntityIds Country)> SetUpSchoolAsync(string name, string domain)
    {
        await TestApp.RunAsAdministratorAsync();
        var country = await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");
        var school = await TestApp.AddSchoolAsync(name, domain, country.Id);
        var session = await SignInToSchoolAsync(school.Id, domain, $"admin@{school.Id}.campus.local");
        return (session, school.Id, country);
    }

    private static async Task<HttpSession> SignInToSchoolAsync(int schoolId, string domain, string email)
    {
        await TestApp.RunAsUserAsync(schoolId, email, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(domain);
        using var response = await session.SignInAsync(email, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return session;
    }

    private static async Task<Guid> CreateCampusAsync(HttpSession session, string name, object details)
    {
        using var response = await session.PostAsync("/api/Branches", new { name, details });
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
        using var response = await session.GetAsync("/api/Branches");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<CampusReadModel>>(Json))!;
    }

    // The API sends enums by name (ENG-ENUM-01).
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private sealed record CampusReadModel(Guid Id, string Name, CampusDetailsReadModel Details);

    private sealed record CampusDetailsReadModel(
        string? Address,
        Guid? StateId,
        string? StateName,
        Guid? CityId,
        string? CityName,
        string? PostalCode,
        string? Phone,
        string? Website,
        string? SchoolNumber,
        string? ShortName,
        Guid? PrincipalId,
        string? PrincipalName);
}
