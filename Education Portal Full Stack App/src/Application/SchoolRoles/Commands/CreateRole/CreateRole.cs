using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolRoles.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.SchoolRoles.Commands.CreateRole;

/// <summary>
/// Adds a role to the current school and returns its id. The school comes from the host (SEC-AUTHZ-04). Rights can
/// only be given in modules the school has now; audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.UsersRoles)]
public record CreateRoleCommand : SchoolRoleFields, IRequest<Guid>;

public class CreateRoleCommandValidator : SchoolRoleFieldsValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator(IApplicationDbContext context, IModuleAccess moduleAccess, ICurrentTenant tenant)
        : base(moduleAccess, tenant)
    {
        RuleFor(r => r.Name)
            .MustAsync(async (name, cancellationToken) =>
            {
                var trimmed = name.Trim();
                return !await context.SchoolRoles.AnyAsync(r => r.Name == trimmed, cancellationToken);
            })
            .WithMessage(_ => ValidationMessages.RoleNameTaken);
    }
}

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IModuleAccess _moduleAccess;
    private readonly ICurrentTenant _tenant;
    private readonly ISecurityAuditLog _auditLog;

    public CreateRoleCommandHandler(IApplicationDbContext context, IModuleAccess moduleAccess, ICurrentTenant tenant, ISecurityAuditLog auditLog)
    {
        _context = context;
        _moduleAccess = moduleAccess;
        _tenant = tenant;
        _auditLog = auditLog;
    }

    public async Task<Guid> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var role = SchoolRole.Create(schoolId, request.Name);
        _context.SchoolRoles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        await SchoolRoleStore.SetPermissionsAsync(_context, _moduleAccess, role, request.Permissions, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.RoleCreated,
            SecurityAuditOutcome.Success,
            targetType: nameof(SchoolRole),
            targetId: role.Id.ToString(CultureInfo.InvariantCulture));

        return role.PublicId;
    }
}
