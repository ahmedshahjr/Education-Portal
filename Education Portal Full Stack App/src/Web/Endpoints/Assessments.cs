using EducationPortal.Application.Academics.Commands.CreateAssessment;
using EducationPortal.Application.Academics.Common;
using EducationPortal.Application.Academics.Queries.GetAssessments;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>Assessments of a course (ADR-019).</summary>
public class Assessments : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetAssessments)
            .RequireAuthorization();

        groupBuilder.MapPost(CreateAssessment)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List the assessments of a course")]
    [EndpointDescription("Returns the assessments of one course by name. Needs the Exams module and the View right.")]
    public static async Task<Ok<IReadOnlyList<AssessmentDto>>> GetAssessments(ISender sender, Guid courseId)
    {
        return TypedResults.Ok(await sender.Send(new GetAssessmentsQuery(courseId)));
    }

    [EndpointSummary("Add an assessment")]
    [EndpointDescription("Adds an assessment to a course: its weight (the weights of a course cannot add up to more than 100) and its maximum score. The course's teacher or an administrator adds it. Needs the Exams module and the Create right.")]
    public static async Task<Ok<Guid>> CreateAssessment(ISender sender, CreateAssessmentCommand command)
    {
        return TypedResults.Ok(await sender.Send(command));
    }
}
