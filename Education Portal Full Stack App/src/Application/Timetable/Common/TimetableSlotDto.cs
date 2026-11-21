using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Timetable.Common;

/// <summary>A lesson of the timetable: its course, day, times, campus (<c>null</c> for the whole school) and room.</summary>
public sealed record TimetableSlotDto(Guid Id, Guid CourseId, string CourseName, Weekday Day, TimeOnly Start, TimeOnly End, Guid? CampusId, string? Room);
