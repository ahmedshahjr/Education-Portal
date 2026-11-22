using EducationPortal.Application.Academics.Commands.CreateAssessment;
using EducationPortal.Application.Academics.Common;
using EducationPortal.Application.Academics.Queries.GetAssessments;
using EducationPortal.Application.Common.Security;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EducationPortal.Web.Controllers;

/// <summary>Assessments of a course (ADR-019).</summary>
[Route("api/Assessments")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class AssessmentsController(
    IRequestRules rules,
    GetAssessmentsQueryHandler getAssessments,
    CreateAssessmentCommandHandler createAssessment) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List the assessments of a course")]
    [EndpointDescription("Returns the assessments of one course by name. Needs the Exams module and the View right.")]
    public async Task<ActionResult<IReadOnlyList<AssessmentDto>>> GetAssessments([BindRequired] Guid courseId, CancellationToken cancellationToken)
    {
        var query = new GetAssessmentsQuery(courseId);
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getAssessments.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Add an assessment")]
    [EndpointDescription("Adds an assessment to a course: its weight (the weights of a course cannot add up to more than 100) and its maximum score. The course's teacher or an administrator adds it. Needs the Exams module and the Create right.")]
    public async Task<ActionResult<Guid>> CreateAssessment(CreateAssessmentCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        return Ok(await createAssessment.Handle(command, cancellationToken));
    }
}
