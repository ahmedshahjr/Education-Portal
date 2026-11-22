using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolPublicPage.Commands.PublishSchoolProfile;
using EducationPortal.Application.SchoolPublicPage.Commands.UnpublishSchoolProfile;
using EducationPortal.Application.SchoolPublicPage.Commands.UpdateSchoolProfile;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Application.SchoolPublicPage.Queries.GetSchoolProfile;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// The text of a school's public page, for its administrator (School module). The master tenant names the school with
/// <c>schoolId</c>; a school user may leave it out, and naming another school is a 404.
/// </summary>
[Route("api/SchoolProfile")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class SchoolProfileController(
    IRequestRules rules,
    GetSchoolProfileQueryHandler getProfile,
    UpdateSchoolProfileCommandHandler updateProfile,
    PublishSchoolProfileCommandHandler publish,
    UnpublishSchoolProfileCommandHandler unpublish) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("Get the school's public page text")]
    [EndpointDescription("Returns the tagline, about text, established year, gender policy, visiting hours, tour link and publication state. Needs the School module.")]
    public async Task<ActionResult<SchoolProfileDto>> GetSchoolProfile(Guid? schoolId, CancellationToken cancellationToken)
    {
        var query = new GetSchoolProfileQuery { SchoolId = schoolId };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getProfile.Handle(query, cancellationToken));
    }

    [HttpPut]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Save the school's public page text")]
    [EndpointDescription("Creates the page on first save, then replaces its text. Needs the School module.")]
    public async Task<IActionResult> UpdateSchoolProfile(Guid? schoolId, UpdateSchoolProfileCommand command, CancellationToken cancellationToken)
    {
        var request = command with { SchoolId = schoolId };
        await rules.CheckAsync(request, cancellationToken);
        await updateProfile.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("publish")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Publish the school's public page")]
    [EndpointDescription("Shows the page to the public. The page needs a tagline and an about text. Needs the School module.")]
    public async Task<IActionResult> PublishSchoolProfile(Guid? schoolId, CancellationToken cancellationToken)
    {
        var command = new PublishSchoolProfileCommand { SchoolId = schoolId };
        await rules.CheckAsync(command, cancellationToken);
        await publish.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("unpublish")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Unpublish the school's public page")]
    [EndpointDescription("Hides the page from the public. The text is kept. Needs the School module.")]
    public async Task<IActionResult> UnpublishSchoolProfile(Guid? schoolId, CancellationToken cancellationToken)
    {
        var command = new UnpublishSchoolProfileCommand { SchoolId = schoolId };
        await rules.CheckAsync(command, cancellationToken);
        await unpublish.Handle(command, cancellationToken);
        return NoContent();
    }
}
