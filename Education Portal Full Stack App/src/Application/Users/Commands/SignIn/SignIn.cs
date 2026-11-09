using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Users.Commands.SignIn;

/// <summary>
/// Signs a user in with email and password. This is the one request that runs before sign-in
/// (SEC-AUTHN-01); its endpoint is rate limited and antiforgery protected, and every outcome is audited.
/// </summary>
public record SignInCommand : IRequest<bool>
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}

public class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
            .MaximumLength(256)
            .EmailAddress();

        RuleFor(c => c.Password)
            .NotEmpty()
            .MaximumLength(PasswordRules.MaximumLength);
    }
}

public class SignInCommandHandler : IRequestHandler<SignInCommand, bool>
{
    private readonly IIdentityService _identityService;
    private readonly ISecurityAuditLog _auditLog;

    public SignInCommandHandler(IIdentityService identityService, ISecurityAuditLog auditLog)
    {
        _identityService = identityService;
        _auditLog = auditLog;
    }

    public async Task<bool> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var result = await _identityService.PasswordSignInAsync(request.Email, request.Password);
        var succeeded = result.Status == PasswordSignInStatus.Succeeded;

        var eventType = result.Status switch
        {
            PasswordSignInStatus.Succeeded => SecurityAuditEventType.SignInSucceeded,
            PasswordSignInStatus.LockedOut => SecurityAuditEventType.SignInLockedOut,
            _ => SecurityAuditEventType.SignInFailed,
        };

        await _auditLog.WriteAsync(
            eventType,
            succeeded ? SecurityAuditOutcome.Success : SecurityAuditOutcome.Failure,
            actorUserId: result.UserId,
            targetType: "User",
            targetId: result.UserId);

        return succeeded;
    }
}
