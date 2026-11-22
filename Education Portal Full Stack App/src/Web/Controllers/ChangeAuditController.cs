using EducationPortal.Application.Audit.Common;
using EducationPortal.Application.Audit.Queries.GetChangeAudit;
using EducationPortal.Application.Common.Security;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>The school's recorded changes, read only (ADR-016, ADR-023).</summary>
[Route("api/ChangeAudit")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class ChangeAuditController(IRequestRules rules, GetChangeAuditQueryHandler getChangeAudit) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List the recorded changes")]
    [EndpointDescription("Returns the recorded changes of the school, newest first, paged (take 1 to 200, default 100) and filtered by entity type and date. Administrators see every change; others see the modules where their roles give the ViewHistory right.")]
    public async Task<ActionResult<IReadOnlyList<ChangeAuditDto>>> GetChangeAudit(
        int skip = 0,
        int take = 100,
        string? entityType = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetChangeAuditQuery
        {
            Skip = skip,
            Take = take,
            EntityType = entityType,
            From = from,
            To = to,
        };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getChangeAudit.Handle(query, cancellationToken));
    }
}
