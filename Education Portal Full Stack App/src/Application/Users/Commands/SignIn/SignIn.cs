using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Users.Commands.SignIn;

/// <summary>
/// Signs a user in with email and password. This is the one request that runs before sign-in
/// (SEC-AUTHN-01); its endpoint is rate limited and antiforgery protected, and every outcome is audited.
/// Only accounts of the school that owns the request's domain can sign in there (AGENTS.md section 2).
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

        var result = await _identityService.PasswordSignInAsync(schoolId, request.Email, request.Password);
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
