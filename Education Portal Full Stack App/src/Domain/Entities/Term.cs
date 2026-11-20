using EducationPortal.Domain.Common;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A term of an academic year, for example <c>First term</c>. Owned by the school (SEC-AUTHZ-03). Its dates fall inside its
/// year, and terms of one year neither share a sequence number nor overlap; the use case checks both, as it has the year and
/// the other terms.
/// </summary>
public class Term : BaseSoftDeletableEntity, ITenantOwned
{
    public const int NameMaxLength = 50;
    public const int MaxSequence = 12;

    private Term()
    {
        // For EF Core.
        Name = string.Empty;
    }

    public static Term Create(int schoolId, int academicYearId, string name, int sequence, DateOnly startDate, DateOnly endDate)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(academicYearId);

        var term = new Term { SchoolId = schoolId, AcademicYearId = academicYearId };
        term.Change(name, sequence, startDate, endDate);
        return term;
    }

    public int SchoolId { get; private set; }

    public int AcademicYearId { get; private set; }

    public string Name { get; private set; }

    /// <summary>The term's place in its year, from 1.</summary>
    public int Sequence { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    /// <summary>True when the two date ranges share at least one day.</summary>
    public bool Overlaps(DateOnly startDate, DateOnly endDate) => StartDate <= endDate && startDate <= EndDate;

    public void Change(string name, int sequence, DateOnly startDate, DateOnly endDate)
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("A deleted term cannot be changed.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));
        ArgumentOutOfRangeException.ThrowIfLessThan(sequence, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(sequence, MaxSequence);
        if (endDate < startDate)
        {
            throw new ArgumentException("The end date must be on or after the start date.", nameof(endDate));
        }

        Name = name.Trim();
        Sequence = sequence;
        StartDate = startDate;
        EndDate = endDate;
    }
}
