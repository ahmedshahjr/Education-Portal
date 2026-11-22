using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Modules.Commands.AddModuleDependency;
using EducationPortal.Application.Modules.Commands.RemoveModuleDependency;
using EducationPortal.Application.Modules.Commands.UpdateModule;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Application.Modules.Queries.GetEnabledModules;
using EducationPortal.Application.Modules.Queries.GetModule;
using EducationPortal.Application.Modules.Queries.GetModules;
using EducationPortal.Domain.Enums;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// The module catalogue: what the platform sells. Changing it is for master tenant administrators only (SEC-AUTHZ-07);
/// the list of modules switched on is for any signed-in user of the current school. Modules are named by their stable
/// code, for example <c>FEES</c>.
/// </summary>
[Route("api/Modules")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class ModulesController(
    IRequestRules rules,
    GetModulesQueryHandler getModules,
    GetModuleQueryHandler getModule,
    GetEnabledModulesQueryHandler getEnabled,
    UpdateModuleCommandHandler updateModule,
    AddModuleDependencyCommandHandler addDependency,
    RemoveModuleDependencyCommandHandler removeDependency) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List modules")]
    [EndpointDescription("Returns the module catalogue in sort order: each module's code, name, description, whether it is core (in every subscription) or paid, whether it is active, and the codes of the modules it requires. Master tenant only.")]
    public async Task<ActionResult<IReadOnlyList<ModuleDto>>> GetModules(CancellationToken cancellationToken)
    {
        var query = new GetModulesQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getModules.Handle(query, cancellationToken));
    }

    // A literal segment wins over "{code:alpha}", so this stays the current school's list.
    [HttpGet("enabled")]
    [EndpointSummary("List the current school's modules")]
    [EndpointDescription("Returns the codes of the modules switched on for the school of the request's domain, in sort order. Any signed-in user.")]
    public async Task<ActionResult<IReadOnlyList<ModuleCode>>> GetEnabledModules(CancellationToken cancellationToken)
    {
        var query = new GetEnabledModulesQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getEnabled.Handle(query, cancellationToken));
    }

    [HttpGet("{code:alpha}")]
    [EndpointSummary("Get a module")]
    [EndpointDescription("Returns one module of the catalogue by code, with the codes of the modules it requires. Master tenant only.")]
    public async Task<ActionResult<ModuleDto>> GetModule(ModuleCode code, CancellationToken cancellationToken)
    {
        var query = new GetModuleQuery(code);
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getModule.Handle(query, cancellationToken));
    }

    [HttpPut("{code:alpha}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change a module")]
    [EndpointDescription("Sets the module's name, description, whether it is core, its sort order, whether it is on by default for a registering school and whether it is active. An inactive module is off for every school. A core module requires only core modules. Every module can be switched off, core ones included. The code cannot change. Master tenant only.")]
    public async Task<IActionResult> UpdateModule(ModuleCode code, UpdateModuleCommand command, CancellationToken cancellationToken)
    {
        // The route names the module; a code in the body is ignored.
        var request = command with { Code = code };
        await rules.CheckAsync(request, cancellationToken);
        await updateModule.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{code}/dependencies")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Make a module require another")]
    [EndpointDescription("Adds a dependency, for example FEES requires STUDENTS. Refused when it would create a loop, when a module would require itself, or when a core module would require a paid one. Master tenant only.")]
    public async Task<IActionResult> AddModuleDependency(ModuleCode code, AddModuleDependencyCommand command, CancellationToken cancellationToken)
    {
        // The route names the module; a code in the body is ignored.
        var request = command with { Code = code };
        await rules.CheckAsync(request, cancellationToken);
        await addDependency.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{code}/dependencies/{requiresCode}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Remove a module dependency")]
    [EndpointDescription("Stops a module requiring another. Answers 404 when it does not require it. Master tenant only.")]
    public async Task<IActionResult> RemoveModuleDependency(ModuleCode code, ModuleCode requiresCode, CancellationToken cancellationToken)
    {
        var command = new RemoveModuleDependencyCommand(code, requiresCode);
        await rules.CheckAsync(command, cancellationToken);
        await removeDependency.Handle(command, cancellationToken);
        return NoContent();
    }
}
