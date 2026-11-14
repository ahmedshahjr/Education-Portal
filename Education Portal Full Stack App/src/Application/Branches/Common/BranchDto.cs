using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Branches.Common;

/// <summary>
/// A campus of a school (the class keeps its name). <see cref="Code"/> is the four-digit code; <see cref="DisplayCode"/> is
/// the code as the school shows it, with the school's prefix when the prefix is on. <see cref="Kind"/> is
/// <see cref="BranchKind.Main"/> for the registration campus. <see cref="Location"/> and <see cref="Shifts"/> are the
/// campus's place and its daily shifts (Morning, Evening).
/// </summary>
public sealed record BranchDto(
    Guid Id,
    string Name,
    string Code,
    string DisplayCode,
    BranchKind Kind,
    CampusLocationDto Location,
    IReadOnlyList<CampusShiftDto> Shifts);

/// <summary>
/// Where a campus is: its province and city (public ids from the location data), its address and its pin on the map. Every part
/// is null when it is not set.
/// </summary>
public sealed record CampusLocationDto(
    Guid? StateId,
    Guid? CityId,
    string? AddressLine,
    decimal? Latitude,
    decimal? Longitude,
    string? GooglePlaceId,
    string? GoogleMapsUrl);

/// <summary>A daily shift of a campus, with its start and end time in the school's country time zone.</summary>
public sealed record CampusShiftDto(string Name, TimeOnly StartTime, TimeOnly EndTime);
