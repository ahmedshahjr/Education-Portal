using EducationPortal.Application.Academics.Commands.EnrolStudent;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>Enrolments of students in class offerings (ADR-019).</summary>
public class Enrolments : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(EnrolStudent)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("Enrol a student")]
    [EndpointDescription("Enrols a student of the school in a class offering, once. Needs the Academic setup module and the Create right.")]
    public static async Task<Ok<Guid>> EnrolStudent(ISender sender, EnrolStudentCommand command)
    {
        return TypedResults.Ok(await sender.Send(command));
    }
}
