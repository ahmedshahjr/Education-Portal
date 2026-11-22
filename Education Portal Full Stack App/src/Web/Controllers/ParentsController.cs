using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Parents.Queries.GetMyChildren;
using EducationPortal.Application.Users.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>A parent's own view of their children. A parent sees only the students linked to them (SEC-AUTHZ-04).</summary>
[Route("api/Parents")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class ParentsController(IRequestRules rules, GetMyChildrenQueryHandler getMyChildren) : ApiControllerBase
{
    [HttpGet("me/children")]
    [EndpointSummary("List my children")]
    [EndpointDescription("Returns the students linked to the signed-in parent, and no one else's. Needs the Students module, and the one-time password must have been changed.")]
    public async Task<ActionResult<IReadOnlyList<UserDetailsDto>>> GetMyChildren(CancellationToken cancellationToken)
    {
        var query = new GetMyChildrenQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getMyChildren.Handle(query, cancellationToken));
    }
}
