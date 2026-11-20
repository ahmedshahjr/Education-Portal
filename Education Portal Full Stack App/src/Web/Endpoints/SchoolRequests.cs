using EducationPortal.Application.SchoolRequests.Commands.SubmitSchoolRequest;
using EducationPortal.Application.SchoolRequests.Queries.GetRegistrationCountries;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// The public self-service "register a school" form (decided 2026-11-19). A visitor picks a country, names the school and its
/// administrator, and chooses a password: the school, its trial and its administrator account are created at once. No master
/// approval is needed.
/// </summary>
public class SchoolRequests : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(SubmitSchoolRequest)
            // compliance-exception: SEC-AUTHN-01 a school registers before it has any account; the form creates the first administrator
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.SchoolRequest)
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapGet(GetRegistrationCountries, "countries")
            .AllowAnonymous();
    }

    [EndpointSummary("Register a school")]
    [EndpointDescription("Registers a school for the visitor: the school with its country's data region, the modules on by default, a 3-month free trial of the Full plan and its first administrator (the contact email, with the password chosen). Returns the school's id. Anonymous, rate limited per address.")]
    public static async Task<Created<Guid>> SubmitSchoolRequest(ISender sender, SubmitSchoolRequestCommand command)
    {
        var id = await sender.Send(command);

        return TypedResults.Created($"/api/Schools/{id}", id);
    }

    [EndpointSummary("List the countries a school can register in")]
    [EndpointDescription("Returns the countries that have a price book, for the registration form. Anonymous.")]
    public static async Task<Ok<IReadOnlyList<RegistrationCountryDto>>> GetRegistrationCountries(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetRegistrationCountriesQuery()));
    }
}
