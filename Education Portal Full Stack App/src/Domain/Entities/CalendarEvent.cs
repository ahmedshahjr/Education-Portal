using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// An event on the school calendar, for example a parent-teacher meeting or sports day, for the whole school or one campus
/// (<see cref="CampusId"/>). Owned by the school (SEC-AUTHZ-03). The times are optional: an event without a start time
/// lasts the whole day; an end time needs a start time and comes after it.
/// </summary>
public class CalendarEvent : BaseSoftDeletableEntity, ITenantOwned
{
    public const int TitleMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    private CalendarEvent()
    {
        // For EF Core.
        Title = string.Empty;
    }

    public static CalendarEvent Create(
        int schoolId,
        int? campusId,
        string title,
        string? description,
        DateOnly eventDate,
        TimeOnly? startTime,
        TimeOnly? endTime,
        CalendarEventType type,
        CalendarEventAudience audience)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);

        var calendarEvent = new CalendarEvent { SchoolId = schoolId };
        calendarEvent.Change(campusId, title, description, eventDate, startTime, endTime, type, audience);
        return calendarEvent;
    }

    public int SchoolId { get; private set; }

    /// <summary>The campus the event is for, or <c>null</c> for every campus of the school.</summary>
    public int? CampusId { get; private set; }

    public string Title { get; private set; }

    public string? Description { get; private set; }

    public DateOnly EventDate { get; private set; }

    /// <summary>The start in the school's local time, or <c>null</c> for an all-day event.</summary>
    public TimeOnly? StartTime { get; private set; }

    public TimeOnly? EndTime { get; private set; }

    public CalendarEventType Type { get; private set; }

    public CalendarEventAudience Audience { get; private set; }

    public void Change(
        int? campusId,
        string title,
        string? description,
        DateOnly eventDate,
        TimeOnly? startTime,
        TimeOnly? endTime,
        CalendarEventType type,
        CalendarEventAudience audience)
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("A deleted event cannot be changed.");
        }

        if (campusId is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(campusId), campusId, "A campus id is positive.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(title.Trim().Length, TitleMaxLength, nameof(title));

        var normalizedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (normalizedDescription is not null)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(normalizedDescription.Length, DescriptionMaxLength, nameof(description));
        }

        if (endTime is not null && startTime is null)
        {
            throw new ArgumentException("An end time needs a start time.", nameof(endTime));
        }

        if (endTime is { } end && startTime is { } start && end <= start)
        {
            throw new ArgumentException("The end time must be after the start time.", nameof(endTime));
        }

        if (!Enum.IsDefined(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown event type.");
        }

        if (!Enum.IsDefined(audience))
        {
            throw new ArgumentOutOfRangeException(nameof(audience), audience, "Unknown event audience.");
        }

        CampusId = campusId;
        Title = title.Trim();
        Description = normalizedDescription;
        EventDate = eventDate;
        StartTime = startTime;
        EndTime = endTime;
        Type = type;
        Audience = audience;
    }
}
