using EducationPortal.Application.Schools.Commands.UpdateSchoolDetails;
using EducationPortal.Application.Schools.Queries.GetSchoolDetails;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>A school's registration and contact details (School module). The master tenant names the school with <c>schoolId</c>.</summary>
public class SchoolDetails : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetSchoolDetails)
            .RequireAuthorization();

        groupBuilder.MapPut(UpdateSchoolDetails, "")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("Read the school's details")]
    [EndpointDescription("Returns the school's registration and contact details. Needs the School module.")]
    public static async Task<Ok<SchoolDetailsDto>> GetSchoolDetails(ISender sender, Guid? schoolId)
    {
        return TypedResults.Ok(await sender.Send(new GetSchoolDetailsQuery { SchoolId = schoolId }));
    }

    [EndpointSummary("Change the school's details")]
    [EndpointDescription("Replaces the school's registration and contact details. The city must be in the school's country. Needs the School module.")]
    public static async Task<NoContent> UpdateSchoolDetails(ISender sender, Guid? schoolId, UpdateSchoolDetailsCommand command)
    {
        await sender.Send(command with { SchoolId = schoolId });

        return TypedResults.NoContent();
    }
}
