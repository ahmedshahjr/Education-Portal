using EducationPortal.Application.Academics.Common;
using EducationPortal.Application.Academics.Queries.GetReportCard;
using EducationPortal.Application.Common.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace EducationPortal.Web.Controllers;

/// <summary>Report cards of students in a class (ADR-019).</summary>
[Route("api/ReportCards")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class ReportCardsController(IRequestRules rules, GetReportCardQueryHandler getReportCard) : ApiControllerBase
{
    [HttpGet("{studentId:guid}")]
    [EndpointSummary("Get a report card")]
    [EndpointDescription("Returns a student's report card for one class, graded with one grading scale. Administrators and the student or a linked parent see every course; a teacher sees only their own courses.")]
    public async Task<ActionResult<ReportCardDto>> GetReportCard(
        Guid studentId, [BindRequired] Guid classOfferingId, [BindRequired] Guid gradingScaleId, CancellationToken cancellationToken)
    {
        var query = new GetReportCardQuery
        {
            StudentId = studentId,
            ClassOfferingId = classOfferingId,
            GradingScaleId = gradingScaleId,
        };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getReportCard.Handle(query, cancellationToken));
    }
}
