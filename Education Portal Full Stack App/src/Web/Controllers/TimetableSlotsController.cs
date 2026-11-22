using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Timetable.Commands.CreateTimetableSlot;
using EducationPortal.Application.Timetable.Commands.UpdateTimetableSlot;
using EducationPortal.Application.Timetable.Common;
using EducationPortal.Application.Timetable.Queries.CheckTimetableConflicts;
using EducationPortal.Application.Timetable.Queries.GetTimetableSlots;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>The timetable: lessons of courses, checked for clashes (ADR-019).</summary>
[Route("api/TimetableSlots")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class TimetableSlotsController(
    IRequestRules rules,
    GetTimetableSlotsQueryHandler getSlots,
    CreateTimetableSlotCommandHandler createSlot,
    UpdateTimetableSlotCommandHandler updateSlot,
    CheckTimetableConflictsQueryHandler checkConflicts) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List the lessons")]
    [EndpointDescription("Returns the lessons of the timetable by day and start time, optionally for one course. Needs the Timetable module and the View right.")]
    public async Task<ActionResult<IReadOnlyList<TimetableSlotDto>>> GetTimetableSlots(Guid? courseId, CancellationToken cancellationToken)
    {
        var query = new GetTimetableSlotsQuery { CourseId = courseId };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getSlots.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Add a lesson")]
    [EndpointDescription("Adds a lesson to the timetable. Refused when the teacher, the room or the class is already booked at that time. Needs the Timetable module and the Create right.")]
    public async Task<ActionResult<Guid>> CreateTimetableSlot(CreateTimetableSlotCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        return Ok(await createSlot.Handle(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Move a lesson")]
    [EndpointDescription("Moves a lesson to another day, time, room or campus, refused on a clash. Needs the Timetable module and the Edit right.")]
    public async Task<IActionResult> UpdateTimetableSlot(Guid id, UpdateTimetableSlotCommand command, CancellationToken cancellationToken)
    {
        var request = command with { Id = id };
        await rules.CheckAsync(request, cancellationToken);
        await updateSlot.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("conflicts")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Check a lesson for clashes")]
    [EndpointDescription("Lists the lessons a lesson would clash with, without saving it. Needs the Timetable module and the View right.")]
    public async Task<ActionResult<IReadOnlyList<TimetableConflictDto>>> CheckTimetableConflicts(CheckTimetableConflictsQuery query, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await checkConflicts.Handle(query, cancellationToken));
    }
}
