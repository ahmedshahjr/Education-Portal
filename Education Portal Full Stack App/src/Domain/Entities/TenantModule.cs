using EducationPortal.Domain.Enums;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A module a school has: through its subscription (the plan's modules, add-ons and core modules, written with
/// <see cref="Subscription.AddModule"/>), or as a default grant when the school registers (<see cref="CreateDefault"/>,
/// with no subscription and no end). Every row names a branch: <see cref="BranchId"/> is the school's main branch for a
/// module the whole school has (<see cref="IsSchoolWide"/>), or another branch for a module limited to that branch. Owned
/// by the school (<c>SchoolId</c>) and covered by the tenant query filter (SEC-AUTHZ-03). The module's data is kept when
/// the row expires or is suspended.
/// </summary>
public class TenantModule : BaseAuditableEntity
{
    private TenantModule()
    {
        // For EF Core.
    }

    private TenantModule(int schoolId, PortalModule module, Branch mainBranch, DateTimeOffset validFrom)
    {
        SchoolId = schoolId;
        ModuleId = module.Id;
        BranchId = mainBranch.Id;
        IsSchoolWide = true;
        Status = TenantModuleStatus.Active;
        ValidFrom = validFrom;
        ValidTo = null;
    }

    /// <summary>
    /// The default grant of a module to a school that registers: the whole school, from <paramref name="now"/>, with no end
    /// and no subscription. Free of charge, and not suspended by billing. The caller checks the school has no grant of it yet.
    /// </summary>
    public static TenantModule CreateDefault(int schoolId, PortalModule module, Branch mainBranch, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentNullException.ThrowIfNull(module);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(module.Id);
        ArgumentNullException.ThrowIfNull(mainBranch);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(mainBranch.Id);

        if (mainBranch.Kind != BranchKind.Main || mainBranch.SchoolId != schoolId)
        {
            throw new ArgumentException("A default grant is for the school's main branch.", nameof(mainBranch));
        }

        return new TenantModule(schoolId, module, mainBranch, now);
    }

    internal TenantModule(Subscription subscription, PortalModule module, Branch branch, DateTimeOffset validFrom, DateTimeOffset? validTo)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(module.Id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(branch.Id);

        if (validTo is not null && validTo <= validFrom)
        {
            throw new ArgumentException("A module is valid for a period that ends after it starts.", nameof(validTo));
        }

        Subscription = subscription;
        SchoolId = subscription.SchoolId;
        ModuleId = module.Id;
        BranchId = branch.Id;
        IsSchoolWide = branch.Kind == BranchKind.Main;
        Status = TenantModuleStatus.Active;
        ValidFrom = validFrom;
        ValidTo = validTo;
    }

    public int SchoolId { get; private set; }

    public int ModuleId { get; private set; }

    /// <summary>The subscription that gives the module, or <c>null</c> for a default grant.</summary>
    public int? SubscriptionId { get; private set; }

    public Subscription? Subscription { get; private set; }

    /// <summary>The branch the grant is for: the school's main branch when the module is for the whole school.</summary>
    public int BranchId { get; private set; }

    /// <summary>
    /// True when the grant is for the whole school. It is the case exactly when <see cref="BranchId"/> is the school's
    /// main branch, so one grant on the main branch covers every branch of the school.
    /// </summary>
    public bool IsSchoolWide { get; private set; }

    public TenantModuleStatus Status { get; private set; }

    public DateTimeOffset ValidFrom { get; private set; }

    /// <summary>When the module stops working unless renewed, or <c>null</c> for no end.</summary>
    public DateTimeOffset? ValidTo { get; private set; }

    /// <summary>Switches the module off while the subscription is suspended. The module's data is kept.</summary>
    public void Suspend() => Status = TenantModuleStatus.Suspended;

    /// <summary>Switches a suspended module back on.</summary>
    public void Reactivate() => Status = TenantModuleStatus.Active;

    /// <summary>Ends the module's grant now (removed from the subscription). The row is kept for audit.</summary>
    public void Expire(DateTimeOffset now)
    {
        Status = TenantModuleStatus.Expired;

        if (ValidTo is null || ValidTo > now)
        {
            ValidTo = now > ValidFrom ? now : ValidFrom.AddTicks(1);
        }
    }

    /// <summary>Gives an expired or suspended grant back until <paramref name="validTo"/> (the module is on the subscription again).</summary>
    public void Restore(DateTimeOffset validTo)
    {
        if (validTo <= ValidFrom)
        {
            throw new ArgumentException("A module is valid for a period that ends after it starts.", nameof(validTo));
        }

        Status = TenantModuleStatus.Active;
        ValidTo = validTo;
    }

    /// <summary>Moves the end of the module's validity later (a paid renewal). It never shortens it.</summary>
    public void ExtendTo(DateTimeOffset validTo)
    {
        if (validTo > ValidTo)
        {
            ValidTo = validTo;
        }
    }
}
