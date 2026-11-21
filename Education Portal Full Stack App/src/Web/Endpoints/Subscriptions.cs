using EducationPortal.Application.Subscriptions.Queries.GetMySubscription;
using EducationPortal.Application.Subscriptions.Queries.GetTrialNotice;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>The signed-in school administrator's own subscription. The school comes from the host, never from the request (SEC-AUTHZ-04).</summary>
public class Subscriptions : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetMySubscription, "mine")
            .RequireAuthorization();

        groupBuilder.MapGet(GetTrialNotice, "trial-notice")
            // compliance-exception: SEC-AUTHN-01 the sign-in page shows the host school's trial notice before anyone signs in;
            // only a flag and a day count, for the school of the host (like branding)
            .AllowAnonymous();
    }

    [EndpointSummary("Get my subscription")]
    [EndpointDescription("Returns the current school's subscription (or none), the paid modules it could add with their prices, and its add-on requests.")]
    public static async Task<Ok<MySubscriptionDto>> GetMySubscription(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetMySubscriptionQuery()));
    }

    [EndpointSummary("Get the trial notice")]
    [EndpointDescription("Returns whether the school of this host is on its free trial and the whole days left. Open without sign-in, for the banner above the sign-in page.")]
    public static async Task<Ok<TrialNoticeDto>> GetTrialNotice(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetTrialNoticeQuery()));
    }
}
