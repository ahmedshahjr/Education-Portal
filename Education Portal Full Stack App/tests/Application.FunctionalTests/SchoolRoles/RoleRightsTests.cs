using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.SchoolRoles;

/// <summary>
/// A user has a right in a module only through a school role of their own school that gives it (SEC-AUTHZ-02, SEC-AUTHZ-04).
/// </summary>
public class RoleRightsTests : TestBase
{
    [Test]
    public async Task ShouldGiveOnlyTheRightsTheUsersRoleHolds()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Rights School", "role-rights.localhost");
        await TestApp.SubscribeAsync(school.Id, [ModuleCode.Exams]);
        var userId = await TestApp.RunAsUserAsync(school.Id, "grader@role-rights.local", TestApp.AdministratorPassword, []);
        await GiveRoleAsync(school.Id, userId, canView: true, canEdit: true, canDelete: false);

        (await CheckAsync(school.Id, userId, AccessRight.View)).ShouldBeTrue();
        (await CheckAsync(school.Id, userId, AccessRight.Edit)).ShouldBeTrue();
        (await CheckAsync(school.Id, userId, AccessRight.Delete)).ShouldBeFalse();
    }

    [Test]
    public async Task ShouldRefuseAUserWithNoRole()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("No Role School", "no-role.localhost");
        await TestApp.SubscribeAsync(school.Id, [ModuleCode.Exams]);
        var userId = await TestApp.RunAsUserAsync(school.Id, "plain@no-role.local", TestApp.AdministratorPassword, []);

        (await CheckAsync(school.Id, userId, AccessRight.View)).ShouldBeFalse();
    }

    [Test]
    public async Task ShouldNotGiveAnotherSchoolsRoleToAUser()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Home School", "home-school.localhost");
        var other = await TestApp.AddSchoolAsync("Other School", "other-school.localhost");
        await TestApp.SubscribeAsync(school.Id, [ModuleCode.Exams]);
        await TestApp.SubscribeAsync(other.Id, [ModuleCode.Exams]);
        var userId = await TestApp.RunAsUserAsync(school.Id, "home@home-school.local", TestApp.AdministratorPassword, []);
        await GiveRoleAsync(school.Id, userId, canView: true, canEdit: true, canDelete: true);

        (await CheckAsync(other.Id, userId, AccessRight.View)).ShouldBeFalse();
    }

    /// <summary>Gives the user a role of the school that holds the given rights in Exams.</summary>
    private static async Task GiveRoleAsync(int schoolId, string userId, bool canView, bool canEdit, bool canDelete)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(TestApp.MasterSchoolId, IsMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var role = SchoolRole.Create(schoolId, "Grader");
        context.SchoolRoles.Add(role);
        await context.SaveChangesAsync();

        var module = await context.Modules.SingleAsync(m => m.Code == ModuleCode.Exams);
        context.RolePermissions.Add(RolePermission.Create(role, module, canView, canCreate: false, canEdit, canDelete));
        context.AccountRoles.Add(AccountRole.Create(schoolId, userId, role.Id));
        await context.SaveChangesAsync();
    }

    /// <summary>Asks the right as the school's own request would: the tenant is the school, not the master.</summary>
    private static async Task<bool> CheckAsync(int schoolId, string userId, AccessRight right)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(schoolId, IsMaster: false));
        var rights = scope.ServiceProvider.GetRequiredService<IRoleRights>();

        return await rights.HasRightAsync(userId, schoolId, ModuleCode.Exams, right, CancellationToken.None);
    }
}
