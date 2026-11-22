using EducationPortal.Application.Academics.Commands.EnrolStudent;
using EducationPortal.Application.Common.Security;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>Enrolments of students in class offerings (ADR-019).</summary>
[Route("api/Enrolments")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class EnrolmentsController(IRequestRules rules, EnrolStudentCommandHandler enrolStudent) : ApiControllerBase
{
    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Enrol a student")]
    [EndpointDescription("Enrols a student of the school in a class offering, once. Needs the Academic setup module and the Create right.")]
    public async Task<ActionResult<Guid>> EnrolStudent(EnrolStudentCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        return Ok(await enrolStudent.Handle(command, cancellationToken));
    }
}
