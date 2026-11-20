using EducationPortal.Domain.Common;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A subject taught to a class offering by one teacher (the table <c>Courses</c>). The teacher is a person of the school
/// (<see cref="UserDetails"/> of type Teacher); only that teacher grades the course (SEC-AUTHZ-04). Owned by the school and
/// covered by the tenant query filter (SEC-AUTHZ-03).
/// </summary>
public class Course : BaseAuditableEntity, ITenantOwned
{
    public const int NameMaxLength = 100;

    private Course()
    {
        // For EF Core.
    }

    private Course(int schoolId, int classOfferingId, string name, int teacherUserDetailsId, int? campusId)
    {
        SchoolId = schoolId;
        ClassOfferingId = classOfferingId;
        Name = name;
        TeacherUserDetailsId = teacherUserDetailsId;
        CampusId = campusId;
    }

    public static Course Create(int schoolId, int classOfferingId, string name, int teacherUserDetailsId, int? campusId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(classOfferingId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(teacherUserDetailsId);
        if (campusId is { } campus)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(campus);
        }

        var trimmed = (name ?? string.Empty).Trim();
        ArgumentException.ThrowIfNullOrWhiteSpace(trimmed, nameof(name));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, NameMaxLength, nameof(name));

        return new Course(schoolId, classOfferingId, trimmed, teacherUserDetailsId, campusId);
    }

    public int SchoolId { get; private set; }

    public int ClassOfferingId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int TeacherUserDetailsId { get; private set; }

    public int? CampusId { get; private set; }
}
