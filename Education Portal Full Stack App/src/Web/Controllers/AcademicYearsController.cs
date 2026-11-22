using EducationPortal.Application.AcademicCalendar.Commands.CreateAcademicYear;
using EducationPortal.Application.AcademicCalendar.Commands.CreateTerm;
using EducationPortal.Application.AcademicCalendar.Commands.DeleteAcademicYear;
using EducationPortal.Application.AcademicCalendar.Commands.DeleteTerm;
using EducationPortal.Application.AcademicCalendar.Commands.SetCurrentAcademicYear;
using EducationPortal.Application.AcademicCalendar.Commands.UpdateAcademicYear;
using EducationPortal.Application.AcademicCalendar.Commands.UpdateTerm;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.AcademicCalendar.Queries.GetAcademicYears;
using EducationPortal.Application.Common.Security;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>A school's academic years and their terms (Academic setup module). The master tenant names the school with <c>schoolId</c>.</summary>
[Route("api/AcademicYears")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class AcademicYearsController(
    IRequestRules rules,
    GetAcademicYearsQueryHandler getYears,
    CreateAcademicYearCommandHandler createYear,
    UpdateAcademicYearCommandHandler updateYear,
    DeleteAcademicYearCommandHandler deleteYear,
    SetCurrentAcademicYearCommandHandler setCurrent,
    CreateTermCommandHandler createTerm,
    UpdateTermCommandHandler updateTerm,
    DeleteTermCommandHandler deleteTerm) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List academic years")]
    [EndpointDescription("Returns the school's academic years, newest first, each with its terms. Needs the Academic setup module.")]
    public async Task<ActionResult<IReadOnlyList<AcademicYearDto>>> GetAcademicYears(Guid? schoolId, CancellationToken cancellationToken)
    {
        var query = new GetAcademicYearsQuery { SchoolId = schoolId };
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getYears.Handle(query, cancellationToken));
    }

    [HttpPost]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Add an academic year")]
    [EndpointDescription("Adds a planned academic year whose name is unique and whose dates do not overlap another year. Needs the Academic setup module.")]
    public async Task<ActionResult<Guid>> CreateAcademicYear(Guid? schoolId, CreateAcademicYearCommand command, CancellationToken cancellationToken)
    {
        var request = command with { SchoolId = schoolId };
        await rules.CheckAsync(request, cancellationToken);
        return Ok(await createYear.Handle(request, cancellationToken));
    }

    [HttpPut("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change an academic year")]
    [EndpointDescription("Changes a year's name, dates and status. A closed year can only be reopened. Needs the Academic setup module.")]
    public async Task<IActionResult> UpdateAcademicYear(Guid? schoolId, Guid id, UpdateAcademicYearCommand command, CancellationToken cancellationToken)
    {
        // The route names the year; an id in the body is ignored.
        var request = command with { SchoolId = schoolId, AcademicYearId = id };
        await rules.CheckAsync(request, cancellationToken);
        await updateYear.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Delete an academic year")]
    [EndpointDescription("Deletes a year that has no terms (kept for audit, hidden everywhere). Needs the Academic setup module.")]
    public async Task<IActionResult> DeleteAcademicYear(Guid? schoolId, Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteAcademicYearCommand { SchoolId = schoolId, AcademicYearId = id };
        await rules.CheckAsync(command, cancellationToken);
        await deleteYear.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpPut("{id:guid}/current")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Make an academic year current")]
    [EndpointDescription("Makes the year the school's current year; the previous one stops being current. Needs the Academic setup module.")]
    public async Task<IActionResult> SetCurrentAcademicYear(Guid? schoolId, Guid id, CancellationToken cancellationToken)
    {
        var command = new SetCurrentAcademicYearCommand { SchoolId = schoolId, AcademicYearId = id };
        await rules.CheckAsync(command, cancellationToken);
        await setCurrent.Handle(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/terms")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [EndpointSummary("Add a term")]
    [EndpointDescription("Adds a term inside the academic year. Its number and dates must not clash with the year's other terms. Needs the Academic setup module.")]
    public async Task<ActionResult<Guid>> CreateTerm(Guid? schoolId, Guid id, CreateTermCommand command, CancellationToken cancellationToken)
    {
        var request = command with { SchoolId = schoolId, AcademicYearId = id };
        await rules.CheckAsync(request, cancellationToken);
        return Ok(await createTerm.Handle(request, cancellationToken));
    }

    [HttpPut("terms/{termId:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Change a term")]
    [EndpointDescription("Changes a term's name, number and dates. Needs the Academic setup module.")]
    public async Task<IActionResult> UpdateTerm(Guid? schoolId, Guid termId, UpdateTermCommand command, CancellationToken cancellationToken)
    {
        var request = command with { SchoolId = schoolId, TermId = termId };
        await rules.CheckAsync(request, cancellationToken);
        await updateTerm.Handle(request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("terms/{termId:guid}")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Delete a term")]
    [EndpointDescription("Deletes a term of a year that is not closed (kept for audit, hidden everywhere). Needs the Academic setup module.")]
    public async Task<IActionResult> DeleteTerm(Guid? schoolId, Guid termId, CancellationToken cancellationToken)
    {
        var command = new DeleteTermCommand { SchoolId = schoolId, TermId = termId };
        await rules.CheckAsync(command, cancellationToken);
        await deleteTerm.Handle(command, cancellationToken);
        return NoContent();
    }
}
