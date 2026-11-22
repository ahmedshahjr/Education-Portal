using EducationPortal.Application.Common.Security;
using EducationPortal.Application.DevelopmentData.Commands.GenerateDevelopmentData;
using EducationPortal.Application.DevelopmentData.Common;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// Development only: creates many fake schools, campuses and people in one call, for trying the portal and for load
/// testing. Outside Development the route answers 404. Master tenant administrators only (SEC-AUTHZ-07).
/// </summary>
[Route("api/DevelopmentData")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class DevelopmentDataController(
    IRequestRules rules,
    IWebHostEnvironment environment,
    GenerateDevelopmentDataCommandHandler generate) : ApiControllerBase
{
    [HttpPost("generate")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType<DevelopmentDataSummary>(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [EndpointSummary("Generate fake schools, campuses and people (Development only)")]
    [EndpointDescription("Creates the requested number of schools, each with campuses and people, in one call. Limits: at most 500 schools, 50 campuses per school, 5000 people per campus and 1,000,000 people per call. Development only; master tenant administrators only.")]
    public async Task<ActionResult<DevelopmentDataSummary>> GenerateDevelopmentData(
        [FromBody] GenerateDevelopmentDataCommand command,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        await rules.CheckAsync(command, cancellationToken);
        return Ok(await generate.Handle(command, cancellationToken));
    }
}
