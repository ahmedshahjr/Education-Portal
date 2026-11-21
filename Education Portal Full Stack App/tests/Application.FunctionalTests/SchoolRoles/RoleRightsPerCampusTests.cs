using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.SchoolRoles;

/// <summary>R1.3 (ADR-022): rights per campus. The behaviour asks "any campus?", the handler narrows with the campus set. Another school is always denied.</summary>
public class RoleRightsPerCampusTests : TestBase
{
    [Test]
    public async Task ShouldGiveARightOnlyOnTheCampusTheRoleNames()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Campus Rights", "campus-rights.localhost");
        await TestApp.SubscribeAsync(school.Id, [ModuleCode.Exams]);
        var userId = await TestApp.RunAsUserAsync(school.Id, "teacher@campus-rights.local", TestApp.AdministratorPassword, []);
        var (main, north) = await AddCampusesAsync(school.Id);
        await GiveRoleAsync(school.Id, userId, north, allCampuses: false);

        (await CheckAsync(school.Id, userId, AccessRight.Edit)).ShouldBeTrue();
        var campuses = (await CampusesAsync(school.Id, userId, AccessRight.Edit)).ShouldNotBeNull();
        campuses.ShouldBe(new[] { north });
        campuses.ShouldNotContain(main);
    }

    [Test]
    public async Task ShouldGiveEveryCampusIncludingLaterOnesWhenTheRoleAppliesToAllCampuses()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("All Campuses", "all-campuses.localhost");
        await TestApp.SubscribeAsync(school.Id, [ModuleCode.Exams]);
        var userId = await TestApp.RunAsUserAsync(school.Id, "head@all-campuses.local", TestApp.AdministratorPassword, []);
        var (main, _) = await AddCampusesAsync(school.Id);
        await GiveRoleAsync(school.Id, userId, main, allCampuses: true);

        (await CampusesAsync(school.Id, userId, AccessRight.Edit)).ShouldBeNull();
    }

    [Test]
    public async Task ShouldCombineTwoRolesPerCampus()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Two Roles", "two-roles.localhost");
        await TestApp.SubscribeAsync(school.Id, [ModuleCode.Exams]);
        var userId = await TestApp.RunAsUserAsync(school.Id, "both@two-roles.local", TestApp.AdministratorPassword, []);
        var (main, north) = await AddCampusesAsync(school.Id);
        await GiveRoleAsync(school.Id, userId, main, allCampuses: false, roleName: "A");
        await GiveRoleAsync(school.Id, userId, north, allCampuses: false, roleName: "B");

        (await CampusesAsync(school.Id, userId, AccessRight.Edit)).ShouldNotBeNull().OrderBy(i => i).ShouldBe(new[] { main, north }.OrderBy(i => i));
    }

    [Test]
    public async Task ShouldRefuseTheRightInAnotherSchoolEvenWithAllCampuses()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Home Campus", "home-campus.localhost");
        var other = await TestApp.AddSchoolAsync("Other Campus", "other-campus.localhost");
        await TestApp.SubscribeAsync(school.Id, [ModuleCode.Exams]);
        await TestApp.SubscribeAsync(other.Id, [ModuleCode.Exams]);
        var userId = await TestApp.RunAsUserAsync(school.Id, "home@home-campus.local", TestApp.AdministratorPassword, []);
        var (main, _) = await AddCampusesAsync(school.Id);
        await GiveRoleAsync(school.Id, userId, main, allCampuses: true);

        (await CheckAsync(other.Id, userId, AccessRight.Edit)).ShouldBeFalse();
        (await CampusesAsync(other.Id, userId, AccessRight.Edit)).ShouldNotBeNull().ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldRefuseAModuleTheSchoolDoesNotHave()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("No Exams", "no-exams.localhost");
        var userId = await TestApp.RunAsUserAsync(school.Id, "t@no-exams.local", TestApp.AdministratorPassword, []);

        (await CheckAsync(school.Id, userId, AccessRight.Edit)).ShouldBeFalse();
    }

    private static async Task<(int Main, int North)> AddCampusesAsync(int schoolId)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(TestApp.MasterSchoolId, IsMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var main = await context.Campuses.SingleAsync(c => c.SchoolId == schoolId && c.Kind == CampusKind.Main);
        var north = Campus.Create(schoolId, "North", "0002");
        context.Campuses.Add(north);
        await context.SaveChangesAsync();

        return (main.Id, north.Id);
    }

    private static async Task GiveRoleAsync(int schoolId, string userId, int campusId, bool allCampuses, string roleName = "Grader")
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(TestApp.MasterSchoolId, IsMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var role = SchoolRole.Create(schoolId, roleName);
        context.SchoolRoles.Add(role);
        await context.SaveChangesAsync();

        var module = await context.Modules.SingleAsync(m => m.Code == ModuleCode.Exams);
        var campus = await context.Campuses.SingleAsync(c => c.Id == campusId);
        context.RolePermissions.Add(RolePermission.Create(role, module, campus, allCampuses, true, false, true, false, false, false, false));
        context.AccountRoles.Add(AccountRole.Create(schoolId, userId, role.Id));
        await context.SaveChangesAsync();
    }

    private static async Task<bool> CheckAsync(int schoolId, string userId, AccessRight right)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(schoolId, IsMaster: false));

        return await scope.ServiceProvider.GetRequiredService<IRoleRights>().HasRightAsync(userId, schoolId, ModuleCode.Exams, right, CancellationToken.None);
    }

    private static async Task<IReadOnlyCollection<int>?> CampusesAsync(int schoolId, string userId, AccessRight right)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(schoolId, IsMaster: false));

        return await scope.ServiceProvider.GetRequiredService<IRoleRights>().GetRightCampusIdsAsync(userId, schoolId, ModuleCode.Exams, right, CancellationToken.None);
    }
}
