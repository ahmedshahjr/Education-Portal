namespace EducationPortal.Application.Modules.Common;

/// <summary>A module in the master portal's catalog.</summary>
/// <param name="IsDefault">On for every school unless the master switched it off for that school.</param>
public sealed record ModuleDto(string Key, string Name, string Description, bool IsDefault);

/// <summary>A module as it stands for one school.</summary>
/// <param name="IsEnabled">Whether the school's users can use the module.</param>
/// <param name="FollowsDefault">True when the master made no choice for this school, so the module's default applies.</param>
public sealed record SchoolModuleDto(string Key, string Name, string Description, bool IsDefault, bool IsEnabled, bool FollowsDefault);
