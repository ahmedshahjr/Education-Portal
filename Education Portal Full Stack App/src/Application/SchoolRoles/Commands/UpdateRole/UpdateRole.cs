using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolRoles.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.SchoolRoles.Commands.UpdateRole;

/// <summary>
/// Renames a role of the current school and sets its rights. A role of another school is not found (the tenant filter
/// hides it, SEC-AUTHZ-03). Rights in modules the school does not have now are kept untouched; audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.UsersRoles)]
public record UpdateRoleCommand(Guid Id) : SchoolRoleFields, IRequest;

public class UpdateRoleCommandValidator : SchoolRoleFieldsValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator(IApplicationDbContext context, IModuleAccess moduleAccess, ICurrentTenant tenant)
        : base(moduleAccess, tenant)
    {
        RuleFor(r => r)
            .MustAsync(async (command, cancellationToken) =>
            {
                var trimmed = command.Name.Trim();
                return !await context.SchoolRoles.AnyAsync(r => r.Name == trimmed && r.PublicId != command.Id, cancellationToken);
            })
            .When(r => !string.IsNullOrWhiteSpace(r.Name))
            .WithName(_ => PropertyNames.Name)
            .WithMessage(_ => ValidationMessages.RoleNameTaken);
    }
}

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IModuleAccess _moduleAccess;
    private readonly ISecurityAuditLog _auditLog;

    public UpdateRoleCommandHandler(IApplicationDbContext context, IModuleAccess moduleAccess, ISecurityAuditLog auditLog)
    {
        _context = context;
        _moduleAccess = moduleAccess;
        _auditLog = auditLog;
    }

    public async Task Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.SchoolRoles.FirstOrDefaultAsync(r => r.PublicId == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, role);

        role.Rename(request.Name);
        await SchoolRoleStore.SetPermissionsAsync(_context, _moduleAccess, role, request.Permissions, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.RoleChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolRole),
            targetId: role.Id.ToString(CultureInfo.InvariantCulture));
    }
}
