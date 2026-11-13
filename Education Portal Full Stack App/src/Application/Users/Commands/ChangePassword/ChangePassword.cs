using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.Users.Commands.ChangePassword;

/// <summary>
/// The signed-in user replaces their own password. It is how an account created with a one-time password becomes
/// usable, so it runs while that block is on. The new password follows the password policy (SEC-AUTHN-03); the change
/// ends the user's other sessions (SEC-AUTHN-07) and is audited (SEC-LOG-03).
/// </summary>
[Authorize(AllowWhilePasswordChangeRequired = true)]
public record ChangePasswordCommand : IRequest
{
    public string CurrentPassword { get; init; } = string.Empty;

    public string NewPassword { get; init; } = string.Empty;
}

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(c => c.CurrentPassword)
            .NotEmpty()
            .MaximumLength(PasswordRules.MaximumLength);

        RuleFor(c => c.NewPassword)
            .NotEmpty()
            .MinimumLength(PasswordRules.MinimumLength)
            .MaximumLength(PasswordRules.MaximumLength)
            .NotEqual(c => c.CurrentPassword)
            .WithMessage(_ => ValidationMessages.NewPasswordSameAsCurrent);
    }
}

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand>
{
    private readonly IUser _user;
    private readonly IIdentityService _identityService;
    private readonly ISecurityAuditLog _auditLog;

    public ChangePasswordCommandHandler(IUser user, IIdentityService identityService, ISecurityAuditLog auditLog)
    {
        _user = user;
        _identityService = identityService;
        _auditLog = auditLog;
    }

    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id ?? throw new UnauthorizedAccessException();

        var result = await _identityService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.PasswordChanged,
            result.Succeeded ? SecurityAuditOutcome.Success : SecurityAuditOutcome.Failure,
            targetType: "User",
            targetId: userId);

        if (!result.Succeeded)
        {
            // A wrong current password and a password the policy refuses come back the same way, as a validation error.
            throw new Common.Exceptions.ValidationException(result.Errors.Select(e => new ValidationFailure("Password", e)));
        }
    }
}
