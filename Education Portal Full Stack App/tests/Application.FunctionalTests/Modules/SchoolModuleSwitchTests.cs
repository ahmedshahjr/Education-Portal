using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Domain.Enums;
using System.Net;
using System.Net.Http.Json;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Constants;
using EducationPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Modules;

/// <summary>
/// The master tenant turns any module on or off for one school, core and default modules included. A school cannot change
/// its own modules, the master tenant has every module, and a module switched off for every school cannot be turned on.
/// </summary>
public class SchoolModuleSwitchTests : TestBase
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private const string SchoolDomain = "module-switch.localhost";

    [Test]
    public async Task ShouldTurnACoreModuleOffAndOnForOneSchool()
    {
        var (master, school, session) = await SetUpAsync();
        (await ReadEnabledAsync(session)).ShouldContain(ModuleCode.Students);

        using var off = await master.PutAsync($"/api/Schools/{school}/modules/{ModuleCode.Students}", new { enabled = false });
        off.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadEnabledAsync(session)).ShouldNotContain(ModuleCode.Students);

        using var on = await master.PutAsync($"/api/Schools/{school}/modules/{ModuleCode.Students}", new { enabled = true });
        on.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadEnabledAsync(session)).ShouldContain(ModuleCode.Students);
    }

    [Test]
    public async Task ShouldGiveAPaidModuleToOneSchoolWithoutASubscriptionAndTakeItBack()
    {
        var (master, school, session) = await SetUpAsync();
        (await ReadEnabledAsync(session)).ShouldNotContain(ModuleCode.Fees);

        using var on = await master.PutAsync($"/api/Schools/{school}/modules/{ModuleCode.Fees}", new { enabled = true });
        on.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadEnabledAsync(session)).ShouldContain(ModuleCode.Fees);

        using var off = await master.PutAsync($"/api/Schools/{school}/modules/{ModuleCode.Fees}", new { enabled = false });
        off.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadEnabledAsync(session)).ShouldNotContain(ModuleCode.Fees);
    }

    [Test]
    public async Task ShouldNotTurnOnAModuleSwitchedOffForEverySchool()
    {
        var (master, school, _) = await SetUpAsync();
        (await master.PutAsync($"/api/Modules/{ModuleCode.Library}", new { name = "Library", description = "", isCore = false, isActive = false, isDefaultEnabled = false, sortOrder = 180 })).Dispose();

        using var response = await master.PutAsync($"/api/Schools/{school}/modules/{ModuleCode.Library}", new { enabled = true });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseTheMasterTenantAndSchoolAdministrators()
    {
        var (master, _, session) = await SetUpAsync();
        var master2 = await master.PutAsync($"/api/Schools/{await MasterIdAsync()}/modules/{ModuleCode.Fees}", new { enabled = true });
        master2.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        master2.Dispose();

        using var schoolAdmin = await session.PutAsync($"/api/Schools/{await MasterIdAsync()}/modules/{ModuleCode.Fees}", new { enabled = true });
        schoolAdmin.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    private static async Task<(HttpSession Master, Guid School, HttpSession Session)> SetUpAsync()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Switch School", SchoolDomain);
        var master = await SignInAsMasterAsync();
        var session = await SignInToSchoolAsync(school.Id);
        return (master, school.PublicId, session);
    }

    private static async Task<HttpSession> SignInAsMasterAsync()
    {
        var master = TestHttp.CreateDevelopmentSession();
        using var response = await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return master;
    }

    private static async Task<HttpSession> SignInToSchoolAsync(int schoolId)
    {
        const string email = "admin@module-switch.local";
        await TestApp.RunAsUserAsync(schoolId, email, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(SchoolDomain);
        using var response = await session.SignInAsync(email, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return session;
    }

    private static async Task<Guid> MasterIdAsync()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EducationPortal.Infrastructure.Data.ApplicationDbContext>();
        return await context.Schools.Where(s => s.IsMaster).Select(s => s.PublicId).SingleAsync();
    }

    private static async Task<List<ModuleCode>> ReadEnabledAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/Modules/enabled");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<string>>(Json))!.Select(Enum.Parse<ModuleCode>).ToList();
    }
}
