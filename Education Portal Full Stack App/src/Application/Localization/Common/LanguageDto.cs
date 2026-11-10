using EducationPortal.Domain.Localization;

namespace EducationPortal.Application.Localization.Common;

/// <summary>A language the portal has translations for, as the language menu and the master portal show it.</summary>
public sealed record LanguageDto(string Code, string EnglishName, string NativeName, bool IsRightToLeft)
{
    public static LanguageDto From(Language language) =>
        new(language.Code, language.EnglishName, language.NativeName, language.IsRightToLeft);
}
