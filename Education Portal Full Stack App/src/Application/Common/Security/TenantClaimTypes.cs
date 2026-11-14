namespace EducationPortal.Application.Common.Security;

/// <summary>Claims that tie a session to its school, so it cannot be used on another school's domain.</summary>
public static class TenantClaimTypes
{
    public const string SchoolId = "education_portal/school_id";

    /// <summary>
    /// The user's saved language (ISO 639-1), when they chose one. Named like the OpenID Connect <c>locale</c> claim.
    /// </summary>
    public const string Language = "locale";

    /// <summary>Present (with the value <c>true</c>) while the account must replace its one-time password.</summary>
    public const string MustChangePassword = "education_portal/must_change_password";
}
