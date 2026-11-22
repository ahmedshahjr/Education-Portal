using EducationPortal.Application.Branding.Commands.UpdateBranding;
using EducationPortal.Application.Branding.Common;
using EducationPortal.Application.Branding.Queries.GetBranding;
using EducationPortal.Application.Branding.Queries.GetBrandingImage;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Enums;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>The branding of the school that owns the request's domain.</summary>
[Route("api/Branding")]
public class BrandingController(
    IRequestRules rules,
    GetBrandingQueryHandler getBranding,
    GetBrandingImageQueryHandler getImage,
    UpdateBrandingCommandHandler updateBranding) : ApiControllerBase
{
    [HttpGet]
    // compliance-exception: SEC-AUTHN-01 the login page is themed before sign-in; returns only the school's name and look
    [AllowAnonymous]
    [EndpointSummary("Get the school's branding")]
    [EndpointDescription("Returns the name, accent colour, sidebar colour and image versions of the school that owns the request's domain. Public, so the login page can be themed.")]
    public async Task<ActionResult<BrandingDto>> GetBranding(CancellationToken cancellationToken)
    {
        var query = new GetBrandingQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getBranding.Handle(query, cancellationToken));
    }

    [HttpGet("images/{kind:alpha}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    // compliance-exception: SEC-AUTHN-01 the logo and favicon are shown on the login page before sign-in
    [AllowAnonymous]
    [EndpointSummary("Get the school's logo or favicon")]
    [EndpointDescription("Returns the image, or 404 when the school uses the default one. Sent as an attachment with its checked content type, so a browser never runs it as a page.")]
    public async Task<IActionResult> GetBrandingImage(BrandingImageKind kind, CancellationToken cancellationToken)
    {
        var query = new GetBrandingImageQuery(kind);
        await rules.CheckAsync(query, cancellationToken);
        return BrandingImageResults.Image(await getImage.Handle(query, cancellationToken), kind, Response);
    }

    [HttpPut]
    [Consumes("multipart/form-data")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change the school's branding")]
    [EndpointDescription("Sets the accent colour and sidebar colour, and uploads or removes the logo and favicon of the administrator's own school. Images are checked by content: PNG, JPEG or WebP logos up to 512 KB, ICO or PNG favicons up to 100 KB.")]
    public async Task<IActionResult> UpdateBranding([FromForm] BrandingForm form, CancellationToken cancellationToken)
    {
        var command = new UpdateBrandingCommand
        {
            AccentColor = form.AccentColor,
            SidebarColor = form.SidebarColor,
            Logo = await BrandingForm.ReadAsync(form.Logo, cancellationToken),
            RemoveLogo = form.RemoveLogo,
            Favicon = await BrandingForm.ReadAsync(form.Favicon, cancellationToken),
            RemoveFavicon = form.RemoveFavicon,
        };
        await rules.CheckAsync(command, cancellationToken);
        await updateBranding.Handle(command, cancellationToken);

        return NoContent();
    }
}

/// <summary>Serves a stored logo or favicon. Shared with the master portal's endpoint for any school's images.</summary>
internal static class BrandingImageResults
{
    public static IActionResult Image(BrandingImageDto? image, BrandingImageKind kind, HttpResponse response)
    {
        if (image is null)
        {
            return new NotFoundResult();
        }

        // An <img> or icon link still shows an attachment; opening the URL downloads it instead (SEC-INPUT-04).
        response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";

        return new FileContentResult(image.Content, image.ContentType)
        {
            FileDownloadName = kind.ToString().ToLowerInvariant(),
        };
    }
}
