namespace EducationPortal.Domain.Localization;

/// <summary>
/// A user interface language the portal has translations for. The list is fixed in code because a language is only
/// usable once its translations ship with the app; the master tenant then chooses, per country, which of these a
/// school offers (<see cref="Entities.Country"/>).
/// </summary>
/// <param name="Code">The ISO 639-1 code, for example <c>ur</c>.</param>
/// <param name="EnglishName">The name in English, for the master portal.</param>
/// <param name="NativeName">The name in the language itself, for the language menu.</param>
/// <param name="IsRightToLeft">True for scripts written right to left (Arabic, Urdu).</param>
public sealed record Language(string Code, string EnglishName, string NativeName, bool IsRightToLeft)
{
    public const int CodeLength = 2;

    public static readonly Language English = new("en", "English", "English", IsRightToLeft: false);
    public static readonly Language Arabic = new("ar", "Arabic", "العربية", IsRightToLeft: true);
    public static readonly Language Urdu = new("ur", "Urdu", "اردو", IsRightToLeft: true);

    /// <summary>Every supported language. English is first and is the fallback when a school has no country.</summary>
    public static IReadOnlyList<Language> All { get; } = [English, Arabic, Urdu];

    public static Language? Find(string? code) =>
        code is null ? null : All.FirstOrDefault(l => string.Equals(l.Code, code, StringComparison.OrdinalIgnoreCase));

    public static bool IsSupported(string? code) => Find(code) is not null;
}
