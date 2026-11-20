using EducationPortal.Application.SchoolPublicPage.Commands.SetCurricula;
using EducationPortal.Application.SchoolPublicPage.Queries.GetCurricula;
using EducationPortal.Domain.Enums;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>The curricula a school lists on its public page (School module). The master tenant names the school with <c>schoolId</c>.</summary>
public class SchoolCurricula : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetCurricula)
            .RequireAuthorization();

        groupBuilder.MapPut(SetCurricula, "")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List the school's curricula")]
    [EndpointDescription("Returns the curricula the school shows on its public page. Needs the School module.")]
    public static async Task<Ok<IReadOnlyList<CurriculumKind>>> GetCurricula(ISender sender, Guid? schoolId)
    {
        return TypedResults.Ok(await sender.Send(new GetCurriculaQuery { SchoolId = schoolId }));
    }

    [EndpointSummary("Replace the school's curricula")]
    [EndpointDescription("Replaces the set of curricula shown on the public page; each kind at most once. Needs the School module.")]
    public static async Task<NoContent> SetCurricula(ISender sender, Guid? schoolId, SetCurriculaCommand command)
    {
        await sender.Send(command with { SchoolId = schoolId });

        return TypedResults.NoContent();
    }
}
