namespace EducationPortal.Domain.Entities;

/// <summary>
/// One module requires another, for example FEES requires STUDENTS: a school can only get FEES together with STUDENTS.
/// Platform reference data that only the master tenant changes, so it has no <c>SchoolId</c>. The rules that need every
/// dependency at once (no cycles, core modules require only core modules) live in <see cref="ModuleDependencyGraph"/>.
/// </summary>
public class ModuleDependency : BaseAuditableEntity
{
    private ModuleDependency()
    {
        // For EF Core.
    }

    private ModuleDependency(int moduleId, int requiresModuleId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(moduleId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(requiresModuleId);

        if (moduleId == requiresModuleId)
        {
            throw new ArgumentException("A module cannot require itself.", nameof(requiresModuleId));
        }

        ModuleId = moduleId;
        RequiresModuleId = requiresModuleId;
    }

    public static ModuleDependency Create(PortalModule module, PortalModule requires)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(requires);

        return new ModuleDependency(module.Id, requires.Id);
    }

    /// <summary>The module that needs the other one.</summary>
    public int ModuleId { get; private set; }

    /// <summary>The module it needs.</summary>
    public int RequiresModuleId { get; private set; }
}
