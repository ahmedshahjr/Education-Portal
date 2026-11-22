using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolRoles.Commands.CreateRole;
using EducationPortal.Application.SchoolRoles.Commands.DeleteRole;
using EducationPortal.Application.SchoolRoles.Commands.EnsureBuiltInRoles;
using EducationPortal.Application.SchoolRoles.Commands.UpdateRole;
using EducationPortal.Application.SchoolRoles.Common;
using EducationPortal.Application.SchoolRoles.Queries.GetRoles;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// The current school's roles and the rights they give per module. School administrators only, and only while the
/// school has the Users &amp; Roles module (403 MODULE_NOT_ENABLED otherwise). The school comes from the host.
/// </summary>
[Route("api/SchoolRoles")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class SchoolRolesController(
    IRequestRules rules,
    GetRolesQueryHandler getRoles,
    CreateRoleCommandHandler createRole,
    UpdateRoleCommandHandler updateRole,
    DeleteRoleCommandHandler deleteRole,
    EnsureBuiltInRolesCommandHandler ensureBuiltInRoles) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List the school's roles")]
    [EndpointDescription("Returns the current school's roles by name, each with its rights per module. Rights in modules the school does not have now are not shown. School administrators only.")]
    public async Task<ActionResult<IReadOnlyList<SchoolRoleDto>>> GetRoles(CancellationToken cancellationToken)
    {
        var query = new GetRolesQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getRoles.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Add a role")]
    [EndpointDescription("Adds a role with a name unique in the school and its rights per module code, and returns its id. Creating, editing and deleting need the right to view, and only modules the school has can be listed.")]
    public async Task<ActionResult<Guid>> CreateRole(CreateRoleCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        return Ok(await createRole.Handle(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change a role")]
    [EndpointDescription("Renames the role and sets its rights. Rights in modules the school does not have now are kept as they are.")]
    public async Task<IActionResult> UpdateRole(Guid id, UpdateRoleCommand command, CancellationToken cancellationToken)
    {
        // The route names the role; an id in the body is ignored.
        var request = command with { Id = id };
        await rules.CheckAsync(request, cancellationToken);
        await updateRole.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Delete a role")]
    [EndpointDescription("Deletes the role and its rights.")]
    public async Task<IActionResult> DeleteRole(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteRoleCommand(id);
        await rules.CheckAsync(command, cancellationToken);
        await deleteRole.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("built-in")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Add the built-in roles")]
    [EndpointDescription("Adds the built-in roles (Principal, Teacher, Student, Parent) that the school does not have yet, with their starting rights. Running it again changes nothing. Needs the Users & Roles module.")]
    public async Task<IActionResult> EnsureBuiltInRoles(CancellationToken cancellationToken)
    {
        var command = new EnsureBuiltInRolesCommand();
        await rules.CheckAsync(command, cancellationToken);
        await ensureBuiltInRoles.Handle(command, cancellationToken);
        return NoContent();
    }
}
