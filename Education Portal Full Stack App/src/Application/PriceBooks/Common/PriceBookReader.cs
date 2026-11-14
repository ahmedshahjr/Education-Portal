using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.PriceBooks.Common;

internal static class PriceBookReader
{
    /// <summary>Live price books by country, optionally only one, with their prices (plans first, then modules, by code).</summary>
    public static async Task<List<PriceBookDto>> LoadAsync(IApplicationDbContext context, Guid? id, CancellationToken cancellationToken)
    {
        var books = await context.PriceBooks
            .AsNoTracking()
            .Where(b => id == null || b.PublicId == id)
            .OrderBy(b => b.CountryCode)
            .Select(b => new { b.Id, b.PublicId, b.CountryCode, b.CurrencyCode, b.TaxRate, b.IsActive })
            .ToListAsync(cancellationToken);

        var bookIds = books.Select(b => b.Id).ToList();
        var items = await (
                from i in context.PriceBookItems.AsNoTracking()
                where bookIds.Contains(i.PriceBookId)
                join p in context.Plans on i.PlanId equals p.Id into plans
                from p in plans.DefaultIfEmpty()
                join m in context.Modules on i.ModuleId equals m.Id into modules
                from m in modules.DefaultIfEmpty()
                select new { i.PriceBookId, PlanCode = p.Code, ModuleCode = m.Code, i.BillingCycle, i.Price })
            .ToListAsync(cancellationToken);

        var byBook = items.ToLookup(i => i.PriceBookId);

        return books
            .Select(b => new PriceBookDto(
                b.PublicId,
                b.CountryCode,
                b.CurrencyCode,
                b.TaxRate,
                b.IsActive,
                byBook[b.Id]
                    .OrderBy(i => i.PlanCode is null)
                    .ThenBy(i => i.PlanCode ?? i.ModuleCode, StringComparer.Ordinal)
                    .ThenBy(i => i.BillingCycle)
                    .Select(i => new PriceDto(i.PlanCode, i.ModuleCode, i.BillingCycle, i.Price))
                    .ToList()))
            .ToList();
    }
}
