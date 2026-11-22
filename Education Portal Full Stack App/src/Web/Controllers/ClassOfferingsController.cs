using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolPublicPage.Commands.CreateClassOffering;
using EducationPortal.Application.SchoolPublicPage.Commands.DeleteClassOffering;
using EducationPortal.Application.SchoolPublicPage.Commands.UpdateClassOffering;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Application.SchoolPublicPage.Queries.GetClassOfferings;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>A school's class offerings shown on its public page (School module). The master tenant names the school with <c>schoolId</c>.</summary>
[Route("api/ClassOfferings")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class ClassOfferingsController(
    IRequestRules rules,
    GetClassOfferingsQueryHandler getOfferings,
    CreateClassOfferingCommandHandler createOffering,
    UpdateClassOfferingCommandHandler updateOffering,
    DeleteClassOfferingCommandHandler deleteOffering) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List the school's class offerings")]
    [EndpointDescription("Returns the class offerings of the school that are not deleted. Needs the School module.")]
    public async Task<ActionResult<IReadOnlyList<ClassOfferingDto>>> GetClassOfferings(Guid? schoolId, CancellationToken cancellationToken)
    {
        var query = new GetClassOfferingsQuery { SchoolId = schoolId };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getOfferings.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Add a class offering")]
    [EndpointDescription("Adds a class offering for the whole school or one campus. Needs the School module.")]
    public async Task<ActionResult<Guid>> CreateClassOffering(Guid? schoolId, CreateClassOfferingCommand command, CancellationToken cancellationToken)
    {
        var request = command with { SchoolId = schoolId };
        await rules.CheckAsync(request, cancellationToken);
        return Ok(await createOffering.Handle(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change a class offering")]
    [EndpointDescription("Changes the year, fees, seats, status and fee display of an offering. Needs the School module.")]
    public async Task<IActionResult> UpdateClassOffering(Guid? schoolId, Guid id, UpdateClassOfferingCommand command, CancellationToken cancellationToken)
    {
        // The route names the offering; an id in the body is ignored.
        var request = command with { SchoolId = schoolId, OfferingId = id };
        await rules.CheckAsync(request, cancellationToken);
        await updateOffering.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Delete a class offering")]
    [EndpointDescription("Deletes a class offering (kept for audit, hidden everywhere). Needs the School module.")]
    public async Task<IActionResult> DeleteClassOffering(Guid? schoolId, Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteClassOfferingCommand { SchoolId = schoolId, OfferingId = id };
        await rules.CheckAsync(command, cancellationToken);
        await deleteOffering.Handle(command, cancellationToken);
        return NoContent();
    }
}
