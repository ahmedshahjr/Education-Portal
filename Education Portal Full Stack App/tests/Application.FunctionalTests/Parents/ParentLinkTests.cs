using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Parents;

/// <summary>
/// A parent is linked to a student of the same school, and only people of the right type can be linked (SEC-AUTHZ-03,
/// SEC-AUTHZ-04). The school's staff link and unlink them with the Staff module.
/// </summary>
public class ParentLinkTests : TestBase
{
    private const string SchoolDomain = "parents-one.localhost";

    [Test]
    public async Task ShouldLinkAStudentToAParentAndListTheChild()
    {
        await TestApp.RunAsAdministratorAsync();
        var session = await SetUpSchoolAsync("Parents One", SchoolDomain, "admin@parents-one.local");
        var parent = await AddPersonAsync(session, "Hina", "Parent");
        var student = await AddPersonAsync(session, "Zain", "Student");

        using var link = await session.PostAsync($"/api/UserDetails/{parent}/children", new { studentId = student });

        link.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var children = await ReadChildrenAsync(session, parent);
        children.ShouldHaveSingleItem().FirstName.ShouldBe("Zain");
    }

    [Test]
    public async Task ShouldRefuseALinkWhoseParentIsNotAParent()
    {
        await TestApp.RunAsAdministratorAsync();
        var session = await SetUpSchoolAsync("Parents Type", SchoolDomain, "admin@parents-type.local");
        var teacher = await AddPersonAsync(session, "Bilal", "Teacher");
        var student = await AddPersonAsync(session, "Zain", "Student");

        using var link = await session.PostAsync($"/api/UserDetails/{teacher}/children", new { studentId = student });

        link.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseALinkWhoseChildIsNotAStudent()
    {
        await TestApp.RunAsAdministratorAsync();
        var session = await SetUpSchoolAsync("Parents Child", SchoolDomain, "admin@parents-child.local");
        var parent = await AddPersonAsync(session, "Hina", "Parent");
        var other = await AddPersonAsync(session, "Bilal", "Teacher");

        using var link = await session.PostAsync($"/api/UserDetails/{parent}/children", new { studentId = other });

        link.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseLinkingTheSameStudentTwice()
    {
        await TestApp.RunAsAdministratorAsync();
        var session = await SetUpSchoolAsync("Parents Twice", SchoolDomain, "admin@parents-twice.local");
        var parent = await AddPersonAsync(session, "Hina", "Parent");
        var student = await AddPersonAsync(session, "Zain", "Student");
        (await session.PostAsync($"/api/UserDetails/{parent}/children", new { studentId = student })).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var again = await session.PostAsync($"/api/UserDetails/{parent}/children", new { studentId = student });

        again.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldUnlinkAStudentFromAParent()
    {
        await TestApp.RunAsAdministratorAsync();
        var session = await SetUpSchoolAsync("Parents Unlink", SchoolDomain, "admin@parents-unlink.local");
        var parent = await AddPersonAsync(session, "Hina", "Parent");
        var student = await AddPersonAsync(session, "Zain", "Student");
        (await session.PostAsync($"/api/UserDetails/{parent}/children", new { studentId = student })).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var unlink = await session.DeleteAsync($"/api/UserDetails/{parent}/children/{student}");

        unlink.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadChildrenAsync(session, parent)).ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldRecordTheLinkInTheSchoolsChangeAudit()
    {
        await TestApp.RunAsAdministratorAsync();
        var session = await SetUpSchoolAsync("Parents Audit", SchoolDomain, "admin@parents-audit.local");
        var parent = await AddPersonAsync(session, "Hina", "Parent");
        var student = await AddPersonAsync(session, "Zain", "Student");
        (await session.PostAsync($"/api/UserDetails/{parent}/children", new { studentId = student })).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var response = await session.GetAsync("/api/ChangeAudit");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var entries = (await response.Content.ReadFromJsonAsync<List<AuditReadModel>>())!;
        entries.ShouldContain(e => e.EntityType == "ParentStudent" && e.Action == "Added");
    }

    /// <summary>Adds a person of the given type and returns their public id.</summary>
    private static async Task<Guid> AddPersonAsync(HttpSession session, string firstName, string userType)
    {
        using var create = await session.PostAsync("/api/UserDetails", new { firstName, lastName = "Test", userType });
        create.StatusCode.ShouldBe(HttpStatusCode.OK);
        var people = await ReadPeopleAsync(session);
        return people.Single(p => p.FirstName == firstName).Id;
    }

    private static async Task<List<PersonReadModel>> ReadChildrenAsync(HttpSession session, Guid parent)
    {
        using var response = await session.GetAsync($"/api/UserDetails/{parent}/children");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<PersonReadModel>>(Json))!;
    }

    private static async Task<List<PersonReadModel>> ReadPeopleAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/UserDetails");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<PersonReadModel>>(Json))!;
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

    // The API sends enums by name (ENG-ENUM-01).
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private sealed record PersonReadModel(Guid Id, string FirstName, string? MiddleName, string LastName, UserType UserType, string? Phone);

    private sealed record AuditReadModel(string EntityType, string Action);
}
