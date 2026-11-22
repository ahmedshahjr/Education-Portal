using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolPublicPage.Commands.SetCurricula;
using EducationPortal.Application.SchoolPublicPage.Queries.GetCurricula;
using EducationPortal.Domain.Enums;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>The curricula a school lists on its public page (School module). The master tenant names the school with <c>schoolId</c>.</summary>
[Route("api/SchoolCurricula")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class SchoolCurriculaController(
    IRequestRules rules,
    GetCurriculaQueryHandler getCurricula,
    SetCurriculaCommandHandler setCurricula) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List the school's curricula")]
    [EndpointDescription("Returns the curricula the school shows on its public page. Needs the School module.")]
    public async Task<ActionResult<IReadOnlyList<CurriculumKind>>> GetCurricula(Guid? schoolId, CancellationToken cancellationToken)
    {
        var query = new GetCurriculaQuery { SchoolId = schoolId };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getCurricula.Handle(query, cancellationToken));
    }

    [HttpPut]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Replace the school's curricula")]
    [EndpointDescription("Replaces the set of curricula shown on the public page; each kind at most once. Needs the School module.")]
    public async Task<IActionResult> SetCurricula(Guid? schoolId, SetCurriculaCommand command, CancellationToken cancellationToken)
    {
        var request = command with { SchoolId = schoolId };
        await rules.CheckAsync(request, cancellationToken);
        await setCurricula.Handle(request, cancellationToken);
        return NoContent();
    }
}
