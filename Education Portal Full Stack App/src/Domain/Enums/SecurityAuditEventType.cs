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

    /// <summary>The master tenant changed a module (name, description, core, sort order) or switched it on or off for every school.</summary>
    ModuleChanged = 12,

    /// <summary>The master tenant added a module to the catalogue.</summary>
    ModuleCreated = 13,

    /// <summary>The master tenant deleted a module from the catalogue (soft delete).</summary>
    ModuleDeleted = 14,

    /// <summary>The master tenant made one module require another.</summary>
    ModuleDependencyAdded = 15,

    /// <summary>The master tenant removed a dependency between two modules.</summary>
    ModuleDependencyRemoved = 16,

    /// <summary>The master tenant added a plan.</summary>
    PlanCreated = 17,

    /// <summary>The master tenant changed a plan (name, description, active, or its modules).</summary>
    PlanChanged = 18,

    /// <summary>The master tenant deleted a plan (soft delete).</summary>
    PlanDeleted = 19,

    /// <summary>The master tenant added a price book.</summary>
    PriceBookCreated = 20,

    /// <summary>The master tenant changed a price book's currency or active switch.</summary>
    PriceBookChanged = 21,

    /// <summary>The master tenant deleted a price book (soft delete).</summary>
    PriceBookDeleted = 22,

    /// <summary>The master tenant added, changed or removed a price in a price book.</summary>
    PriceChanged = 23,

    /// <summary>The master tenant started a subscription for a school.</summary>
    SubscriptionCreated = 24,

    /// <summary>The master tenant set a school's data region.</summary>
    DataRegionChanged = 25,

    /// <summary>A school owner added a role.</summary>
    RoleCreated = 26,

    /// <summary>A school owner changed a role's name or rights.</summary>
    RoleChanged = 27,

    /// <summary>A school owner deleted a role.</summary>
    RoleDeleted = 28,

    /// <summary>The master tenant issued an invoice for a school's subscription.</summary>
    InvoiceCreated = 29,

    /// <summary>The master tenant voided an open invoice.</summary>
    InvoiceVoided = 30,

    /// <summary>The master tenant recorded a payment against an invoice.</summary>
    PaymentRecorded = 31,

    /// <summary>The master tenant set or cleared a school's own tax rate.</summary>
    SchoolTaxRateChanged = 32,

    /// <summary>The master tenant added a school (and its first administrator, audited as <see cref="UserCreated"/>).</summary>
    SchoolCreated = 33,

    /// <summary>An account was created (for example a new school's first administrator).</summary>
    UserCreated = 34,

    /// <summary>A user changed their password (including replacing a one-time password).</summary>
    PasswordChanged = 35,
}
