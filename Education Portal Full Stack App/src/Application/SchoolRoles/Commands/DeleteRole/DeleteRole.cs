using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Domain.Constants;
using FluentValidation.Results;

namespace EducationPortal.Application.SchoolRoles.Commands.DeleteRole;

/// <summary>
/// Deletes a role of the current school together with its rights. A role of another school is not found (SEC-AUTHZ-03);
/// a built-in role cannot be deleted; audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.UsersRoles)]
public record DeleteRoleCommand(Guid Id);

public class DeleteRoleCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly IUser _user;
    private readonly TimeProvider _timeProvider;

    public DeleteRoleCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, IUser user, TimeProvider timeProvider)
    {
        _context = context;
        _auditLog = auditLog;
        _user = user;
        _timeProvider = timeProvider;
    }

    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.SchoolRoles.FirstOrDefaultAsync(r => r.PublicId == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, role);

        if (role.IsSystem)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new ValidationFailure(nameof(request.Id), ValidationMessages.SystemRoleNotDeletable)]);
        }

        // Soft delete (R3.3): the role is hidden and kept for audit; its rights are removed so it gives nothing any more.
        var rights = await _context.RolePermissions.Where(p => p.SchoolRoleId == role.Id).ToListAsync(cancellationToken);
        _context.RolePermissions.RemoveRange(rights);
        role.Delete(_timeProvider.GetUtcNow(), _user.Id);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.RoleDeleted,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolRole),
            targetId: role.Id.ToString(CultureInfo.InvariantCulture));
    }
}
