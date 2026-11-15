namespace EducationPortal.Application.Branches.Common;

/// <summary>Whether a school's campus codes show a prefix, and the prefix.</summary>
public sealed record CampusSettingsDto(bool PrefixEnabled, string? Prefix);
