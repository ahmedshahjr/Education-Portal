using EducationPortal.Domain.Common;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// One graded piece of a course (a test, an assignment): its weight in the course percent (0 to 100) and the most it gives
/// (the table <c>Assessments</c>). A course has several (ADR-019).
/// </summary>
public class Assessment : BaseAuditableEntity, ITenantOwned
{
    public const int NameMaxLength = 100;

    private Assessment()
    {
        // For EF Core.
    }

    private Assessment(int schoolId, int courseId, string name, decimal weight, decimal maximumScore)
    {
        SchoolId = schoolId;
        CourseId = courseId;
        Name = name;
        Weight = weight;
        MaximumScore = maximumScore;
    }

    public static Assessment Create(int schoolId, int courseId, string name, decimal weight, decimal maximumScore)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(courseId);
        ArgumentOutOfRangeException.ThrowIfNegative(weight);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(weight, 100m);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumScore);

        var trimmed = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(trimmed, nameof(name));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, NameMaxLength, nameof(name));

        return new Assessment(schoolId, courseId, trimmed, weight, maximumScore);
    }

    public int SchoolId { get; private set; }

    public int CourseId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public decimal Weight { get; private set; }

    public decimal MaximumScore { get; private set; }
}
