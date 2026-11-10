namespace EducationPortal.Application.Common.Security;

/// <summary>Claims that tie a session to its school, so it cannot be used on another school's domain.</summary>
public static class TenantClaimTypes
{
    public const string SchoolId = "education_portal/school_id";
}
