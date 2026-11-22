using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Countries.Commands.CreateCountry;
using EducationPortal.Application.Countries.Commands.UpdateCountry;
using EducationPortal.Application.Countries.Commands.UpdateCountryRegionalSettings;
using EducationPortal.Application.Countries.Common;
using EducationPortal.Application.Countries.Queries.GetCountries;
using EducationPortal.Application.Countries.Queries.GetLanguages;
using EducationPortal.Application.Localization.Common;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>Master portal: countries and the languages, time zone and currency of each. Master tenant administrators only (SEC-AUTHZ-07).</summary>
[Route("api/Countries")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class CountriesController(
    IRequestRules rules,
    GetCountriesQueryHandler getCountries,
    GetLanguagesQueryHandler getLanguages,
    CreateCountryCommandHandler createCountry,
    UpdateCountryCommandHandler updateCountry,
    UpdateCountryRegionalSettingsCommandHandler updateRegionalSettings) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List countries")]
    [EndpointDescription("Returns one page of countries by name, with the languages, time zone and currency of each. Master tenant only.")]
    public async Task<ActionResult<PaginatedList<CountryDto>>> GetCountries(int pageNumber = 1, int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var query = new GetCountriesQuery { PageNumber = pageNumber, PageSize = pageSize };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getCountries.Handle(query, cancellationToken));
    }

    [HttpGet("languages")]
    [EndpointSummary("List supported languages")]
    [EndpointDescription("Returns the languages the portal has translations for, which a country can offer. Master tenant only.")]
    public async Task<ActionResult<IReadOnlyList<LanguageDto>>> GetLanguages(CancellationToken cancellationToken)
    {
        var query = new GetLanguagesQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getLanguages.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Add a country")]
    [EndpointDescription("Adds a country with its languages, default language, IANA time zone and ISO 4217 currency, and returns its id. Master tenant only.")]
    public async Task<ActionResult<Guid>> CreateCountry(CreateCountryCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        return Ok(await createCountry.Handle(command, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change a country")]
    [EndpointDescription("Changes a country's name, languages, default language, time zone and currency. Its schools follow from their next request. The code cannot change. Master tenant only.")]
    public async Task<IActionResult> UpdateCountry(Guid id, UpdateCountryCommand command, CancellationToken cancellationToken)
    {
        // The route names the country; an id in the body is ignored.
        var request = command with { Id = id };
        await rules.CheckAsync(request, cancellationToken);
        await updateCountry.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/regional-settings")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change a country's regional settings")]
    [EndpointDescription("Changes a country's currency decimals, phone number pattern, first day of the week and weekend days. Master tenant only.")]
    public async Task<IActionResult> UpdateCountryRegionalSettings(Guid id, UpdateCountryRegionalSettingsCommand command, CancellationToken cancellationToken)
    {
        // The route names the country; an id in the body is ignored.
        var request = command with { Id = id };
        await rules.CheckAsync(request, cancellationToken);
        await updateRegionalSettings.Handle(request, cancellationToken);
        return NoContent();
    }
}
