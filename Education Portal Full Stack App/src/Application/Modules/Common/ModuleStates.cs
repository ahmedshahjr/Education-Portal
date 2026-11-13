using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.Modules.Common;

/// <summary>One module with the master's switch for a school, if there is one.</summary>
public sealed record ModuleState(int Id, string Code, string Name, string Description, bool IsCore, bool IsActive, int SortOrder, bool? SchoolChoice);

internal static class ModuleStates
{
    /// <summary>
    /// Every module in catalogue order with the school's switch. The tenant query filter still applies to
    /// <c>SchoolModules</c>, so a school can only ever read its own switches (SEC-AUTHZ-03).
    /// </summary>
    public static IQueryable<ModuleState> ForSchool(IApplicationDbContext context, int schoolId) =>
        context.Modules
            .AsNoTracking()
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Code)
            .Select(m => new ModuleState(
                m.Id,
                m.Code,
                m.Name,
                m.Description,
                m.IsCore,
                m.IsActive,
                m.SortOrder,
                context.SchoolModules
                    .Where(c => c.SchoolId == schoolId && c.ModuleId == m.Id)
                    .Select(c => (bool?)c.IsEnabled)
                    .FirstOrDefault()));
}
