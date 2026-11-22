using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolPublicPage.Commands.RemoveFacility;
using EducationPortal.Application.SchoolPublicPage.Commands.SetFacility;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Application.SchoolPublicPage.Queries.GetFacilities;
using EducationPortal.Domain.Enums;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>The facilities a school lists on its public page, one per kind (School module). The master tenant names the school with <c>schoolId</c>.</summary>
[Route("api/SchoolFacilities")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class SchoolFacilitiesController(
    IRequestRules rules,
    GetFacilitiesQueryHandler getFacilities,
    SetFacilityCommandHandler setFacility,
    RemoveFacilityCommandHandler removeFacility) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List the school's facilities")]
    [EndpointDescription("Returns the facilities the school lists on its public page, with their notes. Needs the School module.")]
    public async Task<ActionResult<IReadOnlyList<SchoolFacilityDto>>> GetFacilities(Guid? schoolId, CancellationToken cancellationToken)
    {
        var query = new GetFacilitiesQuery { SchoolId = schoolId };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getFacilities.Handle(query, cancellationToken));
    }

    [HttpPut("{kind}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Add or change a facility")]
    [EndpointDescription("Lists a facility kind on the public page, or changes its note when already listed. The kind is in the route. Needs the School module.")]
    public async Task<IActionResult> SetFacility(Guid? schoolId, FacilityKind kind, SetFacilityCommand command, CancellationToken cancellationToken)
    {
        var request = command with { SchoolId = schoolId, Kind = kind };
        await rules.CheckAsync(request, cancellationToken);
        await setFacility.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{kind}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Remove a facility")]
    [EndpointDescription("Stops listing a facility kind on the public page. Needs the School module.")]
    public async Task<IActionResult> RemoveFacility(Guid? schoolId, FacilityKind kind, CancellationToken cancellationToken)
    {
        var command = new RemoveFacilityCommand { SchoolId = schoolId, Kind = kind };
        await rules.CheckAsync(command, cancellationToken);
        await removeFacility.Handle(command, cancellationToken);
        return NoContent();
    }
}
