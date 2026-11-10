namespace EducationPortal.Application.Countries.Common;

public sealed record CountryDto(
    int Id,
    string Code,
    string Name,
    IReadOnlyList<string> Languages,
    string DefaultLanguage,
    string TimeZoneId,
    string CurrencyCode);
