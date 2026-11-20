using EducationPortal.Domain.Common;

namespace EducationPortal.Domain.Entities;

/// <summary>A student's score in one assessment (the table <c>Scores</c>): from 0 to the assessment's maximum, one per pair.</summary>
public class Score : BaseAuditableEntity, ITenantOwned
{
    private Score()
    {
        // For EF Core.
    }

    private Score(int schoolId, int assessmentId, int enrolmentId, decimal value)
    {
        SchoolId = schoolId;
        AssessmentId = assessmentId;
        EnrolmentId = enrolmentId;
        Value = value;
    }

    public static Score Record(int schoolId, int assessmentId, int enrolmentId, decimal value, decimal maximumScore)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(assessmentId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(enrolmentId);
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, maximumScore);

        return new Score(schoolId, assessmentId, enrolmentId, value);
    }

    /// <summary>Changes the score. The same bounds apply as when it is recorded.</summary>
    public void Change(decimal value, decimal maximumScore)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, maximumScore);

        Value = value;
    }

    public int SchoolId { get; private set; }

    public int AssessmentId { get; private set; }

    public int EnrolmentId { get; private set; }

    public decimal Value { get; private set; }
}
