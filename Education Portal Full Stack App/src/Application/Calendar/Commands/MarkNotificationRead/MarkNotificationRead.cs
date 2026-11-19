using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;

namespace EducationPortal.Application.Calendar.Commands.MarkNotificationRead;

/// <summary>
/// Marks one of the signed-in account's notifications as read. Another account's notification is reported as not found, so
/// its existence is not revealed (AGENTS.md section 2, IDOR).
/// </summary>
[Authorize]
public record MarkNotificationReadCommand : IRequest
{
    public Guid Id { get; init; }
}

public class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
{
    public MarkNotificationReadCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public MarkNotificationReadCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notificationId = await _context.Notifications.ResolveIdAsync(request.Id, cancellationToken);

        var notification = await _context.Notifications.FirstOrDefaultAsync(
            n => n.Id == notificationId && n.RecipientAccountId == _user.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, notification);

        notification.MarkRead(DateTimeOffset.UtcNow);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
