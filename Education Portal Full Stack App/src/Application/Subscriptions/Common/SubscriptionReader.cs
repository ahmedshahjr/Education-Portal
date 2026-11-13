using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.Subscriptions.Common;

/// <summary>Reads a school's current subscription as a <see cref="SubscriptionDto"/>.</summary>
internal static class SubscriptionReader
{
    /// <summary>The school's subscription that is not cancelled, with its modules, or <c>null</c> when it has none.</summary>
    public static async Task<SubscriptionDto?> ReadAsync(IApplicationDbContext context, int schoolId, CancellationToken cancellationToken)
    {
        var subscription = await (
                from s in context.Subscriptions.AsNoTracking()
                join p in context.Plans on s.PlanId equals p.Id into plans
                from p in plans.DefaultIfEmpty()
                join b in context.PriceBooks on s.PriceBookId equals b.Id
                where s.SchoolId == schoolId && s.Status != SubscriptionStatus.Cancelled
                select new { s.Id, s.PublicId, PlanCode = p.Code, b.CountryCode, s.CurrencyCode, s.BillingCycle, s.Amount, s.Status, s.StartDate, s.EndDate, s.TrialEndDate })
            .FirstOrDefaultAsync(cancellationToken);
        if (subscription is null)
        {
            return null;
        }

        var modules = await (
                from t in context.TenantModules.AsNoTracking()
                join m in context.Modules on t.ModuleId equals m.Id
                where t.SubscriptionId == subscription.Id
                orderby m.SortOrder, m.Code
                select new TenantModuleDto(m.Code, t.Status, t.ValidFrom, t.ValidTo))
            .ToListAsync(cancellationToken);

        return new SubscriptionDto(
            subscription.PublicId,
            subscription.PlanCode,
            subscription.CountryCode,
            subscription.CurrencyCode,
            subscription.BillingCycle,
            subscription.Amount,
            subscription.Status,
            subscription.StartDate,
            subscription.EndDate,
            subscription.TrialEndDate,
            modules);
    }
}
