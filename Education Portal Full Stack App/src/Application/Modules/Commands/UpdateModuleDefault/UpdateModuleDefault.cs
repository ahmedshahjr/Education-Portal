using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Modules.Commands.UpdateModuleDefault;

/// <summary>
/// Makes a module default (on for every school unless the master switched it off for that school) or custom (off unless
/// switched on). Schools with their own choice keep it. Core modules are always default. Master tenant administrators
/// only (SEC-AUTHZ-07); audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record UpdateModuleDefaultCommand : IRequest
{
    public string Key { get; init; } = string.Empty;

    public bool IsDefault { get; init; }
}

public class UpdateModuleDefaultCommandValidator : AbstractValidator<UpdateModuleDefaultCommand>
{
    public UpdateModuleDefaultCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.Key).NotEmpty().MaximumLength(PortalModule.KeyMaxLength);

        RuleFor(c => c)
            .MustAsync(async (command, cancellationToken) =>
                command.IsDefault || !await context.Modules.AnyAsync(m => m.Key == command.Key && m.IsCore, cancellationToken))
            .WithName(nameof(UpdateModuleDefaultCommand.IsDefault))
            .WithMessage("A core module is always on for every school.");
    }
}

public class UpdateModuleDefaultCommandHandler : IRequestHandler<UpdateModuleDefaultCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public UpdateModuleDefaultCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(UpdateModuleDefaultCommand request, CancellationToken cancellationToken)
    {
        var module = await _context.Modules.FirstOrDefaultAsync(m => m.Key == request.Key, cancellationToken);
        Guard.Against.NotFound(request.Key, module);

        module.SetDefault(request.IsDefault);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.ModuleDefaultChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(PortalModule),
            targetId: module.Key);
    }
}
