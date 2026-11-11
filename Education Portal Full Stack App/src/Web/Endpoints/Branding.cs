using EducationPortal.Application.Branding.Commands.UpdateBranding;
using EducationPortal.Application.Branding.Common;
using EducationPortal.Application.Branding.Queries.GetBranding;
using EducationPortal.Application.Branding.Queries.GetBrandingImage;
using EducationPortal.Domain.Enums;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Endpoints;

/// <summary>The branding of the school that owns the request's domain.</summary>
public class Branding : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetBranding)
            // compliance-exception: SEC-AUTHN-01 the login page is themed before sign-in; returns only the school's name and look
            .AllowAnonymous();

        groupBuilder.MapGet(GetBrandingImage, "images/{kind}")
            // compliance-exception: SEC-AUTHN-01 the logo and favicon are shown on the login page before sign-in
            .AllowAnonymous();

        groupBuilder.MapPut(UpdateBranding, "")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("Get the school's branding")]
    [EndpointDescription("Returns the name, accent colour, sidebar colour and image versions of the school that owns the request's domain. Public, so the login page can be themed.")]
    public static async Task<Ok<BrandingDto>> GetBranding(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetBrandingQuery()));
    }

    [EndpointSummary("Get the school's logo or favicon")]
    [EndpointDescription("Returns the image, or 404 when the school uses the default one. Sent as an attachment with its checked content type, so a browser never runs it as a page.")]
    public static async Task<Results<FileContentHttpResult, NotFound>> GetBrandingImage(ISender sender, HttpContext httpContext, BrandingImageKind kind)
    {
        return ToImageResult(await sender.Send(new GetBrandingImageQuery(kind)), kind, httpContext);
    }

    /// <summary>Serves a stored logo or favicon. Shared with the master portal's endpoint for any school's images.</summary>
    internal static Results<FileContentHttpResult, NotFound> ToImageResult(BrandingImageDto? image, BrandingImageKind kind, HttpContext httpContext)
    {
        if (image is null)
        {
            return TypedResults.NotFound();
        }

        // An <img> or icon link still shows an attachment; opening the URL downloads it instead (SEC-INPUT-04).
        httpContext.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";

        return TypedResults.File(image.Content, image.ContentType, fileDownloadName: kind.ToString().ToLowerInvariant());
    }

    [EndpointSummary("Change the school's branding")]
    [EndpointDescription("Sets the accent colour and sidebar colour, and uploads or removes the logo and favicon of the administrator's own school. Images are checked by content: PNG, JPEG or WebP logos up to 512 KB, ICO or PNG favicons up to 100 KB.")]
    public static async Task<NoContent> UpdateBranding(ISender sender, [FromForm] BrandingForm form, CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateBrandingCommand
        {
            AccentColor = form.AccentColor,
            SidebarColor = form.SidebarColor,
            Logo = await BrandingForm.ReadAsync(form.Logo, cancellationToken),
            RemoveLogo = form.RemoveLogo,
            Favicon = await BrandingForm.ReadAsync(form.Favicon, cancellationToken),
            RemoveFavicon = form.RemoveFavicon,
        }, cancellationToken);

        return TypedResults.NoContent();
    }
}
