using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.Modules.Common;

/// <summary>
/// The modules a school may use, in catalogue order: every module for the master tenant, otherwise the modules switched on
/// for the school (ADR-012). Shared by the enabled-modules query and by sign-in, which returns the list with the session.
/// </summary>
public static class EnabledModules
{
    public static async Task<IReadOnlyList<ModuleCode>> ForTenantAsync(
        IApplicationDbContext context,
        IModuleAccess moduleAccess,
        int schoolId,
        bool isMaster,
        CancellationToken cancellationToken)
    {
        if (isMaster)
        {
            // The master tenant oversees everything, so every module is on for it.
            return await context.Modules
                .AsNoTracking()
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.Code)
                .Select(m => m.Code)
                .ToListAsync(cancellationToken);
        }

        return await moduleAccess.GetEnabledCodesAsync(schoolId, cancellationToken);
    }
}
