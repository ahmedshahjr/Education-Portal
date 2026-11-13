using System.Net;
using System.Net.Http.Json;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.FunctionalTests.Modules;

/// <summary>
/// The module catalogue (AGENTS.md section 2): the master tenant manages what the platform sells, core modules are on for
/// every school, and paid modules are on only for the schools the master gives them to. Every change is for master
/// tenant administrators only, audited, and one school's modules never reach another (SEC-AUTHZ-01, -06, -07, -09,
/// SEC-LOG-03).
/// </summary>
public class ModuleTests : TestBase
{
    private const string SchoolDomain = "school1.localhost";
    private const string OtherSchoolDomain = "school2.localhost";
    private const string SchoolAdminEmail = "admin@school1.local";

    [Test]
    public async Task ShouldGiveANewSchoolTheCoreModulesOnly()
    {
        await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var school = await SignInToSchoolAsync();

        var enabled = await ReadEnabledAsync(school);

        enabled.ShouldBe(ModuleCatalog.CreateModules().Where(m => m.IsCore).OrderBy(m => m.SortOrder).Select(m => m.Code).ToList());
        enabled.ShouldNotContain(ModuleCodes.Fees);
    }

    [Test]
    public async Task ShouldListTheCatalogueInSortOrderWithDependencies()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        var modules = await ReadModulesAsync(master);

        modules.Select(m => m.Code).ShouldBe(ModuleCatalog.CreateModules().OrderBy(m => m.SortOrder).Select(m => m.Code).ToList());
        modules.Single(m => m.Code == ModuleCodes.Fees).ShouldSatisfyAllConditions(
            m => m.IsCore.ShouldBeFalse(),
            m => m.Requires.ShouldBe([ModuleCodes.Students]));
        modules.Single(m => m.Code == ModuleCodes.Timetable).Requires.ShouldBe([ModuleCodes.AcademicSetup, ModuleCodes.Staff]);
        (await ReadModuleAsync(master, ModuleCodes.Students)).IsCore.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldGiveAPaidModuleOnlyToTheSchoolWhoseSubscriptionHasIt()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        await TestApp.SubscribeAsync(schoolId.Id, [ModuleCodes.Fees]);

        var modules = await ReadSchoolModulesAsync(master, schoolId.PublicId);
        modules.Single(m => m.Code == ModuleCodes.Fees).IsEnabled.ShouldBeTrue();
        modules.Single(m => m.Code == ModuleCodes.Exams).IsEnabled.ShouldBeFalse();
        modules.Single(m => m.Code == ModuleCodes.Students).ShouldSatisfyAllConditions(m => m.IsCore.ShouldBeTrue(), m => m.IsEnabled.ShouldBeTrue());

        using var school = await SignInToSchoolAsync();
        (await ReadEnabledAsync(school)).ShouldContain(ModuleCodes.Fees);

