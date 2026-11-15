using EducationPortal.Domain.Entities;

namespace EducationPortal.Domain.ValueObjects;

/// <summary>
/// The registration and contact details of a campus. Required: the address, the state, the city, the postal code, at least
/// one phone number, the school number, the short name and the location on the map (latitude and longitude). Optional: the
/// website and the principal. Build it with
/// <see cref="Create"/>, which trims the text and rejects anything that breaks a rule, so a campus never stores a value in
/// another form. The ids are internal keys: the use case resolves the public ids to them first.
/// </summary>
public sealed record CampusDetails
{
    public const int AddressMaxLength = 300;
    public const int SchoolNumberMaxLength = 50;
    public const int ShortNameMaxLength = 25;

    /// <summary>Latitude runs from -90 (south pole) to 90 (north pole).</summary>
    public const decimal LatitudeLimit = 90m;

    /// <summary>Longitude runs from -180 to 180.</summary>
    public const decimal LongitudeLimit = 180m;

    /// <summary>Decimal places kept for a coordinate: about 0.1 m, the precision the map picker gives.</summary>
    public const int CoordinateDecimals = 6;

    private CampusDetails()
    {
        Address = string.Empty;
        PostalCode = string.Empty;
        Phones = [];
        SchoolNumber = string.Empty;
        ShortName = string.Empty;
        Latitude = 0m;
        Longitude = 0m;
    }

    public string Address { get; private init; }

    public int StateId { get; private init; }

    public int CityId { get; private init; }

    public string PostalCode { get; private init; }

    /// <summary>One to <see cref="Campus.MaxPhones"/> numbers, in the order given.</summary>
    public IReadOnlyList<string> Phones { get; private init; }

    /// <summary>The website, or <c>null</c> when the campus has none.</summary>
    public string? Website { get; private init; }

    /// <summary>The school's registration number for the campus.</summary>
    public string SchoolNumber { get; private init; }

    public string ShortName { get; private init; }

    /// <summary>The campus's place on the map, in degrees, rounded to <see cref="CoordinateDecimals"/> places.</summary>
    public decimal Latitude { get; private init; }

    public decimal Longitude { get; private init; }

    /// <summary>The <see cref="UserDetails"/> row of the principal. It must belong to the same school and be a principal; the use case checks that.</summary>
    public int? PrincipalUserDetailsId { get; private init; }

    /// <summary>Validates and normalises the values. Throws <see cref="ArgumentException"/> for the first rule broken.</summary>
    public static CampusDetails Create(
        string? address,
        int stateId,
        int cityId,
        string? postalCode,
        IEnumerable<string>? phones,
        string? website,
        string? schoolNumber,
        string? shortName,
        int? principalUserDetailsId,
        decimal? latitude,
        decimal? longitude)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(stateId, nameof(stateId));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cityId, nameof(cityId));

        var normalizedAddress = RequiredText(address, nameof(address), AddressMaxLength);

        var normalizedPostalCode = RequiredText(postalCode, nameof(postalCode), ContactRules.PostalCodeMaxLength);
        if (!ContactRules.IsValidPostalCode(normalizedPostalCode))
        {
            throw new ArgumentException("A postal code has letters, digits, spaces and hyphens only.", nameof(postalCode));
        }

        var normalizedPhones = NormalizePhones(phones);

        var normalizedWebsite = OptionalText(website, nameof(website), ContactRules.WebsiteMaxLength);
        if (normalizedWebsite is not null && !ContactRules.IsValidWebsite(normalizedWebsite))
        {
            throw new ArgumentException("A website must be an absolute http or https address.", nameof(website));
        }

        var (normalizedLatitude, normalizedLongitude) = NormalizeCoordinates(latitude, longitude);

        return new CampusDetails
        {
            Address = normalizedAddress,
            StateId = stateId,
            CityId = cityId,
            PostalCode = normalizedPostalCode,
            Phones = normalizedPhones,
            Website = normalizedWebsite,
            SchoolNumber = RequiredText(schoolNumber, nameof(schoolNumber), SchoolNumberMaxLength),
            ShortName = RequiredText(shortName, nameof(shortName), ShortNameMaxLength),
            PrincipalUserDetailsId = principalUserDetailsId,
            Latitude = normalizedLatitude,
            Longitude = normalizedLongitude,
        };
    }

    /// <summary>Both coordinates are required and within their limits. They are rounded to <see cref="CoordinateDecimals"/> places.</summary>
    private static (decimal Latitude, decimal Longitude) NormalizeCoordinates(decimal? latitude, decimal? longitude)
    {
        if (latitude is null || longitude is null)
        {
            throw new ArgumentException("A campus needs its place on the map.", latitude is null ? nameof(latitude) : nameof(longitude));
        }

        if (Math.Abs(latitude.Value) > LatitudeLimit)
        {
            throw new ArgumentOutOfRangeException(nameof(latitude), $"Latitude must be between -{LatitudeLimit} and {LatitudeLimit}.");
        }

        if (Math.Abs(longitude.Value) > LongitudeLimit)
        {
            throw new ArgumentOutOfRangeException(nameof(longitude), $"Longitude must be between -{LongitudeLimit} and {LongitudeLimit}.");
        }

        return (Math.Round(latitude.Value, CoordinateDecimals), Math.Round(longitude.Value, CoordinateDecimals));
    }

    /// <summary>Between one and <see cref="Campus.MaxPhones"/> numbers: each trimmed, valid, and not repeated.</summary>
    private static IReadOnlyList<string> NormalizePhones(IEnumerable<string>? phones)
    {
        var numbers = (phones ?? []).Select(p => p?.Trim() ?? string.Empty).Where(p => p.Length > 0).ToList();
        if (numbers.Count == 0)
        {
            throw new ArgumentException("A campus needs at least one phone number.", nameof(phones));
        }

        if (numbers.Count > Campus.MaxPhones)
        {
            throw new ArgumentException($"A campus has at most {Campus.MaxPhones} phone numbers.", nameof(phones));
        }

        foreach (var number in numbers)
        {
            if (!CampusPhone.IsValid(number))
            {
                throw new ArgumentException("A phone number has 7 to 15 digits and an optional leading +.", nameof(phones));
            }
        }

        if (numbers.Distinct(StringComparer.Ordinal).Count() != numbers.Count)
        {
            throw new ArgumentException("A phone number is listed only once.", nameof(phones));
        }

        return numbers;
    }

    /// <summary>Trims the text. Blank is refused, and so is text longer than the limit.</summary>
    private static string RequiredText(string? value, string parameter, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            throw new ArgumentException("This field is required.", parameter);
        }

        if (trimmed.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(parameter, $"Must be at most {maxLength} characters.");
        }

        return trimmed;
    }

    /// <summary>Trims the text. Blank becomes <c>null</c>. Throws when it is longer than the limit.</summary>
    private static string? OptionalText(string? value, string parameter, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        if (trimmed.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(parameter, $"Must be at most {maxLength} characters.");
        }

        return trimmed;
    }
}
