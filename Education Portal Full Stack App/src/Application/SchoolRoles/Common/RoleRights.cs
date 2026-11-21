using System.Linq.Expressions;
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
        var flag = Flag(right);

        var permissions = await PermissionsAsync(userId, schoolId, moduleCode, cancellationToken);

        return permissions is not null && await permissions.AnyAsync(flag, cancellationToken);
    }

    public async Task<IReadOnlyCollection<int>?> GetRightCampusIdsAsync(string userId, int schoolId, ModuleCode moduleCode, AccessRight right, CancellationToken cancellationToken)
    {
        var flag = Flag(right);

        var permissions = await PermissionsAsync(userId, schoolId, moduleCode, cancellationToken);
        if (permissions is null)
        {
            return [];
        }

        var given = permissions.Where(flag);
        if (await given.AnyAsync(p => p.AllCampuses, cancellationToken))
        {
            return null;
        }

        return await given.Select(p => p.CampusId).Distinct().ToListAsync(cancellationToken);
    }

    /// <summary>The permissions of the user's roles in the module, or <c>null</c> when the module is unknown.</summary>
    private async Task<IQueryable<RolePermission>?> PermissionsAsync(string userId, int schoolId, ModuleCode moduleCode, CancellationToken cancellationToken)
    {
        var moduleId = await _context.Modules
            .Where(m => m.Code == moduleCode)
            .Select(m => m.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (moduleId == 0)
        {
            return null;
        }

        var roleIds = _context.AccountRoles
            .Where(a => a.SchoolId == schoolId && a.UserId == userId)
            .Select(a => a.SchoolRoleId);

        return _context.RolePermissions
            .Where(p => p.SchoolId == schoolId && p.ModuleId == moduleId && roleIds.Contains(p.SchoolRoleId));
    }

    private static Expression<Func<RolePermission, bool>> Flag(AccessRight right) => right switch
    {
        AccessRight.View => p => p.CanView,
        AccessRight.Create => p => p.CanCreate,
        AccessRight.Edit => p => p.CanEdit,
        AccessRight.Delete => p => p.CanDelete,
        AccessRight.Approve => p => p.CanApprove,
        AccessRight.Export => p => p.CanExport,
        AccessRight.ViewHistory => p => p.CanViewHistory,
        AccessRight.None => throw new ArgumentOutOfRangeException(nameof(right), right, "A right must be named to be checked."),
        _ => throw new ArgumentOutOfRangeException(nameof(right), right, "Unknown right."),
    };
}
