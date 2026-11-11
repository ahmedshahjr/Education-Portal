using System.Net;
using System.Net.Http.Json;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.FunctionalTests.Modules;

/// <summary>
/// Modules per school (AGENTS.md section 2): the master tenant decides which modules are default and switches modules on
/// or off per school; a school's users get only the modules switched on for their school. Master settings are for master
/// tenant administrators only, audited, and one school's switches never reach another (SEC-AUTHZ-01, -06, -07, -09, SEC-LOG-03).
/// </summary>
public class ModuleTests : TestBase
{
    private const string SchoolDomain = "school1.localhost";
    private const string OtherSchoolDomain = "school2.localhost";
    private const string SchoolAdminEmail = "admin@school1.local";

    [Test]
    public async Task ShouldGiveANewSchoolTheCoreAndDefaultModules()
    {
        await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var school = await SignInToSchoolAsync();

        var enabled = await ReadEnabledAsync(school);

        enabled.ShouldBe(
            ModuleCatalog.CreateModules().Where(m => m.IsDefault).Select(m => m.Key).ToList(),
            ignoreOrder: true);
        enabled.ShouldContain(ModuleKeys.Branches);
        enabled.ShouldNotContain(ModuleKeys.Parents);
    }

    [Test]
    public async Task ShouldLetTheMasterSwitchModulesOnAndOffForOneSchoolOnly()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var custom = await master.PutAsync($"/api/Schools/{schoolId}/modules/{ModuleKeys.Parents}", new { isEnabled = true });
        using var @default = await master.PutAsync($"/api/Schools/{schoolId}/modules/{ModuleKeys.Grades}", new { isEnabled = false });

        custom.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        @default.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var modules = await ReadSchoolModulesAsync(master, schoolId);
        modules.Single(m => m.Key == ModuleKeys.Parents).IsEnabled.ShouldBeTrue();
        modules.Single(m => m.Key == ModuleKeys.Grades).ShouldSatisfyAllConditions(
            m => m.IsEnabled.ShouldBeFalse(),
            m => m.FollowsDefault.ShouldBeFalse());
        modules.Single(m => m.Key == ModuleKeys.Students).FollowsDefault.ShouldBeTrue();

        await TestApp.RunAsUserAsync(schoolId, SchoolAdminEmail, TestApp.AdministratorPassword, [Roles.Administrator]);
        using var school = TestHttp.CreateDevelopmentSession(SchoolDomain);
        (await school.SignInAsync(SchoolAdminEmail, TestApp.AdministratorPassword)).Dispose();
        var enabled = await ReadEnabledAsync(school);
        enabled.ShouldContain(ModuleKeys.Parents);
        enabled.ShouldNotContain(ModuleKeys.Grades);

        // The other school keeps the defaults.
        await TestApp.RunAsUserAsync((await TestApp.FindSchoolIdAsync(OtherSchoolDomain))!.Value, "admin@school2.local", TestApp.AdministratorPassword, [Roles.Administrator]);
        using var other = TestHttp.CreateDevelopmentSession(OtherSchoolDomain);
        (await other.SignInAsync("admin@school2.local", TestApp.AdministratorPassword)).Dispose();
        var otherEnabled = await ReadEnabledAsync(other);
        otherEnabled.ShouldContain(ModuleKeys.Grades);
        otherEnabled.ShouldNotContain(ModuleKeys.Parents);

