using EducationPortal.Application.Branding.Common;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Modules.Commands.SetSchoolModule;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Application.Modules.Queries.GetSchoolModules;
using EducationPortal.Application.Schools.Commands.UpdateSchoolBranding;
using EducationPortal.Application.Schools.Commands.UpdateSchoolCountry;
using EducationPortal.Application.Schools.Queries.GetSchoolBranding;
using EducationPortal.Application.Schools.Queries.GetSchools;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Endpoints;

/// <summary>Master portal: every school. Master tenant administrators only (SEC-AUTHZ-07).</summary>
public class Schools : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetSchools)
            .RequireAuthorization();

        groupBuilder.MapGet(GetSchoolBranding, "{schoolId:int}/branding")
            .RequireAuthorization();

        groupBuilder.MapPut(UpdateSchoolBranding, "{schoolId:int}/branding")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdateSchoolCountry, "{schoolId:int}/country")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapGet(GetSchoolModules, "{schoolId:int}/modules")
            .RequireAuthorization();

        groupBuilder.MapPut(SetSchoolModule, "{schoolId:int}/modules/{key}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List a school's modules")]
    [EndpointDescription("Returns every module as it stands for the school: on or off, and whether that follows the module's default or the master's choice. Master tenant only.")]
    public static async Task<Ok<IReadOnlyList<SchoolModuleDto>>> GetSchoolModules(ISender sender, int schoolId)
    {
        return TypedResults.Ok(await sender.Send(new GetSchoolModulesQuery(schoolId)));
    }

    [EndpointSummary("Switch a module on or off for a school")]
    [EndpointDescription("The choice wins over the module's default from then on. The master tenant cannot be changed. Master tenant only.")]
    public static async Task<NoContent> SetSchoolModule(ISender sender, int schoolId, string key, SetSchoolModuleCommand command)
    {
        // The route names the school and the module; values in the body are ignored.
        await sender.Send(command with { SchoolId = schoolId, Key = key });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Change a school's country")]
    [EndpointDescription("Assigns a school to a country, which sets its languages, time zone and currency, or to none (English only, UTC). Master tenant only.")]
    public static async Task<NoContent> UpdateSchoolCountry(ISender sender, int schoolId, UpdateSchoolCountryCommand command)
    {
        // The route names the school; an id in the body is ignored.
        await sender.Send(command with { SchoolId = schoolId });

        return TypedResults.NoContent();
    }

    [EndpointSummary("List schools")]
    [EndpointDescription("Returns one page of schools, the master tenant first, optionally only those whose name contains the search text (ignoring case) and those in one country. Master tenant only.")]
    public static async Task<Ok<PaginatedList<SchoolSummaryDto>>> GetSchools(
        ISender sender,
        int pageNumber = 1,
        int pageSize = 25,
        string? search = null,
        int? countryId = null)
    {
        return TypedResults.Ok(await sender.Send(new GetSchoolsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            CountryId = countryId,
        }));
    }

    [EndpointSummary("Get a school's branding")]
    [EndpointDescription("Returns any school's branding. Master tenant only.")]
    public static async Task<Ok<BrandingDto>> GetSchoolBranding(ISender sender, int schoolId)
    {
        return TypedResults.Ok(await sender.Send(new GetSchoolBrandingQuery(schoolId)));
    }

    [EndpointSummary("Change a school's branding")]
    [EndpointDescription("Sets any school's theme preset, sidebar colour, logo and favicon, with the same checks as the school's own branding. Master tenant only.")]
    public static async Task<NoContent> UpdateSchoolBranding(ISender sender, int schoolId, [FromForm] BrandingForm form, CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateSchoolBrandingCommand
        {
            SchoolId = schoolId,
            ThemePreset = form.ThemePreset,
            SidebarColor = form.SidebarColor,
            Logo = await BrandingForm.ReadAsync(form.Logo, cancellationToken),
            RemoveLogo = form.RemoveLogo,
            Favicon = await BrandingForm.ReadAsync(form.Favicon, cancellationToken),
            RemoveFavicon = form.RemoveFavicon,
        }, cancellationToken);

        return TypedResults.NoContent();
    }
}
