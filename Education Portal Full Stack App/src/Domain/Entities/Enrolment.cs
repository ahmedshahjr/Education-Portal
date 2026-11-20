using EducationPortal.Domain.Common;

namespace EducationPortal.Domain.Entities;

/// <summary>A student of the school in a class offering (the table <c>Enrolments</c>). A student is enrolled once per class.</summary>
public class Enrolment : BaseAuditableEntity, ITenantOwned
{
    private Enrolment()
    {
        // For EF Core.
    }

    private Enrolment(int schoolId, int studentUserDetailsId, int classOfferingId)
    {
        SchoolId = schoolId;
        StudentUserDetailsId = studentUserDetailsId;
        ClassOfferingId = classOfferingId;
    }

    public static Enrolment Create(int schoolId, int studentUserDetailsId, int classOfferingId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(studentUserDetailsId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(classOfferingId);

        return new Enrolment(schoolId, studentUserDetailsId, classOfferingId);
    }

    public int SchoolId { get; private set; }

    public int StudentUserDetailsId { get; private set; }

    public int ClassOfferingId { get; private set; }
}
