using EducationPortal.Application.Academics.Commands.CreateGradingScale;
using EducationPortal.Application.Academics.Common;
using EducationPortal.Application.Academics.Queries.GetGradingScales;
using EducationPortal.Application.Common.Security;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>The school's grading scales (ADR-019).</summary>
[Route("api/GradingScales")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class GradingScalesController(
    IRequestRules rules,
    GetGradingScalesQueryHandler getGradingScales,
    CreateGradingScaleCommandHandler createGradingScale) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List grading scales")]
    [EndpointDescription("Returns the school's grading scales with their bands. Needs the Exams module and the View right.")]
    public async Task<ActionResult<IReadOnlyList<GradingScaleDto>>> GetGradingScales(CancellationToken cancellationToken)
    {
        var query = new GetGradingScalesQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getGradingScales.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Create a grading scale")]
    [EndpointDescription("Creates a grading scale. Its bands must cover 0 to 100 once, with no gap and no overlap. Needs the Exams module and the Create right.")]
    public async Task<ActionResult<Guid>> CreateGradingScale(CreateGradingScaleCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        return Ok(await createGradingScale.Handle(command, cancellationToken));
    }
}
