using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Application.Users.Queries.GetMyRights;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Web.Services;
using EducationPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Users;

/// <summary>
/// The signed-in user's own rights for the menu (R1.5, ADR-022): only modules the school has, only rights their own
/// school's roles give, campus-limited roles fail closed without an assignment, and administrators see every right.
/// </summary>
public class GetMyRightsTests : TestBase
{
    [Test]
    public async Task ShouldListTheRightsARoleGivesInAModuleTheSchoolHas()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Rights Menu", "rights-menu.localhost");
        await TestApp.SubscribeAsync(school.Id, [ModuleCode.Exams]);
        var userId = await TestApp.RunAsUserAsync(school.Id, "grader@rights-menu.local", TestApp.AdministratorPassword, []);
        await GiveRoleAsync(school.Id, userId, allCampuses: true, canEdit: true);
        await AssignMainCampusAsync(school.Id, userId);

        var rights = await MyRightsAsync(school.Id);

        // The role gives every campus, and the user is assigned to the main campus: the rights hold where both agree.
        var exams = rights.Modules.ShouldHaveSingleItem();
        exams.Module.ShouldBe(ModuleCode.Exams);
        var mainCampus = await MainCampusPublicIdAsync(school.Id);
        exams.Rights.ShouldContain(g => g.Right == AccessRight.Edit && !g.AllCampuses && g.CampusIds.SequenceEqual(new[] { mainCampus }));
        exams.Rights.ShouldContain(g => g.Right == AccessRight.View && !g.AllCampuses && g.CampusIds.SequenceEqual(new[] { mainCampus }));
        exams.Rights.ShouldNotContain(g => g.Right == AccessRight.Delete);
    }

    [Test]
    public async Task ShouldNotListACampusLimitedRightWithoutAnAssignment()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Campus Menu", "campus-menu.localhost");
        await TestApp.SubscribeAsync(school.Id, [ModuleCode.Exams]);
        var userId = await TestApp.RunAsUserAsync(school.Id, "teacher@campus-menu.local", TestApp.AdministratorPassword, []);
        await GiveRoleAsync(school.Id, userId, allCampuses: false, canEdit: true);

        // The role gives Edit on the main campus only, and the user is not assigned to any campus: fail closed.
        var rights = await MyRightsAsync(school.Id);

        rights.Modules.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldListACampusLimitedRightOnTheCampusTheUserIsAssignedTo()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Limited Menu", "limited-menu.localhost");
        await TestApp.SubscribeAsync(school.Id, [ModuleCode.Exams]);
        var userId = await TestApp.RunAsUserAsync(school.Id, "limited@limited-menu.local", TestApp.AdministratorPassword, []);
        await GiveRoleAsync(school.Id, userId, allCampuses: false, canEdit: true);
        await AssignMainCampusAsync(school.Id, userId);

        var rights = await MyRightsAsync(school.Id);

        rights.Modules.Select(m => m.Module).ShouldBe([ModuleCode.Exams]);
        // The role gives View and Edit, both on the main campus only.
        var grants = rights.Modules.ShouldHaveSingleItem().Rights;
        grants.Select(g => g.Right).ShouldBe([AccessRight.View, AccessRight.Edit], ignoreOrder: true);
        var edit = grants.Single(g => g.Right == AccessRight.Edit);
        edit.AllCampuses.ShouldBeFalse();
        edit.CampusIds.ShouldBe([await MainCampusPublicIdAsync(school.Id)]);
    }

    [Test]
    public async Task ShouldNotListAModuleTheSchoolDoesNotHave()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("No Exams Menu", "no-exams-menu.localhost");
        var userId = await TestApp.RunAsUserAsync(school.Id, "t@no-exams-menu.local", TestApp.AdministratorPassword, []);
        await GiveRoleAsync(school.Id, userId, allCampuses: true, canEdit: true);

        var rights = await MyRightsAsync(school.Id);

        rights.Modules.Select(m => m.Module).ShouldNotContain(ModuleCode.Exams);
    }

    [Test]
    public async Task ShouldNotShowAnotherSchoolsRole()
    {
        await TestApp.RunAsAdministratorAsync();
        var home = await TestApp.AddSchoolAsync("Home Menu", "home-menu.localhost");
        var other = await TestApp.AddSchoolAsync("Other Menu", "other-menu.localhost");
        await TestApp.SubscribeAsync(home.Id, [ModuleCode.Exams]);
        await TestApp.SubscribeAsync(other.Id, [ModuleCode.Exams]);
        var userId = await TestApp.RunAsUserAsync(home.Id, "home@home-menu.local", TestApp.AdministratorPassword, []);
        await GiveRoleAsync(other.Id, userId, allCampuses: true, canEdit: true);

        var rights = await MyRightsAsync(home.Id);

        rights.Modules.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldGiveAnAdministratorEveryRightInEveryModuleTheSchoolHas()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Admin Menu", "admin-menu.localhost");
        await TestApp.SubscribeAsync(school.Id, [ModuleCode.Exams]);
        await TestApp.RunAsUserAsync(school.Id, "head@admin-menu.local", TestApp.AdministratorPassword, [Roles.Administrator]);

        var rights = await MyRightsAsync(school.Id);

        var exams = rights.Modules.Single(m => m.Module == ModuleCode.Exams);
        // Administrators are not limited by assignments: they may act on every campus (ADR-018).
        exams.Rights.Select(r => r.Right).ShouldBe(Enum.GetValues<AccessRight>().Where(r => r != AccessRight.None).ToList(), ignoreOrder: true);
        exams.Rights.ShouldAllBe(r => r.AllCampuses);
    }

    /// <summary>Asks the query as the user who was last run, on the school's tenant.</summary>
    private static async Task<MyRightsDto> MyRightsAsync(int schoolId)
    {
        TestApp.UseTenant(schoolId);
        return await TestApp.SendAsync<MyRightsDto>(new GetMyRightsQuery());
    }

    /// <summary>Makes the account a person of the school and assigns that person to the main campus (UserCampuses, ADR-022).</summary>
    private static async Task AssignMainCampusAsync(int schoolId, string userId)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(TestApp.MasterSchoolId, IsMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var person = UserDetails.Create(schoolId, "Teacher", null, "One", UserType.Teacher, null);
        context.UserDetails.Add(person);
        await context.SaveChangesAsync();

        var campus = await context.Campuses.SingleAsync(c => c.SchoolId == schoolId && c.Kind == CampusKind.Main);
        context.UserCampuses.Add(UserCampus.Create(schoolId, person.Id, campus.Id));

        var user = await userManager.FindByIdAsync(userId) ?? throw new InvalidOperationException("The account is missing.");
        user.UserDetailsId = person.Id;
        await userManager.UpdateAsync(user);
        await context.SaveChangesAsync();
    }

    private static async Task<Guid> MainCampusPublicIdAsync(int schoolId)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(TestApp.MasterSchoolId, IsMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Campuses.Where(c => c.SchoolId == schoolId && c.Kind == CampusKind.Main).Select(c => c.PublicId).SingleAsync();
    }

    /// <summary>Gives the user a role of the school whose Exams rights are Edit (and View) on all campuses or on the main campus only.</summary>
    private static async Task GiveRoleAsync(int schoolId, string userId, bool allCampuses, bool canEdit)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(TestApp.MasterSchoolId, IsMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var role = SchoolRole.Create(schoolId, "Grader");
        context.SchoolRoles.Add(role);
        await context.SaveChangesAsync();

        var module = await context.Modules.SingleAsync(m => m.Code == ModuleCode.Exams);
        var campus = await context.Campuses.SingleAsync(c => c.SchoolId == schoolId && c.Kind == CampusKind.Main);
        context.RolePermissions.Add(RolePermission.Create(role, module, campus, allCampuses, true, false, canEdit, false, false, false, false));
        context.AccountRoles.Add(AccountRole.Create(schoolId, userId, role.Id));
        await context.SaveChangesAsync();
    }
}
