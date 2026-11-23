using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Entities;

namespace EducationPortal.Application.AcademicCalendar.Common;

/// <summary>
/// A whole-school item is stored as one row per campus of the school, all sharing one <c>WholeSchoolKey</c> (ADR-027). An
/// item for one campus is a single row with no key.
/// </summary>
public static class WholeSchoolCopies
{
    /// <summary>The campuses an item is saved for: the one named, or every campus of the school for a whole-school item.</summary>
    public static async Task<IReadOnlyList<int>> CampusIdsAsync(
        IApplicationDbContext context,
        int schoolId,
        int? campusId,
        CancellationToken cancellationToken)
    {
        if (campusId is { } id)
        {
            return [id];
        }

        var all = await context.Campuses
            .Where(c => c.SchoolId == schoolId)
            .OrderBy(c => c.Id)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        return all.Count > 0
            ? all
            : throw CalendarRules.Invalid("CampusId", "Add a campus before saving a whole-school item.");
    }

    /// <summary>
    /// Gives a newly added campus a copy of every whole-school holiday, event and photo the school has, under the same key, so
    /// the campus shows what the school shows (ADR-027). The copies are added to the context; the caller saves them.
    /// </summary>
    public static async Task CopyToNewCampusAsync(IApplicationDbContext context, int schoolId, int campusId, CancellationToken cancellationToken)
    {
        var holidays = await context.Holidays
            .Where(h => h.SchoolId == schoolId && h.WholeSchoolKey != null)
            .OrderBy(h => h.Id)
            .ToListAsync(cancellationToken);
        foreach (var source in holidays.DistinctBy(h => h.WholeSchoolKey))
        {
            if (source.WholeSchoolKey is not { } key)
            {
                continue;
            }

            var copy = Holiday.Create(schoolId, campusId, source.Title, source.StartDate, source.EndDate, source.Type, source.AppliesTo, source.IsTentative);
            copy.JoinWholeSchool(key);
            context.Holidays.Add(copy);
        }

        var events = await context.CalendarEvents
            .Where(e => e.SchoolId == schoolId && e.WholeSchoolKey != null)
            .OrderBy(e => e.Id)
            .ToListAsync(cancellationToken);
        foreach (var source in events.DistinctBy(e => e.WholeSchoolKey))
        {
            if (source.WholeSchoolKey is not { } key)
            {
                continue;
            }

            var copy = CalendarEvent.Create(schoolId, campusId, source.Title, source.Description, source.EventDate, source.StartTime, source.EndTime, source.Type, source.Audience);
            copy.JoinWholeSchool(key);
            context.CalendarEvents.Add(copy);
        }

        var photos = await context.SchoolPhotos
            .Where(p => p.SchoolId == schoolId && p.WholeSchoolKey != null)
            .OrderBy(p => p.Id)
            .ToListAsync(cancellationToken);
        foreach (var source in photos.DistinctBy(p => p.WholeSchoolKey))
        {
            if (source.WholeSchoolKey is not { } key)
            {
                continue;
            }

            var copy = SchoolPhoto.Create(schoolId, campusId, source.Kind, source.Caption, source.SortOrder, source.Content);
            copy.JoinWholeSchool(key);
            context.SchoolPhotos.Add(copy);
        }
    }
}
