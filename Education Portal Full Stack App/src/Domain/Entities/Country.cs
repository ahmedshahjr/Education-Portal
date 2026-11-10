using System.Text.RegularExpressions;
using EducationPortal.Domain.Localization;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// Regional settings that the master tenant manages and assigns to schools: which languages a school in the country
/// offers, the default one, the time zone and the currency. Dates and numbers are formatted by the culture made of a
/// language and the country code (for example <c>ur-PK</c>), so they follow the country's conventions.
/// Countries are platform reference data, not owned by a school, so they have no <c>SchoolId</c>; only master-tenant
/// use cases change them.
/// </summary>
public partial class Country : BaseAuditableEntity
{
    public const int CodeLength = 2;
    public const int NameMaxLength = 100;
    public const int TimeZoneIdMaxLength = 64;
    public const int CurrencyCodeLength = 3;

    private readonly List<string> _languages = [];

    private Country()
    {
        // For EF Core.
        Code = string.Empty;
        Name = string.Empty;
        DefaultLanguage = string.Empty;
        TimeZoneId = string.Empty;
        CurrencyCode = string.Empty;
    }

    private Country(string code) : this()
    {
        if (!IsValidCode(code))
        {
            throw new ArgumentException("A country code is two letters (ISO 3166-1 alpha-2), for example PK.", nameof(code));
        }

        Code = code.ToUpperInvariant();
    }

    public static Country Create(
        string code,
        string name,
        IEnumerable<string> languages,
        string defaultLanguage,
        string timeZoneId,
        string currencyCode)
    {
        var country = new Country(code);
        country.Update(name, languages, defaultLanguage, timeZoneId, currencyCode);
        return country;
    }

    /// <summary>The ISO 3166-1 alpha-2 code in upper case, for example <c>PK</c>. Set once.</summary>
    public string Code { get; private set; }

    public string Name { get; private set; }

    /// <summary>The ISO 639-1 codes of the languages schools in this country offer, in menu order.</summary>
    public IReadOnlyList<string> Languages => _languages.AsReadOnly();

    /// <summary>The language used when a visitor has not picked one and their browser asks for none of the others.</summary>
    public string DefaultLanguage { get; private set; }

    /// <summary>The IANA time zone, for example <c>Asia/Karachi</c>.</summary>
    public string TimeZoneId { get; private set; }

    /// <summary>The ISO 4217 currency code, for example <c>PKR</c>.</summary>
    public string CurrencyCode { get; private set; }

    public void Update(string name, IEnumerable<string> languages, string defaultLanguage, string timeZoneId, string currencyCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(languages);
        ArgumentException.ThrowIfNullOrWhiteSpace(defaultLanguage);
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));

        var codes = languages.Select(l => l.Trim().ToLowerInvariant()).Distinct(StringComparer.Ordinal).ToList();

        if (codes.Count == 0)
        {
            throw new ArgumentException("A country offers at least one language.", nameof(languages));
        }

        if (codes.FirstOrDefault(code => !Language.IsSupported(code)) is { } unsupported)
        {
            throw new ArgumentException($"The portal has no translations for the language '{unsupported}'.", nameof(languages));
        }

        var normalizedDefault = defaultLanguage.Trim().ToLowerInvariant();

        if (!codes.Contains(normalizedDefault))
        {
            throw new ArgumentException("The default language must be one of the country's languages.", nameof(defaultLanguage));
        }

        if (!IsValidTimeZone(timeZoneId))
        {
            throw new ArgumentException("The time zone must be an IANA time zone, for example Asia/Karachi.", nameof(timeZoneId));
        }

        if (!CurrencyCodePattern().IsMatch(currencyCode))
        {
            throw new ArgumentException("A currency code is three upper-case letters (ISO 4217), for example PKR.", nameof(currencyCode));
        }

        Name = name.Trim();
        _languages.Clear();
        _languages.AddRange(codes);
        DefaultLanguage = normalizedDefault;
        TimeZoneId = timeZoneId;
        CurrencyCode = currencyCode;
    }

    public bool Offers(string? language) =>
        language is not null && _languages.Contains(language.ToLowerInvariant());

    public static bool IsValidCode(string? code) => code is not null && CountryCodePattern().IsMatch(code);

    /// <summary>
    /// True for an IANA time zone known to the runtime. Windows ids (such as "Pakistan Standard Time") are refused, so
    /// the same id works in .NET and in the browser's <c>Intl</c> API.
    /// </summary>
    public static bool IsValidTimeZone(string? timeZoneId) =>
        !string.IsNullOrWhiteSpace(timeZoneId)
        && timeZoneId.Length <= TimeZoneIdMaxLength
        && TimeZoneInfo.TryFindSystemTimeZoneById(timeZoneId, out var zone)
        && (zone.HasIanaId || timeZoneId is "UTC" or "Etc/UTC");

    [GeneratedRegex("^[A-Za-z]{2}$")]
    private static partial Regex CountryCodePattern();

    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex CurrencyCodePattern();
}
