using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolRequests.Commands.ApproveSchoolRequest;
using EducationPortal.Application.SchoolRequests.Commands.RejectSchoolRequest;
using EducationPortal.Application.SchoolRequests.Commands.SubmitSchoolRequest;
using EducationPortal.Application.SchoolRequests.Queries.GetSchoolRequests;
using EducationPortal.Web.Filters;
using EducationPortal.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// The "request a school" form, and the master tenant's queue of requests. Anyone can send a request; nothing becomes active
/// until the master tenant approves it, and approval registers the school with its 3-month (90-day) free trial.
/// </summary>
[Route("api/SchoolRequests")]
public class SchoolRequestsController(
    IRequestRules rules,
    SubmitSchoolRequestCommandHandler submit,
    GetSchoolRequestsQueryHandler getRequests,
    ApproveSchoolRequestCommandHandler approve,
    RejectSchoolRequestCommandHandler reject) : ApiControllerBase
{
    [HttpPost]
    // compliance-exception: SEC-AUTHN-01 a school applies before it has any account; the form only creates a pending request
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.SchoolRequest)]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Request a school")]
    [EndpointDescription("Sends a request for a school to join. It stays pending until the master tenant approves it.")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> SubmitSchoolRequest(SubmitSchoolRequestCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        var id = await submit.Handle(command, cancellationToken);
        return Created($"/api/SchoolRequests/{id}", id);
    }

    [HttpGet]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [EndpointSummary("List the school requests")]
    [EndpointDescription("Returns the school requests, pending ones first. Master tenant only.")]
    public async Task<ActionResult<IReadOnlyList<SchoolRequestDto>>> GetSchoolRequests(CancellationToken cancellationToken)
    {
        var query = new GetSchoolRequestsQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getRequests.Handle(query, cancellationToken));
    }

    [HttpPost("{id:guid}/approve")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Approve a school request")]
    [EndpointDescription("Registers the school: its domain, country, first administrator, the modules on by default and a 3-month (90-day) free trial of the Full plan. Returns the school's id. Master tenant only.")]
    public async Task<ActionResult<Guid>> ApproveSchoolRequest(Guid id, ApproveSchoolRequestCommand command, CancellationToken cancellationToken)
    {
        // The route names the request; an id in the body is ignored.
        var request = command with { RequestId = id };
        await rules.CheckAsync(request, cancellationToken);
        return Ok(await approve.Handle(request, cancellationToken));
    }

    [HttpPost("{id:guid}/reject")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Reject a school request")]
    [EndpointDescription("Rejects a pending school request. No school and no account is created. Master tenant only.")]
    public async Task<IActionResult> RejectSchoolRequest(Guid id, CancellationToken cancellationToken)
    {
        var command = new RejectSchoolRequestCommand { RequestId = id };
        await rules.CheckAsync(command, cancellationToken);
        await reject.Handle(command, cancellationToken);
        return NoContent();
    }
}
