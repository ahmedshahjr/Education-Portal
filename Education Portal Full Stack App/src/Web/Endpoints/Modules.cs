using EducationPortal.Application.Modules.Commands.UpdateModuleDefault;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Application.Modules.Queries.GetEnabledModules;
using EducationPortal.Application.Modules.Queries.GetModules;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// The portal's modules. The catalog and the default/custom split are master-portal settings (SEC-AUTHZ-07); the list of
/// modules switched on is for any signed-in user of the current school.
/// </summary>
public class Modules : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetModules)
            .RequireAuthorization();

        groupBuilder.MapGet(GetEnabledModules, "enabled")
            .RequireAuthorization();

        groupBuilder.MapPut(UpdateModuleDefault, "{key}/default")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List modules")]
    [EndpointDescription("Returns every module and whether it is core (always on), default (on for every school unless switched off) or custom (off unless switched on). Master tenant only.")]
    public static async Task<Ok<IReadOnlyList<ModuleDto>>> GetModules(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetModulesQuery()));
    }

    [EndpointSummary("List the current school's modules")]
    [EndpointDescription("Returns the keys of the modules switched on for the school of the request's domain. Any signed-in user.")]
    public static async Task<Ok<IReadOnlyList<string>>> GetEnabledModules(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetEnabledModulesQuery()));
    }

    [EndpointSummary("Make a module default or custom")]
    [EndpointDescription("A default module is on for every school unless the master switched it off for that school; a custom module is off unless switched on. Core modules are always default. Master tenant only.")]
    public static async Task<NoContent> UpdateModuleDefault(ISender sender, string key, UpdateModuleDefaultCommand command)
    {
        // The route names the module; a key in the body is ignored.
        await sender.Send(command with { Key = key });

        return TypedResults.NoContent();
    }
}
