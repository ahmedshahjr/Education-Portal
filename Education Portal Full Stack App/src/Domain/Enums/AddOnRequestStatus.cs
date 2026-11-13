namespace EducationPortal.Domain.Enums;

/// <summary>
/// Where a school's request for an add-on module stands. Stored as its number and sent over the API by name
/// (ENG-ENUM-01..03): never change or reuse a number, and never rename a member.
/// </summary>
public enum AddOnRequestStatus
{
    /// <summary>Waiting for the master tenant to decide.</summary>
    Pending = 1,

    /// <summary>The master tenant added the module to the school's subscription.</summary>
    Approved = 2,

    /// <summary>The master tenant turned the request down.</summary>
    Rejected = 3,
}
