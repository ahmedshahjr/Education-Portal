using EducationPortal.Application.Plans.Commands.CreatePlan;
using EducationPortal.Application.Plans.Commands.DeletePlan;
using EducationPortal.Application.Plans.Commands.UpdatePlan;
using EducationPortal.Application.Plans.Common;
using EducationPortal.Application.Plans.Queries.GetPlan;
using EducationPortal.Application.Plans.Queries.GetPlans;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// The plans the platform sells (for example Starter, Standard, Full): named bundles of paid modules. Master tenant
/// administrators only (SEC-AUTHZ-07). Prices live in the price books, see <c>/api/PriceBooks</c>.
/// </summary>
public class Plans : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetPlans)
            .RequireAuthorization();

        groupBuilder.MapGet(GetPlan, "{id:guid}")
            .RequireAuthorization();

        groupBuilder.MapPost(CreatePlan)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdatePlan, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(DeletePlan, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List plans")]
    [EndpointDescription("Returns the plans by name, each with its code, name, description, whether it is active and the codes of its paid modules. Core modules are in every plan and are not listed. Master tenant only.")]
    public static async Task<Ok<IReadOnlyList<PlanDto>>> GetPlans(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetPlansQuery()));
    }

    [EndpointSummary("Get a plan")]
    [EndpointDescription("Returns one plan with its paid modules. Master tenant only.")]
    public static async Task<Ok<PlanDto>> GetPlan(ISender sender, Guid id)
    {
        return TypedResults.Ok(await sender.Send(new GetPlanQuery(id)));
    }

    [EndpointSummary("Add a plan")]
    [EndpointDescription("Adds a plan with a new code (upper-case letters, digits and underscores, for example STARTER), a name, a description and the codes of its paid modules, and returns its id. Core modules cannot be listed. Prices are set per price book. Master tenant only.")]
    public static async Task<Ok<Guid>> CreatePlan(ISender sender, CreatePlanCommand command)
    {
        return TypedResults.Ok(await sender.Send(command));
    }

    [EndpointSummary("Change a plan")]
    [EndpointDescription("Sets the plan's name, description, active switch and paid modules (the list replaces the current one). An inactive plan cannot be chosen for new subscriptions. The code cannot change. Master tenant only.")]
    public static async Task<NoContent> UpdatePlan(ISender sender, Guid id, UpdatePlanCommand command)
    {
        // The route names the plan; an id in the body is ignored.
        await sender.Send(command with { Id = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a plan")]
    [EndpointDescription("Deletes a plan and its prices. The row stays for audit and the code can be used again. Master tenant only.")]
    public static async Task<NoContent> DeletePlan(ISender sender, Guid id)
    {
        await sender.Send(new DeletePlanCommand(id));

        return TypedResults.NoContent();
    }
}
