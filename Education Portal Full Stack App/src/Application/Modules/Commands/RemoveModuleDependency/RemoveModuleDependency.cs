using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Modules.Commands.RemoveModuleDependency;

/// <summary>
/// Removes a dependency between two modules. Answers 404 when the module does not require the other one. Master tenant
/// administrators only (SEC-AUTHZ-07); audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record RemoveModuleDependencyCommand(string Code, string RequiresCode) : IRequest;

public class RemoveModuleDependencyCommandValidator : AbstractValidator<RemoveModuleDependencyCommand>
{
    public RemoveModuleDependencyCommandValidator()
    {
        RuleFor(c => c.Code).NotEmpty().MaximumLength(PortalModule.CodeMaxLength);
        RuleFor(c => c.RequiresCode).NotEmpty().MaximumLength(PortalModule.CodeMaxLength);
    }
}

public class RemoveModuleDependencyCommandHandler : IRequestHandler<RemoveModuleDependencyCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public RemoveModuleDependencyCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(RemoveModuleDependencyCommand request, CancellationToken cancellationToken)
    {
        var dependency = await (
                from d in _context.ModuleDependencies
                join m in _context.Modules on d.ModuleId equals m.Id
                join r in _context.Modules on d.RequiresModuleId equals r.Id
                where m.Code == request.Code && r.Code == request.RequiresCode
                select d)
            .FirstOrDefaultAsync(cancellationToken);
        Guard.Against.NotFound($"{request.Code}/{request.RequiresCode}", dependency);

        _context.ModuleDependencies.Remove(dependency);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.ModuleDependencyRemoved,
            SecurityAuditOutcome.Success,
            targetType: nameof(PortalModule),
            targetId: $"{request.Code}/{request.RequiresCode}");
    }
}
