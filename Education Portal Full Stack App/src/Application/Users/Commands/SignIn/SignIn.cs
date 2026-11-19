using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Users.Commands.SignIn;

/// <summary>
/// Signs a user in with their user name and password. The user name is the account's login name (accounts are created
/// with their email address as the name, so either works). This is the one request that runs before sign-in
/// (SEC-AUTHN-01); its endpoint is rate limited and antiforgery protected, and every outcome is audited.
/// Only accounts of the school that owns the request's domain can sign in there (AGENTS.md section 2).
/// </summary>
public record SignInCommand : IRequest<bool>
{
    public string UserName { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}

public class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(c => c.UserName)
            .NotEmpty()
            .MaximumLength(256)
            // No control characters (CR and LF in particular), so a name cannot forge a line in the logs (log injection).
            .Must(name => !name.Any(char.IsControl));

        RuleFor(c => c.Password)
            .NotEmpty()
            .MaximumLength(PasswordRules.MaximumLength);
    }
}

public class SignInCommandHandler : IRequestHandler<SignInCommand, bool>
{
    private readonly IIdentityService _identityService;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;

    public SignInCommandHandler(IIdentityService identityService, ISecurityAuditLog auditLog, ICurrentTenant tenant)
    {
        _identityService = identityService;
        _auditLog = auditLog;
        _tenant = tenant;
    }

    public async Task<bool> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new UnauthorizedAccessException();

        var result = await _identityService.PasswordSignInAsync(schoolId, request.UserName, request.Password);
        var succeeded = result.Status == PasswordSignInStatus.Succeeded;

        var eventType = result.Status switch
        {
            PasswordSignInStatus.Succeeded => SecurityAuditEventType.SignInSucceeded,
            PasswordSignInStatus.LockedOut => SecurityAuditEventType.SignInLockedOut,
            PasswordSignInStatus.WrongSchool => SecurityAuditEventType.TenantMismatch,
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
