namespace EducationPortal.Domain.Entities;

/// <summary>
/// The dependencies between modules as one graph, for the rules that need all of them at once. Pure functions over
/// (module, required module) pairs, so they work on ids loaded in one query and are easy to test.
/// </summary>
public static class ModuleDependencyGraph
{
    /// <summary>
    /// True when adding "<paramref name="moduleId"/> requires <paramref name="requiresModuleId"/>" would close a cycle,
    /// that is when the required module already needs the module, directly or through others (or it is the module
    /// itself). A cycle would make two modules impossible to sell apart and could loop dependency resolution.
    /// </summary>
    public static bool WouldCreateCycle(IEnumerable<(int ModuleId, int RequiresModuleId)> dependencies, int moduleId, int requiresModuleId)
    {
        ArgumentNullException.ThrowIfNull(dependencies);

        if (moduleId == requiresModuleId)
        {
            return true;
        }

        return RequiredBy(dependencies, [requiresModuleId]).Contains(moduleId);
    }

    /// <summary>
    /// The given modules plus every module they require, directly or through others. Safe on a graph with a cycle
    /// (each module is visited once), although the catalogue never stores one.
    /// </summary>
    public static IReadOnlySet<int> RequiredBy(IEnumerable<(int ModuleId, int RequiresModuleId)> dependencies, IEnumerable<int> moduleIds)
    {
        ArgumentNullException.ThrowIfNull(dependencies);
        ArgumentNullException.ThrowIfNull(moduleIds);

        var requires = dependencies.ToLookup(d => d.ModuleId, d => d.RequiresModuleId);
        var found = new HashSet<int>();
        var pending = new Stack<int>(moduleIds);

        while (pending.Count > 0)
        {
            var current = pending.Pop();

            if (!found.Add(current))
            {
                continue;
            }

            foreach (var next in requires[current])
            {
                pending.Push(next);
            }
        }

        return found;
    }
}
