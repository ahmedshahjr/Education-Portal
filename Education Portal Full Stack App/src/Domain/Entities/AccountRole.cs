using EducationPortal.Domain.Common;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A school role given to a login account (the table <c>AccountRoles</c>). An account can hold several roles; the rights of
/// the roles are the rights of the account in each module (ADR-012). Owned by the school and covered by the tenant query
/// filter (SEC-AUTHZ-03).
/// </summary>
public class AccountRole : BaseAuditableEntity, ITenantOwned
{
    /// <summary>The length of an Identity user id.</summary>
    public const int UserIdMaxLength = 450;

    private AccountRole()
    {
        // For EF Core.
    }

    private AccountRole(int schoolId, string userId, int schoolRoleId)
    {
        SchoolId = schoolId;
        UserId = userId;
        SchoolRoleId = schoolRoleId;
    }

    /// <summary>Gives a role to an account of the same school.</summary>
    public static AccountRole Create(int schoolId, string userId, int schoolRoleId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolRoleId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(userId.Length, UserIdMaxLength, nameof(userId));

        return new AccountRole(schoolId, userId, schoolRoleId);
    }

    public int SchoolId { get; private set; }

    /// <summary>The Identity user id of the account (the account lives in the identity schema).</summary>
    public string UserId { get; private set; } = string.Empty;

    public int SchoolRoleId { get; private set; }
}
