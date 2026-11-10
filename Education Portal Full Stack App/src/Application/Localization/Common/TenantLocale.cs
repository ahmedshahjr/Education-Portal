using EducationPortal.Domain.Localization;

namespace EducationPortal.Application.Localization.Common;

/// <summary>The regional settings one school uses. See <see cref="Interfaces.ITenantLocaleSettings"/>.</summary>
/// <param name="CountryCode">The ISO 3166-1 alpha-2 code, or <c>null</c> when the school has no country.</param>
/// <param name="Languages">The ISO 639-1 codes the school offers, in menu order. Never empty.</param>
/// <param name="DefaultLanguage">One of <paramref name="Languages"/>.</param>
/// <param name="TimeZoneId">An IANA time zone.</param>
/// <param name="CurrencyCode">The ISO 4217 code, or <c>null</c> when the school has no country.</param>
public sealed record TenantLocale(
    string? CountryCode,
    IReadOnlyList<string> Languages,
    string DefaultLanguage,
    string TimeZoneId,
    string? CurrencyCode)
{
    /// <summary>The settings of a school without a country: English only, UTC, no currency.</summary>
    public static TenantLocale Default { get; } = new(null, [Language.English.Code], Language.English.Code, "UTC", null);

    public bool Offers(string? language) =>
        language is not null && Languages.Contains(language, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The culture name for a language in this school's country, for example <c>ur-PK</c>, so dates and numbers follow
    /// the country's conventions. Without a country it is the language alone.
    /// </summary>
    public string CultureFor(string language) =>
        CountryCode is null ? language.ToLowerInvariant() : $"{language.ToLowerInvariant()}-{CountryCode}";
}
