using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolRoles.Common;

/// <inheritdoc cref="IRoleRights"/>
public sealed class RoleRights : IRoleRights
{
    private readonly IApplicationDbContext _context;

    public RoleRights(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasRightAsync(string userId, int schoolId, ModuleCode moduleCode, AccessRight right, CancellationToken cancellationToken)
    {
        if (right == AccessRight.None)
        {
            throw new ArgumentOutOfRangeException(nameof(right), right, "A right must be named to be checked.");
        }

        var moduleId = await _context.Modules
            .Where(m => m.Code == moduleCode)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (moduleId == 0)
        {
            return false;
        }

        var roleIds = _context.AccountRoles
            .Where(a => a.SchoolId == schoolId && a.UserId == userId)
            .Select(a => a.SchoolRoleId);

        var permissions = _context.RolePermissions
            .Where(p => p.SchoolId == schoolId && p.ModuleId == moduleId && roleIds.Contains(p.SchoolRoleId));

        return right switch
        {
            AccessRight.View => await permissions.AnyAsync(p => p.CanView, cancellationToken),
            AccessRight.Create => await permissions.AnyAsync(p => p.CanCreate, cancellationToken),
            AccessRight.Edit => await permissions.AnyAsync(p => p.CanEdit, cancellationToken),
            AccessRight.Delete => await permissions.AnyAsync(p => p.CanDelete, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(right), right, "Unknown right."),
        };
    }
}
