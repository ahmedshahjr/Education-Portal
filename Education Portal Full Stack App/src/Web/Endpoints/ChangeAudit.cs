using EducationPortal.Application.Audit.Common;
using EducationPortal.Application.Audit.Queries.GetChangeAudit;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>The school's recorded changes, read only (ADR-016, ADR-023).</summary>
public class ChangeAudit : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetChangeAudit)
            .RequireAuthorization();
    }

    [EndpointSummary("List the recorded changes")]
    [EndpointDescription("Returns the recorded changes of the school, newest first, paged (take 1 to 200, default 100) and filtered by entity type and date. Administrators see every change; others see the modules where their roles give the ViewHistory right.")]
    public static async Task<Ok<IReadOnlyList<ChangeAuditDto>>> GetChangeAudit(
        ISender sender,
        int skip = 0,
        int take = 100,
        string? entityType = null,
        DateTimeOffset? from = null,
        DateTimeOffset? to = null)
    {
        return TypedResults.Ok(await sender.Send(new GetChangeAuditQuery
        {
            Skip = skip,
            Take = take,
            EntityType = entityType,
            From = from,
            To = to,
        }));
    }
}
