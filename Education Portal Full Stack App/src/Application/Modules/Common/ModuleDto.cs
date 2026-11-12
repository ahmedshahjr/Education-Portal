namespace EducationPortal.Application.Modules.Common;

/// <summary>A module in the master portal's catalog.</summary>
/// <param name="Type">Default (on for every school unless switched off for one) or custom (off unless switched on).</param>
/// <param name="IsActive">False when the master switched the module off for every school.</param>
public sealed record ModuleDto(string Key, string Name, string Description, ModuleType Type, bool IsActive);

/// <summary>A module as it stands for one school.</summary>
/// <param name="IsEnabled">Whether the school's users can use the module.</param>
/// <param name="FollowsDefault">True when the master made no choice for this school, so the module's default applies.</param>
/// <param name="IsActive">False when the master switched the module off for every school; then it is off here too.</param>
public sealed record SchoolModuleDto(string Key, string Name, string Description, ModuleType Type, bool IsActive, bool IsEnabled, bool FollowsDefault);
