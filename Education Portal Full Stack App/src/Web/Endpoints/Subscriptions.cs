using EducationPortal.Application.Subscriptions.Queries.GetMySubscription;
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
    }

    [EndpointSummary("Get my subscription")]
    [EndpointDescription("Returns the current school's subscription (or none), the paid modules it could add with their prices, and its add-on requests.")]
    public static async Task<Ok<MySubscriptionDto>> GetMySubscription(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetMySubscriptionQuery()));
    }
}
