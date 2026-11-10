using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Countries.Commands.CreateCountry;
using EducationPortal.Application.Countries.Commands.UpdateCountry;
using EducationPortal.Application.Countries.Common;
using EducationPortal.Application.Countries.Queries.GetCountries;
using EducationPortal.Application.Countries.Queries.GetLanguages;
using EducationPortal.Application.Localization.Common;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>Master portal: countries and the languages, time zone and currency of each. Master tenant administrators only (SEC-AUTHZ-07).</summary>
public class Countries : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetCountries)
            .RequireAuthorization();

        groupBuilder.MapGet(GetLanguages, "languages")
            .RequireAuthorization();

        groupBuilder.MapPost(CreateCountry)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdateCountry, "{id:int}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List countries")]
    [EndpointDescription("Returns one page of countries by name, with the languages, time zone and currency of each. Master tenant only.")]
    public static async Task<Ok<PaginatedList<CountryDto>>> GetCountries(ISender sender, int pageNumber = 1, int pageSize = 25)
    {
        return TypedResults.Ok(await sender.Send(new GetCountriesQuery { PageNumber = pageNumber, PageSize = pageSize }));
    }

    [EndpointSummary("List supported languages")]
    [EndpointDescription("Returns the languages the portal has translations for, which a country can offer. Master tenant only.")]
    public static async Task<Ok<IReadOnlyList<LanguageDto>>> GetLanguages(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetLanguagesQuery()));
    }

    [EndpointSummary("Add a country")]
    [EndpointDescription("Adds a country with its languages, default language, IANA time zone and ISO 4217 currency, and returns its id. Master tenant only.")]
    public static async Task<Ok<int>> CreateCountry(ISender sender, CreateCountryCommand command)
    {
        return TypedResults.Ok(await sender.Send(command));
    }

    [EndpointSummary("Change a country")]
    [EndpointDescription("Changes a country's name, languages, default language, time zone and currency. Its schools follow from their next request. The code cannot change. Master tenant only.")]
    public static async Task<NoContent> UpdateCountry(ISender sender, int id, UpdateCountryCommand command)
    {
        // The route names the country; an id in the body is ignored.
        await sender.Send(command with { Id = id });

        return TypedResults.NoContent();
    }
}
