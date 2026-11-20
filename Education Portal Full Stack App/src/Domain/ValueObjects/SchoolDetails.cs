namespace EducationPortal.Domain.ValueObjects;

/// <summary>
/// A school's registration and contact details. Every field is optional except <see cref="AcademicYearStartMonth"/>. Build it
/// with <see cref="Create"/>, which trims the text, turns blanks into <c>null</c> and rejects anything that breaks a rule, so
/// a school never stores a value in another form. <see cref="CityId"/> is an internal key: the use case resolves the public
/// id first and checks the city is in the school's country.
/// </summary>
public sealed record SchoolDetails
{
    public const int LocalNameMaxLength = 200;
    public const int RegistrationNumberMaxLength = 50;
    public const int RegistrationAuthorityMaxLength = 200;
    public const int TaxNumberMaxLength = 50;
    public const int AddressMaxLength = 300;

    /// <summary>The month an academic year starts in when the school has not said: August.</summary>
    public const int DefaultAcademicYearStartMonth = 8;

    private SchoolDetails()
    {
    }

    /// <summary>The school's name in the local language and script, for example in Urdu or Arabic.</summary>
    public string? LocalName { get; private init; }

    public string? RegistrationNumber { get; private init; }

    /// <summary>The body the school is registered with, for example a provincial education board.</summary>
    public string? RegistrationAuthority { get; private init; }

    public string? TaxNumber { get; private init; }

    public string? Address { get; private init; }

    public int? CityId { get; private init; }

    public string? Phone { get; private init; }

    public string? Email { get; private init; }

    public string? Website { get; private init; }

    /// <summary>The month (1 to 12) the school's academic year usually starts in. Used to suggest the dates of a new year.</summary>
    public int AcademicYearStartMonth { get; private init; } = DefaultAcademicYearStartMonth;

    /// <summary>Validates and normalises the values. Throws <see cref="ArgumentException"/> for the first rule broken.</summary>
    public static SchoolDetails Create(
        string? localName,
        string? registrationNumber,
        string? registrationAuthority,
        string? taxNumber,
        string? address,
        int? cityId,
        string? phone,
        string? email,
        string? website,
        int academicYearStartMonth)
    {
        if (cityId is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(cityId), cityId, "A city id is positive.");
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(academicYearStartMonth, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(academicYearStartMonth, 12);

        var normalizedPhone = Optional(phone, ContactRules.PhoneMaxLength, nameof(phone));
        if (normalizedPhone is not null && !ContactRules.IsValidPhone(normalizedPhone))
        {
            throw new ArgumentException("A phone number has 7 to 15 digits.", nameof(phone));
        }

        var normalizedEmail = Optional(email, ContactRules.EmailMaxLength, nameof(email));
        if (normalizedEmail is not null && !ContactRules.IsValidEmail(normalizedEmail))
        {
            throw new ArgumentException("The email address is not valid.", nameof(email));
        }

        var normalizedWebsite = Optional(website, ContactRules.WebsiteMaxLength, nameof(website));
        if (normalizedWebsite is not null && !ContactRules.IsValidWebsite(normalizedWebsite))
        {
            throw new ArgumentException("A website is an http or https address.", nameof(website));
        }

        return new SchoolDetails
        {
            LocalName = Optional(localName, LocalNameMaxLength, nameof(localName)),
            RegistrationNumber = Optional(registrationNumber, RegistrationNumberMaxLength, nameof(registrationNumber)),
            RegistrationAuthority = Optional(registrationAuthority, RegistrationAuthorityMaxLength, nameof(registrationAuthority)),
            TaxNumber = Optional(taxNumber, TaxNumberMaxLength, nameof(taxNumber)),
            Address = Optional(address, AddressMaxLength, nameof(address)),
            CityId = cityId,
            Phone = normalizedPhone,
            Email = normalizedEmail,
            Website = normalizedWebsite,
            AcademicYearStartMonth = academicYearStartMonth,
        };
    }

    private static string? Optional(string? value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, maxLength, parameterName);
        return trimmed;
    }
}
