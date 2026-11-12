using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Users.Commands.SignOut;

/// <summary>
/// Signs the current user out everywhere: the security stamp is rotated, so every session cookie the
/// user has stops working on its next request (SEC-AUTHN-07).
/// </summary>
[Authorize]
public record SignOutCommand : IRequest;

public class SignOutCommandHandler : IRequestHandler<SignOutCommand>
{
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly ISecurityAuditLog _auditLog;

    public SignOutCommandHandler(IUser user, IIdentityService identityService, ISecurityAuditLog auditLog)
    {
        _user = user;
        _identityService = identityService;
        _auditLog = auditLog;
    }

    public async Task Handle(SignOutCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id ?? throw new UnauthorizedAccessException();

        await _identityService.SignOutAsync(userId);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.SignedOut,
            SecurityAuditOutcome.Success,
            actorUserId: userId,
            targetType: "User",
            targetId: userId);
    }
}
