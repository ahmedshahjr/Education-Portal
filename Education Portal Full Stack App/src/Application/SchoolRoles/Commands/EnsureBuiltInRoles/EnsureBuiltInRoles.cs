using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolRoles.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolRoles.Commands.EnsureBuiltInRoles;

/// <summary>
/// Adds the built-in roles (Principal, Teacher, Student, Parent) to the school, each one that is not there yet, with its
/// starting rights for the modules the school has. Running it again changes nothing. Needs the Users &amp; Roles module.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.UsersRoles)]
public record EnsureBuiltInRolesCommand : IRequest;

public class EnsureBuiltInRolesCommandHandler : IRequestHandler<EnsureBuiltInRolesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IModuleAccess _moduleAccess;
    private readonly ICurrentTenant _tenant;

    public EnsureBuiltInRolesCommandHandler(IApplicationDbContext context, IModuleAccess moduleAccess, ICurrentTenant tenant)
    {
        _context = context;
        _moduleAccess = moduleAccess;
        _tenant = tenant;
    }

    public async Task Handle(EnsureBuiltInRolesCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var existing = await _context.SchoolRoles
            .Where(r => r.SchoolId == schoolId)
            .Select(r => r.Name)
            .ToListAsync(cancellationToken);

        foreach (var (name, permissions) in BuiltInRoles.All.Where(b => !existing.Contains(b.Name)))
        {
            var role = SchoolRole.Create(schoolId, name);
            _context.SchoolRoles.Add(role);

            // The role needs its id before its rights are saved against it.
            await _context.SaveChangesAsync(cancellationToken);

            await SchoolRoleStore.SetPermissionsAsync(_context, _moduleAccess, role, permissions, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
