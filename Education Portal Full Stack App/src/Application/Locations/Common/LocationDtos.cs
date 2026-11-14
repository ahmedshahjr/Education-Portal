using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Locations.Common;

/// <summary>A country in the address dropdown. <paramref name="Code"/> is the ISO 3166-1 alpha-2 code.</summary>
public sealed record LocationCountryDto(Guid Id, string Code, string? Iso3, string Name, string? NativeName, string? PhoneCode, string CurrencyCode);

/// <summary>A state, province, prefecture, emirate, governorate or territory in the address dropdown.</summary>
public sealed record StateDto(Guid Id, string Name, string? NativeName, string? IsoCode, StateType Type);

/// <summary>A city in the address dropdown.</summary>
public sealed record CityDto(Guid Id, string Name, string? NativeName, decimal? Latitude, decimal? Longitude);
