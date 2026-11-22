using EducationPortal.Application.AddOnRequests;
using EducationPortal.Application.AddOnRequests.Commands.ApproveAddOnRequest;
using EducationPortal.Application.AddOnRequests.Commands.RejectAddOnRequest;
using EducationPortal.Application.AddOnRequests.Commands.RequestAddOn;
using EducationPortal.Application.AddOnRequests.Queries.GetAddOnRequests;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Enums;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// Requests for add-on modules. A school administrator asks for a module of the school's own subscription; the master
/// tenant lists every school's requests and approves or rejects them (SEC-AUTHZ-07).
/// </summary>
[Route("api/AddOnRequests")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class AddOnRequestsController(
    IRequestRules rules,
    GetAddOnRequestsQueryHandler getRequests,
    RequestAddOnCommandHandler requestAddOn,
    ApproveAddOnRequestCommandHandler approve,
    RejectAddOnRequestCommandHandler reject) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List add-on requests")]
    [EndpointDescription("Returns every school's add-on requests, newest first; pass status to see only Pending ones. Master tenant only.")]
    public async Task<ActionResult<IReadOnlyList<AddOnRequestDto>>> GetAddOnRequests(AddOnRequestStatus? status, CancellationToken cancellationToken)
    {
        var query = new GetAddOnRequestsQuery(status);
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getRequests.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Ask for an add-on module")]
    [EndpointDescription("The signed-in school administrator asks for a paid module (moduleCode) for the school's own subscription and gets the request's id. Nothing changes until the master tenant approves it.")]
    public async Task<ActionResult<Guid>> RequestAddOn(RequestAddOnCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        return Ok(await requestAddOn.Handle(command, cancellationToken));
    }

    [HttpPost("{id:guid}/approve")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Approve an add-on request")]
    [EndpointDescription("Adds the requested module (and what it requires) to the school's subscription. Master tenant only.")]
    public async Task<IActionResult> ApproveAddOnRequest(Guid id, CancellationToken cancellationToken)
    {
        var command = new ApproveAddOnRequestCommand(id);
        await rules.CheckAsync(command, cancellationToken);
        await approve.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/reject")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Reject an add-on request")]
    [EndpointDescription("Turns the request down; the subscription is unchanged. Master tenant only.")]
    public async Task<IActionResult> RejectAddOnRequest(Guid id, CancellationToken cancellationToken)
    {
        var command = new RejectAddOnRequestCommand(id);
        await rules.CheckAsync(command, cancellationToken);
        await reject.Handle(command, cancellationToken);
        return NoContent();
    }
}
