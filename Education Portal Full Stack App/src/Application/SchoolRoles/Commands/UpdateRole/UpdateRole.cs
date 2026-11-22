using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolRoles.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.SchoolRoles.Commands.UpdateRole;

/// <summary>
/// Renames a role of the current school and sets its description and rights. A built-in role keeps its name. A role of another school is not found (the tenant filter
/// hides it, SEC-AUTHZ-03). Rights in modules the school does not have now are kept untouched; audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.UsersRoles)]
public record UpdateRoleCommand(Guid Id) : SchoolRoleFields;

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

        // A built-in role keeps its name (its rights and description can still change).
        RuleFor(r => r)
            .MustAsync(async (command, cancellationToken) =>
            {
                var trimmed = command.Name.Trim();
                return !await context.SchoolRoles.AnyAsync(r => r.PublicId == command.Id && r.IsSystem && r.Name != trimmed, cancellationToken);
            })
            .When(r => !string.IsNullOrWhiteSpace(r.Name))
            .WithName(_ => PropertyNames.Name)
            .WithMessage(_ => ValidationMessages.SystemRoleKeepsName);
    }
}

public class UpdateRoleCommandHandler
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
        role.Describe(request.Description);
        await SchoolRoleStore.SetPermissionsAsync(_context, _moduleAccess, role, request.Permissions, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.RoleChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolRole),
            targetId: role.Id.ToString(CultureInfo.InvariantCulture));
    }
}
