using EducationPortal.Application.DevelopmentData.Commands.GenerateDevelopmentData;
using EducationPortal.Application.DevelopmentData.Common;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// Development only: creates many fake schools, campuses and people in one call, for trying the portal and for load
/// testing. Outside Development the route answers 404. Master tenant administrators only (SEC-AUTHZ-07).
/// </summary>
public class DevelopmentData : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(GenerateDevelopmentData, "generate")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("Generate fake schools, campuses and people (Development only)")]
    [EndpointDescription("Creates the requested number of schools, each with campuses and people, in one call. Limits: at most 500 schools, 50 campuses per school, 5000 people per campus and 1,000,000 people per call. Development only; master tenant administrators only.")]
    public static async Task<Results<Ok<DevelopmentDataSummary>, NotFound>> GenerateDevelopmentData(
        ISender sender,
        IWebHostEnvironment environment,
        [FromBody] GenerateDevelopmentDataCommand command)
    {
        if (!environment.IsDevelopment())
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(await sender.Send(command));
    }
}
