using EducationPortal.Application.Campuses.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Users.Queries.GetMyRights;

/// <summary>
/// The signed-in user's own rights: for every module the school has, the rights their roles give and the campuses they
/// hold on (ADR-022, R1.5). Only for the menu and route guards; the server still checks the module, the right and the
/// campus on every request (SEC-AUTHZ-01, SEC-AUTHZ-02). The school comes from the host, never from the client.
/// </summary>
[Authorize]
public record GetMyRightsQuery;

/// <param name="IsParent">True when the signed-in user's person is a parent: only parents see "My children" (R5.3).</param>
public record MyRightsDto(IReadOnlyList<ModuleRightsDto> Modules, bool IsParent = false);

public record ModuleRightsDto(ModuleCode Module, IReadOnlyList<RightGrantDto> Rights);

/// <summary>A right and where it holds: every campus of the school, or the listed campuses (public ids).</summary>
public record RightGrantDto(AccessRight Right, bool AllCampuses, IReadOnlyList<Guid> CampusIds);

public class GetMyRightsQueryHandler
{
    private static readonly AccessRight[] AllRights = Enum.GetValues<AccessRight>().Where(r => r != AccessRight.None).ToArray();

    private readonly IApplicationDbContext _context;
    private readonly IUser _user;
    private readonly ICurrentTenant _tenant;
    private readonly IModuleAccess _moduleAccess;
    private readonly ICampusAccess _campusAccess;
    private readonly IIdentityService _identityService;

    public GetMyRightsQueryHandler(
        IApplicationDbContext context,
        IUser user,
        ICurrentTenant tenant,
        IModuleAccess moduleAccess,
        ICampusAccess campusAccess,
        IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _tenant = tenant;
        _moduleAccess = moduleAccess;
        _campusAccess = campusAccess;
        _identityService = identityService;
    }

    public async Task<MyRightsDto> Handle(GetMyRightsQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id ?? throw new UnauthorizedAccessException();
        var schoolId = _tenant.SchoolId;

        var enabled = await EnabledModules.ForTenantAsync(_context, _moduleAccess, schoolId, _tenant.IsMaster, cancellationToken);
        var isParent = await IsParentAsync(userId, cancellationToken);

        // Administrators (the school's owners) and the master tenant hold every right on every campus (ADR-018).
        if (_tenant.IsMaster || (_user.Roles?.Contains(Roles.Administrator) ?? false))
        {
            var everything = AllRights.Select(r => new RightGrantDto(r, AllCampuses: true, [])).ToList();

            return new MyRightsDto(enabled.Select(m => new ModuleRightsDto(m, everything)).ToList(), isParent);
        }

        var assigned = await _campusAccess.GetAllowedCampusIdsAsync(cancellationToken);

        var roleIds = _context.AccountRoles
            .Where(a => a.SchoolId == schoolId && a.UserId == userId)
            .Select(a => a.SchoolRoleId);

        var rows = await (
                from p in _context.RolePermissions.AsNoTracking()
                join m in _context.Modules on p.ModuleId equals m.Id
                where p.SchoolId == schoolId && roleIds.Contains(p.SchoolRoleId)
                select new
                {
                    m.Code, p.CampusId, p.AllCampuses, p.CanView, p.CanCreate, p.CanEdit, p.CanDelete, p.CanApprove, p.CanExport, p.CanViewHistory,
                })
            .ToListAsync(cancellationToken);

        var publicIds = await _context.Campuses.AsNoTracking()
            .Where(c => c.SchoolId == schoolId)
            .Select(c => new { c.Id, c.PublicId })
            .ToDictionaryAsync(c => c.Id, c => c.PublicId, cancellationToken);

        var modules = new List<ModuleRightsDto>();
        foreach (var module in enabled.Distinct())
        {
            var moduleRows = rows.Where(r => r.Code == module).ToList();
            var grants = new List<RightGrantDto>();

            foreach (var right in AllRights)
            {
                var given = moduleRows.Where(r => Has(r.CanView, r.CanCreate, r.CanEdit, r.CanDelete, r.CanApprove, r.CanExport, r.CanViewHistory, right)).ToList();
                if (given.Count == 0)
                {
                    continue;
                }

                // Where the roles give the right (null = every campus), cut down to the campuses the user is assigned to.
                IReadOnlyCollection<int>? byRole = given.Any(r => r.AllCampuses) ? null : given.Select(r => r.CampusId).Distinct().ToList();
                var where = CampusSets.Intersect(assigned, byRole);

                if (where is null)
                {
                    grants.Add(new RightGrantDto(right, AllCampuses: true, []));
                }
                else
                {
                    // Campuses that are deleted are hidden by the query filter; a grant with none left is not shown.
                    var live = where.Where(publicIds.ContainsKey).Select(id => publicIds[id]).ToList();
                    if (live.Count > 0)
                    {
                        grants.Add(new RightGrantDto(right, AllCampuses: false, live));
                    }
                }
            }

            if (grants.Count > 0)
            {
                modules.Add(new ModuleRightsDto(module, grants));
            }
        }

        return new MyRightsDto(modules, isParent);
    }

    /// <summary>True when the account's person is a parent (UserType.Parent). An account with no person is not a parent.</summary>
    private async Task<bool> IsParentAsync(string userId, CancellationToken cancellationToken)
    {
        var personId = await _identityService.FindPersonIdOfAccountAsync(userId);
        if (personId is not { } id)
        {
            return false;
        }

        return await _context.UserDetails.AnyAsync(u => u.Id == id && u.UserType == UserType.Parent, cancellationToken);
    }

    private static bool Has(bool view, bool create, bool edit, bool delete, bool approve, bool export, bool history, AccessRight right) => right switch
    {
        AccessRight.View => view,
        AccessRight.Create => create,
        AccessRight.Edit => edit,
        AccessRight.Delete => delete,
        AccessRight.Approve => approve,
        AccessRight.Export => export,
        AccessRight.ViewHistory => history,
        _ => false,
    };
}
