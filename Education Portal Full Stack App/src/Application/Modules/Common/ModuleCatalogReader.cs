using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.Modules.Common;

/// <summary>Reads the module catalogue and its dependencies, shared by the catalogue queries and validators.</summary>
internal static class ModuleCatalogReader
{
    /// <summary>
    /// Every dependency between two modules that are not deleted, as id pairs. The soft-delete filter on the modules
    /// drops a pair whose module was deleted (ENG-EF-02: no-tracking projection, one query).
    /// </summary>
    public static async Task<List<(int ModuleId, int RequiresModuleId)>> LoadDependenciesAsync(IApplicationDbContext context, CancellationToken cancellationToken)
    {
        var pairs = await (
                from d in context.ModuleDependencies.AsNoTracking()
                join m in context.Modules on d.ModuleId equals m.Id
                join r in context.Modules on d.RequiresModuleId equals r.Id
                select new { d.ModuleId, d.RequiresModuleId })
            .ToListAsync(cancellationToken);

        return pairs.Select(p => (p.ModuleId, p.RequiresModuleId)).ToList();
    }

    /// <summary>The modules in catalogue order (sort order, then code), optionally only the one with the code, with what each requires.</summary>
    public static async Task<List<ModuleDto>> LoadAsync(IApplicationDbContext context, string? code, CancellationToken cancellationToken)
    {
        var modules = await context.Modules
            .AsNoTracking()
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Code)
            .Select(m => new { m.Id, m.Code, m.Name, m.Description, m.IsCore, m.IsActive, m.SortOrder })
            .ToListAsync(cancellationToken);

        var dependencies = await LoadDependenciesAsync(context, cancellationToken);
        var order = modules.Select((m, index) => (m.Id, index)).ToDictionary(x => x.Id, x => x.index);
        var codes = modules.ToDictionary(m => m.Id, m => m.Code);
        var requires = dependencies.ToLookup(d => d.ModuleId, d => d.RequiresModuleId);

        return modules
            .Where(m => code is null || m.Code == code)
            .Select(m => new ModuleDto(
                m.Code,
                m.Name,
                m.Description,
                m.IsCore,
                m.IsActive,
                m.SortOrder,
                requires[m.Id].OrderBy(id => order[id]).Select(id => codes[id]).ToList()))
            .ToList();
    }
}
