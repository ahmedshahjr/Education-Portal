namespace EducationPortal.Domain.Entities;

/// <summary>
/// The master tenant's choice to switch a module on or off for one school. It wins over the module's default, so a
/// school keeps it when the default changes later. Owned by the school (<c>SchoolId</c>) and covered by the tenant query
/// filter (SEC-AUTHZ-03).
/// </summary>
public class SchoolModule : BaseAuditableEntity
{
    private SchoolModule()
    {
        // For EF Core.
    }

    private SchoolModule(int schoolId, int moduleId, bool isEnabled)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(moduleId);

        SchoolId = schoolId;
        ModuleId = moduleId;
        IsEnabled = isEnabled;
    }

    /// <summary>Records the master's choice for a module.</summary>
    public static SchoolModule Create(int schoolId, PortalModule module, bool isEnabled)
    {
        ArgumentNullException.ThrowIfNull(module);

        return new SchoolModule(schoolId, module.Id, isEnabled);
    }

    public int SchoolId { get; private set; }

    public int ModuleId { get; private set; }

    public bool IsEnabled { get; private set; }

    public void SetEnabled(bool isEnabled) => IsEnabled = isEnabled;
}
