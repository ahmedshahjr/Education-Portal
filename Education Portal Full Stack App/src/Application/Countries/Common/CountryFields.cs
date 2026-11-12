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
            .WithMessage(_ => ValidationMessages.CountryTooManyLanguages);

        RuleForEach(c => c.Languages)
            .Must(Language.IsSupported)
            .WithMessage(_ => ValidationMessages.LanguageNotSupported);

        RuleFor(c => c.Languages)
            .Must(languages => languages.Distinct(StringComparer.OrdinalIgnoreCase).Count() == languages.Count)
            .WithMessage(_ => ValidationMessages.LanguageListedTwice);

        RuleFor(c => c.DefaultLanguage)
            .NotEmpty()
            .Must((fields, language) => fields.Languages.Contains(language, StringComparer.OrdinalIgnoreCase))
            .WithMessage(_ => ValidationMessages.DefaultLanguageNotOffered);

        RuleFor(c => c.TimeZoneId)
            .Must(Country.IsValidTimeZone)
            .WithMessage(_ => ValidationMessages.TimeZoneInvalid);

        RuleFor(c => c.CurrencyCode)
            .NotEmpty()
            .Matches("^[A-Z]{3}$")
            .WithMessage(_ => ValidationMessages.CurrencyInvalid);
    }
}
