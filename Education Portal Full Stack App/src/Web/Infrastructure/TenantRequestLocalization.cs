using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Localization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Web.Infrastructure;

/// <summary>
/// ASP.NET Core request localization with one culture provider that knows the tenant. Every request is answered in one
/// of the languages the host's school offers (its country's list, set in the master portal), so a cookie or browser
/// header can never switch a Pakistani school to Arabic. The culture is the language plus the school's country, for
/// example <c>ur-PK</c>, so dates and numbers follow the country's conventions.
/// </summary>
public static class TenantRequestLocalization
{
    /// <summary>
    /// The visitor's chosen language, in the standard <see cref="CookieRequestCultureProvider"/> format. Set only by
    /// <c>PUT /api/Localization/language</c>; HttpOnly, because Angular reads the language from the API.
    /// </summary>
    public const string CookieName = "__Host-Culture";

    public static IServiceCollection AddTenantRequestLocalization(this IServiceCollection services)
    {
        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services.Configure<RequestLocalizationOptions>(options =>
        {
            // Every culture of a supported language; the provider narrows this to the school's languages. A culture
            // the runtime does not know (for example ur-AE) falls back to its language (ur).
            var cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures | CultureTypes.SpecificCultures)
                .Where(culture => Language.IsSupported(culture.TwoLetterISOLanguageName))
                .ToList();

            options.DefaultRequestCulture = new RequestCulture(Language.English.Code);
            options.SupportedCultures = cultures;
            options.SupportedUICultures = cultures;
            options.FallBackToParentCultures = true;
            options.FallBackToParentUICultures = true;
            options.ApplyCurrentCultureToResponseHeaders = true; // Content-Language
            options.RequestCultureProviders = [new TenantRequestCultureProvider()];
        });

        return services;
    }

    /// <summary>
    /// Runs after tenant resolution, so the provider knows the school, and after authentication and the session check,
    /// so it knows the user's saved language.
    /// </summary>
    public static IApplicationBuilder UseTenantRequestLocalization(this IApplicationBuilder app) => app.UseRequestLocalization();

    /// <summary>Remembers the visitor's language for a year on this school's domain.</summary>
    public static void WriteLanguageCookie(HttpResponse response, string language)
    {
        response.Cookies.Append(
            CookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(language)),
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                MaxAge = TimeSpan.FromDays(365),
                // A setting the user asked for (user-interface customisation), not tracking (SEC-EU ePrivacy).
                IsEssential = true,
            });
    }
}

/// <summary>
/// Picks the request's language: a signed-in user's saved language (it follows them to every device), then this
/// browser's saved choice, then the browser's languages by preference, then the school's default; the first one the
/// school offers wins.
/// </summary>
internal sealed class TenantRequestCultureProvider : RequestCultureProvider
{
    public override async Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var tenant = httpContext.RequestServices.GetRequiredService<ICurrentTenant>();

        if (tenant.SchoolId is null)
        {
            // Health probes have no tenant; they get the default culture.
            return null;
        }

        var locale = await httpContext.RequestServices.GetRequiredService<ITenantLocaleSettings>().GetAsync(httpContext.RequestAborted);

        var language = CandidateLanguages(httpContext).FirstOrDefault(locale.Offers) ?? locale.DefaultLanguage;

        return new ProviderCultureResult(locale.CultureFor(language));
    }

    private static IEnumerable<string> CandidateLanguages(HttpContext httpContext)
    {
        // Set only by the claims factory from the account; authentication runs before this middleware.
        if (httpContext.User.FindFirst(TenantClaimTypes.Language)?.Value is { Length: > 0 } saved)
        {
            yield return saved;
        }

        if (httpContext.Request.Cookies.TryGetValue(TenantRequestLocalization.CookieName, out var cookie)
            && CookieRequestCultureProvider.ParseCookieValue(cookie) is { } chosen)
        {
            foreach (var culture in chosen.UICultures)
            {
                yield return LanguageOf(culture.Value);
            }
        }

        var acceptLanguage = httpContext.Request.GetTypedHeaders().AcceptLanguage;

        foreach (var header in acceptLanguage.Where(h => h.Quality is not 0).OrderByDescending(h => h.Quality ?? 1))
        {
            yield return LanguageOf(header.Value.Value);
        }
    }

    private static string LanguageOf(string? culture) =>
        string.IsNullOrEmpty(culture) ? string.Empty : culture.Split('-', 2)[0];
}
