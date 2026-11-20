using EducationPortal.Application.AcademicCalendar.Commands.CreateCalendarEvent;
using EducationPortal.Application.AcademicCalendar.Commands.CreateHoliday;
using EducationPortal.Application.AcademicCalendar.Commands.DeleteCalendarEvent;
using EducationPortal.Application.AcademicCalendar.Commands.DeleteHoliday;
using EducationPortal.Application.AcademicCalendar.Commands.UpdateCalendarEvent;
using EducationPortal.Application.AcademicCalendar.Commands.UpdateHoliday;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.AcademicCalendar.Queries.GetSchoolCalendar;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>The school calendar: holidays and events (Academic setup module). The master tenant names the school with <c>schoolId</c>.</summary>
public class SchoolCalendar : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetSchoolCalendar)
            .RequireAuthorization();

        groupBuilder.MapPost(CreateHoliday, "holidays")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdateHoliday, "holidays/{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(DeleteHoliday, "holidays/{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPost(CreateCalendarEvent, "events")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdateCalendarEvent, "events/{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(DeleteCalendarEvent, "events/{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("Read the school calendar")]
    [EndpointDescription("Returns the holidays, calendar events and school events between two dates (both included, at most 400 days), optionally for one campus and the whole school. includeSchoolEvents false leaves out the school events. Needs the Academic setup module.")]
    public static async Task<Ok<SchoolCalendarDto>> GetSchoolCalendar(ISender sender, DateOnly from, DateOnly to, Guid? schoolId, Guid? campusId, bool includeSchoolEvents = true)
    {
        return TypedResults.Ok(await sender.Send(new GetSchoolCalendarQuery { SchoolId = schoolId, From = from, To = to, CampusId = campusId, IncludeSchoolEvents = includeSchoolEvents }));
    }

    [EndpointSummary("Add a holiday")]
    [EndpointDescription("Adds a holiday for the whole school or one campus. Needs the Academic setup module.")]
    public static async Task<Ok<Guid>> CreateHoliday(ISender sender, Guid? schoolId, CreateHolidayCommand command)
    {
        return TypedResults.Ok(await sender.Send(command with { SchoolId = schoolId }));
    }

    [EndpointSummary("Change a holiday")]
    [EndpointDescription("Changes a holiday's campus, title, dates, type, audience and whether it is tentative. Needs the Academic setup module.")]
    public static async Task<NoContent> UpdateHoliday(ISender sender, Guid? schoolId, Guid id, UpdateHolidayCommand command)
    {
        await sender.Send(command with { SchoolId = schoolId, HolidayId = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a holiday")]
    [EndpointDescription("Deletes a holiday (kept for audit, hidden everywhere). Needs the Academic setup module.")]
    public static async Task<NoContent> DeleteHoliday(ISender sender, Guid? schoolId, Guid id)
    {
        await sender.Send(new DeleteHolidayCommand { SchoolId = schoolId, HolidayId = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Add an event")]
    [EndpointDescription("Adds an event to the school calendar for the whole school or one campus. Needs the Academic setup module.")]
    public static async Task<Ok<Guid>> CreateCalendarEvent(ISender sender, Guid? schoolId, CreateCalendarEventCommand command)
    {
        return TypedResults.Ok(await sender.Send(command with { SchoolId = schoolId }));
    }

    [EndpointSummary("Change an event")]
    [EndpointDescription("Changes an event on the school calendar. Needs the Academic setup module.")]
    public static async Task<NoContent> UpdateCalendarEvent(ISender sender, Guid? schoolId, Guid id, UpdateCalendarEventCommand command)
    {
        await sender.Send(command with { SchoolId = schoolId, EventId = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete an event")]
    [EndpointDescription("Deletes an event (kept for audit, hidden everywhere). Needs the Academic setup module.")]
    public static async Task<NoContent> DeleteCalendarEvent(ISender sender, Guid? schoolId, Guid id)
    {
        await sender.Send(new DeleteCalendarEventCommand { SchoolId = schoolId, EventId = id });

        return TypedResults.NoContent();
    }
}
