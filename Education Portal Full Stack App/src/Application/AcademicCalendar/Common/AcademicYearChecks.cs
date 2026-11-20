using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.AcademicCalendar.Common;

/// <summary>The rules an academic year must keep against the school's other years and its own terms.</summary>
public static class AcademicYearChecks
{
    /// <summary>Throws a validation error when the name is taken or the dates overlap another year of the school.</summary>
    public static async Task EnsureFitsAsync(
        IApplicationDbContext context,
        int schoolId,
        int? exceptYearId,
        string name,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        var others = context.AcademicYears.Where(y => y.SchoolId == schoolId && y.Id != exceptYearId);
        var trimmed = name.Trim();

        if (await others.AnyAsync(y => y.Name == trimmed, cancellationToken))
        {
            throw CalendarRules.Invalid("Name", ValidationMessages.AcademicYearNameTaken);
        }

        if (await others.AnyAsync(y => y.StartDate <= endDate && startDate <= y.EndDate, cancellationToken))
        {
            throw CalendarRules.Invalid("StartDate", ValidationMessages.AcademicYearOverlaps);
        }
    }

    /// <summary>Throws a validation error when a closed year is to be changed.</summary>
    public static void EnsureOpen(AcademicYear year)
    {
        if (year.Status == AcademicYearStatus.Closed)
        {
            throw CalendarRules.Invalid("Status", ValidationMessages.AcademicYearClosed);
        }
    }

    /// <summary>
    /// Makes the year current. The school's other current year is cleared and saved first, so the unique index on the current
    /// year (checked row by row) never sees two. Call it inside a transaction, so both saves stand or fall together.
    /// </summary>
    public static async Task MakeCurrentAsync(IApplicationDbContext context, AcademicYear year, CancellationToken cancellationToken)
    {
        var previous = await context.AcademicYears
            .Where(y => y.SchoolId == year.SchoolId && y.IsCurrent && y.Id != year.Id)
            .ToListAsync(cancellationToken);

        foreach (var other in previous)
        {
            other.ClearCurrent();
        }

        await context.SaveChangesAsync(cancellationToken);

        year.MakeCurrent();
        await context.SaveChangesAsync(cancellationToken);
    }
}
