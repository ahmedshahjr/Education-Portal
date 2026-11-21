using EducationPortal.Application.AcademicCalendar.Commands.CreateHoliday;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.AcademicCalendar;

/// <summary>
/// A role's Create right on a campus is not enough on its own: the user must also be assigned to that campus (ADR-022,
/// ICampusAccess.EnsureCampusAsync with module and right). Without the assignment the write is refused.
/// </summary>
public class CampusRightsDenialTests : TestBase
{
    [Test]
    public async Task ShouldRefuseACalendarWriteOnACampusTheUserIsNotAssignedTo()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Calendar Denial", "calendar-denial.localhost");
        await TestApp.SubscribeAsync(school.Id, [ModuleCode.AcademicSetup]);
        var userId = await TestApp.RunAsUserAsync(school.Id, "planner@calendar-denial.local", TestApp.AdministratorPassword, []);
        await GiveCreateRightAsync(school.Id, userId);
        var campusPublicId = await MainCampusPublicIdAsync(school.Id);

        // The role gives Create on every campus, but the user is assigned to none: the campus is refused.
        TestApp.UseTenant(school.Id);
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new CreateHolidayCommand
        {
            CampusId = campusPublicId,
            Title = "Founders day",
            StartDate = new DateOnly(2027, 3, 1),
            EndDate = new DateOnly(2027, 3, 1),
            Type = HolidayType.School,
            AppliesTo = HolidayAudience.Both,
        }));
    }

    private static async Task GiveCreateRightAsync(int schoolId, string userId)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(TestApp.MasterSchoolId, IsMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var role = SchoolRole.Create(schoolId, "Planner");
        context.SchoolRoles.Add(role);
        await context.SaveChangesAsync();

        var module = await context.Modules.SingleAsync(m => m.Code == ModuleCode.AcademicSetup);
        var campus = await context.Campuses.SingleAsync(c => c.SchoolId == schoolId && c.Kind == CampusKind.Main);
        context.RolePermissions.Add(RolePermission.Create(role, module, campus, allCampuses: true, true, true, false, false, false, false, false));
        context.AccountRoles.Add(AccountRole.Create(schoolId, userId, role.Id));
        await context.SaveChangesAsync();
    }

    private static async Task<Guid> MainCampusPublicIdAsync(int schoolId)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(TestApp.MasterSchoolId, IsMaster: true));
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Campuses.Where(c => c.SchoolId == schoolId && c.Kind == CampusKind.Main).Select(c => c.PublicId).SingleAsync();
    }
}