        var audit = await TestApp.FindAuditEventsAsync(masterUserId);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.SchoolModuleChanged && e.TargetId == $"{schoolId}/{ModuleKeys.Parents}");
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.SchoolModuleChanged && e.TargetId == $"{schoolId}/{ModuleKeys.Grades}");
    }

    [Test]
    public async Task ShouldLetTheMasterMakeAModuleDefaultForEverySchoolWithoutItsOwnChoice()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var otherId = await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        (await master.PutAsync($"/api/Schools/{otherId}/modules/{ModuleKeys.Dashboard}", new { isEnabled = false })).Dispose();

        using var response = await master.PutAsync($"/api/Modules/{ModuleKeys.Dashboard}/default", new { isDefault = true });

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var catalog = await ReadModulesAsync(master);
        catalog.Single(m => m.Key == ModuleKeys.Dashboard).IsDefault.ShouldBeTrue();
        (await ReadSchoolModulesAsync(master, schoolId)).Single(m => m.Key == ModuleKeys.Dashboard).IsEnabled.ShouldBeTrue();
        (await ReadSchoolModulesAsync(master, otherId)).Single(m => m.Key == ModuleKeys.Dashboard).IsEnabled.ShouldBeFalse();

        var audit = await TestApp.FindAuditEventsAsync(masterUserId);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.ModuleDefaultChanged && e.TargetId == ModuleKeys.Dashboard);
    }

    [Test]
    public async Task ShouldKeepCoreModulesOn()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var perSchool = await master.PutAsync($"/api/Schools/{schoolId}/modules/{ModuleKeys.Branches}", new { isEnabled = false });
        using var custom = await master.PutAsync($"/api/Modules/{ModuleKeys.UsersAndRoles}/default", new { isDefault = false });

        perSchool.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        custom.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await TestApp.CountAsync<SchoolModule>()).ShouldBe(0);
    }

    [Test]
    public async Task ShouldRefuseToLimitTheMasterTenant()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var response = await master.PutAsync($"/api/Schools/{TestApp.MasterSchoolId}/modules/{ModuleKeys.Grades}", new { isEnabled = false });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadEnabledAsync(master)).Count.ShouldBe(ModuleCatalog.CreateModules().Count);
    }

    [Test]
    public async Task ShouldAnswer404ForAnUnknownSchoolOrModule()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var school = await master.PutAsync($"/api/Schools/999999/modules/{ModuleKeys.Grades}", new { isEnabled = true });
        using var schoolModules = await master.GetAsync("/api/Schools/999999/modules");
        using var module = await master.PutAsync($"/api/Schools/{schoolId}/modules/Fees", new { isEnabled = true });
        using var moduleDefault = await master.PutAsync("/api/Modules/Fees/default", new { isDefault = true });

        school.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        schoolModules.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        module.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        moduleDefault.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ShouldKeepModuleSettingsAwayFromSchoolAdministrators()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var session = await SignInToSchoolAsync();

        using var catalog = await session.GetAsync("/api/Modules");
        using var setDefault = await session.PutAsync($"/api/Modules/{ModuleKeys.Parents}/default", new { isDefault = true });
        using var list = await session.GetAsync($"/api/Schools/{schoolId}/modules");
        using var own = await session.PutAsync($"/api/Schools/{schoolId}/modules/{ModuleKeys.Parents}", new { isEnabled = true });

        catalog.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        setDefault.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        list.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        own.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await TestApp.CountAsync<SchoolModule>()).ShouldBe(0);
        (await ReadEnabledAsync(session)).ShouldNotContain(ModuleKeys.Parents);
    }

    [Test]
    public async Task ShouldRequireTheAdministratorRoleOnTheMasterTenant()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsDefaultUserAsync();
        using var session = TestHttp.CreateDevelopmentSession();
        (await session.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword)).Dispose();

        using var catalog = await session.GetAsync("/api/Modules");
        using var own = await session.PutAsync($"/api/Schools/{schoolId}/modules/{ModuleKeys.Parents}", new { isEnabled = true });

        catalog.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        own.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldRequireSignIn()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var session = TestHttp.CreateDevelopmentSession(SchoolDomain);

        using var enabled = await session.GetAsync("/api/Modules/enabled");
        using var catalog = await session.GetAsync("/api/Modules");
        using var set = await session.PutAsync($"/api/Schools/{schoolId}/modules/{ModuleKeys.Parents}", new { isEnabled = true });

        enabled.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        catalog.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        set.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static async Task<HttpSession> SignInToSchoolAsync()
    {
        var schoolId = (await TestApp.FindSchoolIdAsync(SchoolDomain))!.Value;
        await TestApp.RunAsUserAsync(schoolId, SchoolAdminEmail, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(SchoolDomain);
        using var response = await session.SignInAsync(SchoolAdminEmail, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return session;
    }

    private static async Task<HttpSession> SignInAsMasterAsync()
    {
        var master = TestHttp.CreateDevelopmentSession();
        using var response = await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return master;
    }

    private static async Task<List<string>> ReadEnabledAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/Modules/enabled");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<string>>())!;
    }

    private static async Task<List<ModuleDto>> ReadModulesAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/Modules");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<ModuleDto>>())!;
    }

    private static async Task<List<SchoolModuleDto>> ReadSchoolModulesAsync(HttpSession session, int schoolId)
    {
        using var response = await session.GetAsync($"/api/Schools/{schoolId}/modules");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<SchoolModuleDto>>())!;
    }
}
