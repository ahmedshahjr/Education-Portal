using EducationPortal.Application.Branding.Common;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Application.Modules.Queries.GetSchoolModules;
using EducationPortal.Application.Schools.Commands.CreateSchool;
using EducationPortal.Application.Schools.Commands.SetDataRegion;
using EducationPortal.Application.Schools.Commands.UpdateSchoolBranding;
using EducationPortal.Application.Schools.Commands.UpdateSchoolCountry;
using EducationPortal.Application.Schools.Queries.GetSchoolBranding;
using EducationPortal.Application.Schools.Queries.GetSchoolBrandingImage;
using EducationPortal.Application.Schools.Queries.GetSchools;
using EducationPortal.Application.Subscriptions.Commands.CreateSubscription;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Application.Subscriptions.Queries.GetSchoolSubscription;
using EducationPortal.Domain.Enums;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// Master portal: every school. Master tenant administrators only (SEC-AUTHZ-07). Schools and countries are named by
/// their public <see cref="Guid"/>; the internal <c>int</c> keys never reach the API (ENG-ID-02).
/// </summary>
public class Schools : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetSchools)
            .RequireAuthorization();

        groupBuilder.MapPost(CreateSchool)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapGet(GetSchoolBranding, "{schoolId:guid}/branding")
            .RequireAuthorization();

        groupBuilder.MapGet(GetSchoolBrandingImage, "{schoolId:guid}/branding/images/{kind:alpha}")
            .RequireAuthorization();

        groupBuilder.MapPut(UpdateSchoolBranding, "{schoolId:guid}/branding")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdateSchoolCountry, "{schoolId:guid}/country")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapGet(GetSchoolModules, "{schoolId:guid}/modules")
            .RequireAuthorization();

        groupBuilder.MapPut(SetDataRegion, "{schoolId:guid}/data-region")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapGet(GetSchoolSubscription, "{schoolId:guid}/subscription")
            .RequireAuthorization();

        groupBuilder.MapPost(CreateSubscription, "{schoolId:guid}/subscription")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("Add a school")]
    [EndpointDescription("Adds a school tenant on its own domain (a host name such as school1.com that no other school uses), optionally with its country, which sets its languages, time zone, currency and price book, and its data region. It starts with the default branding and no subscription. Returns the school's id. Master tenant only.")]
    public static async Task<Created<Guid>> CreateSchool(ISender sender, CreateSchoolCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/api/Schools/{id}", id);
    }

    [EndpointSummary("Get a school's subscription")]
    [EndpointDescription("Returns the school's current subscription (not cancelled): its plan, billing cycle, the price and currency copied from the price book, status, dates and the modules it gives. Answers 404 when the school has none. Master tenant only.")]
    public static async Task<Ok<SubscriptionDto>> GetSchoolSubscription(ISender sender, Guid schoolId)
    {
        return TypedResults.Ok(await sender.Send(new GetSchoolSubscriptionQuery(schoolId)));
    }

    [EndpointSummary("Start a school's subscription")]
    [EndpointDescription("Starts a subscription from a plan (planCode) and/or add-on module codes, one billing cycle and optional trialDays, and returns its id. The school gets the plan's modules, the add-ons, the modules they require and every core module, priced from its country's price book. The school needs a country and no subscription that is not cancelled; the master tenant needs none. Master tenant only.")]
    public static async Task<Ok<Guid>> CreateSubscription(ISender sender, Guid schoolId, CreateSubscriptionCommand command)
    {
        // The route names the school; an id in the body is ignored.
        return TypedResults.Ok(await sender.Send(command with { SchoolId = schoolId }));
    }

    [EndpointSummary("Set a school's data region")]
    [EndpointDescription("Sets where the school's data is kept (SouthAsia, MiddleEast or NorthAmerica), or clears it with null. Master tenant only.")]
    public static async Task<NoContent> SetDataRegion(ISender sender, Guid schoolId, SetDataRegionCommand command)
    {
        // The route names the school; an id in the body is ignored.
        await sender.Send(command with { SchoolId = schoolId });

        return TypedResults.NoContent();
    }

    [EndpointSummary("List a school's modules")]
    [EndpointDescription("Returns every module as it stands for the school: core modules are always on, paid modules are on when the school's subscription gives them now. Read only; change them through the subscription. Master tenant only.")]
    public static async Task<Ok<IReadOnlyList<SchoolModuleDto>>> GetSchoolModules(ISender sender, Guid schoolId)
    {
        return TypedResults.Ok(await sender.Send(new GetSchoolModulesQuery(schoolId)));
    }

    [EndpointSummary("Change a school's country")]
    [EndpointDescription("Assigns a school to a country, which sets its languages, time zone and currency, or to none (English only, UTC). Master tenant only.")]
    public static async Task<NoContent> UpdateSchoolCountry(ISender sender, Guid schoolId, UpdateSchoolCountryCommand command)
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
        Guid? countryId = null)
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
    public static async Task<Ok<BrandingDto>> GetSchoolBranding(ISender sender, Guid schoolId)
    {
        return TypedResults.Ok(await sender.Send(new GetSchoolBrandingQuery(schoolId)));
    }

    [EndpointSummary("Get a school's logo or favicon")]
    [EndpointDescription("Returns any school's logo or favicon, or 404 when it uses the default one. Master tenant only.")]
    public static async Task<Results<FileContentHttpResult, NotFound>> GetSchoolBrandingImage(ISender sender, HttpContext httpContext, Guid schoolId, BrandingImageKind kind)
    {
        return Branding.ToImageResult(await sender.Send(new GetSchoolBrandingImageQuery(schoolId, kind)), kind, httpContext);
    }

    [EndpointSummary("Change a school's branding")]
    [EndpointDescription("Sets any school's accent colour, sidebar colour, logo and favicon, with the same checks as the school's own branding. Master tenant only.")]
    public static async Task<NoContent> UpdateSchoolBranding(ISender sender, Guid schoolId, [FromForm] BrandingForm form, CancellationToken cancellationToken)
    {
        await sender.Send(new UpdateSchoolBrandingCommand
        {
            SchoolId = schoolId,
            AccentColor = form.AccentColor,
            SidebarColor = form.SidebarColor,
            Logo = await BrandingForm.ReadAsync(form.Logo, cancellationToken),
            RemoveLogo = form.RemoveLogo,
            Favicon = await BrandingForm.ReadAsync(form.Favicon, cancellationToken),
            RemoveFavicon = form.RemoveFavicon,
        }, cancellationToken);

        return TypedResults.NoContent();
    }
}
