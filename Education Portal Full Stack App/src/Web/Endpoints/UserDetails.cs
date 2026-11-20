using EducationPortal.Application.Parents.Commands.LinkStudent;
using EducationPortal.Application.Parents.Commands.UnlinkStudent;
using EducationPortal.Application.Parents.Queries.GetChildren;
using EducationPortal.Application.Users.Commands.CreateUserDetails;
using EducationPortal.Application.Users.Commands.DeleteUserDetails;
using EducationPortal.Application.Users.Commands.SetAccountRoles;
using EducationPortal.Application.Users.Commands.UpdateUserDetails;
using EducationPortal.Application.Users.Dtos;
using EducationPortal.Application.Users.Queries.GetAccountRoles;
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

        groupBuilder.MapGet(GetChildren, "{id:guid}/children")
            .RequireAuthorization();

        groupBuilder.MapPost(LinkStudent, "{id:guid}/children")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(UnlinkStudent, "{id:guid}/children/{studentId:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapGet(GetAccountRoles, "{id:guid}/roles")
            .RequireAuthorization();

        groupBuilder.MapPut(SetAccountRoles, "{id:guid}/roles")
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

    [EndpointSummary("List a parent's students")]
    [EndpointDescription("Returns the students linked to a parent of the school. Needs the Staff module.")]
    public static async Task<Ok<IReadOnlyList<UserDetailsDto>>> GetChildren(ISender sender, Guid id)
    {
        return TypedResults.Ok(await sender.Send(new GetChildrenQuery { ParentId = id }));
    }

    [EndpointSummary("Link a student to a parent")]
    [EndpointDescription("Links a student (studentId) of the school to a parent. Both must be people of the school, a parent and a student. Needs the Staff module.")]
    public static async Task<NoContent> LinkStudent(ISender sender, Guid id, LinkStudentCommand command)
    {
        // The route names the parent; an id in the body is ignored.
        await sender.Send(command with { ParentId = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Unlink a student from a parent")]
    [EndpointDescription("Removes the link between a parent and a student. Needs the Staff module.")]
    public static async Task<NoContent> UnlinkStudent(ISender sender, Guid id, Guid studentId)
    {
        await sender.Send(new UnlinkStudentCommand { ParentId = id, StudentId = studentId });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Get a person's roles")]
    [EndpointDescription("Returns the public ids of the school roles the person's login account holds. Needs the Users & Roles module.")]
    public static async Task<Ok<IReadOnlyList<Guid>>> GetAccountRoles(ISender sender, Guid id)
    {
        return TypedResults.Ok(await sender.Send(new GetAccountRolesQuery { PersonId = id }));
    }

    [EndpointSummary("Set a person's roles")]
    [EndpointDescription("Sets exactly the school roles (roleIds) the person's login account holds. The person must have an account. Needs the Users & Roles module.")]
    public static async Task<NoContent> SetAccountRoles(ISender sender, Guid id, SetAccountRolesCommand command)
    {
        // The route names the person; an id in the body is ignored.
        await sender.Send(command with { PersonId = id });

        return TypedResults.NoContent();
    }
}
