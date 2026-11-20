using EducationPortal.Application.Academics.Common;
using EducationPortal.Application.Academics.Queries.GetReportCard;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>Report cards of students in a class (ADR-019).</summary>
public class ReportCards : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetReportCard, "{studentId:guid}")
            .RequireAuthorization();
    }

    [EndpointSummary("Get a report card")]
    [EndpointDescription("Returns a student's report card for one class, graded with one grading scale. Administrators and the student or a linked parent see every course; a teacher sees only their own courses.")]
    public static async Task<Ok<ReportCardDto>> GetReportCard(ISender sender, Guid studentId, Guid classOfferingId, Guid gradingScaleId)
    {
        return TypedResults.Ok(await sender.Send(new GetReportCardQuery
        {
            StudentId = studentId,
            ClassOfferingId = classOfferingId,
            GradingScaleId = gradingScaleId,
        }));
    }
}
