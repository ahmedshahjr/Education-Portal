using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Modules.Commands.DeleteModule;

/// <summary>
/// Deletes a paid module from the catalogue. It is a soft delete: the row and the schools' rows that point to it stay
/// for audit (they point to it by id), the module disappears from every list, and its code can be used again. What the module required is removed with
/// it. A core module, or one that other modules require, cannot be deleted. Master tenant administrators only
/// (SEC-AUTHZ-07); audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record DeleteModuleCommand(string Code) : IRequest;

public class DeleteModuleCommandValidator : AbstractValidator<DeleteModuleCommand>
{
    public DeleteModuleCommandValidator(IApplicationDbContext context)
    {
        RuleFor(c => c.Code)
            .NotEmpty()
            .MaximumLength(PortalModule.CodeMaxLength)
            .MustAsync(async (code, cancellationToken) =>
                !await context.Modules.AnyAsync(m => m.Code == code && m.IsCore, cancellationToken))
            .WithMessage(_ => ValidationMessages.CoreModuleAlwaysOn)
            .MustAsync(async (code, cancellationToken) =>
                !await (from d in context.ModuleDependencies
                        join m in context.Modules on d.ModuleId equals m.Id
                        join r in context.Modules on d.RequiresModuleId equals r.Id
                        where r.Code == code
                        select d.Id).AnyAsync(cancellationToken))
            .WithMessage(_ => ValidationMessages.ModuleRequiredByOthers);
    }
}

public class DeleteModuleCommandHandler : IRequestHandler<DeleteModuleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public DeleteModuleCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _auditLog = auditLog;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
    {
        var module = await _context.Modules.FirstOrDefaultAsync(m => m.Code == request.Code, cancellationToken);
        Guard.Against.NotFound(request.Code, module);

        var requires = await _context.ModuleDependencies.Where(d => d.ModuleId == module.Id).ToListAsync(cancellationToken);
        _context.ModuleDependencies.RemoveRange(requires);

        module.Delete(_timeProvider.GetUtcNow(), _user.Id);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.ModuleDeleted,
            SecurityAuditOutcome.Success,
            targetType: nameof(PortalModule),
            targetId: module.Code);
    }
}
