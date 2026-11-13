namespace EducationPortal.Application.Modules.Common;

/// <summary>A module in the master portal's catalogue.</summary>
/// <param name="IsCore">True when the module is part of every subscription and never sold on its own.</param>
/// <param name="IsActive">False when the master switched the module off for every school.</param>
/// <param name="Requires">The codes of the modules it requires directly, in catalogue order.</param>
public sealed record ModuleDto(string Code, string Name, string Description, bool IsCore, bool IsActive, int SortOrder, IReadOnlyList<string> Requires);

/// <summary>A module as it stands for one school.</summary>
/// <param name="IsEnabled">Whether the school's users can use the module.</param>
/// <param name="IsActive">False when the master switched the module off for every school; then it is off here too.</param>
public sealed record SchoolModuleDto(string Code, string Name, string Description, bool IsCore, bool IsActive, bool IsEnabled);
