using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.Modules.Common;

/// <summary>One module with the school's own choice for it, if it has one.</summary>
public sealed record ModuleState(int Id, string Key, string Name, string Description, ModuleType Type, bool IsActive, bool? SchoolChoice);

internal static class ModuleStates
{
    /// <summary>
    /// Every module with the school's choice. The tenant query filter still applies to <c>SchoolModules</c>, so a school
    /// can only ever read its own choices (SEC-AUTHZ-03).
    /// </summary>
    public static IQueryable<ModuleState> ForSchool(IApplicationDbContext context, int schoolId) =>
        context.Modules
            .AsNoTracking()
            .OrderBy(m => m.Id)
            .Select(m => new ModuleState(
                m.Id,
                m.Key,
                m.Name,
                m.Description,
                m.Type,
                m.IsActive,
                context.SchoolModules
                    .Where(c => c.SchoolId == schoolId && c.ModuleId == m.Id)
                    .Select(c => (bool?)c.IsEnabled)
                    .FirstOrDefault()));
}
