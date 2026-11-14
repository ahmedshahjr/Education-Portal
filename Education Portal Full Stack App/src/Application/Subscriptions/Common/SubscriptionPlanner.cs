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

        var dependencies = await context.ModuleDependencies
            .Select(d => new { d.ModuleId, d.RequiresModuleId })
            .ToListAsync(cancellationToken);

        // Checked before the price book, so a bad module choice is reported first.
        var given = SubscriptionQuoteRules.ResolveModules(modules, dependencies.Select(d => (d.ModuleId, d.RequiresModuleId)).ToList(), planModuleIds, addOnCodes);

        var book = await context.PriceBooks.AsNoTracking().FirstOrDefaultAsync(b => b.CountryCode == countryCode && b.IsActive, cancellationToken);
        Reject(book is null, nameof(countryCode), ValidationMessages.PriceMissing);

        var prices = await context.PriceBookItems.AsNoTracking()
            .Where(i => i.PriceBookId == book!.Id && i.BillingCycle == billingCycle)
            .ToListAsync(cancellationToken);

        var amount = SubscriptionQuoteRules.Price(plan, planModuleIds, given, prices);

        return new SubscriptionQuote(plan, book!, amount, given);
    }

    internal static void Reject(bool failed, string property, string message)
    {
        if (failed)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException([new ValidationFailure(property, message)]);
        }
    }
}
