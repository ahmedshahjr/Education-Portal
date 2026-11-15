namespace EducationPortal.Domain.ValueObjects;

/// <summary>
/// The registration and contact details of a campus. Every field is optional. Build it with <see cref="Create"/>, which
/// trims the text, turns blanks into <c>null</c> and rejects anything that breaks a rule, so a campus never stores a value
/// in another form. The ids are internal keys: the use case resolves the public ids to them first.
/// </summary>
public sealed record CampusDetails
{
    public const int AddressMaxLength = 300;
    public const int SchoolNumberMaxLength = 50;
    public const int ShortNameMaxLength = 25;

    private CampusDetails()
    {
    }

    public string? Address { get; private init; }

    public int? StateId { get; private init; }

    public int? CityId { get; private init; }

    public string? PostalCode { get; private init; }

    public string? Phone { get; private init; }

    public string? Website { get; private init; }

    /// <summary>The school's registration number for the campus.</summary>
    public string? SchoolNumber { get; private init; }

    public string? ShortName { get; private init; }

    /// <summary>The <see cref="UserDetails"/> row of the principal. It must belong to the same school and be a principal; the use case checks that.</summary>
    public int? PrincipalUserDetailsId { get; private init; }

    /// <summary>Validates and normalises the values. Throws <see cref="ArgumentException"/> for the first rule broken.</summary>
    public static CampusDetails Create(
        string? address,
        int? stateId,
        int? cityId,
        string? postalCode,
        string? phone,
        string? website,
        string? schoolNumber,
        string? shortName,
        int? principalUserDetailsId)
    {
        if (cityId is not null && stateId is null)
        {
            throw new ArgumentException("A city needs its state.", nameof(cityId));
        }

        var normalizedAddress = Normalize(address, nameof(address), AddressMaxLength);
        var normalizedPostalCode = Normalize(postalCode, nameof(postalCode), ContactRules.PostalCodeMaxLength);
        if (normalizedPostalCode is not null && !ContactRules.IsValidPostalCode(normalizedPostalCode))
        {
            throw new ArgumentException("A postal code has letters, digits, spaces and hyphens only.", nameof(postalCode));
        }

        var normalizedPhone = Normalize(phone, nameof(phone), ContactRules.PhoneMaxLength);
        if (normalizedPhone is not null && !ContactRules.IsValidPhone(normalizedPhone))
        {
            throw new ArgumentException("A phone number has 7 to 15 digits and an optional leading +.", nameof(phone));
        }

        var normalizedWebsite = Normalize(website, nameof(website), ContactRules.WebsiteMaxLength);
        if (normalizedWebsite is not null && !ContactRules.IsValidWebsite(normalizedWebsite))
        {
            throw new ArgumentException("A website must be an absolute http or https address.", nameof(website));
        }

        return new CampusDetails
        {
            Address = normalizedAddress,
            StateId = stateId,
            CityId = cityId,
            PostalCode = normalizedPostalCode,
            Phone = normalizedPhone,
            Website = normalizedWebsite,
            SchoolNumber = Normalize(schoolNumber, nameof(schoolNumber), SchoolNumberMaxLength),
            ShortName = Normalize(shortName, nameof(shortName), ShortNameMaxLength),
            PrincipalUserDetailsId = principalUserDetailsId,
        };
    }

    /// <summary>Trims the text. Blank becomes <c>null</c>. Throws when it is longer than the limit.</summary>
    private static string? Normalize(string? value, string parameter, int maxLength)
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
