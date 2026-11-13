using EducationPortal.Application.Common.Interfaces;
using FluentValidation.Results;

namespace EducationPortal.Application.Subscriptions.Common;

/// <summary>What a choice of plan and add-ons comes to: the price book it is priced from, its price and its modules.</summary>
/// <param name="Modules">Every module the school gets, in catalogue order: core, the plan's, the add-ons and what they require.</param>
internal sealed record SubscriptionQuote(Plan? Plan, PriceBook PriceBook, decimal Amount, IReadOnlyList<PortalModule> Modules);

/// <summary>
/// Works out what a school gets and pays for a plan and add-on modules (the entitlement rules of ADR-011). The modules
/// are the plan's paid modules plus the add-ons, plus everything those require, plus every core module (core modules are
/// in every subscription). The price is the plan's price plus the price of each paid module the plan does not already
/// include, all from the price book of the school's country, in one billing cycle. It only reads; the caller writes the
/// subscription and its modules in one save, so they are stored together or not at all.
/// </summary>
internal static class SubscriptionPlanner
{
    public static async Task<SubscriptionQuote> QuoteAsync(
        IApplicationDbContext context,
        string countryCode,
        string? planCode,
        IReadOnlyCollection<string> addOnCodes,
        BillingCycle billingCycle,
        CancellationToken cancellationToken)
    {
        var modules = await context.Modules.AsNoTracking().OrderBy(m => m.SortOrder).ThenBy(m => m.Code).ToListAsync(cancellationToken);

        Plan? plan = null;
        var planModuleIds = new HashSet<int>();
        if (!string.IsNullOrWhiteSpace(planCode))
        {
            plan = await context.Plans.AsNoTracking().FirstOrDefaultAsync(p => p.Code == planCode && p.IsActive, cancellationToken);
            Reject(plan is null, nameof(planCode), ValidationMessages.PlanNotFound);

            planModuleIds = (await context.PlanModules.Where(m => m.PlanId == plan!.Id).Select(m => m.ModuleId).ToListAsync(cancellationToken)).ToHashSet();
        }

        var addOns = addOnCodes.Select(code => modules.FirstOrDefault(m => m.Code == code)).ToList();
        Reject(addOns.Any(m => m is null), nameof(addOnCodes), ValidationMessages.UnknownModules);
        Reject(addOns.Any(m => m!.IsCore), nameof(addOnCodes), ValidationMessages.CoreModuleNotSold);

        var dependencies = await context.ModuleDependencies
            .Select(d => new { d.ModuleId, d.RequiresModuleId })
            .ToListAsync(cancellationToken);

        var chosen = planModuleIds.Concat(addOns.Select(m => m!.Id)).ToList();
        var granted = ModuleDependencyGraph.RequiredBy(dependencies.Select(d => (d.ModuleId, d.RequiresModuleId)), chosen)
            .Concat(modules.Where(m => m.IsCore).Select(m => m.Id))
            .ToHashSet();

        var given = modules.Where(m => granted.Contains(m.Id)).ToList();
        Reject(given.Any(m => !m.IsActive), nameof(addOnCodes), ValidationMessages.ModuleInactive);

        var book = await context.PriceBooks.AsNoTracking().FirstOrDefaultAsync(b => b.CountryCode == countryCode && b.IsActive, cancellationToken);
        Reject(book is null, nameof(countryCode), ValidationMessages.PriceMissing);

        var prices = await context.PriceBookItems.AsNoTracking()
            .Where(i => i.PriceBookId == book!.Id && i.BillingCycle == billingCycle)
            .ToListAsync(cancellationToken);

        decimal amount = 0;
        if (plan is not null)
        {
            var price = prices.FirstOrDefault(i => i.PlanId == plan.Id);
            Reject(price is null, nameof(planCode), ValidationMessages.PriceMissing);
            amount += price!.Price;
        }

        foreach (var module in given.Where(m => !m.IsCore && !planModuleIds.Contains(m.Id)))
        {
            var price = prices.FirstOrDefault(i => i.ModuleId == module.Id);
            Reject(price is null, nameof(addOnCodes), ValidationMessages.PriceMissing);
            amount += price!.Price;
        }

        return new SubscriptionQuote(plan, book!, amount, given);
    }

    private static void Reject(bool failed, string property, string message)
    {
        if (failed)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException([new ValidationFailure(property, message)]);
        }
    }
}
