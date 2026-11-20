using EducationPortal.Application.Academics.Commands.CreateGradingScale;
using EducationPortal.Application.Academics.Common;
using EducationPortal.Application.Academics.Queries.GetGradingScales;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>The school's grading scales (ADR-019).</summary>
public class GradingScales : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetGradingScales)
            .RequireAuthorization();

        groupBuilder.MapPost(CreateGradingScale)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List grading scales")]
    [EndpointDescription("Returns the school's grading scales with their bands. Needs the Exams module and the View right.")]
    public static async Task<Ok<IReadOnlyList<GradingScaleDto>>> GetGradingScales(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetGradingScalesQuery()));
    }

    [EndpointSummary("Create a grading scale")]
    [EndpointDescription("Creates a grading scale. Its bands must cover 0 to 100 once, with no gap and no overlap. Needs the Exams module and the Create right.")]
    public static async Task<Ok<Guid>> CreateGradingScale(ISender sender, CreateGradingScaleCommand command)
    {
        return TypedResults.Ok(await sender.Send(command));
    }
}
