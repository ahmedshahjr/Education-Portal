namespace EducationPortal.Application.Academics.Common;

/// <summary>A course: its class offering, its teacher and its name, by public id.</summary>
public sealed record CourseDto(Guid Id, string Name, Guid ClassOfferingId, Guid TeacherId);

/// <summary>An assessment of a course: its name, its weight in percent and its maximum score.</summary>
public sealed record AssessmentDto(Guid Id, string Name, decimal Weight, decimal MaximumScore);
