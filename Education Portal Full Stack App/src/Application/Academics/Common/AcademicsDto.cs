namespace EducationPortal.Application.Academics.Common;

/// <summary>A course: its class offering, its teacher and its name, by public id.</summary>
public sealed record CourseDto(Guid Id, string Name, Guid ClassOfferingId, Guid TeacherId);
