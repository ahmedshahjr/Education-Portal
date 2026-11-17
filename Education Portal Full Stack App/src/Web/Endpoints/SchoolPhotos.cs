using EducationPortal.Application.SchoolPublicPage.Commands.DeleteSchoolPhoto;
using EducationPortal.Application.SchoolPublicPage.Commands.UpdateSchoolPhoto;
using EducationPortal.Application.SchoolPublicPage.Commands.UploadSchoolPhoto;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Application.SchoolPublicPage.Queries.GetSchoolPhotoContent;
using EducationPortal.Application.SchoolPublicPage.Queries.GetSchoolPhotos;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// The gallery of a school's public page (School module). The master tenant names the school with <c>schoolId</c>. Photos
/// are PNG, JPEG or WebP of at most 2 MB, checked by their bytes (SEC-INPUT-04).
/// </summary>
public class SchoolPhotos : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetSchoolPhotos)
            .RequireAuthorization();

        groupBuilder.MapGet(GetSchoolPhotoContent, "{id:guid}/content")
            .RequireAuthorization();

        groupBuilder.MapPost(UploadSchoolPhoto)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdateSchoolPhoto, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(DeleteSchoolPhoto, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List the school's gallery photos")]
    [EndpointDescription("Returns the photos in their order, without the image bytes. Needs the School module.")]
    public static async Task<Ok<IReadOnlyList<SchoolPhotoDto>>> GetSchoolPhotos(ISender sender, Guid? schoolId)
    {
        return TypedResults.Ok(await sender.Send(new GetSchoolPhotosQuery { SchoolId = schoolId }));
    }

    [EndpointSummary("Get a gallery photo's image")]
    [EndpointDescription("Returns the image with its checked content type and an ETag for its version. Needs the School module.")]
    public static async Task<Results<FileContentHttpResult, NotFound>> GetSchoolPhotoContent(ISender sender, HttpContext httpContext, Guid? schoolId, Guid id)
    {
        var photo = await sender.Send(new GetSchoolPhotoContentQuery { SchoolId = schoolId, PhotoId = id });
        if (photo is null)
        {
            return TypedResults.NotFound();
        }

        // The platform rule for /api (SecurityHeaders) sets Cache-Control to no-store; the ETag only tells clients the version.
        httpContext.Response.Headers.ETag = $"\"{photo.Version}\"";

        return TypedResults.File(photo.Content, photo.ContentType);
    }

    [EndpointSummary("Upload a gallery photo")]
    [EndpointDescription("Adds a PNG, JPEG or WebP photo of at most 2 MB to the gallery. Needs the School module.")]
    [RequestSizeLimit(SchoolPhoto.MaxBytes + SchoolPhotoForm.EnvelopeAllowance)]
    public static async Task<Ok<Guid>> UploadSchoolPhoto(ISender sender, Guid? schoolId, [FromForm] SchoolPhotoForm form, CancellationToken cancellationToken)
    {
        var id = await sender.Send(new UploadSchoolPhotoCommand
        {
            SchoolId = schoolId,
            CampusId = form.CampusId,
            Kind = form.Kind,
            Caption = form.Caption,
            SortOrder = form.SortOrder,
            Content = await SchoolPhotoForm.ReadAsync(form.File, cancellationToken) ?? [],
        }, cancellationToken);

        return TypedResults.Ok(id);
    }

    [EndpointSummary("Change a gallery photo")]
    [EndpointDescription("Changes the caption and order, and replaces the image when a new file is sent. Needs the School module.")]
    [RequestSizeLimit(SchoolPhoto.MaxBytes + SchoolPhotoForm.EnvelopeAllowance)]
    public static async Task<NoContent> UpdateSchoolPhoto(ISender sender, Guid? schoolId, Guid id, [FromForm] SchoolPhotoForm form, CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateSchoolPhotoCommand
        {
            SchoolId = schoolId,
            PhotoId = id,
            Caption = form.Caption,
            SortOrder = form.SortOrder,
            Content = await SchoolPhotoForm.ReadAsync(form.File, cancellationToken),
        }, cancellationToken);

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a gallery photo")]
    [EndpointDescription("Deletes a gallery photo (kept for audit, hidden everywhere). Needs the School module.")]
    public static async Task<NoContent> DeleteSchoolPhoto(ISender sender, Guid? schoolId, Guid id)
    {
        await sender.Send(new DeleteSchoolPhotoCommand { SchoolId = schoolId, PhotoId = id });

        return TypedResults.NoContent();
    }
}

/// <summary>
/// The multipart form of a gallery photo. The file is read into memory; the request size limit (2 MB plus the form's other
/// fields) stops larger bodies before they are read, and the validator checks the real type and size.
/// </summary>
public sealed class SchoolPhotoForm
{
    /// <summary>Room for the other form fields on top of the image.</summary>
    public const int EnvelopeAllowance = 64 * 1024;

    public Guid? CampusId { get; init; }

    public PhotoKind Kind { get; init; }

    public string? Caption { get; init; }

    public int SortOrder { get; init; }

    public IFormFile? File { get; init; }

    internal static async Task<byte[]?> ReadAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null)
        {
            return null;
        }

        // Read at most one byte over the cap, so an oversized file is refused by the validator without being held in full.
        await using var stream = file.OpenReadStream();
        var limited = new byte[SchoolPhoto.MaxBytes + 1];
        var total = 0;
        int read;
        while ((read = await stream.ReadAsync(limited.AsMemory(total, limited.Length - total), cancellationToken)) > 0)
        {
            total += read;
            if (total == limited.Length)
            {
                break;
            }
        }

        return limited.AsSpan(0, total).ToArray();
    }
}
