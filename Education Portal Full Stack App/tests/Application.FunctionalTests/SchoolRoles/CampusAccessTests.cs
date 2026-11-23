using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.SchoolRoles;

/// <summary>
/// A person who is not an administrator acts only on the campuses they are assigned to, with the rights their roles give
/// (AGENTS.md section 2: modules ∩ role rights ∩ campuses). Only the school's administrator assigns campuses, and only
/// the school's own campuses and people (SEC-AUTHZ-03, SEC-AUTHZ-04).
/// </summary>
public class CampusAccessTests : TestBase
{
    private const string SchoolDomain = "campus-access.localhost";
    private const string OtherSchoolDomain = "campus-access-other.localhost";
    private const string OneTimePassword = "violet harbour lantern 47";
    private const string NewPassword = "a brand new passphrase 2026";
    private const string TeacherEmail = "bilal@campus-access.local";

    [Test]
    public async Task ShouldLetAPersonActOnlyOnTheirOwnCampuses()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Campus Access", SchoolDomain);
        using var admin = await SignInAdministratorAsync(school.Id, SchoolDomain, "admin@campus-access.local");
        var main = await MainCampusAsync(admin);
        var north = await AddCampusAsync(school.Id, "North", "0002");
        var person = await AddPersonWithAccountAsync(admin);
        await GiveAcademicSetupRoleAsync(admin, person);
        (await admin.PutAsync($"/api/UserDetails/{person}/campuses", new { campusIds = new[] { main } })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await CreateShiftAsync(admin, north, "North morning");

        using var teacher = await SignInPersonAsync();

        // Their own campus works; the other campus is refused, and its shifts are not listed.
        await CreateShiftAsync(teacher, main, "Main morning");
        (await teacher.PostAsync("/api/Shifts", Shift(north, "Sneaky"))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await teacher.GetAsync($"/api/Shifts?campusId={north}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        var listed = await ReadJsonAsync(teacher, "/api/Shifts");
        listed.EnumerateArray().Select(s => s.GetProperty("name").GetString()).ShouldBe(["Main morning"]);

        // A whole-school holiday is saved on every campus (one row each), so this person, who has no right on North, is refused.
        (await teacher.PostAsync("/api/SchoolCalendar/holidays", Holiday("Eid", null))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await teacher.PostAsync("/api/SchoolCalendar/holidays", Holiday("Main repairs", main))).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await teacher.PostAsync("/api/SchoolCalendar/holidays", Holiday("North repairs", north))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        // The administrator still sees every campus.
        (await ReadJsonAsync(admin, "/api/Shifts")).GetArrayLength().ShouldBe(2);
    }

    [Test]
    public async Task ShouldRefuseAPersonWithoutACampusOrWithoutTheRight()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Campus Access", SchoolDomain);
        using var admin = await SignInAdministratorAsync(school.Id, SchoolDomain, "admin@campus-access.local");
        var main = await MainCampusAsync(admin);
        var person = await AddPersonWithAccountAsync(admin);

        using (var teacher = await SignInPersonAsync())
        {
            // No role: no right in the module.
            (await teacher.GetAsync("/api/Shifts")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        }

        await GiveAcademicSetupRoleAsync(admin, person);
        using var withRole = await SignInPersonAsync(NewPassword);

        // A role but no campus: the list is empty and no campus can be changed.
        (await ReadJsonAsync(withRole, "/api/Shifts")).GetArrayLength().ShouldBe(0);
        (await withRole.PostAsync("/api/Shifts", Shift(main, "Morning"))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldSetExactlyTheGivenCampusesOfTheSchoolOnly()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Campus Access", SchoolDomain);
        var other = await TestApp.AddSchoolAsync("Other School", OtherSchoolDomain);
        using var admin = await SignInAdministratorAsync(school.Id, SchoolDomain, "admin@campus-access.local");
        var main = await MainCampusAsync(admin);
        var north = await AddCampusAsync(school.Id, "North", "0002");
        var otherCampus = await AddCampusAsync(other.Id, "Elsewhere", "0002");
        var person = await AddPersonWithAccountAsync(admin);

        (await admin.PutAsync($"/api/UserDetails/{person}/campuses", new { campusIds = new[] { main, north } })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await admin.PutAsync($"/api/UserDetails/{person}/campuses", new { campusIds = new[] { north } })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadGuidsAsync(admin, $"/api/UserDetails/{person}/campuses")).ShouldBe([north]);

        // Another school's campus, or a campus listed twice, is refused and changes nothing.
        (await admin.PutAsync($"/api/UserDetails/{person}/campuses", new { campusIds = new[] { otherCampus } })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await admin.PutAsync($"/api/UserDetails/{person}/campuses", new { campusIds = new[] { main, main } })).StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadGuidsAsync(admin, $"/api/UserDetails/{person}/campuses")).ShouldBe([north]);

        // Another school's administrator cannot read or change this person's campuses.
        using var otherAdmin = await SignInAdministratorAsync(other.Id, OtherSchoolDomain, "admin@campus-access-other.local");
        (await otherAdmin.GetAsync($"/api/UserDetails/{person}/campuses")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await otherAdmin.PutAsync($"/api/UserDetails/{person}/campuses", new { campusIds = Array.Empty<Guid>() })).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadGuidsAsync(admin, $"/api/UserDetails/{person}/campuses")).ShouldBe([north]);
    }

    [Test]
    public async Task ShouldLetOnlyAnAdministratorAssignCampuses()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Campus Access", SchoolDomain);
        using var admin = await SignInAdministratorAsync(school.Id, SchoolDomain, "admin@campus-access.local");
        var main = await MainCampusAsync(admin);
        var person = await AddPersonWithAccountAsync(admin);
        await GiveAcademicSetupRoleAsync(admin, person);

        using var teacher = await SignInPersonAsync();

        (await teacher.PutAsync($"/api/UserDetails/{person}/campuses", new { campusIds = new[] { main } })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await teacher.GetAsync($"/api/UserDetails/{person}/campuses")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private static object Shift(Guid campusId, string name) => new { campusId, name, startTime = "07:30:00", endTime = "13:00:00" };

    private static object Holiday(string title, Guid? campusId) => new
    {
        title,
        campusId,
        startDate = "2026-09-10",
        endDate = "2026-09-10",
        type = "Public",
        appliesTo = "Both",
        isTentative = false,
    };

    private static async Task CreateShiftAsync(HttpSession session, Guid campusId, string name)
    {
        using var response = await session.PostAsync("/api/Shifts", Shift(campusId, name));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    /// <summary>A role with every right in Academic setup, given to the person's account.</summary>
    private static async Task GiveAcademicSetupRoleAsync(HttpSession admin, Guid person)
    {
        using var create = await admin.PostAsync("/api/SchoolRoles", new
        {
            name = "Calendar keeper",
            permissions = new[]
            {
                new { module = ModuleCode.AcademicSetup.ToString(), canView = true, canCreate = true, canEdit = true, canDelete = true, canApprove = false, canExport = false },
            },
        });
        create.StatusCode.ShouldBe(HttpStatusCode.OK);
        var role = await create.Content.ReadFromJsonAsync<Guid>();

        (await admin.PutAsync($"/api/UserDetails/{person}/roles", new { roleIds = new[] { role } })).StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    private static async Task<Guid> AddCampusAsync(int schoolId, string name, string code)
    {
        var campus = Campus.Create(schoolId, name, code);
        await TestApp.AddAsync(campus);
        return campus.PublicId;
    }

    private static async Task<Guid> AddPersonWithAccountAsync(HttpSession admin)
    {
        using var create = await admin.PostAsync("/api/UserDetails", new { firstName = "Bilal", lastName = "Ahmed", userType = "Teacher", email = TeacherEmail, oneTimePassword = OneTimePassword });
        create.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await create.Content.ReadFromJsonAsync<Guid>();
    }

    /// <summary>Signs the person in; on the first sign-in the one-time password is replaced, as the app requires.</summary>
    private static async Task<HttpSession> SignInPersonAsync(string password = OneTimePassword)
    {
        var session = TestHttp.CreateDevelopmentSession(SchoolDomain);
        using var signIn = await session.SignInAsync(TeacherEmail, password);
        signIn.StatusCode.ShouldBe(HttpStatusCode.OK);

        if (password == OneTimePassword)
        {
            using var changed = await session.PostAsync("/api/Users/me/password", new { currentPassword = OneTimePassword, newPassword = NewPassword });
            changed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        }

        return session;
    }

    private static async Task<HttpSession> SignInAdministratorAsync(int schoolId, string domain, string email)
    {
        await TestApp.RunAsUserAsync(schoolId, email, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(domain);
        using var response = await session.SignInAsync(email, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return session;
    }

    private static async Task<Guid> MainCampusAsync(HttpSession admin) =>
        (await ReadJsonAsync(admin, "/api/Campuses")).EnumerateArray()
            .Single(c => c.GetProperty("kind").GetString() == nameof(CampusKind.Main))
            .GetProperty("id").GetGuid();

    private static async Task<JsonElement> ReadJsonAsync(HttpSession session, string path)
    {
        using var response = await session.GetAsync(path);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, path);
        return await response.Content.ReadFromJsonAsync<JsonElement>();
    }

    private static async Task<List<Guid>> ReadGuidsAsync(HttpSession session, string path)
    {
        using var response = await session.GetAsync(path);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, path);
        return (await response.Content.ReadFromJsonAsync<List<Guid>>())!;
    }
}
