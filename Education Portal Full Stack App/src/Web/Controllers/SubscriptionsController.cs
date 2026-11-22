using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Subscriptions.Queries.GetMySubscription;
using EducationPortal.Application.Subscriptions.Queries.GetTrialNotice;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>The signed-in school administrator's own subscription. The school comes from the host, never from the request (SEC-AUTHZ-04).</summary>
[Route("api/Subscriptions")]
public class SubscriptionsController(
    IRequestRules rules,
    GetMySubscriptionQueryHandler getMySubscription,
    GetTrialNoticeQueryHandler getTrialNotice) : ApiControllerBase
{
    [HttpGet("mine")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [EndpointSummary("Get my subscription")]
    [EndpointDescription("Returns the current school's subscription (or none), the paid modules it could add with their prices, and its add-on requests.")]
    public async Task<ActionResult<MySubscriptionDto>> GetMySubscription(CancellationToken cancellationToken)
    {
        var query = new GetMySubscriptionQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getMySubscription.Handle(query, cancellationToken));
    }

    [HttpGet("trial-notice")]
    // compliance-exception: SEC-AUTHN-01 the sign-in page shows the host school's trial notice before sign-in; only a flag and a day count (like branding)
    [AllowAnonymous]
    [EndpointSummary("Get the trial notice")]
    [EndpointDescription("Returns whether the school of this host is on its free trial and the whole days left. Open without sign-in, for the banner above the sign-in page.")]
    public async Task<ActionResult<TrialNoticeDto>> GetTrialNotice(CancellationToken cancellationToken)
    {
        var query = new GetTrialNoticeQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getTrialNotice.Handle(query, cancellationToken));
    }
}
