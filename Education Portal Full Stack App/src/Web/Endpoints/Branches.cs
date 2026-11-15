using EducationPortal.Application.Branches.Commands.CreateBranch;
using EducationPortal.Application.Branches.Commands.DeleteBranch;
using EducationPortal.Application.Branches.Commands.UpdateBranch;
using EducationPortal.Application.Branches.Commands.UpdateCampusSettings;
using EducationPortal.Application.Branches.Common;
using EducationPortal.Application.Branches.Queries.GetBranches;
using EducationPortal.Application.Branches.Queries.GetCampusSettings;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// The current school's campuses (the class keeps its name), for its administrators (Branches module). The school is the one the
/// request's host resolves to, so a school never reaches another school's campuses (AGENTS.md section 2).
/// </summary>
public class Branches : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetBranches)
            .RequireAuthorization();

        groupBuilder.MapPost(CreateBranch)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdateBranch, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(DeleteBranch, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapGet(GetCampusSettings, "settings")
            .RequireAuthorization();

        groupBuilder.MapPut(UpdateCampusSettings, "settings")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List the school's campuses")]
    [EndpointDescription("Returns the current school's campuses, the main campus first, with their codes as the school shows them. Needs the Branches module.")]
    public static async Task<Ok<IReadOnlyList<BranchDto>>> GetBranches(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetBranchesQuery()));
    }

    [EndpointSummary("Add a campus")]
    [EndpointDescription("Adds a campus to the current school. Its four-digit code is generated and unique in the school. Needs the Branches module.")]
    public static async Task<Ok<Guid>> CreateBranch(ISender sender, CreateBranchCommand command)
    {
        return TypedResults.Ok(await sender.Send(command));
    }

    [EndpointSummary("Rename a campus")]
    [EndpointDescription("Changes a campus's name. Its code does not change. Needs the Branches module.")]
    public static async Task<NoContent> UpdateBranch(ISender sender, Guid id, UpdateBranchCommand command)
    {
        // The route names the campus; an id in the body is ignored.
        await sender.Send(command with { BranchId = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a campus")]
    [EndpointDescription("Deletes a campus (kept for audit, hidden everywhere). The main campus cannot be deleted, and a campus with module grants must have them turned off first. Needs the Branches module.")]
    public static async Task<NoContent> DeleteBranch(ISender sender, Guid id)
    {
        await sender.Send(new DeleteBranchCommand { BranchId = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Get the campus code settings")]
    [EndpointDescription("Returns whether the school's campus codes show a prefix, and the prefix. Needs the Branches module.")]
    public static async Task<Ok<CampusSettingsDto>> GetCampusSettings(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetCampusSettingsQuery()));
    }

    [EndpointSummary("Set the campus code settings")]
    [EndpointDescription("Turns the campus code prefix on or off, and sets it (1 to 6 letters or digits, required when on). Needs the Branches module.")]
    public static async Task<NoContent> UpdateCampusSettings(ISender sender, UpdateCampusSettingsCommand command)
    {
        await sender.Send(command);

        return TypedResults.NoContent();
    }
}
