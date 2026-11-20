using EducationPortal.Application.SchoolPublicPage.Commands.PublishSchoolProfile;
using EducationPortal.Application.SchoolPublicPage.Commands.UnpublishSchoolProfile;
using EducationPortal.Application.SchoolPublicPage.Commands.UpdateSchoolProfile;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Application.SchoolPublicPage.Queries.GetSchoolProfile;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>
/// The text of a school's public page, for its administrator (School module). The master tenant names the school with
/// <c>schoolId</c>; a school user may leave it out, and naming another school is a 404.
/// </summary>
public class SchoolProfile : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetSchoolProfile)
            .RequireAuthorization();

        groupBuilder.MapPut(UpdateSchoolProfile, "")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPost(PublishSchoolProfile, "publish")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token

        groupBuilder.MapPost(UnpublishSchoolProfile, "unpublish")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("Get the school's public page text")]
    [EndpointDescription("Returns the tagline, about text, established year, gender policy, visiting hours, tour link and publication state. Needs the School module.")]
    public static async Task<Ok<SchoolProfileDto>> GetSchoolProfile(ISender sender, Guid? schoolId)
    {
        return TypedResults.Ok(await sender.Send(new GetSchoolProfileQuery { SchoolId = schoolId }));
    }

    [EndpointSummary("Save the school's public page text")]
    [EndpointDescription("Creates the page on first save, then replaces its text. Needs the School module.")]
    public static async Task<NoContent> UpdateSchoolProfile(ISender sender, Guid? schoolId, UpdateSchoolProfileCommand command)
    {
        await sender.Send(command with { SchoolId = schoolId });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Publish the school's public page")]
    [EndpointDescription("Shows the page to the public. The page needs a tagline and an about text. Needs the School module.")]
    public static async Task<NoContent> PublishSchoolProfile(ISender sender, Guid? schoolId)
    {
        await sender.Send(new PublishSchoolProfileCommand { SchoolId = schoolId });

        return TypedResults.NoContent();
    }

    [EndpointSummary("Unpublish the school's public page")]
    [EndpointDescription("Hides the page from the public. The text is kept. Needs the School module.")]
    public static async Task<NoContent> UnpublishSchoolProfile(ISender sender, Guid? schoolId)
    {
        await sender.Send(new UnpublishSchoolProfileCommand { SchoolId = schoolId });

        return TypedResults.NoContent();
    }
}
