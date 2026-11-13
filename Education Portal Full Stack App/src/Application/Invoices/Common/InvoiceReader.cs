using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.Invoices.Common;

internal static class InvoiceReader
{
    /// <summary>Invoices newest first, optionally one school's or one invoice, with lines and payments.</summary>
    public static async Task<List<InvoiceDto>> LoadAsync(IApplicationDbContext context, Guid? schoolId, Guid? invoiceId, CancellationToken cancellationToken)
    {
        var invoices = await (
                from i in context.Invoices.AsNoTracking()
                join s in context.Schools on i.SchoolId equals s.Id
                where (schoolId == null || s.PublicId == schoolId) && (invoiceId == null || i.PublicId == invoiceId)
                orderby i.IssueDate descending, i.Id descending
                select new { i.Id, i.PublicId, SchoolPublicId = s.PublicId, i.Number, i.Status, i.CurrencyCode, i.Subtotal, i.TaxRate, i.TaxAmount, i.Total, i.IssueDate, i.DueDate, i.PeriodStart, i.PeriodEnd, i.PaidDate })
            .ToListAsync(cancellationToken);

        var ids = invoices.Select(i => i.Id).ToList();
        var lines = (await context.InvoiceLines.AsNoTracking().Where(l => ids.Contains(l.InvoiceId)).OrderBy(l => l.Id)
                .Select(l => new { l.InvoiceId, Line = new InvoiceLineDto(l.Description, l.Amount) }).ToListAsync(cancellationToken))
            .ToLookup(l => l.InvoiceId, l => l.Line);
        var payments = (await context.Payments.AsNoTracking().Where(p => ids.Contains(p.InvoiceId)).OrderBy(p => p.ReceivedDate).ThenBy(p => p.Id)
                .Select(p => new { p.InvoiceId, Payment = new PaymentDto(p.PublicId, p.Amount, p.CurrencyCode, p.Method, p.Reference, p.ReceivedDate, p.Provider) }).ToListAsync(cancellationToken))
            .ToLookup(p => p.InvoiceId, p => p.Payment);

        return invoices
            .Select(i => new InvoiceDto(
                i.PublicId, i.SchoolPublicId, i.Number, i.Status, i.CurrencyCode, i.Subtotal, i.TaxRate, i.TaxAmount, i.Total,
                i.IssueDate, i.DueDate, i.PeriodStart, i.PeriodEnd, i.PaidDate, lines[i.Id].ToList(), payments[i.Id].ToList()))
            .ToList();
    }
}
