namespace EducationPortal.Domain.Enums;

/// <summary>
/// Whether a school's module is usable. Stored as its number and sent over the API by name (ENG-ENUM-01..03): never
/// change or reuse a number, and never rename a member.
/// </summary>
public enum TenantModuleStatus
{
    Active = 1,

    /// <summary>Switched off because the subscription is suspended. The module's data is kept.</summary>
    Suspended = 2,

    /// <summary>Past <c>ValidTo</c> and not renewed. The module's data is kept.</summary>
    Expired = 3,
}
