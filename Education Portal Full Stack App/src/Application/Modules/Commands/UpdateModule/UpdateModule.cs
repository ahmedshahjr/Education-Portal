using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Modules.Commands.UpdateModule;

/// <summary>
/// Changes a module: name, description, core or paid, sort order and whether it is active. An inactive module is off
/// for every school. A core module is always on and requires only core modules. The code never changes. Master tenant
/// administrators only (SEC-AUTHZ-07); audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record UpdateModuleCommand : ModuleFields, IRequest
{
    /// <summary>The module's code, taken from the route.</summary>
    public string Code { get; init; } = string.Empty;

    public bool IsActive { get; init; }
}

public class UpdateModuleCommandValidator : ModuleFieldsValidator<UpdateModuleCommand>
{
    public UpdateModuleCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.Code).NotEmpty().MaximumLength(PortalModule.CodeMaxLength);

        RuleFor(c => c.IsActive)
            .Must((command, isActive) => isActive || !command.IsCore)
            .WithMessage(_ => ValidationMessages.CoreModuleAlwaysOn);

        // A core module is in every subscription, so everything it requires must be too.
        RuleFor(c => c.IsCore)
            .MustAsync(async (command, isCore, cancellationToken) =>
                !isCore || !await RequiresPaidModuleAsync(context, command.Code, cancellationToken))
            .WithMessage(_ => ValidationMessages.CoreModuleRequiresPaid)
            .MustAsync(async (command, isCore, cancellationToken) =>
                isCore || !await IsRequiredByCoreModuleAsync(context, command.Code, cancellationToken))
            .WithMessage(_ => ValidationMessages.CoreModuleRequiresCoreOnly);
    }

    private static Task<bool> RequiresPaidModuleAsync(IApplicationDbContext context, string code, CancellationToken cancellationToken) =>
        (from d in context.ModuleDependencies
         join m in context.Modules on d.ModuleId equals m.Id
         join r in context.Modules on d.RequiresModuleId equals r.Id
         where m.Code == code && !r.IsCore
         select d.Id).AnyAsync(cancellationToken);

    private static Task<bool> IsRequiredByCoreModuleAsync(IApplicationDbContext context, string code, CancellationToken cancellationToken) =>
        (from d in context.ModuleDependencies
         join m in context.Modules on d.ModuleId equals m.Id
         join r in context.Modules on d.RequiresModuleId equals r.Id
         where r.Code == code && m.IsCore
         select d.Id).AnyAsync(cancellationToken);
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
        var module = await _context.Modules.FirstOrDefaultAsync(m => m.Code == request.Code, cancellationToken);
        Guard.Against.NotFound(request.Code, module);

        // Order matters: a module is activated before it can become core, and stops being core before it can be made inactive.
        if (request.IsActive)
        {
            module.Activate();
            module.Update(request.Name, request.Description, request.IsCore, request.SortOrder);
        }
        else
        {
            module.Update(request.Name, request.Description, request.IsCore, request.SortOrder);
            module.Deactivate();
        }

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.ModuleChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(PortalModule),
            targetId: module.Code);
    }
}
