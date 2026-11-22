using EducationPortal.Application.Branding.Common;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Application.Modules.Commands.SetSchoolModule;
using EducationPortal.Application.Modules.Queries.GetSchoolModules;
using EducationPortal.Application.Schools.Commands.CreateSchool;
using EducationPortal.Application.Schools.Commands.DeleteSchool;
using EducationPortal.Application.Schools.Commands.SetDataRegion;
using EducationPortal.Application.Schools.Commands.SetSchoolTaxRate;
using EducationPortal.Application.Schools.Commands.UpdateSchool;
using EducationPortal.Application.Schools.Commands.UpdateSchoolBranding;
using EducationPortal.Application.Schools.Commands.UpdateSchoolCountry;
using EducationPortal.Application.Schools.Queries.GetSchool;
using EducationPortal.Application.Schools.Queries.GetSchoolBranding;
using EducationPortal.Application.Schools.Queries.GetSchoolBrandingImage;
using EducationPortal.Application.Schools.Queries.GetSchools;
using EducationPortal.Application.Subscriptions.Commands.AddSubscriptionModule;
using EducationPortal.Application.Subscriptions.Commands.ChangeSubscriptionPlan;
using EducationPortal.Application.Subscriptions.Commands.CreateSubscription;
using EducationPortal.Application.Subscriptions.Commands.ExtendSubscription;
using EducationPortal.Application.Subscriptions.Commands.ReactivateSubscription;
using EducationPortal.Application.Subscriptions.Commands.RemoveSubscriptionModule;
using EducationPortal.Application.Subscriptions.Commands.SuspendSubscription;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Application.Subscriptions.Queries.GetSchoolSubscription;
using EducationPortal.Domain.Enums;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// Master portal: every school. Master tenant administrators only (SEC-AUTHZ-07). Schools and countries are named by
/// their public <see cref="Guid"/>; the internal <c>int</c> keys never reach the API (ENG-ID-02).
/// </summary>
[Route("api/Schools")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class SchoolsController(
    IRequestRules rules,
    CreateSchoolCommandHandler createSchool,
    GetSchoolsQueryHandler getSchools,
    GetSchoolQueryHandler getSchool,
    UpdateSchoolCommandHandler updateSchool,
    DeleteSchoolCommandHandler deleteSchool,
    UpdateSchoolCountryCommandHandler updateCountry,
    GetSchoolBrandingQueryHandler getBranding,
    GetSchoolBrandingImageQueryHandler getBrandingImage,
    UpdateSchoolBrandingCommandHandler updateBranding,
    GetSchoolModulesQueryHandler getModules,
    SetSchoolModuleCommandHandler setModule,
    SetDataRegionCommandHandler setDataRegion,
    SetSchoolTaxRateCommandHandler setTaxRate,
    GetSchoolSubscriptionQueryHandler getSubscription,
    CreateSubscriptionCommandHandler createSubscription,
    SuspendSubscriptionCommandHandler suspendSubscription,
    ReactivateSubscriptionCommandHandler reactivateSubscription,
    ExtendSubscriptionCommandHandler extendSubscription,
    ChangeSubscriptionPlanCommandHandler changePlan,
    AddSubscriptionModuleCommandHandler addModule,
    RemoveSubscriptionModuleCommandHandler removeModule) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List schools")]
    [EndpointDescription("Returns one page of schools, the master tenant first, optionally only those whose name contains the search text (ignoring case) and those in one country. Master tenant only.")]
    public async Task<ActionResult<PaginatedList<SchoolSummaryDto>>> GetSchools(
        int pageNumber = 1,
        int pageSize = 25,
        string? search = null,
        Guid? countryId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetSchoolsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            CountryId = countryId,
        };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getSchools.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [EndpointSummary("Add a school")]
    [EndpointDescription("Adds a school tenant on its own domain (a host name such as school1.com that no other school uses), optionally with its country, which sets its languages, time zone, currency and price book, and its data region. It starts with the default branding and no subscription. Returns the school's id. Master tenant only.")]
    public async Task<IActionResult> CreateSchool(CreateSchoolCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        var id = await createSchool.Handle(command, cancellationToken);
        return Created($"/api/Schools/{id}", id);
    }

    [HttpGet("{schoolId:guid}/branding")]
    [EndpointSummary("Get a school's branding")]
    [EndpointDescription("Returns any school's branding. Master tenant only.")]
    public async Task<ActionResult<BrandingDto>> GetSchoolBranding(Guid schoolId, CancellationToken cancellationToken)
    {
        var query = new GetSchoolBrandingQuery(schoolId);
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getBranding.Handle(query, cancellationToken));
    }

    [HttpGet("{schoolId:guid}/branding/images/{kind:alpha}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [EndpointSummary("Get a school's logo or favicon")]
    [EndpointDescription("Returns any school's logo or favicon, or 404 when it uses the default one. Master tenant only.")]
    public async Task<IActionResult> GetSchoolBrandingImage(Guid schoolId, BrandingImageKind kind, CancellationToken cancellationToken)
    {
        var query = new GetSchoolBrandingImageQuery(schoolId, kind);
        await rules.CheckAsync(query, cancellationToken);
        return BrandingImageResults.Image(await getBrandingImage.Handle(query, cancellationToken), kind, Response);
    }

    [HttpPut("{schoolId:guid}/branding")]
    [Consumes("multipart/form-data")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change a school's branding")]
    [EndpointDescription("Sets any school's accent colour, sidebar colour, logo and favicon, with the same checks as the school's own branding. Master tenant only.")]
    public async Task<IActionResult> UpdateSchoolBranding(Guid schoolId, [FromForm] BrandingForm form, CancellationToken cancellationToken)
    {
        var command = new UpdateSchoolBrandingCommand
        {
            SchoolId = schoolId,
            AccentColor = form.AccentColor,
            SidebarColor = form.SidebarColor,
            Logo = await BrandingForm.ReadAsync(form.Logo, cancellationToken),
            RemoveLogo = form.RemoveLogo,
            Favicon = await BrandingForm.ReadAsync(form.Favicon, cancellationToken),
            RemoveFavicon = form.RemoveFavicon,
        };
        await rules.CheckAsync(command, cancellationToken);
        await updateBranding.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpPut("{schoolId:guid}/country")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change a school's country")]
    [EndpointDescription("Assigns a school to a country, which sets its languages, time zone and currency, or to none (English only, UTC). Master tenant only.")]
    public async Task<IActionResult> UpdateSchoolCountry(Guid schoolId, UpdateSchoolCountryCommand command, CancellationToken cancellationToken)
    {
        // The route names the school; an id in the body is ignored.
        var request = command with { SchoolId = schoolId };
        await rules.CheckAsync(request, cancellationToken);
        await updateCountry.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpGet("{schoolId:guid}")]
    [EndpointSummary("Get a school")]
    [EndpointDescription("Returns one school: its name, domain and country. Master tenant only.")]
    public async Task<ActionResult<SchoolSummaryDto>> GetSchool(Guid schoolId, CancellationToken cancellationToken)
    {
        var query = new GetSchoolQuery(schoolId);
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getSchool.Handle(query, cancellationToken));
    }

    [HttpPut("{schoolId:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Edit a school's name and domain")]
    [EndpointDescription("Changes a school's name and the domain it is reached on. The domain must be unused by other schools; the master tenant's domain never changes. Master tenant only.")]
    public async Task<IActionResult> UpdateSchool(Guid schoolId, UpdateSchoolCommand command, CancellationToken cancellationToken)
    {
        // The route names the school; an id in the body is ignored.
        var request = command with { SchoolId = schoolId };
        await rules.CheckAsync(request, cancellationToken);
        await updateSchool.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{schoolId:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Delete a school")]
    [EndpointDescription("Deletes a school (kept for audit, hidden everywhere: its host is refused and its subscription stops being run). The master tenant cannot be deleted. Master tenant only.")]
    public async Task<IActionResult> DeleteSchool(Guid schoolId, CancellationToken cancellationToken)
    {
        var command = new DeleteSchoolCommand(schoolId);
        await rules.CheckAsync(command, cancellationToken);
        await deleteSchool.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("{schoolId:guid}/modules")]
    [EndpointSummary("List a school's modules")]
    [EndpointDescription("Returns every module as it stands for the school: a module is on when the school has a grant for it: the default grant from its registration, or its subscription. The master tenant changes any of them with the school's switches (PUT .../modules/{moduleCode}). Master tenant only.")]
    public async Task<ActionResult<IReadOnlyList<SchoolModuleDto>>> GetSchoolModules(Guid schoolId, CancellationToken cancellationToken)
    {
        var query = new GetSchoolModulesQuery(schoolId);
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getModules.Handle(query, cancellationToken));
    }

    [HttpPut("{schoolId:guid}/modules/{moduleCode}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Turn a module on or off for a school")]
    [EndpointDescription("Turns a module on for the school (free, whole school, no end) or off (its active grants end now). Core and default modules can be switched too. A module switched off for every school cannot be turned on. Master tenant only.")]
    public async Task<IActionResult> SetSchoolModule(Guid schoolId, ModuleCode moduleCode, SetSchoolModuleCommand command, CancellationToken cancellationToken)
    {
        // The route names the school and the module; ids in the body are ignored.
        var request = command with { SchoolId = schoolId, ModuleCode = moduleCode };
        await rules.CheckAsync(request, cancellationToken);
        await setModule.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpPut("{schoolId:guid}/data-region")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Set a school's data region")]
    [EndpointDescription("Sets where the school's data is kept (SouthAsia, MiddleEast or NorthAmerica), or clears it with null. Master tenant only.")]
    public async Task<IActionResult> SetDataRegion(Guid schoolId, SetDataRegionCommand command, CancellationToken cancellationToken)
    {
        // The route names the school; an id in the body is ignored.
        var request = command with { SchoolId = schoolId };
        await rules.CheckAsync(request, cancellationToken);
        await setDataRegion.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpPut("{schoolId:guid}/tax-rate")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Set a school's own tax rate")]
    [EndpointDescription("Sets the tax rate in percent (0 to 100, two decimals) that replaces the country's rate on this school's new invoices, or clears it with null so the country's rate applies. Issued invoices keep their rate. Master tenant only.")]
    public async Task<IActionResult> SetSchoolTaxRate(Guid schoolId, SetSchoolTaxRateCommand command, CancellationToken cancellationToken)
    {
        // The route names the school; an id in the body is ignored.
        var request = command with { SchoolId = schoolId };
        await rules.CheckAsync(request, cancellationToken);
        await setTaxRate.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpGet("{schoolId:guid}/subscription")]
    [EndpointSummary("Get a school's subscription")]
    [EndpointDescription("Returns the school's current subscription (not cancelled): its plan, billing cycle, the price and currency copied from the price book, status, dates and the modules it gives. Answers 404 when the school has none. Master tenant only.")]
    public async Task<ActionResult<SubscriptionDto>> GetSchoolSubscription(Guid schoolId, CancellationToken cancellationToken)
    {
        var query = new GetSchoolSubscriptionQuery(schoolId);
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getSubscription.Handle(query, cancellationToken));
    }

    [HttpPost("{schoolId:guid}/subscription")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Start a school's subscription")]
    [EndpointDescription("Starts a subscription from a plan (planCode) and/or add-on module codes, one billing cycle and optional trialDays, and returns its id. The school gets the plan's modules, the add-ons, the modules they require and every core module, priced from its country's price book. The school needs a country and no subscription that is not cancelled; the master tenant needs none. Master tenant only.")]
    public async Task<ActionResult<Guid>> CreateSubscription(Guid schoolId, CreateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        // The route names the school; an id in the body is ignored.
        var request = command with { SchoolId = schoolId };
        await rules.CheckAsync(request, cancellationToken);
        return Ok(await createSubscription.Handle(request, cancellationToken));
    }

    [HttpPost("{schoolId:guid}/subscription/suspend")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Suspend a school's subscription")]
    [EndpointDescription("Suspends the subscription and every module it gives: requests to them are refused with 403 and the data is kept. Master tenant only.")]
    public async Task<IActionResult> SuspendSubscription(Guid schoolId, CancellationToken cancellationToken)
    {
        var command = new SuspendSubscriptionCommand(schoolId);
        await rules.CheckAsync(command, cancellationToken);
        await suspendSubscription.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{schoolId:guid}/subscription/reactivate")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Reactivate a school's subscription")]
    [EndpointDescription("Brings a suspended or past-due subscription back to Active with its modules. Master tenant only.")]
    public async Task<IActionResult> ReactivateSubscription(Guid schoolId, CancellationToken cancellationToken)
    {
        var command = new ReactivateSubscriptionCommand(schoolId);
        await rules.CheckAsync(command, cancellationToken);
        await reactivateSubscription.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{schoolId:guid}/subscription/extend")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Extend a school's subscription")]
    [EndpointDescription("Moves the end of the current period (and trial) later by 1 to 366 days without a payment. Master tenant only.")]
    public async Task<IActionResult> ExtendSubscription(Guid schoolId, ExtendSubscriptionCommand command, CancellationToken cancellationToken)
    {
        // The route names the school; an id in the body is ignored.
        var request = command with { SchoolId = schoolId };
        await rules.CheckAsync(request, cancellationToken);
        await extendSubscription.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpPut("{schoolId:guid}/subscription/plan")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change a school's plan")]
    [EndpointDescription("Moves the subscription to another plan (planCode), keeping its add-on modules. Modules the new plan lacks end now and their data is kept; the price follows the new plan; the current period is neither credited nor charged. Master tenant only.")]
    public async Task<IActionResult> ChangeSubscriptionPlan(Guid schoolId, ChangeSubscriptionPlanCommand command, CancellationToken cancellationToken)
    {
        // The route names the school; an id in the body is ignored.
        var request = command with { SchoolId = schoolId };
        await rules.CheckAsync(request, cancellationToken);
        await changePlan.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{schoolId:guid}/subscription/modules")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Add a module to a school's subscription")]
    [EndpointDescription("Adds a paid add-on module (moduleCode) and the modules it requires, and raises the price by its price in the price book. Master tenant only.")]
    public async Task<IActionResult> AddSubscriptionModule(Guid schoolId, AddSubscriptionModuleCommand command, CancellationToken cancellationToken)
    {
        // The route names the school; an id in the body is ignored.
        var request = command with { SchoolId = schoolId };
        await rules.CheckAsync(request, cancellationToken);
        await addModule.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{schoolId:guid}/subscription/modules/{moduleCode}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Remove a module from a school's subscription")]
    [EndpointDescription("Removes an add-on module: it ends now and its data is kept. A module the plan includes, or another module requires, cannot be removed. Master tenant only.")]
    public async Task<IActionResult> RemoveSubscriptionModule(Guid schoolId, ModuleCode moduleCode, CancellationToken cancellationToken)
    {
        var command = new RemoveSubscriptionModuleCommand { SchoolId = schoolId, ModuleCode = moduleCode };
        await rules.CheckAsync(command, cancellationToken);
        await removeModule.Handle(command, cancellationToken);
        return NoContent();
    }
}
