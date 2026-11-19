using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.People;

/// <summary>
/// A school records its people (principals, teachers and other staff) with the Staff module. A school sees only its own
/// people, and a person may have a middle name.
/// </summary>
public class UserDetailsTests : TestBase
{
    private const string SchoolDomain = "people-one.localhost";
    private const string OtherSchoolDomain = "people-two.localhost";

    [Test]
    public async Task ShouldSaveAPersonWithAMiddleNameAndListItWithItsType()
    {
        await TestApp.RunAsAdministratorAsync();
        var session = await SetUpSchoolAsync("People One", SchoolDomain, "admin@people-one.local");

        using var create = await session.PostAsync("/api/UserDetails", new { firstName = "Ayesha", middleName = "Noor", lastName = "Khan", userType = "Principal", phone = "+92 300 1234567" });
        create.StatusCode.ShouldBe(HttpStatusCode.OK);

        var people = await ReadAsync(session, "/api/UserDetails");
        people.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            p => p.FirstName.ShouldBe("Ayesha"),
            p => p.MiddleName.ShouldBe("Noor"),
            p => p.LastName.ShouldBe("Khan"),
            p => p.UserType.ShouldBe(UserType.Principal),
            p => p.Phone.ShouldBe("+92 300 1234567"));
    }

    [Test]
    public async Task ShouldCreateAPersonWithALinkedAccountThatMustChangeItsPassword()
    {
        await TestApp.RunAsAdministratorAsync();
        var session = await SetUpSchoolAsync("People Account", SchoolDomain, "admin@people-account.local");

        using var create = await session.PostAsync("/api/UserDetails", new { firstName = "Bilal", lastName = "Ahmed", userType = "Teacher", email = "bilal@people-account.local", oneTimePassword = "violet harbour lantern 47" });
        create.StatusCode.ShouldBe(HttpStatusCode.OK);

        var schoolId = (await TestApp.FindSchoolIdAsync(SchoolDomain))!.Id;
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var account = (await users.FindByEmailAsync("bilal@people-account.local")).ShouldNotBeNull();
        account.SchoolId.ShouldBe(schoolId);
        account.MustChangePassword.ShouldBeTrue();
        account.UserDetailsId.ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldRefuseAWeakOneTimePasswordAndSaveNoPerson()
    {
        await TestApp.RunAsAdministratorAsync();
        var session = await SetUpSchoolAsync("People Weak", SchoolDomain, "admin@people-weak.local");

        using var create = await session.PostAsync("/api/UserDetails", new { firstName = "Sana", lastName = "Malik", userType = "Staff", email = "sana@people-weak.local", oneTimePassword = "short" });

        create.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadAsync(session, "/api/UserDetails")).ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldListOnlyThePeopleOfTheRequestedType()
    {
        await TestApp.RunAsAdministratorAsync();
        var session = await SetUpSchoolAsync("People Filter", SchoolDomain, "admin@people-filter.local");
        await session.PostAsync("/api/UserDetails", new { firstName = "Ayesha", lastName = "Khan", userType = "Principal" });
        await session.PostAsync("/api/UserDetails", new { firstName = "Bilal", lastName = "Ahmed", userType = "Teacher" });

        var principals = await ReadAsync(session, "/api/UserDetails?userType=Principal");

        principals.ShouldHaveSingleItem().FirstName.ShouldBe("Ayesha");
    }

    [Test]
    public async Task ShouldNotShowAnotherSchoolsPeople()
    {
        await TestApp.RunAsAdministratorAsync();
        var first = await SetUpSchoolAsync("People Isolated One", SchoolDomain, "admin@people-isolated-one.local");
        using var create = await first.PostAsync("/api/UserDetails", new { firstName = "Ayesha", lastName = "Khan", userType = "Staff" });
        create.StatusCode.ShouldBe(HttpStatusCode.OK);

        var second = await SetUpSchoolAsync("People Isolated Two", OtherSchoolDomain, "admin@people-isolated-two.local");

        (await ReadAsync(second, "/api/UserDetails")).ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldRefuseAPersonWithANameThatIsTooLong()
    {
        await TestApp.RunAsAdministratorAsync();
        var session = await SetUpSchoolAsync("People Long Name", SchoolDomain, "admin@people-long.local");

        using var response = await session.PostAsync("/api/UserDetails", new { firstName = "Ayesha", middleName = new string('m', 101), lastName = "Khan", userType = "Teacher" });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldChangeAPersonsNameTypeAndPhone()
    {
        await TestApp.RunAsAdministratorAsync();
        var session = await SetUpSchoolAsync("People Change", SchoolDomain, "admin@people-change.local");
        var id = await CreatePersonAsync(session, "Ayesha", "Khan", "Teacher");

        using var update = await session.PutAsync($"/api/UserDetails/{id}", new { firstName = "Ayesha", middleName = "Noor", lastName = "Khan", userType = "Principal", phone = "+92 300 7654321" });
        update.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var person = (await ReadAsync(session, "/api/UserDetails")).Single();
        person.ShouldSatisfyAllConditions(
            p => p.MiddleName.ShouldBe("Noor"),
            p => p.UserType.ShouldBe(UserType.Principal),
            p => p.Phone.ShouldBe("+92 300 7654321"));
    }

    [Test]
    public async Task ShouldNotChangeAnotherSchoolsPerson()
    {
        await TestApp.RunAsAdministratorAsync();
        var first = await SetUpSchoolAsync("People Change One", SchoolDomain, "admin@people-change-one.local");
        var id = await CreatePersonAsync(first, "Ayesha", "Khan", "Staff");
        var second = await SetUpSchoolAsync("People Change Two", OtherSchoolDomain, "admin@people-change-two.local");

        using var update = await second.PutAsync($"/api/UserDetails/{id}", new { firstName = "Hacked", lastName = "Khan", userType = "Principal" });

        update.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        // The first school's person is unchanged.
        (await ReadAsync(first, "/api/UserDetails")).ShouldHaveSingleItem().FirstName.ShouldBe("Ayesha");
    }

    [Test]
    public async Task ShouldDeleteAPersonAndHideIt()
    {
        await TestApp.RunAsAdministratorAsync();
        var session = await SetUpSchoolAsync("People Delete", SchoolDomain, "admin@people-delete.local");
        var id = await CreatePersonAsync(session, "Bilal", "Ahmed", "Teacher");

        using var delete = await session.DeleteAsync($"/api/UserDetails/{id}");

        delete.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadAsync(session, "/api/UserDetails")).ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldRefuseDeletingAPrincipalOfACampusUntilAnotherIsChosen()
    {
        await TestApp.RunAsAdministratorAsync();
        var country = await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");
        var school = await TestApp.AddSchoolAsync("People Principal", SchoolDomain, country.Id);
        var email = "admin@people-principal.local";
        await TestApp.RunAsUserAsync(school.Id, email, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(SchoolDomain);
        using (var signIn = await session.SignInAsync(email, TestApp.AdministratorPassword))
        {
            signIn.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        var principal = await CreatePersonAsync(session, "Ayesha", "Khan", "Principal");
        var (stateId, cityId) = await AddSindhKarachiAsync(country.Id);
        using var campus = await session.PostAsync("/api/Campuses", new
        {
            name = "Clifton",
            details = new
            {
                address = "12 Main Road",
                stateId,
                cityId,
                postalCode = "75600",
                phones = new[] { "+92 21 1234567" },
                schoolNumber = "SN-1",
                shortName = "CLF",
                principalId = principal,
                latitude = 24.8138,
                longitude = 67.0300,
            },
        });
        campus.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var delete = await session.DeleteAsync($"/api/UserDetails/{principal}");

        delete.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadAsync(session, "/api/UserDetails")).ShouldHaveSingleItem();
    }

    private static async Task<Guid> CreatePersonAsync(HttpSession session, string firstName, string lastName, string userType)
    {
        using var response = await session.PostAsync("/api/UserDetails", new { firstName, lastName, userType });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<Guid>(Json));
    }

    /// <summary>A state and a city of the given country, saved directly: the campus form takes them from the location data.</summary>
    private static async Task<(Guid StateId, Guid CityId)> AddSindhKarachiAsync(int countryId)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var state = State.CreateCustom(countryId, "Sindh", null, StateType.Province);
        context.States.Add(state);
        await context.SaveChangesAsync();

        var city = City.CreateCustom(state.Id, "Karachi", null, null, null);
        context.Cities.Add(city);
        await context.SaveChangesAsync();

        return (state.PublicId, city.PublicId);
    }

    // The master administrator is set up once per test, by the test itself, before its schools are added.
    private static async Task<HttpSession> SetUpSchoolAsync(string name, string domain, string email)
    {
        var school = await TestApp.AddSchoolAsync(name, domain);
        await TestApp.RunAsUserAsync(school.Id, email, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(domain);
        using var response = await session.SignInAsync(email, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return session;
    }

    private static async Task<List<PersonReadModel>> ReadAsync(HttpSession session, string path)
    {
        using var response = await session.GetAsync(path);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<PersonReadModel>>(Json))!;
    }

    // The API sends enums by name (ENG-ENUM-01).
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private sealed record PersonReadModel(Guid Id, string FirstName, string? MiddleName, string LastName, UserType UserType, string? Phone);
}
