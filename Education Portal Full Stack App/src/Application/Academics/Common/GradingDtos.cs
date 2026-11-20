namespace EducationPortal.Application.Academics.Common;

/// <summary>One band of a grading scale as the API sends it.</summary>
public sealed record GradeBandDto(decimal MinPercent, decimal MaxPercent, string Letter, decimal Points);

/// <summary>A grading scale with its bands, by public id.</summary>
public sealed record GradingScaleDto(Guid Id, string Name, IReadOnlyList<GradeBandDto> Bands);

/// <summary>One line of a report card, by course name.</summary>
public sealed record ReportCardLineDto(string CourseName, decimal? Percent, string? Letter, decimal? Points);

/// <summary>A report card: a line per course and the GPA (null when no course is graded).</summary>
public sealed record ReportCardDto(IReadOnlyList<ReportCardLineDto> Lines, decimal? Gpa);
