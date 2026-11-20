using EducationPortal.Application.Academics.Commands.RecordScore;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>Scores of students in assessments (ADR-019).</summary>
public class Scores : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPut(RecordScore, "{assessmentId:guid}/students/{studentId:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("Record a score")]
    [EndpointDescription("Records or changes a student's score in an assessment. Only the course's teacher or an administrator grades it. Needs the Exams module and the Edit right.")]
    public static async Task<NoContent> RecordScore(ISender sender, Guid assessmentId, Guid studentId, RecordScoreCommand command)
    {
        // The route names the assessment and the student; ids in the body are ignored.
        await sender.Send(command with { AssessmentId = assessmentId, StudentId = studentId });

        return TypedResults.NoContent();
    }
}
