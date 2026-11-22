using EducationPortal.Application.Calendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;

namespace EducationPortal.Application.Calendar.Queries.GetMyNotifications;

/// <summary>The signed-in account's newest notifications and its unread count. Only its own notifications are returned.</summary>
[Authorize]
public record GetMyNotificationsQuery;

public class GetMyNotificationsQueryHandler
{
    /// <summary>How many of the newest notifications one request returns.</summary>
    public const int PageSize = 50;

    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public GetMyNotificationsQueryHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<NotificationsDto> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var items = await _context.Notifications.AsNoTracking()
            .Where(n => n.RecipientAccountId == _user.Id)
            .OrderByDescending(n => n.Created)
            .Take(PageSize)
            .Select(n => new NotificationDto(n.PublicId, n.Title, n.Body, n.Created, n.ReadAt))
            .ToListAsync(cancellationToken);

        var unread = await _context.Notifications
            .CountAsync(n => n.RecipientAccountId == _user.Id && n.ReadAt == null, cancellationToken);

        return new NotificationsDto(items, unread);
    }
}
