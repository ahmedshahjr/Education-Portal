using EducationPortal.Application.Parents.Queries.GetMyChildren;
using EducationPortal.Application.Users.Dtos;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>A parent's own view of their children. A parent sees only the students linked to them (SEC-AUTHZ-04).</summary>
public class Parents : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetMyChildren, "me/children")
            .RequireAuthorization();
    }

    [EndpointSummary("List my children")]
    [EndpointDescription("Returns the students linked to the signed-in parent, and no one else's. Needs the Students module, and the one-time password must have been changed.")]
    public static async Task<Ok<IReadOnlyList<UserDetailsDto>>> GetMyChildren(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetMyChildrenQuery()));
    }
}
