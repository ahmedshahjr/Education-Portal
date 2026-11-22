using EducationPortal.Application.Academics.Commands.RecordScore;
using EducationPortal.Application.Common.Security;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>Scores of students in assessments (ADR-019).</summary>
[Route("api/Scores")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class ScoresController(IRequestRules rules, RecordScoreCommandHandler recordScore) : ApiControllerBase
{
    [HttpPut("{assessmentId:guid}/students/{studentId:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Record a score")]
    [EndpointDescription("Records or changes a student's score in an assessment. Only the course's teacher or an administrator grades it. Needs the Exams module and the Edit right.")]
    public async Task<IActionResult> RecordScore(Guid assessmentId, Guid studentId, RecordScoreCommand command, CancellationToken cancellationToken)
    {
        // The route names the assessment and the student; ids in the body are ignored. The rules check the request as it will run.
        var request = command with { AssessmentId = assessmentId, StudentId = studentId };
        await rules.CheckAsync(request, cancellationToken);
        await recordScore.Handle(request, cancellationToken);

        return NoContent();
    }
}
