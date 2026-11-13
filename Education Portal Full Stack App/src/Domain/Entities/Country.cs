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
public partial class Country : BaseSoftDeletableEntity
{
    public const int CodeLength = 2;
    public const int NameMaxLength = 100;
    public const int TimeZoneIdMaxLength = 64;
    public const int CurrencyCodeLength = 3;
    public const int Iso3Length = 3;
    public const int PhoneCodeMaxLength = 16;
    public const int NativeNameMaxLength = 200;

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

    /// <summary>The ISO 3166-1 alpha-3 code, for example <c>PAK</c>. Filled by the location data import; <c>null</c> for a country it does not cover.</summary>
    public string? Iso3 { get; private set; }

    /// <summary>The international dialling code without the plus sign, for example <c>92</c>.</summary>
    public string? PhoneCode { get; private set; }

    /// <summary>The name in the country's own language and script, for example <c>پاکستان</c>.</summary>
    public string? NativeName { get; private set; }

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

    /// <summary>
    /// Sets the reference codes that come from the location dataset. The master tenant's own settings (name, languages,
    /// time zone and currency) are left alone, so a data update never overwrites them.
    /// </summary>
    public void SetReferenceCodes(string iso3, string? phoneCode, string? nativeName)
    {
        if (iso3 is null || !Iso3Pattern().IsMatch(iso3))
        {
            throw new ArgumentException("An ISO 3166-1 alpha-3 code is three letters, for example PAK.", nameof(iso3));
        }

        var phone = string.IsNullOrWhiteSpace(phoneCode) ? null : phoneCode.Trim().TrimStart('+');
        if (phone is not null && !PhoneCodePattern().IsMatch(phone))
        {
            throw new ArgumentException("A phone code is digits, optionally with a dash, for example 92 or 1-684.", nameof(phoneCode));
        }

        var native = string.IsNullOrWhiteSpace(nativeName) ? null : nativeName.Trim();
        if (native is not null)
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThan(native.Length, NativeNameMaxLength, nameof(nativeName));
        }

        Iso3 = iso3.ToUpperInvariant();
        PhoneCode = phone;
        NativeName = native;
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

    [GeneratedRegex("^[A-Za-z]{3}$")]
    private static partial Regex Iso3Pattern();

    [GeneratedRegex("^[0-9]{1,4}(-[0-9]{1,4})?$")]
    private static partial Regex PhoneCodePattern();
}
