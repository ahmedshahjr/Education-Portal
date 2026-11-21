using EducationPortal.Application.SchoolRequests.Commands.ApproveSchoolRequest;
using EducationPortal.Application.SchoolRequests.Commands.RejectSchoolRequest;
using EducationPortal.Application.SchoolRequests.Commands.SubmitSchoolRequest;
using EducationPortal.Application.SchoolRequests.Queries.GetSchoolRequests;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// The "request a school" form, and the master tenant's queue of requests. Anyone can send a request; nothing becomes active
/// until the master tenant approves it, and approval registers the school with its 3-month (90-day) free trial.
/// </summary>
public class SchoolRequests : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(SubmitSchoolRequest)
            // compliance-exception: SEC-AUTHN-01 a school applies before it has any account; the form only creates a pending request
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.SchoolRequest)
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapGet(GetSchoolRequests)
            .RequireAuthorization();

        groupBuilder.MapPost(ApproveSchoolRequest, "{id:guid}/approve")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPost(RejectSchoolRequest, "{id:guid}/reject")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("Request a school")]
    [EndpointDescription("Sends a request for a school to join. It stays pending until the master tenant approves it.")]
    public static async Task<Created<Guid>> SubmitSchoolRequest(ISender sender, SubmitSchoolRequestCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/api/SchoolRequests/{id}", id);
    }

    [EndpointSummary("List the school requests")]
    [EndpointDescription("Returns the school requests, pending ones first. Master tenant only.")]
    public static async Task<Ok<IReadOnlyList<SchoolRequestDto>>> GetSchoolRequests(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetSchoolRequestsQuery()));
    }

    [EndpointSummary("Approve a school request")]
    [EndpointDescription("Registers the school: its domain, country, first administrator, the modules on by default and a 3-month (90-day) free trial of the Full plan. Returns the school's id. Master tenant only.")]
    public static async Task<Ok<Guid>> ApproveSchoolRequest(ISender sender, Guid id, ApproveSchoolRequestCommand command)
    {
        // The route names the request; an id in the body is ignored.
        return TypedResults.Ok(await sender.Send(command with { RequestId = id }));
    }

    [EndpointSummary("Reject a school request")]
    [EndpointDescription("Rejects a pending school request. No school and no account is created. Master tenant only.")]
    public static async Task<NoContent> RejectSchoolRequest(ISender sender, Guid id)
    {
        await sender.Send(new RejectSchoolRequestCommand { RequestId = id });

        return TypedResults.NoContent();
    }
}
