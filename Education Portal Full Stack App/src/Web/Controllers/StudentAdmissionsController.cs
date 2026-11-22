using EducationPortal.Application.Common.Security;
using EducationPortal.Application.StudentAdmissions.Commands.CreateStudentAdmission;
using EducationPortal.Application.StudentAdmissions.Dtos;
using EducationPortal.Application.StudentAdmissions.Queries.GetStudentAdmission;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>
/// Student admissions of the current school: the admission form of each student (Students module). The school is the one
/// the request's host resolves to, so a school never reaches another school's students (AGENTS.md section 2).
/// </summary>
[Route("api/StudentAdmissions")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class StudentAdmissionsController(
    IRequestRules rules,
    CreateStudentAdmissionCommandHandler createStudentAdmission,
    GetStudentAdmissionQueryHandler getStudentAdmission) : ApiControllerBase
{
    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Admit a student")]
    [EndpointDescription("Adds a student and the basic fields of the admission form (date of birth, gender, father's name, nationality, address, city, class applied for, previous school, guardian phone, admission date). The student must be 3 to 30 years old on the admission date. Needs the Students module.")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateStudentAdmission([FromBody] CreateStudentAdmissionCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        var id = await createStudentAdmission.Handle(command, cancellationToken);
        return Created($"/api/StudentAdmissions/{id}", id);
    }

    [HttpGet("{id:guid}")]
    [EndpointSummary("Get a student's admission form")]
    [EndpointDescription("Returns one student's admission form, named by the student's id. Needs the Students module.")]
    public async Task<ActionResult<StudentAdmissionDto>> GetStudentAdmission(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetStudentAdmissionQuery(id);
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getStudentAdmission.Handle(query, cancellationToken));
    }
}
