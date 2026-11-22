using EducationPortal.Application.Academics.Commands.CreateCourse;
using EducationPortal.Application.Academics.Common;
using EducationPortal.Application.Academics.Queries.GetCourses;
using EducationPortal.Application.Common.Security;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>The school's courses (ADR-019). Each use case checks its module and right on the server.</summary>
[Route("api/Courses")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class CoursesController(
    IRequestRules rules,
    GetCoursesQueryHandler getCourses,
    CreateCourseCommandHandler createCourse) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List courses")]
    [EndpointDescription("Returns the school's courses by name, optionally for one class offering. Needs the Academic setup module and the View right.")]
    public async Task<ActionResult<IReadOnlyList<CourseDto>>> GetCourses(Guid? classOfferingId, CancellationToken cancellationToken)
    {
        var query = new GetCoursesQuery { ClassOfferingId = classOfferingId };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getCourses.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Add a course")]
    [EndpointDescription("Adds a course to a class offering, taught by a teacher of the school. Needs the Academic setup module and the Create right.")]
    public async Task<ActionResult<Guid>> CreateCourse(CreateCourseCommand command, CancellationToken cancellationToken)
    {
        await rules.CheckAsync(command, cancellationToken);
        return Ok(await createCourse.Handle(command, cancellationToken));
    }
}
