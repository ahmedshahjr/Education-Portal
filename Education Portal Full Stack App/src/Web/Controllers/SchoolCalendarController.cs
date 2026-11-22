using EducationPortal.Application.AcademicCalendar.Commands.CreateCalendarEvent;
using EducationPortal.Application.AcademicCalendar.Commands.CreateHoliday;
using EducationPortal.Application.AcademicCalendar.Commands.DeleteCalendarEvent;
using EducationPortal.Application.AcademicCalendar.Commands.DeleteHoliday;
using EducationPortal.Application.AcademicCalendar.Commands.UpdateCalendarEvent;
using EducationPortal.Application.AcademicCalendar.Commands.UpdateHoliday;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.AcademicCalendar.Queries.GetSchoolCalendar;
using EducationPortal.Application.Common.Security;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EducationPortal.Web.Controllers;

/// <summary>The school calendar: holidays and events (Academic setup module). The master tenant names the school with <c>schoolId</c>.</summary>
[Route("api/SchoolCalendar")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class SchoolCalendarController(
    IRequestRules rules,
    GetSchoolCalendarQueryHandler getCalendar,
    CreateHolidayCommandHandler createHoliday,
    UpdateHolidayCommandHandler updateHoliday,
    DeleteHolidayCommandHandler deleteHoliday,
    CreateCalendarEventCommandHandler createEvent,
    UpdateCalendarEventCommandHandler updateEvent,
    DeleteCalendarEventCommandHandler deleteEvent) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("Read the school calendar")]
    [EndpointDescription("Returns the holidays, calendar events and school events between two dates (both included, at most 400 days), optionally for one campus and the whole school. includeSchoolEvents false leaves out the school events. Needs the Academic setup module.")]
    public async Task<ActionResult<SchoolCalendarDto>> GetSchoolCalendar(
        [BindRequired] DateOnly from,
        [BindRequired] DateOnly to,
        Guid? schoolId,
        Guid? campusId,
        bool includeSchoolEvents = true,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSchoolCalendarQuery { SchoolId = schoolId, From = from, To = to, CampusId = campusId, IncludeSchoolEvents = includeSchoolEvents };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getCalendar.Handle(query, cancellationToken));
    }

    [HttpPost("holidays")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Add a holiday")]
    [EndpointDescription("Adds a holiday for the whole school or one campus. Needs the Academic setup module.")]
    public async Task<ActionResult<Guid>> CreateHoliday(Guid? schoolId, CreateHolidayCommand command, CancellationToken cancellationToken)
    {
        var request = command with { SchoolId = schoolId };
        await rules.CheckAsync(request, cancellationToken);
        return Ok(await createHoliday.Handle(request, cancellationToken));
    }

    [HttpPut("holidays/{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change a holiday")]
    [EndpointDescription("Changes a holiday's campus, title, dates, type, audience and whether it is tentative. Needs the Academic setup module.")]
    public async Task<IActionResult> UpdateHoliday(Guid? schoolId, Guid id, UpdateHolidayCommand command, CancellationToken cancellationToken)
    {
        var request = command with { SchoolId = schoolId, HolidayId = id };
        await rules.CheckAsync(request, cancellationToken);
        await updateHoliday.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("holidays/{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Delete a holiday")]
    [EndpointDescription("Deletes a holiday (kept for audit, hidden everywhere). Needs the Academic setup module.")]
    public async Task<IActionResult> DeleteHoliday(Guid? schoolId, Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteHolidayCommand { SchoolId = schoolId, HolidayId = id };
        await rules.CheckAsync(command, cancellationToken);
        await deleteHoliday.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("events")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Add an event")]
    [EndpointDescription("Adds an event to the school calendar for the whole school or one campus. Needs the Academic setup module.")]
    public async Task<ActionResult<Guid>> CreateCalendarEvent(Guid? schoolId, CreateCalendarEventCommand command, CancellationToken cancellationToken)
    {
        var request = command with { SchoolId = schoolId };
        await rules.CheckAsync(request, cancellationToken);
        return Ok(await createEvent.Handle(request, cancellationToken));
    }

    [HttpPut("events/{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change an event")]
    [EndpointDescription("Changes an event on the school calendar. Needs the Academic setup module.")]
    public async Task<IActionResult> UpdateCalendarEvent(Guid? schoolId, Guid id, UpdateCalendarEventCommand command, CancellationToken cancellationToken)
    {
        var request = command with { SchoolId = schoolId, EventId = id };
        await rules.CheckAsync(request, cancellationToken);
        await updateEvent.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("events/{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Delete an event")]
    [EndpointDescription("Deletes an event (kept for audit, hidden everywhere). Needs the Academic setup module.")]
    public async Task<IActionResult> DeleteCalendarEvent(Guid? schoolId, Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteCalendarEventCommand { SchoolId = schoolId, EventId = id };
        await rules.CheckAsync(command, cancellationToken);
        await deleteEvent.Handle(command, cancellationToken);
        return NoContent();
    }
}
