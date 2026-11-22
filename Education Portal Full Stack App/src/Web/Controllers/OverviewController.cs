using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Overview.Queries.GetSchoolCampusOverview;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// The master tenant's overview: every school with its campuses. The master tenant is the software seller and is not a school,
/// so only its administrators reach this (the master portal's dashboard).
/// </summary>
[Route("api/Overview")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class OverviewController(IRequestRules rules, GetSchoolCampusOverviewQueryHandler getOverview) : ApiControllerBase
{
    [HttpGet("schools")]
    [EndpointSummary("List the schools and their campuses")]
    [EndpointDescription("Returns every school with its campuses, for the master portal. Master tenant administrators only.")]
    public async Task<ActionResult<IReadOnlyList<SchoolCampusOverviewDto>>> GetSchoolCampusOverview(CancellationToken cancellationToken)
    {
        var query = new GetSchoolCampusOverviewQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getOverview.Handle(query, cancellationToken));
    }
}
