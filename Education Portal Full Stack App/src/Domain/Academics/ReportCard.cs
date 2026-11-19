namespace EducationPortal.Domain.Academics;

/// <summary>One course of a report card: its name and the scores the student has in its assessments (only the recorded ones).</summary>
public readonly record struct CourseResult(string CourseName, IReadOnlyCollection<AssessmentScore> Scores);

/// <summary>One line of a report card: the course percent, its band letter and grade points. Null when the course is not graded yet.</summary>
public readonly record struct ReportCardLine(string CourseName, decimal? Percent, string? Letter, decimal? Points);

/// <summary>The report card of one student in one class: a line per course and the GPA over the graded courses (ADR-019).</summary>
public sealed record ReportCard(IReadOnlyList<ReportCardLine> Lines, decimal? Gpa)
{
    /// <summary>Builds the card from each course's scores and the school's grading bands (already validated by <see cref="GradeScale"/>).</summary>
    public static ReportCard Build(IReadOnlyList<CourseResult> courses, IReadOnlyList<GradeBand> bands)
    {
        ArgumentNullException.ThrowIfNull(courses);
        ArgumentNullException.ThrowIfNull(bands);

        var lines = new List<ReportCardLine>(courses.Count);
        foreach (var course in courses)
        {
            var percent = GradeScale.CoursePercent(course.Scores.ToList());
            if (percent is not { } value)
            {
                lines.Add(new ReportCardLine(course.CourseName, null, null, null));
                continue;
            }

            var band = GradeScale.BandFor(bands, value);
            lines.Add(new ReportCardLine(course.CourseName, value, band.Letter, band.Points));
        }

        var points = lines.Where(l => l.Points is not null).Select(l => l.Points!.Value).ToList();

        return new ReportCard(lines, GradeScale.Gpa(points));
    }
}
