using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;
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
