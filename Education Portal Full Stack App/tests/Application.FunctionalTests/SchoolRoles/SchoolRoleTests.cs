using System.Text.Json;
using System.Text.Json.Serialization;
using System.Net;
using System.Net.Http.Json;
using EducationPortal.Application.SchoolRoles.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Application.FunctionalTests.SchoolRoles;

/// <summary>
/// A school's roles and their rights per module: only the school's administrators, only their own school's roles, and
/// rights only in modules the school has now (SEC-AUTHZ-01, -03, -06, -09).
/// </summary>
public class SchoolRoleTests : TestBase
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private const string SchoolDomain = "school1.localhost";
    private const string OtherSchoolDomain = "school2.localhost";

    [Test]
    public async Task ShouldAddListChangeAndDeleteARole()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var session = await SignInAsync(school.Id, SchoolDomain, "admin@school1.local");

        var id = await CreateAsync(session, "Registrar", [new(ModuleCode.Students, true, true, true, false, false, false)]);

        var roles = await ListAsync(session);
        roles.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            r => r.Id.ShouldBe(id),
            r => r.Name.ShouldBe("Registrar"),
            r => r.Permissions.ShouldBe([new RolePermissionDto(ModuleCode.Students, true, true, true, false, false, false)]));

        using var update = await session.PutAsync($"/api/SchoolRoles/{id}", Body("Admissions officer", [new(ModuleCode.Students, true, false, false, false, false, false), new(ModuleCode.Staff, true, false, false, false, false, false)]));
        update.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var changed = (await ListAsync(session)).Single();
        changed.Name.ShouldBe("Admissions officer");
        changed.Permissions.Select(p => p.Module).ShouldBe([ModuleCode.Staff, ModuleCode.Students], ignoreOrder: true);

        using var delete = await session.DeleteAsync($"/api/SchoolRoles/{id}");
        delete.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ListAsync(session)).ShouldBeEmpty();
        (await TestApp.CountAsync<RolePermission>()).ShouldBe(0);
    }

    [Test]
    public async Task ShouldSaveTheDescriptionAndTheApproveAndExportRights()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var session = await SignInAsync(school.Id, SchoolDomain, "admin@school1.local");

        using var create = await session.PostAsync("/api/SchoolRoles", Body("Registrar", [new(ModuleCode.Students, true, false, false, false, true, true)], " Keeps the records "));
        create.StatusCode.ShouldBe(HttpStatusCode.OK);

        var role = (await ListAsync(session)).Single();
        role.ShouldSatisfyAllConditions(
            r => r.Description.ShouldBe("Keeps the records"),
            r => r.IsSystem.ShouldBeFalse(),
            r => r.Permissions.ShouldBe([new RolePermissionDto(ModuleCode.Students, true, false, false, false, true, true)]));

        using var exportWithoutView = await session.PostAsync("/api/SchoolRoles", Body("Clerk", [new(ModuleCode.Students, false, false, false, false, false, true)]));
        exportWithoutView.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldKeepTheNameOfABuiltInRoleAndRefuseToDeleteIt()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var session = await SignInAsync(school.Id, SchoolDomain, "admin@school1.local");
        (await session.PostAsync("/api/SchoolRoles/built-in", new { })).Dispose();
        var teacher = (await ListAsync(session)).Single(r => r.Name == "Teacher");
        teacher.IsSystem.ShouldBeTrue();

        using var rename = await session.PutAsync($"/api/SchoolRoles/{teacher.Id}", Body("Tutor", []));
        using var describe = await session.PutAsync($"/api/SchoolRoles/{teacher.Id}", Body("Teacher", [], "Teaches classes"));
        using var delete = await session.DeleteAsync($"/api/SchoolRoles/{teacher.Id}");

        rename.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        describe.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        delete.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var kept = (await ListAsync(session)).Single(r => r.Id == teacher.Id);
        kept.Name.ShouldBe("Teacher");
        kept.Description.ShouldBe("Teaches classes");
    }

    [Test]
    public async Task ShouldRefuseADuplicateNameInTheSameSchoolButAllowItInAnother()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var other = await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        using var session = await SignInAsync(school.Id, SchoolDomain, "admin@school1.local");
        using var otherSession = await SignInAsync(other.Id, OtherSchoolDomain, "admin@school2.local");

        await CreateAsync(session, "Registrar", []);
        using var duplicate = await session.PostAsync("/api/SchoolRoles", Body("Registrar", []));
        duplicate.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        await CreateAsync(otherSession, "Registrar", []);
    }

    [Test]
    public async Task ShouldRefuseRightsWithoutViewAndAModuleListedTwice()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var session = await SignInAsync(school.Id, SchoolDomain, "admin@school1.local");

        using var noView = await session.PostAsync("/api/SchoolRoles", Body("A", [new(ModuleCode.Students, false, true, false, false, false, false)]));
        using var twice = await session.PostAsync("/api/SchoolRoles", Body("B", [new(ModuleCode.Students, true, false, false, false, false, false), new(ModuleCode.Students, true, true, false, false, false, false)]));

        noView.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        twice.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await TestApp.CountAsync<SchoolRole>()).ShouldBe(0);
    }

    [Test]
    public async Task ShouldRefuseRightsInAModuleTheSchoolDoesNotHave()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var session = await SignInAsync(school.Id, SchoolDomain, "admin@school1.local");

        using var unsold = await session.PostAsync("/api/SchoolRoles", Body("Cashier", [new(ModuleCode.Fees, true, true, false, false, false, false)]));
        using var unknown = await session.PostAsync("/api/SchoolRoles", Body("Ghost", [new((ModuleCode)99, true, false, false, false, false, false)]));

        unsold.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        unknown.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldHideRightsInAModuleWhileItIsOffAndKeepThemForWhenItComesBack()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var subscription = await TestApp.SubscribeAsync(school.Id, [ModuleCode.Fees], start: DateTimeOffset.UtcNow.AddDays(-1));
        using var session = await SignInAsync(school.Id, SchoolDomain, "admin@school1.local");
        var id = await CreateAsync(session, "Cashier", [new(ModuleCode.Fees, true, true, false, false, false, false), new(ModuleCode.Students, true, false, false, false, false, false)]);

        await ChangeGrantsAsync(subscription.Id, school.Id, grant => grant.Suspend());
        var hidden = (await ListAsync(session)).Single();
        hidden.Permissions.Select(p => p.Module).ShouldBe([ModuleCode.Students]);

        // Saving while Fees is off must not lose the Fees rights.
        using var update = await session.PutAsync($"/api/SchoolRoles/{id}", Body("Cashier", [new(ModuleCode.Students, true, true, false, false, false, false)]));
        update.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        await ChangeGrantsAsync(subscription.Id, school.Id, grant => grant.Reactivate());
        (await ListAsync(session)).Single().Permissions.ShouldBe(
        [
            new RolePermissionDto(ModuleCode.Students, true, true, false, false, false, false),
            new RolePermissionDto(ModuleCode.Fees, true, true, false, false, false, false),
        ], ignoreOrder: true);
    }

    [Test]
    public async Task ShouldNotShowOrChangeAnotherSchoolsRole()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var other = await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        using var session = await SignInAsync(school.Id, SchoolDomain, "admin@school1.local");
        using var otherSession = await SignInAsync(other.Id, OtherSchoolDomain, "admin@school2.local");
        var id = await CreateAsync(session, "Registrar", []);

        (await ListAsync(otherSession)).ShouldBeEmpty();
        using var update = await otherSession.PutAsync($"/api/SchoolRoles/{id}", Body("Hijacked", []));
        using var delete = await otherSession.DeleteAsync($"/api/SchoolRoles/{id}");

        update.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        delete.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ListAsync(session)).Single().Name.ShouldBe("Registrar");
    }

    [Test]
    public async Task ShouldRequireSignInAndAnAdministrator()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var anonymous = TestHttp.CreateDevelopmentSession(SchoolDomain);
        using var anonymousResponse = await anonymous.GetAsync("/api/SchoolRoles");
        anonymousResponse.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        await TestApp.RunAsUserAsync(school.Id, "teacher@school1.local", TestApp.DefaultUserPassword, []);
        using var teacher = TestHttp.CreateDevelopmentSession(SchoolDomain);
        (await teacher.SignInAsync("teacher@school1.local", TestApp.DefaultUserPassword)).StatusCode.ShouldBe(HttpStatusCode.OK);
        using var forbidden = await teacher.GetAsync("/api/SchoolRoles");
        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldAuditRoleChanges()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var session = await SignInAsync(school.Id, SchoolDomain, "admin@school1.local");
        var userId = TestApp.GetUserId()!;

        var id = await CreateAsync(session, "Registrar", []);
        (await session.PutAsync($"/api/SchoolRoles/{id}", Body("Registrar 2", []))).Dispose();
        (await session.DeleteAsync($"/api/SchoolRoles/{id}")).Dispose();

        var events = (await TestApp.FindAuditEventsAsync(userId)).Select(e => e.EventType).ToList();
        events.ShouldContain(SecurityAuditEventType.RoleCreated);
        events.ShouldContain(SecurityAuditEventType.RoleChanged);
        events.ShouldContain(SecurityAuditEventType.RoleDeleted);
    }

    private static object Body(string name, List<RolePermissionDto> permissions, string? description = null) =>
        new { name, description, permissions = permissions.Select(p => new { module = p.Module, p.CanView, p.CanCreate, p.CanEdit, p.CanDelete, p.CanApprove, p.CanExport }) };

    private static async Task<Guid> CreateAsync(HttpSession session, string name, List<RolePermissionDto> permissions)
    {
        using var response = await session.PostAsync("/api/SchoolRoles", Body(name, permissions));
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<Guid>(Json);
    }

    private static async Task<List<SchoolRoleDto>> ListAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/SchoolRoles");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<SchoolRoleDto>>(Json))!;
    }

    private static async Task<HttpSession> SignInAsync(int schoolId, string domain, string email)
    {
        await TestApp.RunAsUserAsync(schoolId, email, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(domain);
        using var response = await session.SignInAsync(email, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return session;
    }

    private static async Task ChangeGrantsAsync(int subscriptionId, int schoolId, Action<TenantModule> change)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<EducationPortal.Web.Services.CurrentTenant>().Set(new ResolvedTenant(TestApp.MasterSchoolId, IsMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<EducationPortal.Infrastructure.Data.ApplicationDbContext>();
        foreach (var grant in await context.TenantModules.Where(t => t.SubscriptionId == subscriptionId).ToListAsync())
        {
            change(grant);
        }

        await context.SaveChangesAsync();
        scope.ServiceProvider.GetRequiredService<IModuleEntitlementStore>().Invalidate(schoolId);
    }
}
