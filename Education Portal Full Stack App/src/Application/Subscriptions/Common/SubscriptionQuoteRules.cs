namespace EducationPortal.Application.Subscriptions.Common;

/// <summary>
/// The pure rules behind a subscription quote (ADR-011), kept apart from the database reads so they can be unit-tested:
/// which modules a choice gives, and what it costs for one billing cycle. A bad choice is refused with a validation
/// error naming the field, as the API reports it.
/// </summary>
public static class SubscriptionQuoteRules
{
    /// <summary>
    /// The modules a school gets, in catalogue order: the plan's paid modules and the add-ons, everything they require
    /// (directly or through other modules), and every core module. Refuses unknown add-ons, core modules sold as add-ons
    /// and any inactive module in the result.
    /// </summary>
    /// <param name="catalogue">Every module, in catalogue order.</param>
    /// <param name="dependencies">Pairs of (module, the module it requires).</param>
    /// <param name="planModuleIds">The plan's paid modules; empty for add-ons only.</param>
    /// <param name="addOnCodes">The add-on module codes chosen.</param>
    public static IReadOnlyList<PortalModule> ResolveModules(
        IReadOnlyList<PortalModule> catalogue,
        IReadOnlyCollection<(int ModuleId, int RequiresModuleId)> dependencies,
        IReadOnlySet<int> planModuleIds,
        IReadOnlyCollection<string> addOnCodes)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(dependencies);
        ArgumentNullException.ThrowIfNull(planModuleIds);
        ArgumentNullException.ThrowIfNull(addOnCodes);

        var addOns = addOnCodes.Select(code => catalogue.FirstOrDefault(m => m.Code == code)).ToList();
        SubscriptionPlanner.Reject(addOns.Any(m => m is null), "addOnCodes", ValidationMessages.UnknownModules);
        SubscriptionPlanner.Reject(addOns.Any(m => m!.IsCore), "addOnCodes", ValidationMessages.CoreModuleNotSold);

        var chosen = planModuleIds.Concat(addOns.Select(m => m!.Id)).ToList();
        var granted = ModuleDependencyGraph.RequiredBy(dependencies, chosen)
            .Concat(catalogue.Where(m => m.IsCore).Select(m => m.Id))
            .ToHashSet();

        var given = catalogue.Where(m => granted.Contains(m.Id)).ToList();
        SubscriptionPlanner.Reject(given.Any(m => !m.IsActive), "addOnCodes", ValidationMessages.ModuleInactive);

        return given;
    }

    /// <summary>
    /// The price for one billing cycle: the plan's price plus the price of each paid module the plan does not already
    /// include (add-ons and what they require). Core modules are free. Refuses a choice with any price missing.
    /// </summary>
    /// <param name="prices">The price book's prices for the chosen billing cycle.</param>
    public static decimal Price(Plan? plan, IReadOnlySet<int> planModuleIds, IReadOnlyCollection<PortalModule> given, IReadOnlyCollection<PriceBookItem> prices)
    {
        ArgumentNullException.ThrowIfNull(planModuleIds);
        ArgumentNullException.ThrowIfNull(given);
        ArgumentNullException.ThrowIfNull(prices);

        decimal amount = 0;
        if (plan is not null)
        {
            var price = prices.FirstOrDefault(i => i.PlanId == plan.Id);
            SubscriptionPlanner.Reject(price is null, "planCode", ValidationMessages.PriceMissing);
            amount += price!.Price;
        }

        foreach (var module in given.Where(m => !m.IsCore && !planModuleIds.Contains(m.Id)))
        {
            var price = prices.FirstOrDefault(i => i.ModuleId == module.Id);
            SubscriptionPlanner.Reject(price is null, "addOnCodes", ValidationMessages.PriceMissing);
            amount += price!.Price;
        }

        return amount;
    }
}
