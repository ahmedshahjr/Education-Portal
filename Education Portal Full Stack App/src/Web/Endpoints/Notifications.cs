using EducationPortal.Application.Calendar.Commands.MarkNotificationRead;
using EducationPortal.Application.Calendar.Common;
using EducationPortal.Application.Calendar.Queries.GetMyNotifications;
using EducationPortal.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EducationPortal.Web.Endpoints;

/// <summary>The signed-in account's own notifications (ADR-019). Each one is shown to its recipient only.</summary>
public class Notifications : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(GetMyNotifications)
            .RequireAuthorization();

        groupBuilder.MapPost(MarkNotificationRead, "{id:guid}/read")
            .RequireAuthorization()
            .WithMetadata(new RequireAntiforgeryTokenAttribute()); // SEC-WEB-03: unsafe requests need the token
    }

    [EndpointSummary("List my notifications")]
    [EndpointDescription("Returns the newest notifications of the signed-in account and the unread count.")]
    public static async Task<Ok<NotificationsDto>> GetMyNotifications(ISender sender)
    {
        return TypedResults.Ok(await sender.Send(new GetMyNotificationsQuery()));
    }

    [EndpointSummary("Mark a notification as read")]
    [EndpointDescription("Marks one of the signed-in account's notifications as read. Another account's notification is not found.")]
    public static async Task<NoContent> MarkNotificationRead(ISender sender, Guid id)
    {
        await sender.Send(new MarkNotificationReadCommand { Id = id });

        return TypedResults.NoContent();
    }
}
