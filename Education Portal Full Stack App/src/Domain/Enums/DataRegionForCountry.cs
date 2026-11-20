namespace EducationPortal.Domain.Enums;

/// <summary>
/// The data region a school is given from its country when it is registered, so nobody has to pick one. Only the countries
/// listed here get a region. Any other country gets <c>null</c> ("not decided yet") until the master sets one, and a new
/// country needs a line here.
/// </summary>
public static class DataRegionForCountry
{
    /// <summary>The region for an ISO 3166-1 alpha-2 country code (case-insensitive), or <c>null</c> when the country has none.</summary>
    public static DataRegion? For(string? countryCode) => countryCode?.ToUpperInvariant() switch
    {
        "PK" => DataRegion.SouthAsia,
        "SA" or "AE" => DataRegion.MiddleEast,
        "US" => DataRegion.NorthAmerica,
        _ => null,
    };
}
