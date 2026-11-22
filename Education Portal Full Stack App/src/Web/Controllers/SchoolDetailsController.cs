using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Schools.Commands.UpdateSchoolDetails;
using EducationPortal.Application.Schools.Queries.GetSchoolDetails;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>A school's registration and contact details (School module). The master tenant names the school with <c>schoolId</c>.</summary>
[Route("api/SchoolDetails")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class SchoolDetailsController(
    IRequestRules rules,
    GetSchoolDetailsQueryHandler getSchoolDetails,
    UpdateSchoolDetailsCommandHandler updateSchoolDetails) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("Read the school's details")]
    [EndpointDescription("Returns the school's registration and contact details. Needs the School module.")]
    public async Task<ActionResult<SchoolDetailsDto>> GetSchoolDetails(Guid? schoolId, CancellationToken cancellationToken)
    {
        var query = new GetSchoolDetailsQuery { SchoolId = schoolId };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getSchoolDetails.Handle(query, cancellationToken));
    }

    [HttpPut]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change the school's details")]
    [EndpointDescription("Replaces the school's registration and contact details. The city must be in the school's country. Needs the School module.")]
    public async Task<IActionResult> UpdateSchoolDetails(Guid? schoolId, UpdateSchoolDetailsCommand command, CancellationToken cancellationToken)
    {
        var request = command with { SchoolId = schoolId };
        await rules.CheckAsync(request, cancellationToken);
        await updateSchoolDetails.Handle(request, cancellationToken);
        return NoContent();
    }
}
