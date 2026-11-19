using EducationPortal.Application.Overview.Queries.GetSchoolCampusOverview;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// The master tenant's overview: every school with its campuses. The master tenant is the software seller and is not a school,
/// so only its administrators reach this (the master portal's dashboard).
/// </summary>
public class Overview : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetSchoolCampusOverview, "schools")
            .RequireAuthorization();
    }

    [EndpointSummary("List the schools and their campuses")]
    [EndpointDescription("Returns every school with its campuses, for the master portal. Master tenant administrators only.")]
    public static async Task<Ok<IReadOnlyList<SchoolCampusOverviewDto>>> GetSchoolCampusOverview(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetSchoolCampusOverviewQuery()));
    }
}
