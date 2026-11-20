using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A school's academic year, for example <c>2026-27</c>, from its first to its last day. Owned by the school and covered by
/// the tenant query filter (SEC-AUTHZ-03). A school has at most one current year; the use case clears the old one when it
/// makes another current, and a unique index backs it up. Years of a school do not overlap; the use case checks that, as it
/// has the other years. A closed year cannot be changed or made current.
/// </summary>
public class AcademicYear : BaseSoftDeletableEntity, ITenantOwned
{
    public const int NameMaxLength = 20;

    /// <summary>The longest an academic year can run: two years, which covers any school calendar.</summary>
    public const int MaxLengthInDays = 731;

    private AcademicYear()
    {
        // For EF Core.
        Name = string.Empty;
    }

    public static AcademicYear Create(int schoolId, string name, DateOnly startDate, DateOnly endDate)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);

        var year = new AcademicYear { SchoolId = schoolId, Status = AcademicYearStatus.Planned };
        year.Change(name, startDate, endDate);
        return year;
    }

    public int SchoolId { get; private set; }

    public string Name { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    /// <summary>True for the year the school works in now; screens open on it.</summary>
    public bool IsCurrent { get; private set; }

    public AcademicYearStatus Status { get; private set; }

    /// <summary>True when the two date ranges share at least one day.</summary>
    public bool Overlaps(DateOnly startDate, DateOnly endDate) => StartDate <= endDate && startDate <= EndDate;

    /// <summary>True when the day is inside the year.</summary>
    public bool Contains(DateOnly date) => StartDate <= date && date <= EndDate;

    public void Change(string name, DateOnly startDate, DateOnly endDate)
    {
        EnsureOpen();
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));
        EnsureValidRange(startDate, endDate);

        Name = name.Trim();
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>Moves the year to another status. Closing it also stops it being the current year.</summary>
    public void SetStatus(AcademicYearStatus status)
    {
        EnsureNotDeleted();
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown academic year status.");
        }

        Status = status;
        if (status == AcademicYearStatus.Closed)
        {
            IsCurrent = false;
        }
    }

    /// <summary>Makes this the school's current year. The use case clears the flag on the school's other years in the same save.</summary>
    public void MakeCurrent()
    {
        EnsureOpen();
        IsCurrent = true;
    }

    public void ClearCurrent() => IsCurrent = false;

    /// <summary>The start must be on or before the end, and a year runs at most <see cref="MaxLengthInDays"/> days.</summary>
    public static void EnsureValidRange(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("The end date must be on or after the start date.", nameof(endDate));
        }

        if (endDate.DayNumber - startDate.DayNumber >= MaxLengthInDays)
        {
            throw new ArgumentException("An academic year runs at most two years.", nameof(endDate));
        }
    }

    private void EnsureOpen()
    {
        EnsureNotDeleted();
        if (Status == AcademicYearStatus.Closed)
        {
            throw new InvalidOperationException("A closed academic year cannot be changed.");
        }
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("A deleted academic year cannot be changed.");
        }
    }
}
