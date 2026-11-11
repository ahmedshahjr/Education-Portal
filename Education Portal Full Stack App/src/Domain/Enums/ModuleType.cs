namespace EducationPortal.Domain.Enums;

/// <summary>
/// The two types of module (decided 2026-11-11). Stored by name, so append new values and never rename existing ones.
/// </summary>
public enum ModuleType
{
    /// <summary>On for every school automatically, new schools included. The master can still switch it off for one school.</summary>
    Default = 1,

    /// <summary>Off until the master tenant switches it on for a school.</summary>
    Custom = 2,
}
