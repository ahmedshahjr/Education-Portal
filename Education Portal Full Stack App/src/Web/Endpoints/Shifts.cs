using EducationPortal.Application.AcademicCalendar.Commands.CreateShift;
using EducationPortal.Application.AcademicCalendar.Commands.DeleteShift;
using EducationPortal.Application.AcademicCalendar.Commands.UpdateShift;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.AcademicCalendar.Queries.GetShifts;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>The shifts of a school's campuses (Academic setup module). The master tenant names the school with <c>schoolId</c>.</summary>
public class Shifts : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetShifts)
            .RequireAuthorization();

        groupBuilder.MapPost(CreateShift)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdateShift, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(DeleteShift, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List shifts")]
    [EndpointDescription("Returns the shifts of the school's campuses, or of one campus, by start time. Needs the Academic setup module.")]
    public static async Task<Ok<IReadOnlyList<ShiftDto>>> GetShifts(ISender sender, Guid? schoolId, Guid? campusId)
    {
        return TypedResults.Ok(await sender.Send(new GetShiftsQuery { SchoolId = schoolId, CampusId = campusId }));
    }

    [EndpointSummary("Add a shift")]
    [EndpointDescription("Adds a shift to a campus; names are unique within a campus. Needs the Academic setup module.")]
    public static async Task<Ok<Guid>> CreateShift(ISender sender, Guid? schoolId, CreateShiftCommand command)
    {
        return TypedResults.Ok(await sender.Send(command with { SchoolId = schoolId }));
    }

    [EndpointSummary("Change a shift")]
    [EndpointDescription("Changes a shift's name and times. Needs the Academic setup module.")]
    public static async Task<NoContent> UpdateShift(ISender sender, Guid? schoolId, Guid id, UpdateShiftCommand command)
    {
        await sender.Send(command with { SchoolId = schoolId, ShiftId = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a shift")]
    [EndpointDescription("Deletes a shift (kept for audit, hidden everywhere). Needs the Academic setup module.")]
    public static async Task<NoContent> DeleteShift(ISender sender, Guid? schoolId, Guid id)
    {
        await sender.Send(new DeleteShiftCommand { SchoolId = schoolId, ShiftId = id });

        return TypedResults.NoContent();
    }
}
