using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Localization.Commands.ChangeLanguage;
using EducationPortal.Application.Localization.Queries.GetLocalization;
using EducationPortal.Web.Filters;
using EducationPortal.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>The language of the school that owns the request's domain, and the visitor's choice among its languages.</summary>
[Route("api/Localization")]
public class LocalizationController(
    IRequestRules rules,
    GetLocalizationQueryHandler getLocalization,
    ChangeLanguageCommandHandler changeLanguage) : ApiControllerBase
{
    [HttpGet]
    // compliance-exception: SEC-AUTHN-01 the login page is shown in the visitor's language; returns only the school's regional settings
    [AllowAnonymous]
    [EndpointSummary("Get the language settings")]
    [EndpointDescription("Returns the language this request is answered in, the languages the school offers, its time zone and currency. Public, so the login page is translated.")]
    public async Task<ActionResult<LocalizationDto>> GetLocalization(CancellationToken cancellationToken)
    {
        var query = new GetLocalizationQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getLocalization.Handle(query, cancellationToken));
    }

    [HttpPut("language")]
    // compliance-exception: SEC-AUTHN-01 the language can be changed on the login page; it only sets a preference cookie
    [AllowAnonymous]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change the language")]
    [EndpointDescription("Remembers the visitor's language in a cookie on this school's domain. Only the school's own languages are accepted; others return 400.")]
    public async Task<IActionResult> ChangeLanguage(ChangeLanguageCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        var language = await changeLanguage.Handle(command, cancellationToken);
        TenantRequestLocalization.WriteLanguageCookie(HttpContext.Response, language);
        return NoContent();
    }
}
