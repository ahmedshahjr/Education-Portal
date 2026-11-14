namespace EducationPortal.Domain.Entities;

/// <summary>One paid module included in a plan. Platform reference data that only the master tenant changes.</summary>
public class PlanModule : BaseAuditableEntity
{
    private PlanModule()
    {
        // For EF Core.
    }

    private PlanModule(int planId, int moduleId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(planId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(moduleId);

        PlanId = planId;
        ModuleId = moduleId;
    }

    /// <summary>Adds a paid module to a plan. Core modules are in every subscription, so a plan never lists one.</summary>
    public static PlanModule Create(Plan plan, PortalModule module)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(module);

        if (module.IsCore)
        {
            throw new ArgumentException("A core module is in every subscription, so a plan does not list it.", nameof(module));
        }

        return new PlanModule(plan.Id, module.Id);
    }

    public int PlanId { get; private set; }

    public int ModuleId { get; private set; }
}
