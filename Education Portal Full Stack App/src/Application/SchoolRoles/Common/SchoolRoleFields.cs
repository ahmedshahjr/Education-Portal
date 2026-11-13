using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.SchoolRoles.Common;

/// <summary>The fields a school administrator sets when adding or changing a role.</summary>
public abstract record SchoolRoleFields
{
    public string Name { get; init; } = string.Empty;

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

        RuleFor(r => r.Permissions)
            .NotNull()
            .Must(p => p.Count <= PortalModule.CodeMaxLength * 10)
            .Must(p => p.Select(x => x.Module).Distinct(StringComparer.Ordinal).Count() == p.Count)
            .WithMessage(_ => ValidationMessages.ModuleListedTwice)
            .Must(p => p.All(x => x.CanView || !(x.CanCreate || x.CanEdit || x.CanDelete)))
            .WithMessage(_ => ValidationMessages.RightsNeedView)
            .CustomAsync(async (permissions, validation, cancellationToken) =>
            {
                if (permissions is null || permissions.Count == 0 || tenant.SchoolId is not { } schoolId)
                {
                    return;
                }

                var enabled = await moduleAccess.GetEnabledCodesAsync(schoolId, cancellationToken);
                if (permissions.Any(p => !enabled.Contains(p.Module, StringComparer.Ordinal)))
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
        var enabled = (await moduleAccess.GetEnabledCodesAsync(role.SchoolId, cancellationToken)).ToHashSet(StringComparer.Ordinal);
        var modules = await context.Modules.Where(m => enabled.Contains(m.Code)).ToListAsync(cancellationToken);
        var current = await context.RolePermissions.Where(p => p.SchoolRoleId == role.Id).ToListAsync(cancellationToken);

        foreach (var module in modules)
        {
            var existing = current.FirstOrDefault(c => c.ModuleId == module.Id);
            var given = wanted.FirstOrDefault(w => w.Module == module.Code);

            if (given is null || !given.CanView)
            {
                if (existing is not null)
                {
                    context.RolePermissions.Remove(existing);
                }
            }
            else if (existing is not null)
            {
                existing.Set(given.CanView, given.CanCreate, given.CanEdit, given.CanDelete);
            }
            else
            {
                context.RolePermissions.Add(RolePermission.Create(role, module, given.CanView, given.CanCreate, given.CanEdit, given.CanDelete));
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
            .Select(r => new { r.Id, r.PublicId, r.Name })
            .ToListAsync(cancellationToken);

        var enabled = (await moduleAccess.GetEnabledCodesAsync(schoolId, cancellationToken)).ToHashSet(StringComparer.Ordinal);
        var roleIds = roles.Select(r => r.Id).ToList();
        var rights = (await (
                from p in context.RolePermissions.AsNoTracking()
                join m in context.Modules on p.ModuleId equals m.Id
                where p.SchoolId == schoolId && roleIds.Contains(p.SchoolRoleId)
                orderby m.SortOrder, m.Code
                select new { p.SchoolRoleId, m.Code, p.CanView, p.CanCreate, p.CanEdit, p.CanDelete })
            .ToListAsync(cancellationToken))
            .Where(p => enabled.Contains(p.Code))
            .ToLookup(p => p.SchoolRoleId, p => new RolePermissionDto(p.Code, p.CanView, p.CanCreate, p.CanEdit, p.CanDelete));

        return roles.Select(r => new SchoolRoleDto(r.PublicId, r.Name, rights[r.Id].ToList())).ToList();
    }
}
