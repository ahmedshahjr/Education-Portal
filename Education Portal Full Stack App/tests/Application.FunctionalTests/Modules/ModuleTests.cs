using System.Text.Json;
using System.Text.Json.Serialization;
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
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

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
        enabled.ShouldNotContain(ModuleCode.Fees);
    }

    [Test]
    public async Task ShouldListTheCatalogueInSortOrderWithDependencies()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        var modules = await ReadModulesAsync(master);

        modules.Select(m => m.Code).ShouldBe(ModuleCatalog.CreateModules().OrderBy(m => m.SortOrder).Select(m => m.Code).ToList());
        modules.Single(m => m.Code == ModuleCode.Fees).ShouldSatisfyAllConditions(
            m => m.IsCore.ShouldBeFalse(),
            m => m.Requires.ShouldBe([ModuleCode.Students]));
        modules.Single(m => m.Code == ModuleCode.Timetable).Requires.ShouldBe([ModuleCode.AcademicSetup, ModuleCode.Staff]);
        (await ReadModuleAsync(master, ModuleCode.Students)).IsCore.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldGiveAPaidModuleOnlyToTheSchoolWhoseSubscriptionHasIt()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        await TestApp.SubscribeAsync(schoolId.Id, [ModuleCode.Fees]);

        var modules = await ReadSchoolModulesAsync(master, schoolId.PublicId);
        modules.Single(m => m.Code == ModuleCode.Fees).IsEnabled.ShouldBeTrue();
        modules.Single(m => m.Code == ModuleCode.Exams).IsEnabled.ShouldBeFalse();
        modules.Single(m => m.Code == ModuleCode.Students).ShouldSatisfyAllConditions(m => m.IsCore.ShouldBeTrue(), m => m.IsEnabled.ShouldBeTrue());

        using var school = await SignInToSchoolAsync();
        (await ReadEnabledAsync(school)).ShouldContain(ModuleCode.Fees);

        // The other school does not get it.
        await TestApp.RunAsUserAsync((await TestApp.FindSchoolIdAsync(OtherSchoolDomain))!.Id, "admin@school2.local", TestApp.AdministratorPassword, [Roles.Administrator]);
        using var other = TestHttp.CreateDevelopmentSession(OtherSchoolDomain);
        (await other.SignInAsync("admin@school2.local", TestApp.AdministratorPassword)).Dispose();
        (await ReadEnabledAsync(other)).ShouldNotContain(ModuleCode.Fees);
    }

    [Test]
    public async Task ShouldTurnAnInactivePaidModuleOffForEverySchoolAndBackOn()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        await TestApp.SubscribeAsync(schoolId.Id, [ModuleCode.Exams]);

        using var deactivated = await master.PutAsync($"/api/Modules/{ModuleCode.Exams}", Update("Exams", isActive: false, sortOrder: 140));

        deactivated.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadModuleAsync(master, ModuleCode.Exams)).IsActive.ShouldBeFalse();
        using var school = await SignInToSchoolAsync();
        (await ReadEnabledAsync(school)).ShouldNotContain(ModuleCode.Exams);

        // Switching it back on restores the school's module.
        (await master.PutAsync($"/api/Modules/{ModuleCode.Exams}", Update("Exams", isActive: true, sortOrder: 140))).Dispose();
        (await ReadEnabledAsync(school)).ShouldContain(ModuleCode.Exams);

        var audit = await TestApp.FindAuditEventsAsync(masterUserId);
        audit.Count(e => e.EventType == SecurityAuditEventType.ModuleChanged && e.TargetId == nameof(ModuleCode.Exams)).ShouldBe(2);
    }

    [Test]
    public async Task ShouldLetTheMasterSwitchACoreModuleOffForEverySchoolAndBackOn()
    {
        await TestApp.RunAsAdministratorAsync();
        await TestApp.AddSchoolAsync("Module School", SchoolDomain);
        using var master = await SignInAsMasterAsync();
        using var school = await SignInToSchoolAsync();
        (await ReadEnabledAsync(school)).ShouldContain(ModuleCode.Students);

        // Every module can be switched off from the master settings, core ones included.
        using var deactivate = await master.PutAsync($"/api/Modules/{ModuleCode.Students}", Update("Students", isActive: false, isCore: true, sortOrder: 50));
        deactivate.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadModuleAsync(master, ModuleCode.Students)).IsActive.ShouldBeFalse();
        (await ReadEnabledAsync(school)).ShouldNotContain(ModuleCode.Students);

        using var reactivate = await master.PutAsync($"/api/Modules/{ModuleCode.Students}", Update("Students", isActive: true, isCore: true, sortOrder: 50));
        reactivate.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadEnabledAsync(school)).ShouldContain(ModuleCode.Students);
    }

    [Test]
    public async Task ShouldKeepCoreModulesRequiringOnlyCoreModules()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        (await master.PostAsync($"/api/Modules/{ModuleCode.Library}/dependencies", new { requiresCode = nameof(ModuleCode.Fees) })).Dispose();

        // LIBRARY requires FEES, which is paid, so LIBRARY cannot become core; nor can a core module require FEES.
        using var libraryCore = await master.PutAsync($"/api/Modules/{ModuleCode.Library}", Update("Library", isActive: true, isCore: true, sortOrder: 180));
        using var coreRequiresPaid = await master.PostAsync($"/api/Modules/{ModuleCode.Students}/dependencies", new { requiresCode = nameof(ModuleCode.Fees) });
        libraryCore.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        coreRequiresPaid.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // FEES requires only STUDENTS, which is core, so FEES can become core, and then a core module may require it.
        using var feesCore = await master.PutAsync($"/api/Modules/{ModuleCode.Fees}", Update("Fees", isActive: true, isCore: true, sortOrder: 120));
        using var coreRequiresCore = await master.PostAsync($"/api/Modules/{ModuleCode.Staff}/dependencies", new { requiresCode = nameof(ModuleCode.Fees) });
        feesCore.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        coreRequiresCore.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // While a core module requires FEES, FEES cannot go back to paid.
        using var feesPaid = await master.PutAsync($"/api/Modules/{ModuleCode.Fees}", Update("Fees", isActive: true, isCore: false, sortOrder: 120));
        feesPaid.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadModuleAsync(master, ModuleCode.Fees)).IsCore.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldAddAndRemoveDependenciesAndRefuseLoops()
    {
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var added = await master.PostAsync($"/api/Modules/{ModuleCode.Fees}/dependencies", new { requiresCode = nameof(ModuleCode.Admissions) });
        using var twice = await master.PostAsync($"/api/Modules/{ModuleCode.Fees}/dependencies", new { requiresCode = nameof(ModuleCode.Admissions) });
        using var direct = await master.PostAsync($"/api/Modules/{ModuleCode.Admissions}/dependencies", new { requiresCode = nameof(ModuleCode.Fees) });
        (await master.PostAsync($"/api/Modules/{ModuleCode.Transport}/dependencies", new { requiresCode = nameof(ModuleCode.Fees) })).Dispose();
        using var indirect = await master.PostAsync($"/api/Modules/{ModuleCode.Admissions}/dependencies", new { requiresCode = nameof(ModuleCode.Transport) });
        using var self = await master.PostAsync($"/api/Modules/{ModuleCode.Fees}/dependencies", new { requiresCode = nameof(ModuleCode.Fees) });
        using var unknown = await master.PostAsync($"/api/Modules/{ModuleCode.Fees}/dependencies", new { requiresCode = "CANTEEN" });
        using var unknownModule = await master.PostAsync("/api/Modules/CANTEEN/dependencies", new { requiresCode = nameof(ModuleCode.Fees) });

        added.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        twice.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        direct.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        indirect.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        self.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        unknown.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        unknownModule.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadModuleAsync(master, ModuleCode.Fees)).Requires.ShouldBe([ModuleCode.Students, ModuleCode.Admissions]);
        (await ReadModuleAsync(master, ModuleCode.Admissions)).Requires.ShouldBe([ModuleCode.Students]);

        using var removed = await master.DeleteAsync($"/api/Modules/{ModuleCode.Fees}/dependencies/{ModuleCode.Admissions}");
        using var removedAgain = await master.DeleteAsync($"/api/Modules/{ModuleCode.Fees}/dependencies/{ModuleCode.Admissions}");
        removed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        removedAgain.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadModuleAsync(master, ModuleCode.Fees)).Requires.ShouldBe([ModuleCode.Students]);

        var audit = await TestApp.FindAuditEventsAsync(masterUserId);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.ModuleDependencyAdded && e.TargetId == $"{ModuleCode.Fees}/{ModuleCode.Admissions}");
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.ModuleDependencyRemoved && e.TargetId == $"{ModuleCode.Fees}/{ModuleCode.Admissions}");
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
    public async Task ShouldAnswer404ForAnUnknownSchoolAnd400ForAnUnknownModuleCode()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        using var schoolModules = await master.GetAsync($"/api/Schools/{Guid.CreateVersion7()}/modules");
        using var get = await master.GetAsync("/api/Modules/CANTEEN");
        using var update = await master.PutAsync("/api/Modules/CANTEEN", Update("Canteen", isActive: true, sortOrder: 0));

        schoolModules.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        get.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        update.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldKeepTheCatalogueAwayFromSchoolAdministrators()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var session = await SignInToSchoolAsync();

        var responses = new[]
        {
            await session.GetAsync("/api/Modules"),
            await session.GetAsync($"/api/Modules/{ModuleCode.Fees}"),
            await session.PutAsync($"/api/Modules/{ModuleCode.Fees}", Update("Fees", isActive: true, isCore: true, sortOrder: 0)),
            await session.PostAsync($"/api/Modules/{ModuleCode.Fees}/dependencies", new { requiresCode = nameof(ModuleCode.Admissions) }),
            await session.DeleteAsync($"/api/Modules/{ModuleCode.Fees}/dependencies/{ModuleCode.Students}"),
            await session.GetAsync($"/api/Schools/{schoolId.PublicId}/modules"),
        };

        foreach (var response in responses)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden, response.RequestMessage?.RequestUri?.ToString());
            response.Dispose();
        }

        (await TestApp.CountAsync<PortalModule>()).ShouldBe(ModuleCatalog.CreateModules().Count);
        (await TestApp.CountAsync<ModuleDependency>()).ShouldBe(ModuleCatalog.Dependencies.Count);
        (await ReadEnabledAsync(session)).ShouldNotContain(ModuleCode.Fees);
    }

    [Test]
    public async Task ShouldRequireTheAdministratorRoleOnTheMasterTenant()
    {
        await TestApp.RunAsDefaultUserAsync();
        using var session = TestHttp.CreateDevelopmentSession();
        (await session.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword)).Dispose();

        using var catalog = await session.GetAsync("/api/Modules");
        using var dependency = await session.PostAsync($"/api/Modules/{ModuleCode.Fees}/dependencies", new { requiresCode = nameof(ModuleCode.Admissions) });

        catalog.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        dependency.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldRequireSignIn()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var session = TestHttp.CreateDevelopmentSession(SchoolDomain);

        using var enabled = await session.GetAsync("/api/Modules/enabled");
        using var catalog = await session.GetAsync("/api/Modules");

        enabled.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        catalog.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
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

    private static async Task<List<ModuleCode>> ReadEnabledAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/Modules/enabled");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<string>>(Json))!.Select(Enum.Parse<ModuleCode>).ToList();
    }

    private static async Task<List<ModuleDto>> ReadModulesAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/Modules");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<ModuleDto>>(Json))!;
    }

    private static async Task<ModuleDto> ReadModuleAsync(HttpSession session, ModuleCode code)
    {
        using var response = await session.GetAsync($"/api/Modules/{code}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<ModuleDto>(Json))!;
    }

    private static async Task<List<SchoolModuleDto>> ReadSchoolModulesAsync(HttpSession session, Guid schoolId)
    {
        using var response = await session.GetAsync($"/api/Schools/{schoolId}/modules");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<SchoolModuleDto>>(Json))!;
    }
}
