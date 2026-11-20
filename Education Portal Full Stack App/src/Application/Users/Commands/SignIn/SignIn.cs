using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Users.Commands.SignIn;

/// <summary>
/// Signs a user in with email and password. This is the one request that runs before sign-in
/// (SEC-AUTHN-01); its endpoint is rate limited and antiforgery protected, and every outcome is audited.
/// Only accounts of the school that owns the request's domain can sign in there (AGENTS.md section 2). On success the
/// answer carries the school's enabled modules, so the app can show its menu without another request.
/// </summary>
public record SignInCommand : IRequest<SignInOutcome>
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

/// <summary>
/// The answer to a sign-in: whether it succeeded and, on success, the modules the school may use (in catalogue order).
/// </summary>
public sealed record SignInOutcome(bool SignedIn, IReadOnlyList<ModuleCode> Modules)
{
    public static SignInOutcome Failed { get; } = new(false, []);
}

/// <summary>What the sign-in endpoint returns on success: the modules the school may use, in catalogue order.</summary>
public sealed record SignInResponse(IReadOnlyList<ModuleCode> Modules);

public class SignInCommandHandler : IRequestHandler<SignInCommand, SignInOutcome>
{
    private readonly IIdentityService _identityService;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;
    private readonly IApplicationDbContext _context;
    private readonly IModuleAccess _moduleAccess;

    public SignInCommandHandler(
        IIdentityService identityService,
        ISecurityAuditLog auditLog,
        ICurrentTenant tenant,
        IApplicationDbContext context,
        IModuleAccess moduleAccess)
    {
        _identityService = identityService;
        _auditLog = auditLog;
        _tenant = tenant;
        _context = context;
        _moduleAccess = moduleAccess;
    }

    public async Task<SignInOutcome> Handle(SignInCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new UnauthorizedAccessException();

        var result = await _identityService.PasswordSignInAsync(schoolId, _tenant.IsMaster, request.Email, request.Password);
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

        if (!succeeded)
        {
            return SignInOutcome.Failed;
        }

        var modules = await EnabledModules.ForTenantAsync(_context, _moduleAccess, schoolId, _tenant.IsMaster, cancellationToken);
        return new SignInOutcome(true, modules);
    }
}
