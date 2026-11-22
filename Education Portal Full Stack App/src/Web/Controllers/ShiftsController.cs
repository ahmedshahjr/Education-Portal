using EducationPortal.Application.AcademicCalendar.Commands.CreateShift;
using EducationPortal.Application.AcademicCalendar.Commands.DeleteShift;
using EducationPortal.Application.AcademicCalendar.Commands.UpdateShift;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.AcademicCalendar.Queries.GetShifts;
using EducationPortal.Application.Common.Security;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>The shifts of a school's campuses (Academic setup module). The master tenant names the school with <c>schoolId</c>.</summary>
[Route("api/Shifts")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class ShiftsController(
    IRequestRules rules,
    GetShiftsQueryHandler getShifts,
    CreateShiftCommandHandler createShift,
    UpdateShiftCommandHandler updateShift,
    DeleteShiftCommandHandler deleteShift) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List shifts")]
    [EndpointDescription("Returns the shifts of the school's campuses, or of one campus, by start time. Needs the Academic setup module.")]
    public async Task<ActionResult<IReadOnlyList<ShiftDto>>> GetShifts(Guid? schoolId, Guid? campusId, CancellationToken cancellationToken)
    {
        var query = new GetShiftsQuery { SchoolId = schoolId, CampusId = campusId };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getShifts.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Add a shift")]
    [EndpointDescription("Adds a shift to a campus; names are unique within a campus. Needs the Academic setup module.")]
    public async Task<ActionResult<Guid>> CreateShift(Guid? schoolId, CreateShiftCommand command, CancellationToken cancellationToken)
    {
        var request = command with { SchoolId = schoolId };
        await rules.CheckAsync(request, cancellationToken);
        return Ok(await createShift.Handle(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change a shift")]
    [EndpointDescription("Changes a shift's name and times. Needs the Academic setup module.")]
    public async Task<IActionResult> UpdateShift(Guid? schoolId, Guid id, UpdateShiftCommand command, CancellationToken cancellationToken)
    {
        var request = command with { SchoolId = schoolId, ShiftId = id };
        await rules.CheckAsync(request, cancellationToken);
        await updateShift.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Delete a shift")]
    [EndpointDescription("Deletes a shift (kept for audit, hidden everywhere). Needs the Academic setup module.")]
    public async Task<IActionResult> DeleteShift(Guid? schoolId, Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteShiftCommand { SchoolId = schoolId, ShiftId = id };
        await rules.CheckAsync(command, cancellationToken);
        await deleteShift.Handle(command, cancellationToken);
        return NoContent();
    }
}
