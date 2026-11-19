using EducationPortal.Application.Calendar.Commands.CreateSchoolEvent;
using EducationPortal.Application.Calendar.Common;
using EducationPortal.Application.Calendar.Queries.GetCalendar;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>The school calendar (ADR-019).</summary>
public class SchoolEvents : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetCalendar)
            .RequireAuthorization();

        groupBuilder.MapPost(CreateSchoolEvent)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("Get the calendar")]
    [EndpointDescription("Returns the school events that overlap a range, shown to the signed-in person by audience. Needs the School module.")]
    public static async Task<Ok<IReadOnlyList<SchoolEventDto>>> GetCalendar(ISender sender, [AsParameters] GetCalendarQuery query)
    {
        return TypedResults.Ok(await sender.Send(query));
    }

    [EndpointSummary("Add a school event")]
    [EndpointDescription("Adds an event for an audience and notifies each account it reaches. Needs the School module and the Create right.")]
    public static async Task<Ok<Guid>> CreateSchoolEvent(ISender sender, CreateSchoolEventCommand command)
    {
        return TypedResults.Ok(await sender.Send(command));
    }
}
