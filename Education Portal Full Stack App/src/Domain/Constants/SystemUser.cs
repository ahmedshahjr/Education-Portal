namespace EducationPortal.Domain.Constants;

/// <summary>
/// The system account. Rows written with no signed-in user (the development seed, background jobs) record it as their
/// creator and last modifier, so <c>CreatedBy</c> and <c>LastModifiedBy</c> are never null. The account has no password
/// and is locked, so nobody can sign in as it. It is seeded with the master tenant.
/// </summary>
public static class SystemUser
{
    /// <summary>The account id, stored in the audit fields.</summary>
    public const string Id = "system";

    /// <summary>The account's user name and e-mail.</summary>
    public const string UserName = "system@localhost";
}
