using EducationPortal.Application.Campuses.Commands.CreateCampus;
using EducationPortal.Application.Campuses.Commands.DeleteCampus;
using EducationPortal.Application.Campuses.Commands.UpdateCampus;
using EducationPortal.Application.Campuses.Commands.UpdateCampusSettings;
using EducationPortal.Application.Campuses.Common;
using EducationPortal.Application.Campuses.Queries.GetCampuses;
using EducationPortal.Application.Campuses.Queries.GetCampusLocations;
using EducationPortal.Application.Campuses.Queries.GetCampusSettings;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// The current school's campuses (the class keeps its name), for its administrators (Campuses module). The school is the one the
/// request's host resolves to, so a school never reaches another school's campuses (AGENTS.md section 2).
/// </summary>
public class Campuses : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetCampuses)
            .RequireAuthorization();

        groupBuilder.MapGet(GetCampusLocations, "locations")
            .RequireAuthorization();

        groupBuilder.MapPost(CreateCampus)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdateCampus, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(DeleteCampus, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapGet(GetCampusSettings, "settings")
            .RequireAuthorization();

        groupBuilder.MapPut(UpdateCampusSettings, "settings")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List the school's campuses")]
    [EndpointDescription("Returns the current school's campuses, the main campus first, with their codes as the school shows them. Needs the Campuses module.")]
    public static async Task<Ok<IReadOnlyList<CampusDto>>> GetCampuses(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetCampusesQuery()));
    }

    [EndpointSummary("Add a campus")]
    [EndpointDescription("Adds a campus to the current school. Its four-digit code is generated and unique in the school. Needs the Campuses module.")]
    public static async Task<Ok<Guid>> CreateCampus(ISender sender, CreateCampusCommand command)
    {
        return TypedResults.Ok(await sender.Send(command));
    }

    [EndpointSummary("Rename a campus")]
    [EndpointDescription("Changes a campus's name. Its code does not change. Needs the Campuses module.")]
    public static async Task<NoContent> UpdateCampus(ISender sender, Guid id, UpdateCampusCommand command)
    {
        // The route names the campus; an id in the body is ignored.
        await sender.Send(command with { CampusId = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a campus")]
    [EndpointDescription("Deletes a campus (kept for audit, hidden everywhere). The main campus cannot be deleted, and a campus with module grants must have them turned off first. Needs the Campuses module.")]
    public static async Task<NoContent> DeleteCampus(ISender sender, Guid id)
    {
        await sender.Send(new DeleteCampusCommand { CampusId = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("List the states and cities a campus can be in")]
    [EndpointDescription("Returns the states of the school's country, each with its cities, for a campus's address. Needs the Campuses module.")]
    public static async Task<Ok<CampusLocationsDto>> GetCampusLocations(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetCampusLocationsQuery()));
    }

    [EndpointSummary("Get the campus code settings")]
    [EndpointDescription("Returns whether the school's campus codes show a prefix, and the prefix. Needs the Campuses module.")]
    public static async Task<Ok<CampusSettingsDto>> GetCampusSettings(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetCampusSettingsQuery()));
    }

    [EndpointSummary("Set the campus code settings")]
    [EndpointDescription("Turns the campus code prefix on or off, and sets it (1 to 6 letters or digits, required when on). Needs the Campuses module.")]
    public static async Task<NoContent> UpdateCampusSettings(ISender sender, UpdateCampusSettingsCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }
}
