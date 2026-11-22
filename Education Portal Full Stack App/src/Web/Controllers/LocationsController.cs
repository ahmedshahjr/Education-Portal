using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Locations.Common;
using EducationPortal.Application.Locations.Queries.GetCities;
using EducationPortal.Application.Locations.Queries.GetLocationCountries;
using EducationPortal.Application.Locations.Queries.GetStates;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// Read-only countries, states and cities for cascading address dropdowns (country, then state, then city). Any signed-in
/// user on a school or the master domain; the master portal's country settings stay under <c>Countries</c>.
/// Data by Countries States Cities Database, https://github.com/dr5hn/countries-states-cities-database, ODbL v1.0.
/// </summary>
[Route("api/Locations")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class LocationsController(
    IRequestRules rules,
    GetLocationCountriesQueryHandler getCountries,
    GetStatesQueryHandler getStates,
    GetCitiesQueryHandler getCities) : ApiControllerBase
{
    [HttpGet("countries")]
    [EndpointSummary("List countries for address dropdowns")]
    [EndpointDescription("Returns the active countries by name, with ISO codes, native name, phone code and currency.")]
    public async Task<ActionResult<IReadOnlyList<LocationCountryDto>>> GetLocationCountries(CancellationToken cancellationToken)
    {
        var query = new GetLocationCountriesQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getCountries.Handle(query, cancellationToken));
    }

    [HttpGet("countries/{countryId:guid}/states")]
    [EndpointSummary("List a country's states")]
    [EndpointDescription("Returns the active states, provinces, prefectures, emirates or governorates of a country by name. 404 for an unknown or inactive country.")]
    public async Task<ActionResult<IReadOnlyList<StateDto>>> GetStates(Guid countryId, CancellationToken cancellationToken)
    {
        var query = new GetStatesQuery(countryId);
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getStates.Handle(query, cancellationToken));
    }

    [HttpGet("states/{stateId:guid}/cities")]
    [EndpointSummary("List a state's cities")]
    [EndpointDescription("Returns the active cities of a state by name. 404 for an unknown or inactive state.")]
    public async Task<ActionResult<IReadOnlyList<CityDto>>> GetCities(Guid stateId, CancellationToken cancellationToken)
    {
        var query = new GetCitiesQuery(stateId);
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getCities.Handle(query, cancellationToken));
    }
}
