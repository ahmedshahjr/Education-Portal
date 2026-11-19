using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Domain.Academics;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.Timetable.Common;

/// <summary>
/// The shared rules of the timetable use cases (ADR-019): the school's slots on the day are loaded, the candidate is checked
/// with <see cref="TimetableConflicts"/>, and any clash is reported by the public ids of the slots it clashes with.
/// </summary>
public static class TimetableRules
{
    public static DayOfWeek ToDayOfWeek(Weekday day) => day switch
    {
        Weekday.Monday => DayOfWeek.Monday,
        Weekday.Tuesday => DayOfWeek.Tuesday,
        Weekday.Wednesday => DayOfWeek.Wednesday,
        Weekday.Thursday => DayOfWeek.Thursday,
        Weekday.Friday => DayOfWeek.Friday,
        Weekday.Saturday => DayOfWeek.Saturday,
        Weekday.Sunday => DayOfWeek.Sunday,
        _ => throw new ArgumentOutOfRangeException(nameof(day), day, "Not a weekday."),
    };

    /// <summary>
    /// The clashes of a lesson with the school's other lessons on the same day. The teacher and class come from the course;
    /// <paramref name="slotId"/> is 0 for a new lesson, so the lesson is never a clash with its own saved version.
    /// </summary>
    public static async Task<IReadOnlyList<TimetableConflictDto>> FindConflictsAsync(
        IApplicationDbContext context,
        int slotId,
        int courseId,
        Weekday day,
        TimeOnly start,
        TimeOnly end,
        int? campusId,
        string? room,
        CancellationToken cancellationToken)
    {
        var course = await context.Courses.AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new { c.TeacherUserDetailsId, c.ClassOfferingId })
            .FirstAsync(cancellationToken);

        var dayOfWeek = ToDayOfWeek(day);

        var others = await (
            from slot in context.TimetableSlots.AsNoTracking()
            join other in context.Courses.AsNoTracking() on slot.CourseId equals other.Id
            where slot.Day == day
            select new
            {
                slot.Id,
                slot.PublicId,
                slot.Start,
                slot.End,
                slot.CampusId,
                slot.Room,
                other.TeacherUserDetailsId,
                other.ClassOfferingId,
            })
            .ToListAsync(cancellationToken);

        var publicIds = others.ToDictionary(o => o.Id, o => o.PublicId);
        var existing = others.Select(o => new TimetableSlotSpec(
            o.Id, dayOfWeek, o.Start, o.End, o.TeacherUserDetailsId, o.CampusId, o.Room, o.ClassOfferingId)).ToList();

        var candidate = new TimetableSlotSpec(slotId, dayOfWeek, start, end, course.TeacherUserDetailsId, campusId, room, course.ClassOfferingId);

        return TimetableConflicts.Find(existing, candidate)
            .Select(c => new TimetableConflictDto(publicIds[c.SlotId], c.Kind))
            .ToList();
    }

    /// <summary>The validation error that refuses a lesson which clashes, naming each lesson it clashes with.</summary>
    public static EducationPortal.Application.Common.Exceptions.ValidationException ToValidationException(IReadOnlyList<TimetableConflictDto> conflicts)
    {
        var failures = conflicts.Select(c => new ValidationFailure(
            "Start",
            string.Format(System.Globalization.CultureInfo.CurrentCulture, MessageFor(c.Kind), c.SlotId))).ToList();

        return new EducationPortal.Application.Common.Exceptions.ValidationException(failures);
    }

    private static string MessageFor(TimetableConflictKind kind) => kind switch
    {
        TimetableConflictKind.Teacher => ValidationMessages.TimetableTeacherClash,
        TimetableConflictKind.Room => ValidationMessages.TimetableRoomClash,
        _ => ValidationMessages.TimetableClassClash,
    };
}
