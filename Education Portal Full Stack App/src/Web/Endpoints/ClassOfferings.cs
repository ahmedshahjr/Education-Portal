using EducationPortal.Application.SchoolPublicPage.Commands.CreateClassOffering;
using EducationPortal.Application.SchoolPublicPage.Commands.DeleteClassOffering;
using EducationPortal.Application.SchoolPublicPage.Commands.UpdateClassOffering;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Application.SchoolPublicPage.Queries.GetClassOfferings;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>A school's class offerings shown on its public page (School module). The master tenant names the school with <c>schoolId</c>.</summary>
public class ClassOfferings : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetClassOfferings)
            .RequireAuthorization();

        groupBuilder.MapPost(CreateClassOffering)
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPut(UpdateClassOffering, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapDelete(DeleteClassOffering, "{id:guid}")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List the school's class offerings")]
    [EndpointDescription("Returns the class offerings of the school that are not deleted. Needs the School module.")]
    public static async Task<Ok<IReadOnlyList<ClassOfferingDto>>> GetClassOfferings(ISender sender, Guid? schoolId)
    {
        return TypedResults.Ok(await sender.Send(new GetClassOfferingsQuery { SchoolId = schoolId }));
    }

    [EndpointSummary("Add a class offering")]
    [EndpointDescription("Adds a class offering for the whole school or one campus. Needs the School module.")]
    public static async Task<Ok<Guid>> CreateClassOffering(ISender sender, Guid? schoolId, CreateClassOfferingCommand command)
    {
        return TypedResults.Ok(await sender.Send(command with { SchoolId = schoolId }));
    }

    [EndpointSummary("Change a class offering")]
    [EndpointDescription("Changes the year, fees, seats, status and fee display of an offering. Needs the School module.")]
    public static async Task<NoContent> UpdateClassOffering(ISender sender, Guid? schoolId, Guid id, UpdateClassOfferingCommand command)
    {
        // The route names the offering; an id in the body is ignored.
        await sender.Send(command with { SchoolId = schoolId, OfferingId = id });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete a class offering")]
    [EndpointDescription("Deletes a class offering (kept for audit, hidden everywhere). Needs the School module.")]
    public static async Task<NoContent> DeleteClassOffering(ISender sender, Guid? schoolId, Guid id)
    {
        await sender.Send(new DeleteClassOfferingCommand { SchoolId = schoolId, OfferingId = id });

        return TypedResults.NoContent();
    }
}
