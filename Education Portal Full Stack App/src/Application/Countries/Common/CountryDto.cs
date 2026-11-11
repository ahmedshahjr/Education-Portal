namespace EducationPortal.Application.Countries.Common;

public sealed record CountryDto(
    Guid Id,
    string Code,
    string Name,
    IReadOnlyList<string> Languages,
    string DefaultLanguage,
    string TimeZoneId,
    string CurrencyCode);
