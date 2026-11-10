namespace EducationPortal.Domain.Enums;

/// <summary>
/// Security-relevant actions that are written to the security audit log (SEC-LOG-03).
/// Stored by name, so append new values and never rename existing ones.
/// </summary>
public enum SecurityAuditEventType
{
    SignInSucceeded = 1,
    SignInFailed = 2,
    SignInLockedOut = 3,
    SignedOut = 4,
    AccessDenied = 5,

    /// <summary>A sign-in or session was used on another school's domain than the user's own.</summary>
    TenantMismatch = 6,

    /// <summary>A school's branding (theme, sidebar colour, logo or favicon) was changed.</summary>
    BrandingChanged = 7,
}
