namespace EducationPortal.Domain.Enums;

/// <summary>
/// The two types of module (decided 2026-11-11). Stored as its number and sent over the API by name (ENG-ENUM-01..03):
/// never change or reuse a number, and never rename a member, because API clients send and receive the name.
/// </summary>
public enum ModuleType
{
    /// <summary>On for every school automatically, new schools included. The master can still switch it off for one school.</summary>
    Default = 1,

    /// <summary>Off until the master tenant switches it on for a school.</summary>
    Custom = 2,
}
