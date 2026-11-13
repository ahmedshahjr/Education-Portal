using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using FluentValidation.Results;

namespace EducationPortal.Application.Subscriptions.Common;

/// <summary>
/// What the master tenant's subscription changes (change plan, add or remove a module, approve an add-on request) have in
/// common (ADR-014): load the school's current subscription, work out what it holds now (its plan and add-on modules),
/// re-quote the wanted choice from the subscription's price book and apply the result with
/// <see cref="Subscription.Reconfigure"/>. A change takes effect at once and never credits or charges the current
/// period; the next invoice uses the new price.
/// </summary>
internal static class SubscriptionChanges
{
    /// <summary>The school's current subscription (not cancelled) with its modules; 404 when it has none.</summary>
    public static async Task<Subscription> LoadAsync(IApplicationDbContext context, int schoolId, Guid notFoundKey, CancellationToken cancellationToken)
    {
        var subscription = await context.Subscriptions
            .Include(s => s.Modules)
            .FirstOrDefaultAsync(s => s.SchoolId == schoolId && s.Status != SubscriptionStatus.Cancelled, cancellationToken);
        Guard.Against.NotFound(notFoundKey, subscription);

        return subscription;
    }

    /// <summary>
    /// The plan code and the add-on module codes the subscription holds now: the add-ons are the paid whole-school
    /// modules it gives that its plan does not include (a module that an add-on requires counts as an add-on too, as it
    /// was priced that way).
    /// </summary>
    public static async Task<(string? PlanCode, List<string> AddOns)> CurrentChoiceAsync(
        IApplicationDbContext context, Subscription subscription, CancellationToken cancellationToken)
    {
        string? planCode = null;
        var planModuleIds = new HashSet<int>();
        if (subscription.PlanId is { } planId)
        {
            planCode = await context.Plans.Where(p => p.Id == planId).Select(p => p.Code).FirstOrDefaultAsync(cancellationToken);
            planModuleIds = (await context.PlanModules.Where(m => m.PlanId == planId).Select(m => m.ModuleId).ToListAsync(cancellationToken)).ToHashSet();
        }

        var heldIds = subscription.Modules
            .Where(m => m.BranchId is null && m.Status != TenantModuleStatus.Expired)
            .Select(m => m.ModuleId)
            .Where(id => !planModuleIds.Contains(id))
            .ToList();

        var addOns = await context.Modules
            .Where(m => heldIds.Contains(m.Id) && !m.IsCore)
            .OrderBy(m => m.SortOrder).ThenBy(m => m.Code)
            .Select(m => m.Code)
            .ToListAsync(cancellationToken);

        return (planCode, addOns);
    }

    /// <summary>
    /// Re-quotes <paramref name="planCode"/> and <paramref name="addOns"/> from the subscription's own price book and
    /// applies them. Returns the codes of every module the school now has.
    /// </summary>
    public static async Task<IReadOnlyList<string>> ApplyAsync(
        IApplicationDbContext context,
        Subscription subscription,
        string? planCode,
        IReadOnlyCollection<string> addOns,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var book = await context.PriceBooks.AsNoTracking().FirstAsync(b => b.Id == subscription.PriceBookId, cancellationToken);

        var quote = await SubscriptionPlanner.QuoteAsync(context, book.CountryCode, planCode, addOns, subscription.BillingCycle, cancellationToken);

        if (quote.PriceBook.CurrencyCode != subscription.CurrencyCode)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new ValidationFailure(nameof(planCode), ValidationMessages.SubscriptionCurrencyMismatch)]);
        }

        subscription.Reconfigure(quote.Plan, quote.PriceBook, quote.Amount, quote.Modules, timeProvider.GetUtcNow());

        return quote.Modules.Select(m => m.Code).ToList();
    }

    public static Task WriteAuditAsync(ISecurityAuditLog auditLog, Subscription subscription) =>
        auditLog.WriteAsync(
            SecurityAuditEventType.SubscriptionChanged,
            SecurityAuditOutcome.Success,
            targetType: nameof(Subscription),
            targetId: subscription.Id.ToString(CultureInfo.InvariantCulture));
}
