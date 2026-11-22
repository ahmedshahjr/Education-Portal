using EducationPortal.Application.Calendar.Commands.MarkNotificationRead;
using EducationPortal.Application.Calendar.Common;
using EducationPortal.Application.Calendar.Queries.GetMyNotifications;
using EducationPortal.Application.Common.Security;
using EducationPortal.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace EducationPortal.Web.Controllers;

/// <summary>The signed-in account's own notifications (ADR-019). Each one is shown to its recipient only.</summary>
[Route("api/Notifications")]
[Microsoft.AspNetCore.Authorization.Authorize]
public class NotificationsController(
    IRequestRules rules,
    GetMyNotificationsQueryHandler getMyNotifications,
    MarkNotificationReadCommandHandler markRead) : ApiControllerBase
{
    [HttpGet]
    [EndpointSummary("List my notifications")]
    [EndpointDescription("Returns the newest notifications of the signed-in account and the unread count.")]
    public async Task<ActionResult<NotificationsDto>> GetMyNotifications(CancellationToken cancellationToken)
    {
        var query = new GetMyNotificationsQuery();
        await rules.CheckAsync(query, cancellationToken);
        return Ok(await getMyNotifications.Handle(query, cancellationToken));
    }

    [HttpPost("{id:guid}/read")]
    [ValidateAntiforgeryToken] // SEC-WEB-03: unsafe requests need the token
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Mark a notification as read")]
    [EndpointDescription("Marks one of the signed-in account's notifications as read. Another account's notification is not found.")]
    public async Task<IActionResult> MarkNotificationRead(Guid id, CancellationToken cancellationToken)
    {
        var command = new MarkNotificationReadCommand { Id = id };
        await rules.CheckAsync(command, cancellationToken);
        await markRead.Handle(command, cancellationToken);
        return NoContent();
    }
}
