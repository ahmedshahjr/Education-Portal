using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Modules.Commands.UpdateModule;

/// <summary>
/// Sets a module's type (default or custom) and whether it is active. An inactive module is off for every school.
/// Schools with their own choice keep it while the module is active. Master tenant administrators only (SEC-AUTHZ-07);
/// audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record UpdateModuleCommand : IRequest
{
    public string Key { get; init; } = string.Empty;

    public ModuleType Type { get; init; }

    public bool IsActive { get; init; }
}

public class UpdateModuleCommandValidator : AbstractValidator<UpdateModuleCommand>
{
    public UpdateModuleCommandValidator()
    {
        RuleFor(c => c.Key).NotEmpty().MaximumLength(PortalModule.KeyMaxLength);
        RuleFor(c => c.Type).IsInEnum();
    }
}

public class UpdateModuleCommandHandler : IRequestHandler<UpdateModuleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public UpdateModuleCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _context.Modules.FirstOrDefaultAsync(m => m.Key == request.Key, cancellationToken);
        Guard.Against.NotFound(request.Key, module);

        module.SetType(request.Type);

        if (request.IsActive)
        {
            module.Activate();
        }
        else
        {
            module.Deactivate();
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.ModuleChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(PortalModule),
            targetId: module.Key);
    }
}
