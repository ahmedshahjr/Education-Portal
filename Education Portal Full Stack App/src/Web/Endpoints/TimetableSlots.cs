using EducationPortal.Application.Timetable.Commands.CreateTimetableSlot;
using EducationPortal.Application.Timetable.Commands.UpdateTimetableSlot;
using EducationPortal.Application.Timetable.Common;
using EducationPortal.Application.Timetable.Queries.CheckTimetableConflicts;
using EducationPortal.Application.Timetable.Queries.GetTimetableSlots;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>The timetable: lessons of courses, checked for clashes (ADR-019).</summary>
public class TimetableSlots : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetTimetableSlots)
            .RequireAuthorization();

        groupBuilder.MapPost(CreateTimetableSlot)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdateTimetableSlot, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute());

        groupBuilder.MapPost(CheckTimetableConflicts, "conflicts")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute());
    }

    [EndpointSummary("List the lessons")]
    [EndpointDescription("Returns the lessons of the timetable by day and start time, optionally for one course. Needs the Timetable module and the View right.")]
    public static async Task<Ok<IReadOnlyList<TimetableSlotDto>>> GetTimetableSlots(ISender sender, Guid? courseId)
    {
        return TypedResults.Ok(await sender.Send(new GetTimetableSlotsQuery { CourseId = courseId }));
    }

    [EndpointSummary("Add a lesson")]
    [EndpointDescription("Adds a lesson to the timetable. Refused when the teacher, the room or the class is already booked at that time. Needs the Timetable module and the Create right.")]
    public static async Task<Ok<Guid>> CreateTimetableSlot(ISender sender, CreateTimetableSlotCommand command)
    {
        return TypedResults.Ok(await sender.Send(command));
    }

    [EndpointSummary("Move a lesson")]
    [EndpointDescription("Moves a lesson to another day, time, room or campus, refused on a clash. Needs the Timetable module and the Edit right.")]
    public static async Task<NoContent> UpdateTimetableSlot(ISender sender, Guid id, UpdateTimetableSlotCommand command)
    {
        await sender.Send(command with { Id = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Check a lesson for clashes")]
    [EndpointDescription("Lists the lessons a lesson would clash with, without saving it. Needs the Timetable module and the View right.")]
    public static async Task<Ok<IReadOnlyList<TimetableConflictDto>>> CheckTimetableConflicts(ISender sender, CheckTimetableConflictsQuery query)
    {
        return TypedResults.Ok(await sender.Send(query));
    }
}
