using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Schools.Commands.RegisterSchool;
using EducationPortal.Application.Schools.Queries.GetRegistrationCountries;
using EducationPortal.Web.Filters;
using EducationPortal.Web.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// Self-service school registration (ADR-018): anyone on the master tenant's domain can register a school with its first
/// administrator and get a free trial at once. Both routes answer 404 on any other host.
/// </summary>
[Route("api/SchoolRegistrations")]
public class SchoolRegistrationsController(
    IRequestRules rules,
    RegisterSchoolCommandHandler registerSchool,
    GetRegistrationCountriesQueryHandler getCountries) : ApiControllerBase
{
    [HttpPost]
    // compliance-exception: SEC-AUTHN-01 a school registers before it has any account; rate limited per client address
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitPolicies.SchoolRequest)]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Register a school")]
    [EndpointDescription("Creates a school, its first administrator (with the password given) and a 90-day free trial of the Full plan, with no approval. Master tenant's domain only. Returns the school's id.")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterSchool(RegisterSchoolCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        var id = await registerSchool.Handle(command, cancellationToken);
        return Created($"/api/SchoolRegistrations/{id}", id);
    }

    [HttpGet("countries")]
    // compliance-exception: SEC-AUTHN-01 the registration form lists the countries before sign-in; names only
    [AllowAnonymous]
    [EndpointSummary("List the countries a school can register in")]
    [EndpointDescription("Returns the countries that have a price book, with their public id and name. Master tenant's domain only.")]
    public async Task<ActionResult<IReadOnlyList<RegistrationCountryDto>>> GetRegistrationCountries(CancellationToken cancellationToken)
    {
        var query = new GetRegistrationCountriesQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getCountries.Handle(query, cancellationToken));
    }
}
