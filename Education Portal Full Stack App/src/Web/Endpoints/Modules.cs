using EducationPortal.Application.Modules.Commands.AddModuleDependency;
using EducationPortal.Application.Modules.Commands.CreateModule;
using EducationPortal.Application.Modules.Commands.DeleteModule;
using EducationPortal.Application.Modules.Commands.RemoveModuleDependency;
using EducationPortal.Application.Modules.Commands.UpdateModule;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Application.Modules.Queries.GetEnabledModules;
using EducationPortal.Application.Modules.Queries.GetModule;
using EducationPortal.Application.Modules.Queries.GetModules;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// The module catalogue: what the platform sells. Changing it is for master tenant administrators only (SEC-AUTHZ-07);
/// the list of modules switched on is for any signed-in user of the current school. Modules are named by their stable
/// code, for example <c>FEES</c>.
/// </summary>
public class Modules : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetModules)
            .RequireAuthorization();

        // A literal segment wins over "{code}", so this stays the current school's list.
        groupBuilder.MapGet(GetEnabledModules, "enabled")
            .RequireAuthorization();

        groupBuilder.MapGet(GetModule, "{code}")
            .RequireAuthorization();

        groupBuilder.MapPost(CreateModule)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdateModule, "{code}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(DeleteModule, "{code}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPost(AddModuleDependency, "{code}/dependencies")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(RemoveModuleDependency, "{code}/dependencies/{requiresCode}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List modules")]
    [EndpointDescription("Returns the module catalogue in sort order: each module's code, name, description, whether it is core (in every subscription) or paid, whether it is active, and the codes of the modules it requires. Master tenant only.")]
    public static async Task<Ok<IReadOnlyList<ModuleDto>>> GetModules(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetModulesQuery()));
    }

    [EndpointSummary("Get a module")]
    [EndpointDescription("Returns one module of the catalogue by code, with the codes of the modules it requires. Master tenant only.")]
    public static async Task<Ok<ModuleDto>> GetModule(ISender sender, string code)
    {
        return TypedResults.Ok(await sender.Send(new GetModuleQuery(code)));
    }

    [EndpointSummary("List the current school's modules")]
    [EndpointDescription("Returns the codes of the modules switched on for the school of the request's domain, in sort order. Any signed-in user.")]
    public static async Task<Ok<IReadOnlyList<string>>> GetEnabledModules(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetEnabledModulesQuery()));
    }

    [EndpointSummary("Add a module")]
    [EndpointDescription("Adds a module to the catalogue with a new code (upper-case letters, digits and underscores, for example FEES) and returns the code. Master tenant only.")]
    public static async Task<Ok<string>> CreateModule(ISender sender, CreateModuleCommand command)
    {
        return TypedResults.Ok(await sender.Send(command));
    }

    [EndpointSummary("Change a module")]
    [EndpointDescription("Sets the module's name, description, whether it is core, its sort order and whether it is active. An inactive module is off for every school. A core module is always active and requires only core modules. The code cannot change. Master tenant only.")]
    public static async Task<NoContent> UpdateModule(ISender sender, string code, UpdateModuleCommand command)
    {
        // The route names the module; a code in the body is ignored.
        await sender.Send(command with { Code = code });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a module")]
    [EndpointDescription("Removes a paid module from the catalogue. Its data and the schools' rows stay for audit and the code stays taken. Core modules and modules that others require cannot be deleted. Master tenant only.")]
    public static async Task<NoContent> DeleteModule(ISender sender, string code)
    {
        await sender.Send(new DeleteModuleCommand(code));

        return TypedResults.NoContent();
    }

    [EndpointSummary("Make a module require another")]
    [EndpointDescription("Adds a dependency, for example FEES requires STUDENTS. Refused when it would create a loop, when a module would require itself, or when a core module would require a paid one. Master tenant only.")]
    public static async Task<NoContent> AddModuleDependency(ISender sender, string code, AddModuleDependencyCommand command)
    {
        // The route names the module; a code in the body is ignored.
        await sender.Send(command with { Code = code });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Remove a module dependency")]
    [EndpointDescription("Stops a module requiring another. Answers 404 when it does not require it. Master tenant only.")]
    public static async Task<NoContent> RemoveModuleDependency(ISender sender, string code, string requiresCode)
    {
        await sender.Send(new RemoveModuleDependencyCommand(code, requiresCode));

        return TypedResults.NoContent();
    }
}
