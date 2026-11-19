using EducationPortal.Domain.Common;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A parent of a student of the same school (the table <c>ParentStudents</c>). One parent can have several students and a
/// student several parents. Both are people of the school (<see cref="UserDetails"/>). The link is owned by the school
/// (<see cref="SchoolId"/>) and covered by the tenant query filter (SEC-AUTHZ-03). A parent sees only the students linked to
/// them (SEC-AUTHZ-04).
/// </summary>
public class ParentStudent : BaseAuditableEntity, ITenantOwned
{
    private ParentStudent()
    {
        // For EF Core.
    }

    private ParentStudent(int schoolId, int parentUserDetailsId, int studentUserDetailsId)
    {
        SchoolId = schoolId;
        ParentUserDetailsId = parentUserDetailsId;
        StudentUserDetailsId = studentUserDetailsId;
    }

    /// <summary>Links a parent to a student. Every id must be a saved row (positive) and a person cannot be their own parent.</summary>
    public static ParentStudent Create(int schoolId, int parentUserDetailsId, int studentUserDetailsId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(parentUserDetailsId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(studentUserDetailsId);

        if (parentUserDetailsId == studentUserDetailsId)
        {
            throw new ArgumentException("A person cannot be the parent of themselves.", nameof(studentUserDetailsId));
        }

        return new ParentStudent(schoolId, parentUserDetailsId, studentUserDetailsId);
    }

    public int SchoolId { get; private set; }

    public int ParentUserDetailsId { get; private set; }

    public int StudentUserDetailsId { get; private set; }
}
