using EducationPortal.Application.SchoolRoles.Commands.CreateRole;
using EducationPortal.Application.SchoolRoles.Commands.DeleteRole;
using EducationPortal.Application.SchoolRoles.Commands.UpdateRole;
using EducationPortal.Application.SchoolRoles.Common;
using EducationPortal.Application.SchoolRoles.Queries.GetRoles;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// The current school's roles and the rights they give per module. School administrators only, and only while the
/// school has the Users &amp; Roles module (403 MODULE_NOT_ENABLED otherwise). The school comes from the host.
/// </summary>
public class SchoolRoles : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetRoles)
            .RequireAuthorization();

        groupBuilder.MapPost(CreateRole)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdateRole, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(DeleteRole, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List the school's roles")]
    [EndpointDescription("Returns the current school's roles by name, each with its rights per module. Rights in modules the school does not have now are not shown. School administrators only.")]
    public static async Task<Ok<IReadOnlyList<SchoolRoleDto>>> GetRoles(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetRolesQuery()));
    }

    [EndpointSummary("Add a role")]
    [EndpointDescription("Adds a role with a name unique in the school and its rights per module code, and returns its id. Creating, editing and deleting need the right to view, and only modules the school has can be listed.")]
    public static async Task<Ok<Guid>> CreateRole(ISender sender, CreateRoleCommand command)
    {
        return TypedResults.Ok(await sender.Send(command));
    }

    [EndpointSummary("Change a role")]
    [EndpointDescription("Renames the role and sets its rights. Rights in modules the school does not have now are kept as they are.")]
    public static async Task<NoContent> UpdateRole(ISender sender, Guid id, UpdateRoleCommand command)
    {
        // The route names the role; an id in the body is ignored.
        await sender.Send(command with { Id = id });
        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a role")]
    [EndpointDescription("Deletes the role and its rights.")]
    public static async Task<NoContent> DeleteRole(ISender sender, Guid id)
    {
        await sender.Send(new DeleteRoleCommand(id));
        return TypedResults.NoContent();
    }
}
