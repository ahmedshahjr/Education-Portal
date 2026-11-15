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

    /// <summary>
    /// When the subscription lapsed (trial ended, period ended or an invoice fell due unpaid), while it is past due. The
    /// grace period before the daily lifecycle job suspends it counts from here. Empty while it is in good standing.
    /// </summary>
    public DateTimeOffset? PastDueSince { get; private set; }

    public IReadOnlyCollection<TenantModule> Modules => _modules;

    /// <summary>
    /// Applies a paid invoice. An invoice for a period beyond the current end extends the subscription to that period's
    /// end (a trial or a renewal becomes Active, and every module's validity is extended). An invoice for the current
    /// period brings a past-due or suspended subscription back to Active and reactivates its suspended modules. A
    /// cancelled subscription is never changed.
    /// </summary>
    public void ApplyPayment(Invoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        if (invoice.SubscriptionId != Id || invoice.Status != InvoiceStatus.Paid)
        {
            throw new ArgumentException("Only a paid invoice of this subscription can be applied.", nameof(invoice));
        }

        if (Status == SubscriptionStatus.Cancelled)
        {
            return;
        }

        var lapsed = Status is SubscriptionStatus.PastDue or SubscriptionStatus.Suspended;

        if (invoice.PeriodEnd > EndDate)
        {
            EndDate = invoice.PeriodEnd;
        }

        Status = SubscriptionStatus.Active;
        PastDueSince = null;

        foreach (var module in _modules)
        {
            module.ExtendTo(EndDate);

            if (lapsed && module.Status == TenantModuleStatus.Suspended)
            {
                module.Reactivate();
            }
        }
    }

    /// <summary>Stops the subscription: it and every module it gives are suspended (data is kept). Cancelled and already suspended subscriptions are refused.</summary>
    public void Suspend()
    {
        if (Status is SubscriptionStatus.Cancelled or SubscriptionStatus.Suspended)
        {
            throw new InvalidOperationException("Only a subscription that is running can be suspended.");
        }

        Status = SubscriptionStatus.Suspended;
        foreach (var module in _modules.Where(m => m.Status == TenantModuleStatus.Active))
        {
            module.Suspend();
        }
    }

    /// <summary>Brings a suspended or past-due subscription back: it becomes Active and its suspended modules are active again.</summary>
    public void Reactivate()
    {
        if (Status is not (SubscriptionStatus.Suspended or SubscriptionStatus.PastDue))
        {
            throw new InvalidOperationException("Only a suspended or past-due subscription can be reactivated.");
        }

        Status = SubscriptionStatus.Active;
        PastDueSince = null;
        foreach (var module in _modules.Where(m => m.Status == TenantModuleStatus.Suspended))
        {
            module.Reactivate();
        }
    }

    /// <summary>
    /// Where the next paid period starts when a renewal is invoiced at <paramref name="issueDate"/>: at the current end, so
    /// the billing date never moves, unless that end has already passed. A lapsed subscription restarts on the issue date
    /// instead, so the school is not billed for days it was past due or suspended, and the new period is not over before
    /// it is paid (ADR-015).
    /// </summary>
    public static DateTimeOffset RenewalStart(DateTimeOffset endDate, DateTimeOffset issueDate) =>
        endDate >= issueDate ? endDate : issueDate;

    /// <summary>
    /// Ends a trial whose end date has passed: the school has not paid, so the subscription becomes past due from the
    /// trial's end. Returns false, and changes nothing, when it is not on trial or the trial has not ended yet.
    /// </summary>
    public bool EndTrial(DateTimeOffset now)
    {
        if (Status != SubscriptionStatus.Trial || TrialEndDate is null || TrialEndDate > now)
        {
            return false;
        }

        Status = SubscriptionStatus.PastDue;
        PastDueSince = TrialEndDate;
        return true;
    }

    /// <summary>
    /// Marks an active subscription past due from <paramref name="since"/> (an unpaid invoice's due date or the end of an
    /// unpaid period). The modules keep working during the grace period. Returns false, and changes nothing, when the
    /// subscription is not active.
    /// </summary>
    public bool MarkPastDue(DateTimeOffset since)
    {
        if (Status != SubscriptionStatus.Active)
        {
            return false;
        }

        Status = SubscriptionStatus.PastDue;
        PastDueSince = since;
        return true;
    }

    /// <summary>
    /// Suspends a past-due subscription once <paramref name="graceDays"/> days have passed since it lapsed (see
    /// <see cref="Suspend"/>: modules are switched off, no data is deleted). Returns false, and changes nothing, when it
    /// is not past due or the grace period is still running.
    /// </summary>
    public bool SuspendAfterGrace(DateTimeOffset now, int graceDays)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(graceDays);

        if (Status != SubscriptionStatus.PastDue || PastDueSince is null || PastDueSince.Value.AddDays(graceDays) > now)
        {
            return false;
        }

        Suspend();
        return true;
    }

    public const int MaxExtensionDays = 366;

    /// <summary>
    /// Moves the end of the current period (and of the trial, while on trial) later by <paramref name="days"/> without a
    /// payment, and every module's validity with it. A cancelled subscription is refused.
    /// </summary>
    public void Extend(int days)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(days);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(days, MaxExtensionDays);

        if (Status == SubscriptionStatus.Cancelled)
        {
            throw new InvalidOperationException("A cancelled subscription cannot be extended.");
        }

        EndDate = EndDate.AddDays(days);
        if (Status == SubscriptionStatus.Trial)
        {
            TrialEndDate = EndDate;
        }

        foreach (var module in _modules)
        {
            module.ExtendTo(EndDate);
        }
    }

    /// <summary>
    /// Changes what the subscription gives and costs: the plan, the price for one cycle and the modules (every module, core
    /// and paid). Modules no longer in the list end now (their rows are kept); new ones start now and run to the end of the
    /// current period; modules that return are restored. The price book must have the subscription's currency. A
    /// cancelled subscription is refused. Changes take effect at once, with no credit or charge for the current period.
    /// Modules that come with the whole school are granted on <paramref name="mainBranch"/>.
    /// </summary>
    public void Reconfigure(Plan? plan, PriceBook priceBook, decimal amount, IReadOnlyCollection<PortalModule> modules, DateTimeOffset now, Branch mainBranch)
    {
        ArgumentNullException.ThrowIfNull(priceBook);
        ArgumentNullException.ThrowIfNull(modules);
        ArgumentNullException.ThrowIfNull(mainBranch);

        if (mainBranch.Kind != BranchKind.Main || mainBranch.SchoolId != SchoolId)
        {
            throw new ArgumentException("Whole-school modules are granted on the school's main branch.", nameof(mainBranch));
        }

        if (Status == SubscriptionStatus.Cancelled)
        {
            throw new InvalidOperationException("A cancelled subscription cannot be changed.");
        }

        if (priceBook.CurrencyCode != CurrencyCode)
        {
            throw new ArgumentException("The price book's currency is not the subscription's.", nameof(priceBook));
        }

        if (!PriceBookItem.IsValidPrice(amount))
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "An amount is zero or more, with at most two decimal places.");
        }

        PlanId = plan?.Id;
        PriceBookId = priceBook.Id;
        Amount = amount;

        var wanted = modules.Select(m => m.Id).ToHashSet();
        foreach (var grant in _modules.Where(m => m.IsSchoolWide))
        {
            if (!wanted.Contains(grant.ModuleId))
            {
                if (grant.Status != TenantModuleStatus.Expired)
                {
                    grant.Expire(now);
                }
            }
            else if (grant.Status == TenantModuleStatus.Expired)
            {
                grant.Restore(EndDate);
            }
        }

        foreach (var module in modules.Where(m => _modules.All(g => !g.IsSchoolWide || g.ModuleId != m.Id)))
        {
            _modules.Add(new TenantModule(this, module, mainBranch, now < EndDate ? now : StartDate, EndDate));
        }
    }

    /// <summary>
    /// Gives the school a module for the subscription's current period, on <paramref name="branch"/>: the school's main
    /// branch for the whole school, or another branch for that branch only. A module can be given to each branch once.
    /// </summary>
    public TenantModule AddModule(PortalModule module, Branch branch)
    {
        ArgumentNullException.ThrowIfNull(module);
        ArgumentNullException.ThrowIfNull(branch);

        if (branch.SchoolId != SchoolId)
        {
            throw new ArgumentException("A branch belongs to the subscription's school.", nameof(branch));
        }

        if (_modules.Any(m => m.ModuleId == module.Id && m.BranchId == branch.Id))
        {
            throw new InvalidOperationException("The subscription already has this module.");
        }

        var tenantModule = new TenantModule(this, module, branch, StartDate, EndDate);
        _modules.Add(tenantModule);
        return tenantModule;
    }
}
