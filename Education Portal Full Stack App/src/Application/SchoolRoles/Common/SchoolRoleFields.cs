using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Subscriptions.Common;

namespace EducationPortal.Application.SchoolRoles.Common;

/// <summary>The fields a school administrator sets when adding or changing a role.</summary>
public abstract record SchoolRoleFields
{
    public string Name { get; init; } = string.Empty;

    /// <summary>What the role is for; optional, at most 500 characters.</summary>
    public string? Description { get; init; }

    /// <summary>The rights per module. A module that is not listed (or has no view right) gives the role nothing.</summary>
    public IReadOnlyList<RolePermissionDto> Permissions { get; init; } = [];
}

/// <summary>
/// The rules every role's fields follow (SEC-INPUT-01): a name, each module once, rights need view, a module with rights
/// names its campuses or all of them (ADR-022), and only modules the school has now (a module that is off cannot get rights).
/// </summary>
public abstract class SchoolRoleFieldsValidator<T> : AbstractValidator<T>
    where T : SchoolRoleFields
{
    protected SchoolRoleFieldsValidator(IModuleAccess moduleAccess, ICurrentTenant tenant)
    {
        RuleFor(r => r.Name).NotEmpty().MaximumLength(SchoolRole.NameMaxLength);
        RuleFor(r => r.Description).MaximumLength(SchoolRole.DescriptionMaxLength);

        RuleFor(r => r.Permissions)
            .NotNull()
            .Must(p => p.Count <= Enum.GetValues<ModuleCode>().Length)
            .Must(p => p.Select(x => x.Module).Distinct().Count() == p.Count)
            .WithMessage(_ => ValidationMessages.ModuleListedTwice)
            .Must(p => p.All(x => x.CanView || !(x.CanCreate || x.CanEdit || x.CanDelete || x.CanApprove || x.CanExport || x.CanViewHistory)))
            .WithMessage(_ => ValidationMessages.RightsNeedView)
            .Must(p => p.All(x => x.AllCampuses || !x.CanView || x.CampusIds is { Count: > 0 }))
            .WithMessage(_ => ValidationMessages.RoleNeedsCampus)
            .CustomAsync(async (permissions, validation, cancellationToken) =>
            {
                if (permissions is null || permissions.Count == 0 || tenant.SchoolId is not { } schoolId)
                {
                    return;
                }

                var enabled = await moduleAccess.GetEnabledCodesAsync(schoolId, cancellationToken);
                if (permissions.Any(p => !enabled.Contains(p.Module)))
                {
                    validation.AddFailure(ValidationMessages.ModuleNotEnabledForSchool);
                }
            });
    }
}

internal static class SchoolRoleStore
{
    /// <summary>
    /// Makes the role's rights exactly the given ones for the modules the school has now. Rights in modules that are off
    /// are left as they are, so they come back with the module. Each module's rows are made to match the campuses it names
    /// (ADR-022): all campuses is one row on the main campus; named campuses get one row each.
    /// </summary>
    public static async Task SetPermissionsAsync(
        IApplicationDbContext context, IModuleAccess moduleAccess, SchoolRole role, IReadOnlyList<RolePermissionDto> wanted, CancellationToken cancellationToken)
    {
        var enabled = (await moduleAccess.GetEnabledCodesAsync(role.SchoolId, cancellationToken)).ToHashSet();
        var modules = await context.Modules.Where(m => enabled.Contains(m.Code)).ToListAsync(cancellationToken);
        var current = await context.RolePermissions.Where(p => p.SchoolRoleId == role.Id).ToListAsync(cancellationToken);
        var mainCampus = await SchoolCampuses.MainAsync(context, role.SchoolId, cancellationToken);

        foreach (var module in modules)
        {
            var rows = current.Where(c => c.ModuleId == module.Id).ToList();
            var given = wanted.FirstOrDefault(w => w.Module == module.Code);

            if (given is null || !given.CanView)
            {
                context.RolePermissions.RemoveRange(rows);
                continue;
            }

            var targets = await TargetCampusesAsync(context, role.SchoolId, mainCampus, given, cancellationToken);

            // Rows of campuses the module no longer names go; rows of the named campuses are updated; a named campus
            // without a row gets one.
            context.RolePermissions.RemoveRange(rows.Where(r => targets.All(t => t.Campus.Id != r.CampusId)));

            foreach (var (campus, allCampuses) in targets)
            {
                var row = rows.FirstOrDefault(r => r.CampusId == campus.Id);
                if (row is null)
                {
                    context.RolePermissions.Add(RolePermission.Create(
                        role, module, campus, allCampuses,
                        given.CanView, given.CanCreate, given.CanEdit, given.CanDelete, given.CanApprove, given.CanExport, given.CanViewHistory));
                    continue;
                }

                row.SetAllCampuses(allCampuses);
                row.Set(given.CanView, given.CanCreate, given.CanEdit, given.CanDelete, given.CanApprove, given.CanExport, given.CanViewHistory);
            }
        }
    }

