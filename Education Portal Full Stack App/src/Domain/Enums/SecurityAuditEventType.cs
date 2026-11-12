namespace EducationPortal.Domain.Enums;

/// <summary>
/// Security-relevant actions that are written to the security audit log (SEC-LOG-03).
/// Stored as its number (ENG-ENUM-02), so append new members with the next number and never change or reuse a number.
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

    /// <summary>The master tenant created or changed a country's languages, time zone or currency.</summary>
    CountryChanged = 8,

    /// <summary>The master tenant assigned a school to another country (or to none).</summary>
    SchoolCountryChanged = 9,

    /// <summary>No longer written: modules now have a type and an active switch (<see cref="ModuleChanged"/>). Kept so older records still read.</summary>
    ModuleDefaultChanged = 10,

    /// <summary>The master tenant switched a module on or off for one school.</summary>
    SchoolModuleChanged = 11,

    /// <summary>The master tenant changed a module's type (default or custom) or switched it on or off for every school.</summary>
    ModuleChanged = 12,
}
