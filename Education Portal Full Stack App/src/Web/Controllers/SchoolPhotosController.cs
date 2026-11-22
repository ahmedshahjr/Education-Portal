using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolPublicPage.Commands.DeleteSchoolPhoto;
using EducationPortal.Application.SchoolPublicPage.Commands.UpdateSchoolPhoto;
using EducationPortal.Application.SchoolPublicPage.Commands.UploadSchoolPhoto;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Application.SchoolPublicPage.Queries.GetSchoolPhotoContent;
using EducationPortal.Application.SchoolPublicPage.Queries.GetSchoolPhotos;
using EducationPortal.Domain.Entities;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// The gallery of a school's public page (School module). The master tenant names the school with <c>schoolId</c>. Photos
/// are PNG, JPEG or WebP of at most 2 MB, checked by their bytes (SEC-INPUT-04).
/// </summary>
[Route("api/SchoolPhotos")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class SchoolPhotosController(
    IRequestRules rules,
    GetSchoolPhotosQueryHandler getPhotos,
    GetSchoolPhotoContentQueryHandler getPhotoContent,
    UploadSchoolPhotoCommandHandler uploadPhoto,
    UpdateSchoolPhotoCommandHandler updatePhoto,
    DeleteSchoolPhotoCommandHandler deletePhoto) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List the school's gallery photos")]
    [EndpointDescription("Returns the photos in their order, without the image bytes. Needs the School module.")]
    public async Task<ActionResult<IReadOnlyList<SchoolPhotoDto>>> GetSchoolPhotos(Guid? schoolId, CancellationToken cancellationToken)
    {
        var query = new GetSchoolPhotosQuery { SchoolId = schoolId };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getPhotos.Handle(query, cancellationToken));
    }

    [HttpGet("{id:guid}/content")]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a gallery photo's image")]
    [EndpointDescription("Returns the image with its checked content type and an ETag for its version. Needs the School module.")]
    public async Task<IActionResult> GetSchoolPhotoContent(Guid? schoolId, Guid id, CancellationToken cancellationToken)
    {
        var query = new GetSchoolPhotoContentQuery { SchoolId = schoolId, PhotoId = id };
        await rules.CheckAsync(query, cancellationToken);
        var photo = await getPhotoContent.Handle(query, cancellationToken);
        if (photo is null)
        {
            return NotFound();
        }

        // The platform rule for /api (SecurityHeaders) sets Cache-Control to no-store; the ETag only tells clients the version.
        Response.Headers.ETag = $"\"{photo.Version}\"";
        return File(photo.Content, photo.ContentType);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [RequestSizeLimit(SchoolPhoto.MaxBytes + SchoolPhotoForm.EnvelopeAllowance)]
    [EndpointSummary("Upload a gallery photo")]
    [EndpointDescription("Adds a PNG, JPEG or WebP photo of at most 2 MB to the gallery. Needs the School module.")]
    public async Task<ActionResult<Guid>> UploadSchoolPhoto(Guid? schoolId, [FromForm] SchoolPhotoForm form, CancellationToken cancellationToken)
    {
        var command = new UploadSchoolPhotoCommand
        {
            SchoolId = schoolId,
            CampusId = form.CampusId,
            Kind = form.Kind,
            Caption = form.Caption,
            SortOrder = form.SortOrder,
            Content = await SchoolPhotoForm.ReadAsync(form.File, cancellationToken) ?? [],
        };
        await rules.CheckAsync(command, cancellationToken);
        return Ok(await uploadPhoto.Handle(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [RequestSizeLimit(SchoolPhoto.MaxBytes + SchoolPhotoForm.EnvelopeAllowance)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change a gallery photo")]
    [EndpointDescription("Changes the caption and order, and replaces the image when a new file is sent. Needs the School module.")]
    public async Task<IActionResult> UpdateSchoolPhoto(Guid? schoolId, Guid id, [FromForm] SchoolPhotoForm form, CancellationToken cancellationToken)
    {
        var command = new UpdateSchoolPhotoCommand
        {
            SchoolId = schoolId,
            PhotoId = id,
            Caption = form.Caption,
            SortOrder = form.SortOrder,
            Content = await SchoolPhotoForm.ReadAsync(form.File, cancellationToken),
        };
        await rules.CheckAsync(command, cancellationToken);
        await updatePhoto.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Delete a gallery photo")]
    [EndpointDescription("Deletes a gallery photo (kept for audit, hidden everywhere). Needs the School module.")]
    public async Task<IActionResult> DeleteSchoolPhoto(Guid? schoolId, Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteSchoolPhotoCommand { SchoolId = schoolId, PhotoId = id };
        await rules.CheckAsync(command, cancellationToken);
        await deletePhoto.Handle(command, cancellationToken);
        return NoContent();
    }
}
