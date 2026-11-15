using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Branches.Common;

/// <summary>
/// A campus of a school (the class keeps its name). <see cref="Code"/> is the four-digit code; <see cref="DisplayCode"/> is
/// the code as the school shows it, with the school's prefix when the prefix is on. <see cref="Kind"/> is
/// <see cref="BranchKind.Main"/> for the registration campus. <see cref="Details"/> holds the contact and registration
/// details, with the names of the state, city and principal for display.
/// </summary>
public sealed record BranchDto(Guid Id, string Name, string Code, string DisplayCode, BranchKind Kind, CampusDetailsDto Details);

/// <summary>The contact and registration details of a campus. Links are public ids; names are for display.</summary>
public sealed record CampusDetailsDto(
    string? Address,
    Guid? StateId,
    string? StateName,
    Guid? CityId,
    string? CityName,
    string? PostalCode,
    string? Phone,
    string? Website,
    string? SchoolNumber,
    string? ShortName,
    Guid? PrincipalId,
    string? PrincipalName);
