using EducationPortal.Application.Localization.Commands.ChangeLanguage;
using EducationPortal.Application.Localization.Queries.GetLocalization;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>The language of the school that owns the request's domain, and the visitor's choice among its languages.</summary>
public class Localization : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetLocalization)
            // compliance-exception: SEC-AUTHN-01 the login page is shown in the visitor's language; returns only the school's regional settings
            .AllowAnonymous();

        groupBuilder.MapPut(ChangeLanguage, "language")
            // compliance-exception: SEC-AUTHN-01 the language can be changed on the login page; it only sets a preference cookie
            .AllowAnonymous()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("Get the language settings")]
    [EndpointDescription("Returns the language this request is answered in, the languages the school offers, its time zone and currency. Public, so the login page is translated.")]
    public static async Task<Ok<LocalizationDto>> GetLocalization(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetLocalizationQuery()));
    }

    [EndpointSummary("Change the language")]
    [EndpointDescription("Remembers the visitor's language in a cookie on this school's domain. Only the school's own languages are accepted; others return 400.")]
    public static async Task<NoContent> ChangeLanguage(ISender sender, HttpContext httpContext, ChangeLanguageCommand command)
    {
        var language = await sender.Send(command);

        TenantRequestLocalization.WriteLanguageCookie(httpContext.Response, language);

        return TypedResults.NoContent();
    }
}
