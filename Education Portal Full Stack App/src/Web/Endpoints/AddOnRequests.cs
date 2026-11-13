using EducationPortal.Application.AddOnRequests;
using EducationPortal.Application.AddOnRequests.Commands.ApproveAddOnRequest;
using EducationPortal.Application.AddOnRequests.Commands.RejectAddOnRequest;
using EducationPortal.Application.AddOnRequests.Commands.RequestAddOn;
using EducationPortal.Application.AddOnRequests.Queries.GetAddOnRequests;
using EducationPortal.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// Requests for add-on modules. A school administrator asks for a module of the school's own subscription; the master
/// tenant lists every school's requests and approves or rejects them (SEC-AUTHZ-07).
/// </summary>
public class AddOnRequests : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetAddOnRequests)
            .RequireAuthorization();

        groupBuilder.MapPost(RequestAddOn)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPost(ApproveAddOnRequest, "{id:guid}/approve")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPost(RejectAddOnRequest, "{id:guid}/reject")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List add-on requests")]
    [EndpointDescription("Returns every school's add-on requests, newest first; pass status to see only Pending ones. Master tenant only.")]
    public static async Task<Ok<IReadOnlyList<AddOnRequestDto>>> GetAddOnRequests(ISender sender, AddOnRequestStatus? status)
    {
        return TypedResults.Ok(await sender.Send(new GetAddOnRequestsQuery(status)));
    }

    [EndpointSummary("Ask for an add-on module")]
    [EndpointDescription("The signed-in school administrator asks for a paid module (moduleCode) for the school's own subscription and gets the request's id. Nothing changes until the master tenant approves it.")]
    public static async Task<Ok<Guid>> RequestAddOn(ISender sender, RequestAddOnCommand command)
    {
        return TypedResults.Ok(await sender.Send(command));
    }

    [EndpointSummary("Approve an add-on request")]
    [EndpointDescription("Adds the requested module (and what it requires) to the school's subscription. Master tenant only.")]
    public static async Task<NoContent> ApproveAddOnRequest(ISender sender, Guid id)
    {
        await sender.Send(new ApproveAddOnRequestCommand(id));

        return TypedResults.NoContent();
    }

    [EndpointSummary("Reject an add-on request")]
    [EndpointDescription("Turns the request down; the subscription is unchanged. Master tenant only.")]
    public static async Task<NoContent> RejectAddOnRequest(ISender sender, Guid id)
    {
        await sender.Send(new RejectAddOnRequestCommand(id));

        return TypedResults.NoContent();
    }
}
