using EducationPortal.Application.Calendar.Common;

namespace EducationPortal.Application.AcademicCalendar.Common;

/// <summary>An academic year with its terms in order. Ids are public ids (ENG-ID-02).</summary>
public sealed record AcademicYearDto(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsCurrent,
    AcademicYearStatus Status,
    IReadOnlyList<TermDto> Terms);

public sealed record TermDto(Guid Id, string Name, int Sequence, DateOnly StartDate, DateOnly EndDate);

public sealed record ShiftDto(Guid Id, Guid CampusId, string Name, TimeOnly StartTime, TimeOnly EndTime);

/// <summary>A holiday. <see cref="CampusId"/> is <c>null</c> when it is for every campus.</summary>
public sealed record HolidayDto(
    Guid Id,
    Guid? CampusId,
    string Title,
    DateOnly StartDate,
    DateOnly EndDate,
    HolidayType Type,
    HolidayAudience AppliesTo,
    bool IsTentative);

/// <summary>An event. <see cref="CampusId"/> is <c>null</c> when it is for every campus; no start time means all day.</summary>
public sealed record CalendarEventDto(
    Guid Id,
    Guid? CampusId,
    string Title,
    string? Description,
    DateOnly EventDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    CalendarEventType Type,
    CalendarEventAudience Audience);

/// <summary>
/// The holidays, calendar events and school events (timed, with an audience, from the Calendar feature) of a date range.
/// Holidays and events are ordered by date; school events by their start. School events are for the whole school.
/// </summary>
public sealed record SchoolCalendarDto(
    IReadOnlyList<HolidayDto> Holidays,
    IReadOnlyList<CalendarEventDto> Events,
    IReadOnlyList<SchoolEventDto> SchoolEvents);
