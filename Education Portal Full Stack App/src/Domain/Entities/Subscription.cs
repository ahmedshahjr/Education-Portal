namespace EducationPortal.Domain.Entities;

/// <summary>
/// What a school has bought: a plan (or none, for add-ons only), a billing cycle and the price for one cycle in the
/// price book's currency. The price and currency are copied here, so a later price change never changes an existing
/// subscription. The modules it gives are its <see cref="Modules"/>, written together with it. Owned by the school
/// (<c>SchoolId</c>) and covered by the tenant query filter (SEC-AUTHZ-03). A school has at most one subscription that
/// is not cancelled.
/// </summary>
public class Subscription : BaseAuditableEntity
{
    public const int MaxTrialDays = 90;

    private readonly List<TenantModule> _modules = [];

    private Subscription()
    {
        // For EF Core.
        CurrencyCode = string.Empty;
    }

    private Subscription(int schoolId, int? planId, PriceBook priceBook, BillingCycle billingCycle, decimal amount, DateTimeOffset startDate, int trialDays)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(schoolId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(priceBook.Id);
        ArgumentOutOfRangeException.ThrowIfNegative(trialDays);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trialDays, MaxTrialDays);

        if (planId is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(planId), planId, "A plan id is positive.");
        }

        if (!Enum.IsDefined(billingCycle))
        {
            throw new ArgumentOutOfRangeException(nameof(billingCycle), billingCycle, "Unknown billing cycle.");
        }

        if (!PriceBookItem.IsValidPrice(amount))
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "An amount is zero or more, with at most two decimal places.");
        }

        SchoolId = schoolId;
        PlanId = planId;
        PriceBookId = priceBook.Id;
        CurrencyCode = priceBook.CurrencyCode;
        BillingCycle = billingCycle;
        Amount = amount;
        StartDate = startDate;

        if (trialDays > 0)
        {
            Status = SubscriptionStatus.Trial;
            TrialEndDate = startDate.AddDays(trialDays);
            EndDate = TrialEndDate.Value;
        }
        else
        {
            Status = SubscriptionStatus.Active;
            EndDate = PeriodEnd(startDate, billingCycle);
        }
    }

    /// <summary>Starts a subscription: a trial of <paramref name="trialDays"/> days, or active at once when it is 0.</summary>
    public static Subscription Create(int schoolId, Plan? plan, PriceBook priceBook, BillingCycle billingCycle, decimal amount, DateTimeOffset startDate, int trialDays)
    {
        ArgumentNullException.ThrowIfNull(priceBook);

        return new Subscription(schoolId, plan?.Id, priceBook, billingCycle, amount, startDate, trialDays);
    }

    /// <summary>When a period that starts at <paramref name="start"/> ends: one month or one year later.</summary>
    public static DateTimeOffset PeriodEnd(DateTimeOffset start, BillingCycle billingCycle) => billingCycle switch
    {
        BillingCycle.Monthly => start.AddMonths(1),
        BillingCycle.Yearly => start.AddYears(1),
        _ => throw new ArgumentOutOfRangeException(nameof(billingCycle), billingCycle, "Unknown billing cycle."),
    };

    public int SchoolId { get; private set; }

    /// <summary>The plan bought, or <c>null</c> when the school bought add-on modules only.</summary>
    public int? PlanId { get; private set; }

    public int PriceBookId { get; private set; }

    /// <summary>The ISO 4217 currency, copied from the price book when the subscription was made.</summary>
    public string CurrencyCode { get; private set; }

    public BillingCycle BillingCycle { get; private set; }

    /// <summary>The price for one billing cycle in <see cref="CurrencyCode"/>, copied from the price book.</summary>
    public decimal Amount { get; private set; }

    public SubscriptionStatus Status { get; private set; }

    public DateTimeOffset StartDate { get; private set; }

    /// <summary>When the current period ends (the trial's end while on trial).</summary>
    public DateTimeOffset EndDate { get; private set; }

    public DateTimeOffset? TrialEndDate { get; private set; }

    public IReadOnlyCollection<TenantModule> Modules => _modules;

    /// <summary>
    /// Gives the school a module for the subscription's current period: for the whole school, or for one of its branches
    /// when <paramref name="branch"/> is given. The same module can be given to several branches, but only once to each.
    /// </summary>
    public TenantModule AddModule(PortalModule module, Branch? branch = null)
    {
        ArgumentNullException.ThrowIfNull(module);

        if (branch is not null && branch.SchoolId != SchoolId)
        {
            throw new ArgumentException("A branch belongs to the subscription's school.", nameof(branch));
        }

        if (_modules.Any(m => m.ModuleId == module.Id && m.BranchId == branch?.Id))
        {
            throw new InvalidOperationException("The subscription already has this module.");
        }

        var tenantModule = new TenantModule(this, module, branch, StartDate, EndDate);
        _modules.Add(tenantModule);
        return tenantModule;
    }
}
