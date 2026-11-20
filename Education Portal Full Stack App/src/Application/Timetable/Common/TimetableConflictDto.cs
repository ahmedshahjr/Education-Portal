using EducationPortal.Domain.Academics;

namespace EducationPortal.Application.Timetable.Common;

/// <summary>A lesson that clashes with the one being saved, by its public id, and on what (teacher, room or class).</summary>
public sealed record TimetableConflictDto(Guid SlotId, TimetableConflictKind Kind);
