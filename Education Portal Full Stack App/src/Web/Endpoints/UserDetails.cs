using EducationPortal.Application.Users.Commands.CreateUserDetails;
using EducationPortal.Application.Users.Commands.DeleteUserDetails;
using EducationPortal.Application.Users.Commands.UpdateUserDetails;
using EducationPortal.Application.Users.Dtos;
using EducationPortal.Application.Users.Queries.GetUserDetails;
using EducationPortal.Domain.Enums;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// The people of the current school (principals, teachers and other staff), for its administrators (Staff module). The school
/// is the one the request's host resolves to, so a school never reaches another school's people (AGENTS.md section 2).
/// </summary>
public class UserDetails : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetUserDetails)
            .RequireAuthorization();

        groupBuilder.MapPost(CreateUserDetails)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdateUserDetails, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(DeleteUserDetails, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List the school's people")]
    [EndpointDescription("Returns the current school's people, sorted by name. Pass userType to list only one type, for example Principal. Needs the Staff module.")]
    public static async Task<Ok<IReadOnlyList<UserDetailsDto>>> GetUserDetails(ISender sender, UserType? userType)
    {
        return TypedResults.Ok(await sender.Send(new GetUserDetailsQuery { UserType = userType }));
    }

    [EndpointSummary("Add a person")]
    [EndpointDescription("Adds a principal, teacher or other staff member to the current school. Needs the Staff module.")]
    public static async Task<Ok<Guid>> CreateUserDetails(ISender sender, CreateUserDetailsCommand command)
    {
        return TypedResults.Ok(await sender.Send(command));
    }

    [EndpointSummary("Change a person")]
    [EndpointDescription("Changes a person's name, type and phone. Needs the Staff module.")]
    public static async Task<NoContent> UpdateUserDetails(ISender sender, Guid id, UpdateUserDetailsCommand command)
    {
        // The route names the person; an id in the body is ignored.
        await sender.Send(command with { Id = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a person")]
    [EndpointDescription("Deletes a person (kept for audit, hidden everywhere). A principal of a campus cannot be deleted until another principal is chosen. Needs the Staff module.")]
    public static async Task<NoContent> DeleteUserDetails(ISender sender, Guid id)
    {
        await sender.Send(new DeleteUserDetailsCommand { Id = id });

        return TypedResults.NoContent();
    }
}
