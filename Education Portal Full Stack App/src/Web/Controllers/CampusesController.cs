using EducationPortal.Application.Campuses.Commands.CreateCampus;
using EducationPortal.Application.Campuses.Commands.DeleteCampus;
using EducationPortal.Application.Campuses.Commands.UpdateCampus;
using EducationPortal.Application.Campuses.Commands.UpdateCampusSettings;
using EducationPortal.Application.Campuses.Common;
using EducationPortal.Application.Campuses.Queries.GetCampuses;
using EducationPortal.Application.Campuses.Queries.GetCampusLocations;
using EducationPortal.Application.Campuses.Queries.GetCampusMapSettings;
using EducationPortal.Application.Campuses.Queries.GetCampusSettings;
using EducationPortal.Application.Common.Security;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// The current school's campuses, for its administrators (Campuses module). The school is the one the request's host
/// resolves to, so a school never reaches another school's campuses (AGENTS.md section 2). Each action checks its request
/// (IRequestRules, ADR-025) and then calls the use case.
/// </summary>
[Route("api/Campuses")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class CampusesController(
    IRequestRules rules,
    GetCampusesQueryHandler getCampuses,
    GetCampusLocationsQueryHandler getLocations,
    GetCampusMapSettingsQueryHandler getMapSettings,
    GetCampusSettingsQueryHandler getSettings,
    CreateCampusCommandHandler createCampus,
    UpdateCampusCommandHandler updateCampus,
    DeleteCampusCommandHandler deleteCampus,
    UpdateCampusSettingsCommandHandler updateSettings) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List the school's campuses")]
    [EndpointDescription("Returns the current school's campuses, the main campus first, with their codes as the school shows them. Needs the Campuses module.")]
    public async Task<ActionResult<IReadOnlyList<CampusDto>>> GetCampuses(CancellationToken cancellationToken)
    {
        var query = new GetCampusesQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getCampuses.Handle(query, cancellationToken));
    }

    [HttpGet("map-settings")]
    [EndpointSummary("Get the campus map settings")]
    [EndpointDescription("Returns the Google Maps key for the campus map picker, or no key when none is configured. Needs the Campuses module.")]
    public async Task<ActionResult<CampusMapSettingsDto>> GetCampusMapSettings(CancellationToken cancellationToken)
    {
        var query = new GetCampusMapSettingsQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getMapSettings.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Add a campus")]
    [EndpointDescription("Adds a campus to the current school. Its four-digit code is generated and unique in the school. Needs the Campuses module.")]
    public async Task<ActionResult<Guid>> CreateCampus(CreateCampusCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        return Ok(await createCampus.Handle(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Rename a campus")]
    [EndpointDescription("Changes a campus's name. Its code does not change. Needs the Campuses module.")]
    public async Task<IActionResult> UpdateCampus(Guid id, UpdateCampusCommand command, CancellationToken cancellationToken)
    {
        // The route names the campus; an id in the body is ignored. The rules check the request as it will run.
        var request = command with { CampusId = id };
        await rules.CheckAsync(request, cancellationToken);
        await updateCampus.Handle(request, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Delete a campus")]
    [EndpointDescription("Deletes a campus (kept for audit, hidden everywhere). The main campus cannot be deleted, and a campus with module grants must have them turned off first. Needs the Campuses module.")]
    public async Task<IActionResult> DeleteCampus(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteCampusCommand { CampusId = id };
        await rules.CheckAsync(command, cancellationToken);
        await deleteCampus.Handle(command, cancellationToken);

        return NoContent();
    }

    [HttpGet("locations")]
    [EndpointSummary("List the states and cities a campus can be in")]
    [EndpointDescription("Returns the states of the school's country, each with its cities, for a campus's address. Needs the Campuses module.")]
    public async Task<ActionResult<CampusLocationsDto>> GetCampusLocations(CancellationToken cancellationToken)
    {
        var query = new GetCampusLocationsQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getLocations.Handle(query, cancellationToken));
    }

    [HttpGet("settings")]
    [EndpointSummary("Get the campus code settings")]
    [EndpointDescription("Returns whether the school's campus codes show a prefix, and the prefix. Needs the Campuses module.")]
    public async Task<ActionResult<CampusSettingsDto>> GetCampusSettings(CancellationToken cancellationToken)
    {
        var query = new GetCampusSettingsQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getSettings.Handle(query, cancellationToken));
    }

    [HttpPut("settings")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Set the campus code settings")]
    [EndpointDescription("Turns the campus code prefix on or off, and sets it (1 to 6 letters or digits, required when on). Needs the Campuses module.")]
    public async Task<IActionResult> UpdateCampusSettings(UpdateCampusSettingsCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        await updateSettings.Handle(command, cancellationToken);

        return NoContent();
    }
}
