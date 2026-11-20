using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.AcademicCalendar.Common;

/// <summary>The rules a term must keep against its academic year and the year's other terms.</summary>
public static class TermChecks
{
    /// <summary>
    /// Throws a validation error when the year is closed, the dates fall outside the year, the number is taken or the dates
    /// overlap another term of the year.
    /// </summary>
    public static async Task EnsureFitsAsync(
        IApplicationDbContext context,
        AcademicYear year,
        int? exceptTermId,
        int sequence,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        AcademicYearChecks.EnsureOpen(year);

        if (!year.Contains(startDate) || !year.Contains(endDate))
        {
            throw CalendarRules.Invalid("StartDate", ValidationMessages.TermOutsideYear);
        }

        var others = context.Terms.Where(t => t.AcademicYearId == year.Id && t.Id != exceptTermId);

        if (await others.AnyAsync(t => t.Sequence == sequence, cancellationToken))
        {
            throw CalendarRules.Invalid("Sequence", ValidationMessages.TermSequenceTaken);
        }

        if (await others.AnyAsync(t => t.StartDate <= endDate && startDate <= t.EndDate, cancellationToken))
        {
            throw CalendarRules.Invalid("StartDate", ValidationMessages.TermOverlaps);
        }
    }
}
