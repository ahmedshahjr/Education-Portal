using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Plans.Commands.CreatePlan;
using EducationPortal.Application.Plans.Commands.DeletePlan;
using EducationPortal.Application.Plans.Commands.UpdatePlan;
using EducationPortal.Application.Plans.Common;
using EducationPortal.Application.Plans.Queries.GetPlan;
using EducationPortal.Application.Plans.Queries.GetPlans;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// The plans the platform sells (for example Starter, Standard, Full): named bundles of paid modules. Master tenant
/// administrators only (SEC-AUTHZ-07). Prices live in the price books, see <c>/api/PriceBooks</c>.
/// </summary>
[Route("api/Plans")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class PlansController(
    IRequestRules rules,
    GetPlansQueryHandler getPlans,
    GetPlanQueryHandler getPlan,
    CreatePlanCommandHandler createPlan,
    UpdatePlanCommandHandler updatePlan,
    DeletePlanCommandHandler deletePlan) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List plans")]
    [EndpointDescription("Returns the plans by name, each with its code, name, description, whether it is active and the codes of its paid modules. Core modules are in every plan and are not listed. Master tenant only.")]
    public async Task<ActionResult<IReadOnlyList<PlanDto>>> GetPlans(CancellationToken cancellationToken)
    {
        var query = new GetPlansQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getPlans.Handle(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get a plan")]
    [EndpointDescription("Returns one plan with its paid modules. Master tenant only.")]
    public async Task<ActionResult<PlanDto>> GetPlan(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPlanQuery(id);
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getPlan.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Add a plan")]
    [EndpointDescription("Adds a plan with a new code (upper-case letters, digits and underscores, for example STARTER), a name, a description and the codes of its paid modules, and returns its id. Core modules cannot be listed. Prices are set per price book. Master tenant only.")]
    public async Task<ActionResult<Guid>> CreatePlan(CreatePlanCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        return Ok(await createPlan.Handle(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change a plan")]
    [EndpointDescription("Sets the plan's name, description, active switch and paid modules (the list replaces the current one). An inactive plan cannot be chosen for new subscriptions. The code cannot change. Master tenant only.")]
    public async Task<IActionResult> UpdatePlan(Guid id, UpdatePlanCommand command, CancellationToken cancellationToken)
    {
        // The route names the plan; an id in the body is ignored.
        var request = command with { Id = id };
        await rules.CheckAsync(request, cancellationToken);
        await updatePlan.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Delete a plan")]
    [EndpointDescription("Deletes a plan and its prices. The row stays for audit and the code can be used again. Master tenant only.")]
    public async Task<IActionResult> DeletePlan(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeletePlanCommand(id);
        await rules.CheckAsync(command, cancellationToken);
        await deletePlan.Handle(command, cancellationToken);
        return NoContent();
    }
}