    /// <summary>
    /// The campuses a module's rights are written for: the main campus with all-campuses set, or each campus the role
    /// names. A campus of another school is not found (SEC-AUTHZ-03).
    /// </summary>
    private static async Task<List<(Campus Campus, bool AllCampuses)>> TargetCampusesAsync(
        IApplicationDbContext context, int schoolId, Campus mainCampus, RolePermissionDto given, CancellationToken cancellationToken)
    {
        if (given.AllCampuses)
        {
            return [(mainCampus, true)];
        }

        var publicIds = (given.CampusIds ?? []).Distinct().ToList();
        var campuses = await context.Campuses
            .Where(c => c.SchoolId == schoolId && publicIds.Contains(c.PublicId))
            .ToListAsync(cancellationToken);

        foreach (var publicId in publicIds)
        {
            Guard.Against.NotFound(publicId, campuses.FirstOrDefault(c => c.PublicId == publicId));
        }

        return campuses.Select(c => (c, false)).ToList();
    }
}

internal static class SchoolRoleReader
{
    /// <summary>The school's roles by name, with rights only for modules the school has now, in catalogue order.</summary>
    public static async Task<List<SchoolRoleDto>> LoadAsync(
        IApplicationDbContext context, IModuleAccess moduleAccess, int schoolId, Guid? id, CancellationToken cancellationToken)
    {
        var roles = await context.SchoolRoles
            .AsNoTracking()
            .Where(r => r.SchoolId == schoolId && (id == null || r.PublicId == id))
            .OrderBy(r => r.Name)
            .Select(r => new { r.Id, r.PublicId, r.Name, r.Description, r.IsSystem })
            .ToListAsync(cancellationToken);

        var enabled = (await moduleAccess.GetEnabledCodesAsync(schoolId, cancellationToken)).ToHashSet();
        var roleIds = roles.Select(r => r.Id).ToList();

        // One row per (role, module, campus). The rows of a module are read together below.
        var rows = await (
                from p in context.RolePermissions.AsNoTracking()
                join m in context.Modules on p.ModuleId equals m.Id
                join c in context.Campuses.AsNoTracking() on p.CampusId equals c.Id
                where p.SchoolId == schoolId && roleIds.Contains(p.SchoolRoleId)
                orderby m.SortOrder, m.Code, c.Id
                select new { p.SchoolRoleId, m.Code, p.AllCampuses, CampusPublicId = c.PublicId, p.CanView, p.CanCreate, p.CanEdit, p.CanDelete, p.CanApprove, p.CanExport, p.CanViewHistory })
            .ToListAsync(cancellationToken);

        var rights = rows
            .Where(p => enabled.Contains(p.Code))
            .GroupBy(p => (SchoolRoleId: p.SchoolRoleId, Code: p.Code))
            .Select(group =>
            {
                // The rows of one module are written together with the same rights (ADR-022), so the first row gives them.
                var first = group.First();
                var allCampuses = group.Any(p => p.AllCampuses);
                // Null means "all campuses" (the same as a caller that names no campus), so the reader and the writer agree.
                IReadOnlyList<Guid>? campusIds = allCampuses ? null : group.Select(p => p.CampusPublicId).ToList();
                var permission = new RolePermissionDto(
                    group.Key.Code, first.CanView, first.CanCreate, first.CanEdit, first.CanDelete, first.CanApprove, first.CanExport, first.CanViewHistory,
                    allCampuses, campusIds);
                return (RoleId: group.Key.SchoolRoleId, Permission: permission);
            })
            .ToLookup(x => x.RoleId, x => x.Permission);

        return roles.Select(r => new SchoolRoleDto(r.PublicId, r.Name, r.Description, r.IsSystem, rights[r.Id].ToList())).ToList();
    }
}
