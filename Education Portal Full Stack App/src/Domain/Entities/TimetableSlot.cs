using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A lesson of a course on a day, at a time, in a room of a campus (the table <c>TimetableSlots</c>). Clashes are refused
/// before a slot is saved (<c>TimetableConflicts</c>).
/// </summary>
public class TimetableSlot : BaseAuditableEntity, ITenantOwned
{
    public const int RoomMaxLength = 50;

    private TimetableSlot()
    {
        // For EF Core.
    }

    private TimetableSlot(int schoolId, int courseId, int? campusId, Weekday day, TimeOnly start, TimeOnly end, string? room)
    {
        SchoolId = schoolId;
        CourseId = courseId;
        CampusId = campusId;
        Day = day;
        Start = start;
        End = end;
        Room = room;
    }

    public static TimetableSlot Create(int schoolId, int courseId, int? campusId, Weekday day, TimeOnly start, TimeOnly end, string? room)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(courseId);
        if (campusId is { } campus)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(campus);
        }

        if (end <= start)
        {
            throw new ArgumentException("A lesson ends after it starts.", nameof(end));
        }

        var trimmed = string.IsNullOrWhiteSpace(room) ? null : room.Trim();
        if (trimmed is not null)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, RoomMaxLength, nameof(room));
        }

        return new TimetableSlot(schoolId, courseId, campusId, day, start, end, trimmed);
    }

    public int SchoolId { get; private set; }

    public int CourseId { get; private set; }

    public int? CampusId { get; private set; }

    public Weekday Day { get; private set; }

    public TimeOnly Start { get; private set; }

    public TimeOnly End { get; private set; }

    public string? Room { get; private set; }

    /// <summary>Moves the lesson to another day, time, room or campus. The same rules as <see cref="Create"/> apply.</summary>
    public void Reschedule(int? campusId, Weekday day, TimeOnly start, TimeOnly end, string? room)
    {
        if (campusId is { } campus)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(campus);
        }

        if (end <= start)
        {
            throw new ArgumentException("A lesson ends after it starts.", nameof(end));
        }

        var trimmed = string.IsNullOrWhiteSpace(room) ? null : room.Trim();
        if (trimmed is not null)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, RoomMaxLength, nameof(room));
        }

        CampusId = campusId;
        Day = day;
        Start = start;
        End = end;
        Room = trimmed;
    }
}
