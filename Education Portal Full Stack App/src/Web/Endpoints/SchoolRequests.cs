using EducationPortal.Application.SchoolRequests.Commands.SubmitSchoolRequest;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>The public "request a school" form. Anyone can send one; nothing becomes active until the master tenant approves it.</summary>
public class SchoolRequests : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(SubmitSchoolRequest)
            // compliance-exception: SEC-AUTHN-01 a school applies before it has any account; the form only creates a pending request
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.SchoolRequest)
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("Request a school")]
    [EndpointDescription("Sends a request for a school to join. It stays pending until the master tenant approves it.")]
    public static async Task<Created<Guid>> SubmitSchoolRequest(ISender sender, SubmitSchoolRequestCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/api/SchoolRequests/{id}", id);
    }
}
