using EducationPortal.Application.Locations.Common;
using EducationPortal.Application.Locations.Queries.GetCities;
using EducationPortal.Application.Locations.Queries.GetLocationCountries;
using EducationPortal.Application.Locations.Queries.GetStates;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// Read-only countries, states and cities for cascading address dropdowns (country, then state, then city). Any signed-in
/// user on a school or the master domain; the master portal's country settings stay under <see cref="Countries"/>.
/// Data by Countries States Cities Database, https://github.com/dr5hn/countries-states-cities-database, ODbL v1.0.
/// </summary>
public class Locations : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetLocationCountries, "countries")
            .RequireAuthorization();

        groupBuilder.MapGet(GetStates, "countries/{countryId:guid}/states")
            .RequireAuthorization();

        groupBuilder.MapGet(GetCities, "states/{stateId:guid}/cities")
            .RequireAuthorization();
    }

    [EndpointSummary("List countries for address dropdowns")]
    [EndpointDescription("Returns the active countries by name, with ISO codes, native name, phone code and currency.")]
    public static async Task<Ok<IReadOnlyList<LocationCountryDto>>> GetLocationCountries(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetLocationCountriesQuery()));
    }

    [EndpointSummary("List a country's states")]
    [EndpointDescription("Returns the active states, provinces, prefectures, emirates or governorates of a country by name. 404 for an unknown or inactive country.")]
    public static async Task<Ok<IReadOnlyList<StateDto>>> GetStates(ISender sender, Guid countryId)
    {
        return TypedResults.Ok(await sender.Send(new GetStatesQuery(countryId)));
    }

    [EndpointSummary("List a state's cities")]
    [EndpointDescription("Returns the active cities of a state by name. 404 for an unknown or inactive state.")]
    public static async Task<Ok<IReadOnlyList<CityDto>>> GetCities(ISender sender, Guid stateId)
    {
        return TypedResults.Ok(await sender.Send(new GetCitiesQuery(stateId)));
    }
}
