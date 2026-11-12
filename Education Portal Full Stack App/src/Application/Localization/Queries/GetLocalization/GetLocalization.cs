using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Localization.Common;
using EducationPortal.Domain.Localization;

namespace EducationPortal.Application.Localization.Queries.GetLocalization;

/// <summary>
/// Returns the language this request is answered in and the languages the host's school offers, so the Angular app can
/// load its translations and show the language menu before sign-in. The language was picked by the request
/// localization middleware from the visitor's choice, their browser and the school's country; the school comes only
/// from the host (SEC-AUTHZ-04). Public, like the branding: it holds nothing beyond the school's regional settings.
/// </summary>
public record GetLocalizationQuery : IRequest<LocalizationDto>;

/// <param name="Language">The ISO 639-1 code of the current language, for example <c>ur</c>.</param>
/// <param name="Culture">The culture for dates and numbers, for example <c>ur-PK</c>.</param>
/// <param name="IsRightToLeft">True when the page is laid out right to left.</param>
/// <param name="Languages">The languages the school offers, in menu order.</param>
/// <param name="TimeZone">The school's IANA time zone.</param>
/// <param name="UserTimeZone">The time zone this caller reads dates in: their own (sent by the browser), else the
/// school's.</param>
/// <param name="CurrencyCode">The school's ISO 4217 currency, or <c>null</c> when it has no country.</param>
public sealed record LocalizationDto(
    string Language,
    string Culture,
    bool IsRightToLeft,
    IReadOnlyList<LanguageDto> Languages,
    string TimeZone,
    string UserTimeZone,
    string? CurrencyCode);

public class GetLocalizationQueryHandler : IRequestHandler<GetLocalizationQuery, LocalizationDto>
{
    private readonly ITenantLocaleSettings _settings;
    private readonly IRequestTimeZone _timeZone;

    public GetLocalizationQueryHandler(ITenantLocaleSettings settings, IRequestTimeZone timeZone)
    {
        _settings = settings;
        _timeZone = timeZone;
    }

    public async Task<LocalizationDto> Handle(GetLocalizationQuery request, CancellationToken cancellationToken)
    {
        var locale = await _settings.GetAsync(cancellationToken);

        // The middleware only ever picks one of the school's languages; the default is a safety net.
        var current = CultureInfo.CurrentUICulture;
        var language = Language.Find(current.TwoLetterISOLanguageName) is { } found && locale.Offers(found.Code)
            ? found
            : Language.Find(locale.DefaultLanguage) ?? Language.English;

        var offered = locale.Languages
            .Select(Language.Find)
            .OfType<Language>()
            .Select(LanguageDto.From)
            .ToList();

        return new LocalizationDto(
            language.Code,
            CultureInfo.CurrentCulture.Name,
            language.IsRightToLeft,
            offered,
            locale.TimeZoneId,
            (await _timeZone.GetAsync(cancellationToken)).Id,
            locale.CurrencyCode);
    }
}
