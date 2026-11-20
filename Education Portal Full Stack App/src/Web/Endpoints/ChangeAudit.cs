using EducationPortal.Application.Audit.Common;
using EducationPortal.Application.Audit.Queries.GetChangeAudit;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>The school's recorded changes, read only, for its administrators (ADR-016).</summary>
public class ChangeAudit : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetChangeAudit)
            .RequireAuthorization();
    }

    [EndpointSummary("List the recorded changes")]
    [EndpointDescription("Returns the latest recorded changes of the school, newest first (1 to 200, default 100). Read only. Administrators only.")]
    public static async Task<Ok<IReadOnlyList<ChangeAuditDto>>> GetChangeAudit(ISender sender, int take = 100)
    {
        return TypedResults.Ok(await sender.Send(new GetChangeAuditQuery { Take = take }));
    }
}
