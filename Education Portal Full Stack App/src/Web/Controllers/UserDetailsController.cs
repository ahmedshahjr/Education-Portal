using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Parents.Commands.LinkStudent;
using EducationPortal.Application.Parents.Commands.UnlinkStudent;
using EducationPortal.Application.Parents.Queries.GetChildren;
using EducationPortal.Application.Users.Commands.CreateUserDetails;
using EducationPortal.Application.Users.Commands.DeleteUserDetails;
using EducationPortal.Application.Users.Commands.SavePersonAccess;
using EducationPortal.Application.Users.Commands.SetAccountRoles;
using EducationPortal.Application.Users.Commands.SetPersonCampuses;
using EducationPortal.Application.Users.Commands.UpdateUserDetails;
using EducationPortal.Application.Users.Dtos;
using EducationPortal.Application.Users.Queries.GetAccountRoles;
using EducationPortal.Application.Users.Queries.GetPersonCampuses;
using EducationPortal.Application.Users.Queries.GetUserDetails;
using EducationPortal.Domain.Enums;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// The people of the current school (principals, teachers and other staff), for its administrators (Staff module). The school
/// is the one the request's host resolves to, so a school never reaches another school's people (AGENTS.md section 2).
/// </summary>
[Route("api/UserDetails")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class UserDetailsController(
    IRequestRules rules,
    GetUserDetailsQueryHandler getUserDetails,
    CreateUserDetailsCommandHandler createUserDetails,
    UpdateUserDetailsCommandHandler updateUserDetails,
    DeleteUserDetailsCommandHandler deleteUserDetails,
    GetChildrenQueryHandler getChildren,
    LinkStudentCommandHandler linkStudent,
    UnlinkStudentCommandHandler unlinkStudent,
    GetAccountRolesQueryHandler getAccountRoles,
    SetAccountRolesCommandHandler setAccountRoles,
    GetPersonCampusesQueryHandler getPersonCampuses,
    SetPersonCampusesCommandHandler setPersonCampuses,
    SavePersonAccessCommandHandler savePersonAccess) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List the school's people")]
    [EndpointDescription("Returns the current school's people, sorted by name. Pass userType to list only one type, for example Principal. Needs the Staff module.")]
    public async Task<ActionResult<IReadOnlyList<UserDetailsDto>>> GetUserDetails(UserType? userType, CancellationToken cancellationToken)
    {
        var query = new GetUserDetailsQuery { UserType = userType };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getUserDetails.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Add a person")]
    [EndpointDescription("Adds a principal, teacher or other staff member to the current school. Needs the Staff module.")]
    public async Task<ActionResult<Guid>> CreateUserDetails(CreateUserDetailsCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        return Ok(await createUserDetails.Handle(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change a person")]
    [EndpointDescription("Changes a person's name, type and phone. Needs the Staff module.")]
    public async Task<IActionResult> UpdateUserDetails(Guid id, UpdateUserDetailsCommand command, CancellationToken cancellationToken)
    {
        // The route names the person; an id in the body is ignored.
        var request = command with { Id = id };
        await rules.CheckAsync(request, cancellationToken);
        await updateUserDetails.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Delete a person")]
    [EndpointDescription("Deletes a person (kept for audit, hidden everywhere). A principal of a campus cannot be deleted until another principal is chosen. Needs the Staff module.")]
    public async Task<IActionResult> DeleteUserDetails(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteUserDetailsCommand { Id = id };
        await rules.CheckAsync(command, cancellationToken);
        await deleteUserDetails.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/children")]
    [EndpointSummary("List a parent's students")]
    [EndpointDescription("Returns the students linked to a parent of the school. Needs the Staff module.")]
    public async Task<ActionResult<IReadOnlyList<UserDetailsDto>>> GetChildren(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetChildrenQuery { ParentId = id };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getChildren.Handle(query, cancellationToken));
    }

    [HttpPost("{id:guid}/children")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Link a student to a parent")]
    [EndpointDescription("Links a student (studentId) of the school to a parent. Both must be people of the school, a parent and a student. Needs the Staff module.")]
    public async Task<IActionResult> LinkStudent(Guid id, LinkStudentCommand command, CancellationToken cancellationToken)
    {
        // The route names the parent; an id in the body is ignored.
        var request = command with { ParentId = id };
        await rules.CheckAsync(request, cancellationToken);
        await linkStudent.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}/children/{studentId:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Unlink a student from a parent")]
    [EndpointDescription("Removes the link between a parent and a student. Needs the Staff module.")]
    public async Task<IActionResult> UnlinkStudent(Guid id, Guid studentId, CancellationToken cancellationToken)
    {
        var command = new UnlinkStudentCommand { ParentId = id, StudentId = studentId };
        await rules.CheckAsync(command, cancellationToken);
        await unlinkStudent.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/roles")]
    [EndpointSummary("Get a person's roles")]
    [EndpointDescription("Returns the public ids of the school roles the person's login account holds. Needs the Users & Roles module.")]
    public async Task<ActionResult<IReadOnlyList<Guid>>> GetAccountRoles(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAccountRolesQuery { PersonId = id };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getAccountRoles.Handle(query, cancellationToken));
    }

    [HttpPut("{id:guid}/roles")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Set a person's roles")]
    [EndpointDescription("Sets exactly the school roles (roleIds) the person's login account holds. The person must have an account. Needs the Users & Roles module.")]
    public async Task<IActionResult> SetAccountRoles(Guid id, SetAccountRolesCommand command, CancellationToken cancellationToken)
    {
        // The route names the person; an id in the body is ignored.
        var request = command with { PersonId = id };
        await rules.CheckAsync(request, cancellationToken);
        await setAccountRoles.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/campuses")]
    [EndpointSummary("Get a person's campuses")]
    [EndpointDescription("Returns the public ids of the campuses the person is assigned to. Someone who is not an administrator acts only on these campuses. Needs the Users & Roles module.")]
    public async Task<ActionResult<IReadOnlyList<Guid>>> GetPersonCampuses(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPersonCampusesQuery { PersonId = id };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getPersonCampuses.Handle(query, cancellationToken));
    }

    [HttpPut("{id:guid}/campuses")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Set a person's campuses")]
    [EndpointDescription("Sets exactly the campuses (campusIds) the person is assigned to. Needs the Users & Roles module.")]
    public async Task<IActionResult> SetPersonCampuses(Guid id, SetPersonCampusesCommand command, CancellationToken cancellationToken)
    {
        // The route names the person; an id in the body is ignored.
        var request = command with { PersonId = id };
        await rules.CheckAsync(request, cancellationToken);
        await setPersonCampuses.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/access")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Save a person's roles and campuses together")]
    [EndpointDescription("Sets the person's school roles and campuses in one transaction: either both change or neither does. Needs the Users & Roles module.")]
    public async Task<IActionResult> SavePersonAccess(Guid id, SavePersonAccessCommand command, CancellationToken cancellationToken)
    {
        // The route names the person; an id in the body is ignored.
        var request = command with { PersonId = id };
        await rules.CheckAsync(request, cancellationToken);
        await savePersonAccess.Handle(request, cancellationToken);
        return NoContent();
    }
}
