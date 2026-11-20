using EducationPortal.Application.Academics.Commands.CreateCourse;
using EducationPortal.Application.Academics.Common;
using EducationPortal.Application.Academics.Queries.GetCourses;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>The school's courses (ADR-019). Each use case checks its module and right on the server.</summary>
public class Courses : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetCourses)
            .RequireAuthorization();

        groupBuilder.MapPost(CreateCourse)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List courses")]
    [EndpointDescription("Returns the school's courses by name, optionally for one class offering. Needs the Academic setup module and the View right.")]
    public static async Task<Ok<IReadOnlyList<CourseDto>>> GetCourses(ISender sender, Guid? classOfferingId)
    {
        return TypedResults.Ok(await sender.Send(new GetCoursesQuery { ClassOfferingId = classOfferingId }));
    }

    [EndpointSummary("Add a course")]
    [EndpointDescription("Adds a course to a class offering, taught by a teacher of the school. Needs the Academic setup module and the Create right.")]
    public static async Task<Ok<Guid>> CreateCourse(ISender sender, CreateCourseCommand command)
    {
        return TypedResults.Ok(await sender.Send(command));
    }
}
