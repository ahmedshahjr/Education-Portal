using EducationPortal.Application.Calendar.Commands.CreateSchoolEvent;
using EducationPortal.Application.Calendar.Common;
using EducationPortal.Application.Calendar.Queries.GetCalendar;
using EducationPortal.Application.Common.Security;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EducationPortal.Web.Controllers;

/// <summary>The school calendar (ADR-019).</summary>
[Route("api/SchoolEvents")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class SchoolEventsController(
    IRequestRules rules,
    GetCalendarQueryHandler getCalendar,
    CreateSchoolEventCommandHandler createSchoolEvent) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("Get the calendar")]
    [EndpointDescription("Returns the school events that overlap a range, shown to the signed-in person by audience. Needs the School module.")]
    public async Task<ActionResult<IReadOnlyList<SchoolEventDto>>> GetCalendar(
        [BindRequired, FromQuery(Name = "From")] DateTimeOffset from,
        [BindRequired, FromQuery(Name = "To")] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        var query = new GetCalendarQuery { From = from, To = to };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getCalendar.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Add a school event")]
    [EndpointDescription("Adds an event for an audience and notifies each account it reaches. Needs the School module and the Create right.")]
    public async Task<ActionResult<Guid>> CreateSchoolEvent(CreateSchoolEventCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        return Ok(await createSchoolEvent.Handle(command, cancellationToken));
    }
}
