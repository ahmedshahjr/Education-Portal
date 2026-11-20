using EducationPortal.Application.AcademicCalendar.Commands.CreateAcademicYear;
using EducationPortal.Application.AcademicCalendar.Commands.CreateTerm;
using EducationPortal.Application.AcademicCalendar.Commands.DeleteAcademicYear;
using EducationPortal.Application.AcademicCalendar.Commands.DeleteTerm;
using EducationPortal.Application.AcademicCalendar.Commands.SetCurrentAcademicYear;
using EducationPortal.Application.AcademicCalendar.Commands.UpdateAcademicYear;
using EducationPortal.Application.AcademicCalendar.Commands.UpdateTerm;
using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.AcademicCalendar.Queries.GetAcademicYears;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>A school's academic years and their terms (Academic setup module). The master tenant names the school with <c>schoolId</c>.</summary>
public class AcademicYears : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetAcademicYears)
            .RequireAuthorization();

        groupBuilder.MapPost(CreateAcademicYear)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdateAcademicYear, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(DeleteAcademicYear, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(SetCurrentAcademicYear, "{id:guid}/current")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPost(CreateTerm, "{id:guid}/terms")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdateTerm, "terms/{termId:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(DeleteTerm, "terms/{termId:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List academic years")]
    [EndpointDescription("Returns the school's academic years, newest first, each with its terms. Needs the Academic setup module.")]
    public static async Task<Ok<IReadOnlyList<AcademicYearDto>>> GetAcademicYears(ISender sender, Guid? schoolId)
    {
        return TypedResults.Ok(await sender.Send(new GetAcademicYearsQuery { SchoolId = schoolId }));
    }

    [EndpointSummary("Add an academic year")]
    [EndpointDescription("Adds a planned academic year whose name is unique and whose dates do not overlap another year. Needs the Academic setup module.")]
    public static async Task<Ok<Guid>> CreateAcademicYear(ISender sender, Guid? schoolId, CreateAcademicYearCommand command)
    {
        return TypedResults.Ok(await sender.Send(command with { SchoolId = schoolId }));
    }

    [EndpointSummary("Change an academic year")]
    [EndpointDescription("Changes a year's name, dates and status. A closed year can only be reopened. Needs the Academic setup module.")]
    public static async Task<NoContent> UpdateAcademicYear(ISender sender, Guid? schoolId, Guid id, UpdateAcademicYearCommand command)
    {
        // The route names the year; an id in the body is ignored.
        await sender.Send(command with { SchoolId = schoolId, AcademicYearId = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete an academic year")]
    [EndpointDescription("Deletes a year that has no terms (kept for audit, hidden everywhere). Needs the Academic setup module.")]
    public static async Task<NoContent> DeleteAcademicYear(ISender sender, Guid? schoolId, Guid id)
    {
        await sender.Send(new DeleteAcademicYearCommand { SchoolId = schoolId, AcademicYearId = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Make an academic year current")]
    [EndpointDescription("Makes the year the school's current year; the previous one stops being current. Needs the Academic setup module.")]
    public static async Task<NoContent> SetCurrentAcademicYear(ISender sender, Guid? schoolId, Guid id)
    {
        await sender.Send(new SetCurrentAcademicYearCommand { SchoolId = schoolId, AcademicYearId = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Add a term")]
    [EndpointDescription("Adds a term inside the academic year. Its number and dates must not clash with the year's other terms. Needs the Academic setup module.")]
    public static async Task<Ok<Guid>> CreateTerm(ISender sender, Guid? schoolId, Guid id, CreateTermCommand command)
    {
        return TypedResults.Ok(await sender.Send(command with { SchoolId = schoolId, AcademicYearId = id }));
    }

    [EndpointSummary("Change a term")]
    [EndpointDescription("Changes a term's name, number and dates. Needs the Academic setup module.")]
    public static async Task<NoContent> UpdateTerm(ISender sender, Guid? schoolId, Guid termId, UpdateTermCommand command)
    {
        await sender.Send(command with { SchoolId = schoolId, TermId = termId });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a term")]
    [EndpointDescription("Deletes a term of a year that is not closed (kept for audit, hidden everywhere). Needs the Academic setup module.")]
    public static async Task<NoContent> DeleteTerm(ISender sender, Guid? schoolId, Guid termId)
    {
        await sender.Send(new DeleteTermCommand { SchoolId = schoolId, TermId = termId });

        return TypedResults.NoContent();
    }
}
