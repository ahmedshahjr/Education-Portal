using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A day or run of days the school is closed, for the whole school or one campus (<see cref="CampusId"/>). Owned by the
/// school (SEC-AUTHZ-03). A tentative holiday is shown but may still move, for example one that depends on the moon.
/// </summary>
public class Holiday : BaseSoftDeletableEntity, ITenantOwned
{
    public const int TitleMaxLength = 200;

    /// <summary>The longest holiday: about four months, which covers a long summer vacation.</summary>
    public const int MaxLengthInDays = 120;

    private Holiday()
    {
        // For EF Core.
        Title = string.Empty;
    }

    public static Holiday Create(
        int schoolId,
        int campusId,
        string title,
        DateOnly startDate,
        DateOnly endDate,
        HolidayType type,
        HolidayAudience appliesTo,
        bool isTentative)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);

        var holiday = new Holiday { SchoolId = schoolId };
        holiday.Change(campusId, title, startDate, endDate, type, appliesTo, isTentative);
        return holiday;
    }

    public int SchoolId { get; private set; }

    /// <summary>The campus the holiday is for, or <c>null</c> for every campus of the school.</summary>
    public int CampusId { get; private set; }

    /// <summary>
    /// The key that ties the copies of one whole-school item together, one copy per campus. It is <c>null</c> for an item
    /// that belongs to one campus. Changing or deleting a copy changes or deletes every copy with the same key, and a
    /// campus added later gets a copy of each key the school has (ADR-027).
    /// </summary>
    public Guid? WholeSchoolKey { get; private set; }

    /// <summary>Marks this row as one campus's copy of a whole-school item.</summary>
    public void JoinWholeSchool(Guid key)
    {
        if (key == Guid.Empty)
        {
            throw new ArgumentException("A whole-school key is not empty.", nameof(key));
        }

        WholeSchoolKey = key;
    }

    public string Title { get; private set; }

    public DateOnly StartDate { get; private set; }

    /// <summary>The last day of the holiday; the same as <see cref="StartDate"/> for a one-day holiday.</summary>
    public DateOnly EndDate { get; private set; }

    public HolidayType Type { get; private set; }

    public HolidayAudience AppliesTo { get; private set; }

    public bool IsTentative { get; private set; }

    public void Change(
        int campusId,
        string title,
        DateOnly startDate,
        DateOnly endDate,
        HolidayType type,
        HolidayAudience appliesTo,
        bool isTentative)
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("A deleted holiday cannot be changed.");
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(campusId);

        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(title.Trim().Length, TitleMaxLength, nameof(title));
        if (endDate < startDate)
        {
            throw new ArgumentException("The end date must be on or after the start date.", nameof(endDate));
        }

        if (endDate.DayNumber - startDate.DayNumber >= MaxLengthInDays)
        {
            throw new ArgumentException("A holiday runs at most 120 days.", nameof(endDate));
        }

        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown holiday type.");
        }

        if (!Enum.IsDefined(appliesTo))
        {
            throw new ArgumentOutOfRangeException(nameof(appliesTo), appliesTo, "Unknown holiday audience.");
        }

        CampusId = campusId;
        Title = title.Trim();
        StartDate = startDate;
        EndDate = endDate;
        Type = type;
        AppliesTo = appliesTo;
        IsTentative = isTentative;
    }
}
