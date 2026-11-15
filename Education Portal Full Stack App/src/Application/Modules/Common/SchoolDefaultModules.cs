using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Modules.Common;

/// <summary>
/// Gives a school that registers every active module that is on by default (<see cref="PortalModule.IsDefaultEnabled"/>),
/// free of charge, for the whole school. A module the school already has a grant for is skipped, so running it again adds
/// nothing. The caller saves the changes, in the same save as the school (SEC-CFG-02: one transaction).
/// </summary>
public static class SchoolDefaultModules
{
    /// <summary>
    /// Adds the default grants of <paramref name="schoolId"/>. The school is named by the caller, which is trusted server
    /// code (the master tenant creating a school, or a seed), so the tenant filters are ignored for this one school.
    /// </summary>
    public static async Task GrantAsync(IApplicationDbContext context, int schoolId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var mainBranch = await context.Branches
            .IgnoreQueryFilters()
            .SingleAsync(b => b.SchoolId == schoolId && b.Kind == BranchKind.Main, cancellationToken);

        var defaults = await context.Modules
            .Where(m => m.IsActive && m.IsDefaultEnabled)
            .ToListAsync(cancellationToken);

        var alreadyGranted = await context.TenantModules
            .IgnoreQueryFilters()
            .Where(t => t.SchoolId == schoolId && t.BranchId == mainBranch.Id)
            .Select(t => t.ModuleId)
            .ToHashSetAsync(cancellationToken);

        foreach (var module in defaults.Where(m => !alreadyGranted.Contains(m.Id)))
        {
            context.TenantModules.Add(TenantModule.CreateDefault(schoolId, module, mainBranch, now));
        }
    }
}
