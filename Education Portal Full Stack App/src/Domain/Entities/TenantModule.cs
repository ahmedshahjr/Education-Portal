namespace EducationPortal.Domain.Entities;

/// <summary>
/// A module a school has through its subscription: core modules, the plan's modules and add-ons. Rows are written with
/// the subscription (<see cref="Subscription.AddModule"/>), never on their own. <see cref="BranchId"/> is empty when the
/// module is for the whole school and set when it is limited to one branch. Owned by the school (<c>SchoolId</c>) and
/// covered by the tenant query filter (SEC-AUTHZ-03). The module's data is kept when the row expires or is suspended.
/// </summary>
public class TenantModule : BaseAuditableEntity
{
    private TenantModule()
    {
        // For EF Core.
    }

    internal TenantModule(Subscription subscription, PortalModule module, Branch? branch, DateTimeOffset validFrom, DateTimeOffset? validTo)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(module.Id);

        if (branch is not null)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(branch.Id);
        }

        if (validTo is not null && validTo <= validFrom)
        {
            throw new ArgumentException("A module is valid for a period that ends after it starts.", nameof(validTo));
        }

        Subscription = subscription;
        SchoolId = subscription.SchoolId;
        ModuleId = module.Id;
        BranchId = branch?.Id;
        Status = TenantModuleStatus.Active;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    public int SchoolId { get; private set; }

    public int ModuleId { get; private set; }

    public int SubscriptionId { get; private set; }

    public Subscription? Subscription { get; private set; }

    /// <summary>The branch the module is limited to, or <c>null</c> for the whole school.</summary>
    public int? BranchId { get; private set; }

    public TenantModuleStatus Status { get; private set; }

    public DateTimeOffset ValidFrom { get; private set; }

    /// <summary>When the module stops working unless renewed, or <c>null</c> for no end.</summary>
    public DateTimeOffset? ValidTo { get; private set; }

    /// <summary>Switches the module off while the subscription is suspended. The module's data is kept.</summary>
    public void Suspend() => Status = TenantModuleStatus.Suspended;

    /// <summary>Switches a suspended module back on.</summary>
    public void Reactivate() => Status = TenantModuleStatus.Active;
}
