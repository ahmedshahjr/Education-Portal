using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.SchoolRoles.Commands.DeleteRole;

/// <summary>
/// Deletes a role of the current school together with its rights. A role of another school is not found (SEC-AUTHZ-03);
/// audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.UsersRoles)]
public record DeleteRoleCommand(Guid Id) : IRequest;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;

    public DeleteRoleCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog)
    {
        _context = context;
        _auditLog = auditLog;
    }

    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.SchoolRoles.FirstOrDefaultAsync(r => r.PublicId == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, role);

        var rights = await _context.RolePermissions.Where(p => p.SchoolRoleId == role.Id).ToListAsync(cancellationToken);
        _context.RolePermissions.RemoveRange(rights);
        _context.SchoolRoles.Remove(role);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.RoleDeleted,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolRole),
            targetId: role.Id.ToString(CultureInfo.InvariantCulture));
    }
}
