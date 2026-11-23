using EducationPortal.Application.AcademicCalendar.Commands.CreateCalendarEvent;
using EducationPortal.Application.AcademicCalendar.Commands.CreateHoliday;
using EducationPortal.Application.AcademicCalendar.Commands.DeleteHoliday;
using EducationPortal.Application.AcademicCalendar.Commands.UpdateHoliday;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.Application.FunctionalTests.AcademicCalendar;

/// <summary>
/// A whole-school holiday or event is stored once per campus, all copies sharing one key (ADR-027). Saving it writes a row for
/// each campus, changing or deleting it changes every copy, a holiday for one campus stays on that campus, and a campus added
/// later gets a copy of each whole-school item.
/// </summary>
public class WholeSchoolRowsTests : TestBase
{
    private const string SchoolDomain = "whole-school.localhost";
    private static readonly DateOnly Day = new(2026, 8, 14);

    [Test]
    public async Task ShouldSaveAWholeSchoolHolidayOncePerCampusUnderOneKey()
    {
        var (schoolId, main, north) = await SetUpSchoolWithTwoCampusesAsync();

        var id = await TestApp.SendAsync<Guid>(Holiday("Independence Day", null));

        var rows = await HolidayRowsAsync(schoolId);
        rows.Select(r => r.CampusId).Order().ShouldBe([main, north]);
        rows.Select(r => r.WholeSchoolKey).Distinct().Single().ShouldNotBeNull();
        rows.Select(r => r.PublicId).ShouldContain(id);
    }

    [Test]
    public async Task ShouldChangeAndDeleteEveryCopyOfAWholeSchoolHoliday()
    {
        var (schoolId, _, _) = await SetUpSchoolWithTwoCampusesAsync();
        var id = await TestApp.SendAsync<Guid>(Holiday("Independence Day", null));

        await TestApp.SendAsync(new UpdateHolidayCommand
        {
            HolidayId = id,
            Title = "Independence holiday",
            StartDate = Day,
            EndDate = Day,
            Type = HolidayType.Public,
            AppliesTo = HolidayAudience.Both,
        });

        var changed = await HolidayRowsAsync(schoolId);
        changed.Count.ShouldBe(2);
        changed.Select(r => r.Title).Distinct().ShouldBe(["Independence holiday"]);

        await TestApp.SendAsync(new DeleteHolidayCommand { HolidayId = id });

        (await HolidayRowsAsync(schoolId)).ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldKeepACampusHolidayOnItsOwnCampusAndRefuseToMakeItWholeSchool()
    {
        var (schoolId, _, north) = await SetUpSchoolWithTwoCampusesAsync();
        var northPublicId = await CampusPublicIdAsync(schoolId, north);

        var id = await TestApp.SendAsync<Guid>(Holiday("Repairs", northPublicId));

        var rows = await HolidayRowsAsync(schoolId);
        rows.ShouldHaveSingleItem().CampusId.ShouldBe(north);
        rows.Single().WholeSchoolKey.ShouldBeNull();

        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new UpdateHolidayCommand
        {
            HolidayId = id,
            Title = "Repairs",
            StartDate = Day,
            EndDate = Day,
            Type = HolidayType.Public,
            AppliesTo = HolidayAudience.Both,
            CampusId = null,
        }));
    }

    [Test]
    public async Task ShouldSaveAWholeSchoolEventOncePerCampus()
    {
        var (schoolId, main, north) = await SetUpSchoolWithTwoCampusesAsync();

        await TestApp.SendAsync<Guid>(new CreateCalendarEventCommand
        {
            Title = "Parent-teacher meeting",
            EventDate = Day,
            Type = CalendarEventType.ParentTeacherMeeting,
            Audience = CalendarEventAudience.Parents,
        });

        await using var scope = await ScopeOnSchoolAsync(schoolId);
        var events = await scope.Context.CalendarEvents.Where(e => e.SchoolId == schoolId).ToListAsync();
        events.Select(e => e.CampusId).Order().ShouldBe([main, north]);
        events.Select(e => e.WholeSchoolKey).Distinct().Single().ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldGiveANewCampusACopyOfEachWholeSchoolItem()
    {
        var (schoolId, main, north) = await SetUpSchoolWithTwoCampusesAsync();
        await TestApp.SendAsync<Guid>(Holiday("Independence Day", null));
        var later = Campus.Create(schoolId, "South", "0003");
        await TestApp.AddAsync(later);

        // The campus is saved and given the copies in one transaction by CreateCampus; this checks the copying itself.
        await using (var scope = await ScopeOnSchoolAsync(schoolId))
        {
            await WholeSchoolCopies.CopyToNewCampusAsync(scope.Context, schoolId, later.Id, CancellationToken.None);
            await scope.Context.SaveChangesAsync();
        }

        var rows = await HolidayRowsAsync(schoolId);
        rows.Select(r => r.CampusId).Order().ToArray().ShouldBe(new[] { main, north, later.Id }.Order().ToArray());
        rows.Select(r => r.WholeSchoolKey).Distinct().Single().ShouldNotBeNull();
    }

    private static object Holiday(string title, Guid? campusId) => new CreateHolidayCommand
    {
        CampusId = campusId,
        Title = title,
        StartDate = Day,
        EndDate = Day,
        Type = HolidayType.Public,
        AppliesTo = HolidayAudience.Both,
    };

    /// <summary>A school with its main campus and a second campus, with an administrator signed in on its tenant.</summary>
    private static async Task<(int SchoolId, int Main, int North)> SetUpSchoolWithTwoCampusesAsync()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Whole School", SchoolDomain);
        await TestApp.SubscribeAsync(school.Id, [ModuleCode.AcademicSetup]);
        var north = Campus.Create(school.Id, "North", "0002");
        await TestApp.AddAsync(north);

        await TestApp.RunAsUserAsync(school.Id, "admin@whole-school.local", TestApp.AdministratorPassword, [Roles.Administrator]);
        TestApp.UseTenant(school.Id);
        return (school.Id, await TestApp.MainCampusIdAsync(school.Id), north.Id);
    }

    private static async Task<Guid> CampusPublicIdAsync(int schoolId, int campusId)
    {
        await using var scope = await ScopeOnSchoolAsync(schoolId);
        return await scope.Context.Campuses.Where(c => c.Id == campusId).Select(c => c.PublicId).SingleAsync();
    }

    private static async Task<List<HolidayRow>> HolidayRowsAsync(int schoolId)
    {
        await using var scope = await ScopeOnSchoolAsync(schoolId);
        return await scope.Context.Holidays
            .Where(h => h.SchoolId == schoolId)
            .Select(h => new HolidayRow(h.PublicId, h.CampusId, h.WholeSchoolKey, h.Title))
            .ToListAsync();
    }

    private static async Task<SchoolScope> ScopeOnSchoolAsync(int schoolId)
    {
        var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        TestApp.SetTenant(scope.ServiceProvider, TestApp.Resolve(schoolId));
        return new SchoolScope(scope, scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    private sealed record HolidayRow(Guid PublicId, int CampusId, Guid? WholeSchoolKey, string Title);

    private sealed class SchoolScope(IServiceScope scope, ApplicationDbContext context) : IAsyncDisposable
    {
        public ApplicationDbContext Context { get; } = context;

        public ValueTask DisposeAsync()
        {
            scope.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
