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
/// The rules every role's fields follow (SEC-INPUT-01): a name, each module once, rights need view, and only modules the
/// school has now (a module that is off cannot get rights).
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
    /// are left as they are, so they come back with the module.
    /// </summary>
    public static async Task SetPermissionsAsync(
        IApplicationDbContext context, IModuleAccess moduleAccess, SchoolRole role, IReadOnlyList<RolePermissionDto> wanted, CancellationToken cancellationToken)
    {
        var enabled = (await moduleAccess.GetEnabledCodesAsync(role.SchoolId, cancellationToken)).ToHashSet();
        var modules = await context.Modules.Where(m => enabled.Contains(m.Code)).ToListAsync(cancellationToken);
        var current = await context.RolePermissions.Where(p => p.SchoolRoleId == role.Id).ToListAsync(cancellationToken);

        // The role screen gives a role's rights on every campus of the school (ADR-022). Rows made here are on the main
        // campus with AllCampuses set; rights on single campuses come with the per-campus role editor.
        var mainCampus = await SchoolCampuses.MainAsync(context, role.SchoolId, cancellationToken);

        foreach (var module in modules)
        {
            // A module can hold rows for several campuses (ADR-022). The screen writes one all-campuses row, so every
            // row of the module is looked at: the row on the main campus is kept (and made all-campuses), the rest go.
            var rows = current.Where(c => c.ModuleId == module.Id).ToList();
            var given = wanted.FirstOrDefault(w => w.Module == module.Code);

            if (given is null || !given.CanView)
            {
                context.RolePermissions.RemoveRange(rows);
                continue;
            }

            var keep = rows.FirstOrDefault(c => c.CampusId == mainCampus.Id);
            context.RolePermissions.RemoveRange(rows.Where(c => c != keep));

            if (keep is not null)
            {
                keep.ApplyToAllCampuses();
                keep.Set(given.CanView, given.CanCreate, given.CanEdit, given.CanDelete, given.CanApprove, given.CanExport, given.CanViewHistory);
            }
            else
            {
                context.RolePermissions.Add(RolePermission.Create(
                    role, module, mainCampus, allCampuses: true,
                    given.CanView, given.CanCreate, given.CanEdit, given.CanDelete, given.CanApprove, given.CanExport, given.CanViewHistory));
            }
        }
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
        var rights = (await (
                from p in context.RolePermissions.AsNoTracking()
                join m in context.Modules on p.ModuleId equals m.Id
                where p.SchoolId == schoolId && roleIds.Contains(p.SchoolRoleId)
                orderby m.SortOrder, m.Code
                select new { p.SchoolRoleId, m.Code, p.CanView, p.CanCreate, p.CanEdit, p.CanDelete, p.CanApprove, p.CanExport, p.CanViewHistory })
            .ToListAsync(cancellationToken))
            .Where(p => enabled.Contains(p.Code))
            .ToLookup(p => p.SchoolRoleId, p => new RolePermissionDto(p.Code, p.CanView, p.CanCreate, p.CanEdit, p.CanDelete, p.CanApprove, p.CanExport, p.CanViewHistory));

        return roles.Select(r => new SchoolRoleDto(r.PublicId, r.Name, r.Description, r.IsSystem, rights[r.Id].ToList())).ToList();
    }
}