        // The other school does not get it.
        await TestApp.RunAsUserAsync((await TestApp.FindSchoolIdAsync(OtherSchoolDomain))!.Id, "admin@school2.local", TestApp.AdministratorPassword, [Roles.Administrator]);
        using var other = TestHttp.CreateDevelopmentSession(OtherSchoolDomain);
        (await other.SignInAsync("admin@school2.local", TestApp.AdministratorPassword)).Dispose();
        (await ReadEnabledAsync(other)).ShouldNotContain(ModuleCodes.Fees);
    }

    [Test]
    public async Task ShouldNoLongerLetTheMasterSwitchModulesPerSchool()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var response = await master.PutAsync($"/api/Schools/{schoolId.PublicId}/modules/{ModuleCodes.Fees}", new { isEnabled = true });

        // Modules now come from the school's subscription only (ADR-012), so the route is gone.
        new[] { HttpStatusCode.NotFound, HttpStatusCode.MethodNotAllowed }.ShouldContain(response.StatusCode);
        (await ReadSchoolModulesAsync(master, schoolId.PublicId)).Single(m => m.Code == ModuleCodes.Fees).IsEnabled.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldTurnAnInactivePaidModuleOffForEverySchoolAndBackOn()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        await TestApp.SubscribeAsync(schoolId.Id, [ModuleCodes.Exams]);

        using var deactivated = await master.PutAsync($"/api/Modules/{ModuleCodes.Exams}", Update("Exams", isActive: false, sortOrder: 140));

        deactivated.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadModuleAsync(master, ModuleCodes.Exams)).IsActive.ShouldBeFalse();
        using var school = await SignInToSchoolAsync();
        (await ReadEnabledAsync(school)).ShouldNotContain(ModuleCodes.Exams);

        // Switching it back on restores the school's module.
        (await master.PutAsync($"/api/Modules/{ModuleCodes.Exams}", Update("Exams", isActive: true, sortOrder: 140))).Dispose();
        (await ReadEnabledAsync(school)).ShouldContain(ModuleCodes.Exams);

        var audit = await TestApp.FindAuditEventsAsync(masterUserId);
        audit.Count(e => e.EventType == SecurityAuditEventType.ModuleChanged && e.TargetId == ModuleCodes.Exams).ShouldBe(2);
    }

    [Test]
    public async Task ShouldAddChangeAndDeleteAModule()
    {
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var created = await master.PostAsync("/api/Modules", new { code = "CANTEEN", name = "Canteen", description = "Meals.", isCore = false, sortOrder = 210 });
        created.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await created.Content.ReadFromJsonAsync<string>()).ShouldBe("CANTEEN");

        using var changed = await master.PutAsync("/api/Modules/CANTEEN", new { name = "Canteen & meals", description = "", isCore = false, isActive = true, sortOrder = 5 });
        changed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var modules = await ReadModulesAsync(master);
        modules.First().Code.ShouldBe("CANTEEN");
        modules.First().ShouldSatisfyAllConditions(m => m.Name.ShouldBe("Canteen & meals"), m => m.IsActive.ShouldBeTrue(), m => m.Requires.ShouldBeEmpty());

        using var deleted = await master.DeleteAsync("/api/Modules/CANTEEN");
        deleted.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadModulesAsync(master)).ShouldNotContain(m => m.Code == "CANTEEN");
        using var gone = await master.GetAsync("/api/Modules/CANTEEN");
        gone.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // The row stays (soft delete), and the code can be used again by a new module.
        using var again = await master.PostAsync("/api/Modules", new { code = "CANTEEN", name = "Canteen", description = "", isCore = false, sortOrder = 210 });
        again.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await ReadModuleAsync(master, "CANTEEN")).SortOrder.ShouldBe(210);

        var audit = await TestApp.FindAuditEventsAsync(masterUserId);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.ModuleCreated && e.TargetId == "CANTEEN");
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.ModuleChanged && e.TargetId == "CANTEEN");
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.ModuleDeleted && e.TargetId == "CANTEEN");
    }

    [TestCase("FEES")]
    [TestCase("fees")]
    [TestCase("CANTEEN-2")]
    [TestCase("ENABLED")]
    [TestCase("")]
    public async Task ShouldRefuseATakenOrBadCode(string code)
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var response = await master.PostAsync("/api/Modules", new { code, name = "Module", description = "", isCore = false, sortOrder = 0 });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadModulesAsync(master)).Count.ShouldBe(ModuleCatalog.CreateModules().Count);
    }

    [Test]
    public async Task ShouldKeepCoreModulesOnAndUndeletable()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var deactivate = await master.PutAsync($"/api/Modules/{ModuleCodes.Students}", Update("Students", isActive: false, isCore: true, sortOrder: 50));
        using var delete = await master.DeleteAsync($"/api/Modules/{ModuleCodes.Students}");
        // A paid module made inactive cannot become core in the same change.
        using var inactiveCore = await master.PutAsync($"/api/Modules/{ModuleCodes.Library}", Update("Library", isActive: false, isCore: true, sortOrder: 180));

        deactivate.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        delete.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        inactiveCore.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadModuleAsync(master, ModuleCodes.Students)).IsActive.ShouldBeTrue();
        (await ReadModuleAsync(master, ModuleCodes.Library)).ShouldSatisfyAllConditions(m => m.IsActive.ShouldBeTrue(), m => m.IsCore.ShouldBeFalse());
    }

    [Test]
    public async Task ShouldKeepCoreModulesRequiringOnlyCoreModules()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        (await master.PostAsync($"/api/Modules/{ModuleCodes.Library}/dependencies", new { requiresCode = ModuleCodes.Fees })).Dispose();

        // LIBRARY requires FEES, which is paid, so LIBRARY cannot become core; nor can a core module require FEES.
        using var libraryCore = await master.PutAsync($"/api/Modules/{ModuleCodes.Library}", Update("Library", isActive: true, isCore: true, sortOrder: 180));
        using var coreRequiresPaid = await master.PostAsync($"/api/Modules/{ModuleCodes.Students}/dependencies", new { requiresCode = ModuleCodes.Fees });
        libraryCore.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        coreRequiresPaid.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // FEES requires only STUDENTS, which is core, so FEES can become core, and then a core module may require it.
        using var feesCore = await master.PutAsync($"/api/Modules/{ModuleCodes.Fees}", Update("Fees", isActive: true, isCore: true, sortOrder: 120));
        using var coreRequiresCore = await master.PostAsync($"/api/Modules/{ModuleCodes.Staff}/dependencies", new { requiresCode = ModuleCodes.Fees });
        feesCore.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        coreRequiresCore.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // While a core module requires FEES, FEES cannot go back to paid.
        using var feesPaid = await master.PutAsync($"/api/Modules/{ModuleCodes.Fees}", Update("Fees", isActive: true, isCore: false, sortOrder: 120));
        feesPaid.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadModuleAsync(master, ModuleCodes.Fees)).IsCore.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldAddAndRemoveDependenciesAndRefuseLoops()
    {
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var added = await master.PostAsync($"/api/Modules/{ModuleCodes.Fees}/dependencies", new { requiresCode = ModuleCodes.Admissions });
        using var twice = await master.PostAsync($"/api/Modules/{ModuleCodes.Fees}/dependencies", new { requiresCode = ModuleCodes.Admissions });
        using var direct = await master.PostAsync($"/api/Modules/{ModuleCodes.Admissions}/dependencies", new { requiresCode = ModuleCodes.Fees });
        (await master.PostAsync($"/api/Modules/{ModuleCodes.Transport}/dependencies", new { requiresCode = ModuleCodes.Fees })).Dispose();
        using var indirect = await master.PostAsync($"/api/Modules/{ModuleCodes.Admissions}/dependencies", new { requiresCode = ModuleCodes.Transport });
        using var self = await master.PostAsync($"/api/Modules/{ModuleCodes.Fees}/dependencies", new { requiresCode = ModuleCodes.Fees });
        using var unknown = await master.PostAsync($"/api/Modules/{ModuleCodes.Fees}/dependencies", new { requiresCode = "CANTEEN" });
        using var unknownModule = await master.PostAsync("/api/Modules/CANTEEN/dependencies", new { requiresCode = ModuleCodes.Fees });

        added.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        twice.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        direct.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        indirect.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        self.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        unknown.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        unknownModule.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadModuleAsync(master, ModuleCodes.Fees)).Requires.ShouldBe([ModuleCodes.Students, ModuleCodes.Admissions]);
        (await ReadModuleAsync(master, ModuleCodes.Admissions)).Requires.ShouldBe([ModuleCodes.Students]);

        // A module others require cannot be deleted until they stop requiring it.
        using var blocked = await master.DeleteAsync($"/api/Modules/{ModuleCodes.Admissions}");
        blocked.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        using var removed = await master.DeleteAsync($"/api/Modules/{ModuleCodes.Fees}/dependencies/{ModuleCodes.Admissions}");
        using var removedAgain = await master.DeleteAsync($"/api/Modules/{ModuleCodes.Fees}/dependencies/{ModuleCodes.Admissions}");
        removed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        removedAgain.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadModuleAsync(master, ModuleCodes.Fees)).Requires.ShouldBe([ModuleCodes.Students]);

        // Deleting a module drops what it required, so the module it needed can be deleted afterwards.
        (await master.DeleteAsync($"/api/Modules/{ModuleCodes.Transport}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await master.DeleteAsync($"/api/Modules/{ModuleCodes.Fees}")).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var audit = await TestApp.FindAuditEventsAsync(masterUserId);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.ModuleDependencyAdded && e.TargetId == $"{ModuleCodes.Fees}/{ModuleCodes.Admissions}");
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.ModuleDependencyRemoved && e.TargetId == $"{ModuleCodes.Fees}/{ModuleCodes.Admissions}");
    }

    [Test]
    public async Task ShouldNeverLimitTheMasterTenant()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        // The master has no subscription and still has every module.
        (await ReadEnabledAsync(master)).Count.ShouldBe(ModuleCatalog.CreateModules().Count);
        (await ReadSchoolModulesAsync(master, TestApp.MasterSchoolPublicId)).ShouldAllBe(m => m.IsEnabled);
    }

    [Test]
    public async Task ShouldAnswer404ForAnUnknownSchoolOrModule()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var schoolModules = await master.GetAsync($"/api/Schools/{Guid.CreateVersion7()}/modules");
        using var get = await master.GetAsync("/api/Modules/CANTEEN");
        using var update = await master.PutAsync("/api/Modules/CANTEEN", Update("Canteen", isActive: true, sortOrder: 0));
        using var delete = await master.DeleteAsync("/api/Modules/CANTEEN");

        schoolModules.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        get.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        update.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        delete.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ShouldKeepTheCatalogueAwayFromSchoolAdministrators()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var session = await SignInToSchoolAsync();

        var responses = new[]
        {
            await session.GetAsync("/api/Modules"),
            await session.GetAsync($"/api/Modules/{ModuleCodes.Fees}"),
            await session.PostAsync("/api/Modules", new { code = "CANTEEN", name = "Canteen", description = "", isCore = false, sortOrder = 0 }),
            await session.PutAsync($"/api/Modules/{ModuleCodes.Fees}", Update("Fees", isActive: true, isCore: true, sortOrder: 0)),
            await session.DeleteAsync($"/api/Modules/{ModuleCodes.Fees}"),
            await session.PostAsync($"/api/Modules/{ModuleCodes.Fees}/dependencies", new { requiresCode = ModuleCodes.Admissions }),
            await session.DeleteAsync($"/api/Modules/{ModuleCodes.Fees}/dependencies/{ModuleCodes.Students}"),
            await session.GetAsync($"/api/Schools/{schoolId.PublicId}/modules"),
        };

        foreach (var response in responses)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden, response.RequestMessage?.RequestUri?.ToString());
            response.Dispose();
        }

        (await TestApp.CountAsync<PortalModule>()).ShouldBe(ModuleCatalog.CreateModules().Count);
        (await TestApp.CountAsync<ModuleDependency>()).ShouldBe(ModuleCatalog.Dependencies.Count);
        (await ReadEnabledAsync(session)).ShouldNotContain(ModuleCodes.Fees);
    }

    [Test]
    public async Task ShouldRequireTheAdministratorRoleOnTheMasterTenant()
    {
        await TestApp.RunAsDefaultUserAsync();
        using var session = TestHttp.CreateDevelopmentSession();
        (await session.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword)).Dispose();

        using var catalog = await session.GetAsync("/api/Modules");
        using var create = await session.PostAsync("/api/Modules", new { code = "CANTEEN", name = "Canteen", description = "", isCore = false, sortOrder = 0 });
        using var dependency = await session.PostAsync($"/api/Modules/{ModuleCodes.Fees}/dependencies", new { requiresCode = ModuleCodes.Admissions });

        catalog.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        create.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        dependency.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldRequireSignIn()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var session = TestHttp.CreateDevelopmentSession(SchoolDomain);

        using var enabled = await session.GetAsync("/api/Modules/enabled");
        using var catalog = await session.GetAsync("/api/Modules");
        using var create = await session.PostAsync("/api/Modules", new { code = "CANTEEN", name = "Canteen", description = "", isCore = false, sortOrder = 0 });

        enabled.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        catalog.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        create.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static object Update(string name, bool isActive, int sortOrder, bool isCore = false) =>
        new { name, description = "", isCore, isActive, sortOrder };

    private static async Task<HttpSession> SignInToSchoolAsync()
    {
        var schoolId = (await TestApp.FindSchoolIdAsync(SchoolDomain))!;
        await TestApp.RunAsUserAsync(schoolId.Id, SchoolAdminEmail, TestApp.AdministratorPassword, [Roles.Administrator]);
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

    private static async Task<ModuleDto> ReadModuleAsync(HttpSession session, string code)
    {
        using var response = await session.GetAsync($"/api/Modules/{code}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<ModuleDto>())!;
    }

    private static async Task<List<SchoolModuleDto>> ReadSchoolModulesAsync(HttpSession session, Guid schoolId)
    {
        using var response = await session.GetAsync($"/api/Schools/{schoolId}/modules");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<SchoolModuleDto>>())!;
    }
}
