using EducationPortal.Application.SchoolPublicPage.Commands.RemoveFacility;
using EducationPortal.Application.SchoolPublicPage.Commands.SetFacility;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Application.SchoolPublicPage.Queries.GetFacilities;
using EducationPortal.Domain.Enums;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>The facilities a school lists on its public page, one per kind (School module). The master tenant names the school with <c>schoolId</c>.</summary>
public class SchoolFacilities : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetFacilities)
            .RequireAuthorization();

        groupBuilder.MapPut(SetFacility, "{kind}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(RemoveFacility, "{kind}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List the school's facilities")]
    [EndpointDescription("Returns the facilities the school lists on its public page, with their notes. Needs the School module.")]
    public static async Task<Ok<IReadOnlyList<SchoolFacilityDto>>> GetFacilities(ISender sender, Guid? schoolId)
    {
        return TypedResults.Ok(await sender.Send(new GetFacilitiesQuery { SchoolId = schoolId }));
    }

    [EndpointSummary("Add or change a facility")]
    [EndpointDescription("Lists a facility kind on the public page, or changes its note when already listed. The kind is in the route. Needs the School module.")]
    public static async Task<NoContent> SetFacility(ISender sender, Guid? schoolId, FacilityKind kind, SetFacilityCommand command)
    {
        await sender.Send(command with { SchoolId = schoolId, Kind = kind });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Remove a facility")]
    [EndpointDescription("Stops listing a facility kind on the public page. Needs the School module.")]
    public static async Task<NoContent> RemoveFacility(ISender sender, Guid? schoolId, FacilityKind kind)
    {
        await sender.Send(new RemoveFacilityCommand { SchoolId = schoolId, Kind = kind });

        return TypedResults.NoContent();
    }
}
