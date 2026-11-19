namespace EducationPortal.Domain.Academics;

/// <summary>One band of a grading scale: the percents it covers (both ends included), its letter and its grade points.</summary>
public readonly record struct GradeBand(decimal MinPercent, decimal MaxPercent, string Letter, decimal Points);

/// <summary>
/// The grading rules of a school (pure, no storage): a scale of bands that covers 0 to 100 exactly once, the percent of a
/// course from its weighted assessments, and a GPA from grade points. Percents and GPA are rounded to 2 decimals, half up.
/// </summary>
public static class GradeScale
{
    public const decimal MinPercent = 0m;
    public const decimal MaxPercent = 100m;
    public const int LetterMaxLength = 5;

    /// <summary>The smallest step between two percents (two decimals).</summary>
    private const decimal Step = 0.01m;

    /// <summary>
    /// Checks a scale: at least one band, each band's ends in 0 to 100 with the minimum not above the maximum, a letter and
    /// non-negative points, and the bands in order with no overlap and no gap, starting at 0 and ending at 100.
    /// </summary>
    public static void Validate(IReadOnlyList<GradeBand> bands)
    {
        ArgumentNullException.ThrowIfNull(bands);
        if (bands.Count == 0)
        {
            throw new ArgumentException("A grading scale needs at least one band.", nameof(bands));
        }

        var ordered = bands.OrderBy(b => b.MinPercent).ToList();
        for (var i = 0; i < ordered.Count; i++)
        {
            var band = ordered[i];
            if (band.MinPercent < MinPercent || band.MaxPercent > MaxPercent || band.MinPercent > band.MaxPercent)
            {
                throw new ArgumentException("A band must lie within 0 to 100, with its minimum not above its maximum.", nameof(bands));
            }

            if (string.IsNullOrWhiteSpace(band.Letter) || band.Letter.Length > LetterMaxLength)
            {
                throw new ArgumentException("Every band needs a letter of up to 5 characters.", nameof(bands));
            }

            if (band.Points < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bands), band.Points, "Grade points are zero or more.");
            }
        }

        if (ordered[0].MinPercent != MinPercent || ordered[^1].MaxPercent != MaxPercent)
        {
            throw new ArgumentException("The bands must start at 0 and end at 100.", nameof(bands));
        }

        for (var i = 1; i < ordered.Count; i++)
        {
            var gap = ordered[i].MinPercent - ordered[i - 1].MaxPercent;
            if (gap != Step)
            {
                // Zero or less overlaps the previous band; more than one step leaves a gap.
                throw new ArgumentException("The bands must not overlap and must not leave a gap.", nameof(bands));
            }
        }
    }

    /// <summary>The band that holds the percent. A percent outside 0 to 100 is refused.</summary>
    public static GradeBand BandFor(IReadOnlyList<GradeBand> bands, decimal percent)
    {
        ArgumentNullException.ThrowIfNull(bands);
        if (percent < MinPercent || percent > MaxPercent)
        {
            throw new ArgumentOutOfRangeException(nameof(percent), percent, "A percent is from 0 to 100.");
        }

        var rounded = Round(percent);
        return bands.Single(b => rounded >= b.MinPercent && rounded <= b.MaxPercent);
    }

    /// <summary>
    /// The percent of a course: the weighted average of its scores (score / maximum, times the weight), rounded to 2
    /// decimals. Null when there are no assessments or their weights add up to zero (the course has no percent yet).
    /// </summary>
    public static decimal? CoursePercent(IReadOnlyCollection<AssessmentScore> scores)
    {
        ArgumentNullException.ThrowIfNull(scores);
        var totalWeight = scores.Sum(s => s.Weight);
        if (scores.Count == 0 || totalWeight == 0)
        {
            return null;
        }

        var weighted = 0m;
        foreach (var score in scores)
        {
            if (score.MaximumScore <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scores), score.MaximumScore, "A maximum score is above zero.");
            }

            if (score.Value < 0 || score.Value > score.MaximumScore)
            {
                throw new ArgumentOutOfRangeException(nameof(scores), score.Value, "A score is from 0 to its maximum.");
            }

            weighted += score.Value / score.MaximumScore * score.Weight;
        }

        return Round(weighted / totalWeight * 100m);
    }

    /// <summary>The GPA: the mean of the graded courses' grade points, rounded to 2 decimals. Null when no course is graded.</summary>
    public static decimal? Gpa(IReadOnlyCollection<decimal> gradePoints)
    {
        ArgumentNullException.ThrowIfNull(gradePoints);
        if (gradePoints.Count == 0)
        {
            return null;
        }

        return Round(gradePoints.Average());
    }

    /// <summary>Rounds to 2 decimals, half away from zero (the usual school rule: 79.555 is 79.56).</summary>
    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}

/// <summary>One assessment of a student in a course: the score, the most the assessment gives, and its weight (0 to 100).</summary>
public readonly record struct AssessmentScore(decimal Value, decimal MaximumScore, decimal Weight);
