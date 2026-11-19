using System.Net;
using System.Net.Http.Json;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Application.SchoolRoles.Common;
using EducationPortal.Domain.Constants;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.SchoolRoles;

/// <summary>
/// The four built-in roles are added once per school, and a person's login account holds exactly the roles it is given
/// (SEC-AUTHZ-03, SEC-AUTHZ-04). Only people who have an account can be given roles.
/// </summary>
public class AccountRoleTests : TestBase
{
    private const string SchoolDomain = "account-roles.localhost";
    private const string OneTimePassword = "violet harbour lantern 47";

    [Test]
    public async Task ShouldAddTheFourBuiltInRolesOnceAndNeverTwice()
    {
        await TestApp.RunAsAdministratorAsync();
        var session = await SetUpSchoolAsync("Built In", SchoolDomain, "admin@built-in.local");

        (await session.PostAsync("/api/SchoolRoles/built-in", new { })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await session.PostAsync("/api/SchoolRoles/built-in", new { })).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var names = (await ReadRolesAsync(session)).Select(r => r.Name).ToList();
        names.OrderBy(n => n).ShouldBe(new[] { "Parent", "Principal", "Student", "Teacher" });
    }

    [Test]
    public async Task ShouldGiveAPersonsAccountExactlyTheRolesItIsGiven()
    {
        await TestApp.RunAsAdministratorAsync();
        var session = await SetUpSchoolAsync("Give Roles", SchoolDomain, "admin@give-roles.local");
        (await session.PostAsync("/api/SchoolRoles/built-in", new { })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var person = await AddPersonWithAccountAsync(session, "bilal@give-roles.local");
        var teacher = (await ReadRolesAsync(session)).Single(r => r.Name == BuiltInRoles.Teacher).Id;
        var student = (await ReadRolesAsync(session)).Single(r => r.Name == BuiltInRoles.Student).Id;

        (await session.PutAsync($"/api/UserDetails/{person}/roles", new { roleIds = new[] { teacher, student } })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await session.PutAsync($"/api/UserDetails/{person}/roles", new { roleIds = new[] { teacher } })).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var held = await ReadAccountRolesAsync(session, person);
        held.ShouldBe(new[] { teacher });
    }

    [Test]
    public async Task ShouldRefuseRolesForAPersonWithoutAnAccount()
    {
        await TestApp.RunAsAdministratorAsync();
        var session = await SetUpSchoolAsync("No Account", SchoolDomain, "admin@no-account.local");
        (await session.PostAsync("/api/SchoolRoles/built-in", new { })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        using var create = await session.PostAsync("/api/UserDetails", new { firstName = "Sana", lastName = "Malik", userType = "Staff" });
        create.StatusCode.ShouldBe(HttpStatusCode.OK);
        var person = (await ReadPeopleAsync(session)).Single(p => p.FirstName == "Sana").Id;
        var teacher = (await ReadRolesAsync(session)).Single(r => r.Name == BuiltInRoles.Teacher).Id;

        using var response = await session.PutAsync($"/api/UserDetails/{person}/roles", new { roleIds = new[] { teacher } });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static async Task<Guid> AddPersonWithAccountAsync(HttpSession session, string email)
    {
        using var create = await session.PostAsync("/api/UserDetails", new { firstName = "Bilal", lastName = "Ahmed", userType = "Teacher", email, oneTimePassword = OneTimePassword });
        create.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await ReadPeopleAsync(session)).Single(p => p.FirstName == "Bilal").Id;
    }

    private static async Task<List<RoleReadModel>> ReadRolesAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/SchoolRoles");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<RoleReadModel>>())!;
    }

    private static async Task<List<PersonReadModel>> ReadPeopleAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/UserDetails");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<PersonReadModel>>())!;
    }

    private static async Task<IReadOnlyList<Guid>> ReadAccountRolesAsync(HttpSession session, Guid person)
    {
        using var response = await session.GetAsync($"/api/UserDetails/{person}/roles");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<Guid>>())!;
    }

    private static async Task<HttpSession> SetUpSchoolAsync(string name, string domain, string email)
    {
        var school = await TestApp.AddSchoolAsync(name, domain);
        await TestApp.RunAsUserAsync(school.Id, email, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(domain);
        using var response = await session.SignInAsync(email, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return session;
    }

    private sealed record RoleReadModel(Guid Id, string Name);

    private sealed record PersonReadModel(Guid Id, string FirstName);
}
