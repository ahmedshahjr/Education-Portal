using EducationPortal.Domain.Localization;

namespace EducationPortal.Application.Countries.Common;

/// <summary>The settings of a country that the master tenant can change. See <see cref="Country"/>.</summary>
public abstract record CountryFields
{
    public string Name { get; init; } = string.Empty;

    /// <summary>ISO 639-1 codes of supported languages, in menu order.</summary>
    public IReadOnlyList<string> Languages { get; init; } = [];

    public string DefaultLanguage { get; init; } = string.Empty;

    /// <summary>An IANA time zone, for example <c>Asia/Karachi</c>.</summary>
    public string TimeZoneId { get; init; } = string.Empty;

    /// <summary>An ISO 4217 code, for example <c>PKR</c>.</summary>
    public string CurrencyCode { get; init; } = string.Empty;
}

/// <summary>The rules shared by creating and changing a country (SEC-INPUT-01). The entity checks them again.</summary>
public abstract class CountryFieldsValidator<T> : AbstractValidator<T>
    where T : CountryFields
{
    protected CountryFieldsValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(Country.NameMaxLength);

        RuleFor(c => c.Languages)
            .NotEmpty()
            .Must(languages => languages.Count <= Language.All.Count)
            .WithMessage("A country can offer at most every supported language once.");

        RuleForEach(c => c.Languages)
            .Must(Language.IsSupported)
            .WithMessage("The portal has no translations for '{PropertyValue}'.");

        RuleFor(c => c.Languages)
            .Must(languages => languages.Distinct(StringComparer.OrdinalIgnoreCase).Count() == languages.Count)
            .WithMessage("Each language can be listed once.");

        RuleFor(c => c.DefaultLanguage)
            .NotEmpty()
            .Must((fields, language) => fields.Languages.Contains(language, StringComparer.OrdinalIgnoreCase))
            .WithMessage("The default language must be one of the country's languages.");

        RuleFor(c => c.TimeZoneId)
            .Must(Country.IsValidTimeZone)
            .WithMessage("The time zone must be an IANA time zone, for example Asia/Karachi.");

        RuleFor(c => c.CurrencyCode)
            .NotEmpty()
            .Matches("^[A-Z]{3}$")
            .WithMessage("The currency must be an ISO 4217 code, for example PKR.");
    }
}
