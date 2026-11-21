using EducationPortal.Application.Schools.Commands.RegisterSchool;
using EducationPortal.Application.Schools.Queries.GetRegistrationCountries;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// Self-service school registration (ADR-018): anyone on the master tenant's domain can register a school with its first
/// administrator and get a free trial at once. Both routes answer 404 on any other host.
/// </summary>
public class SchoolRegistrations : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(RegisterSchool)
            // compliance-exception: SEC-AUTHN-01 a school registers before it has any account; rate limited per client address
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.SchoolRequest)
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapGet(GetRegistrationCountries, "countries")
            // compliance-exception: SEC-AUTHN-01 the registration form lists the countries before sign-in; names only
            .AllowAnonymous();
    }

    [EndpointSummary("Register a school")]
    [EndpointDescription("Creates a school, its first administrator (with the password given) and a 90-day free trial of the Full plan, with no approval. Master tenant's domain only. Returns the school's id.")]
    public static async Task<Created<Guid>> RegisterSchool(ISender sender, RegisterSchoolCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/api/SchoolRegistrations/{id}", id);
    }

    [EndpointSummary("List the countries a school can register in")]
    [EndpointDescription("Returns the countries that have a price book, with their public id and name. Master tenant's domain only.")]
    public static async Task<Ok<IReadOnlyList<RegistrationCountryDto>>> GetRegistrationCountries(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetRegistrationCountriesQuery()));
    }
}
