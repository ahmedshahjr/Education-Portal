using EducationPortal.Domain.Common;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A person of a school works at a campus (the table <c>UserCampus</c>). A person can belong to several campuses. Owned by
/// the school (<see cref="SchoolId"/>) and covered by the tenant query filter (SEC-AUTHZ-03). A campus-scoped list filters
/// its rows by these links.
/// </summary>
public class UserCampus : BaseAuditableEntity, ITenantOwned
{
    private UserCampus()
    {
        // For EF Core.
    }

    private UserCampus(int schoolId, int userDetailsId, int campusId)
    {
        SchoolId = schoolId;
        UserDetailsId = userDetailsId;
        CampusId = campusId;
    }

    /// <summary>Links a person to a campus of the same school. Every id must be a saved row (positive).</summary>
    public static UserCampus Create(int schoolId, int userDetailsId, int campusId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userDetailsId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(campusId);

        return new UserCampus(schoolId, userDetailsId, campusId);
    }

    public int SchoolId { get; private set; }

    public int UserDetailsId { get; private set; }

    public int CampusId { get; private set; }
}
