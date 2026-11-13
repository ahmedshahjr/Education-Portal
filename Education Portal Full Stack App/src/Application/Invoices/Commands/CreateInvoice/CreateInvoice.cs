using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Invoices.Commands.CreateInvoice;

/// <summary>
/// Issues an invoice for a school's subscription and returns its id: for the current period, or for the next one
/// (<see cref="NextPeriod"/>, which is the only choice while the subscription is on trial). The amount is the
/// subscription's price and the tax rate is the school's own override or else its country's price-book rate; both are copied onto the invoice. A
/// subscription has at most one open invoice, and a period is invoiced once. Master tenant administrators only
/// (SEC-AUTHZ-07), so the school id may come from the body (SEC-AUTHZ-04). Audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record CreateInvoiceCommand : IRequest<Guid>
{
    /// <summary>The school's public id (ENG-ID-02).</summary>
    public Guid SchoolId { get; init; }

    /// <summary>True to invoice the period after the current one (a renewal, or the first paid period after a trial).</summary>
    public bool NextPeriod { get; init; }
}

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator(IApplicationDbContext context, TimeProvider timeProvider)
    {
        RuleFor(c => c.SchoolId).NotEmpty();

        RuleFor(c => c)
            .CustomAsync(async (command, validation, cancellationToken) =>
            {
                var subscription = await (
                        from s in context.Subscriptions.AsNoTracking()
                        join school in context.Schools on s.SchoolId equals school.Id
                        where school.PublicId == command.SchoolId && s.Status != SubscriptionStatus.Cancelled
                        select new { s.Id, s.Status, s.StartDate, s.EndDate, s.BillingCycle })
                    .FirstOrDefaultAsync(cancellationToken);

                if (subscription is null)
                {
                    validation.AddFailure(nameof(CreateInvoiceCommand.SchoolId), ValidationMessages.SchoolHasNoSubscription);
                    return;
                }

                if (subscription.Status == SubscriptionStatus.Trial && !command.NextPeriod)
                {
                    validation.AddFailure(nameof(CreateInvoiceCommand.NextPeriod), ValidationMessages.TrialNeedsNextPeriod);
                }

                if (await context.Invoices.AnyAsync(i => i.SubscriptionId == subscription.Id && i.Status == InvoiceStatus.Open, cancellationToken))
                {
                    validation.AddFailure(nameof(CreateInvoiceCommand.SchoolId), ValidationMessages.InvoiceOpenExists);
                    return;
                }

                var periodStart = command.NextPeriod ? Subscription.RenewalStart(subscription.EndDate, timeProvider.GetUtcNow()) : subscription.StartDate;
                if (await context.Invoices.AnyAsync(i => i.SubscriptionId == subscription.Id && i.PeriodStart == periodStart && i.Status != InvoiceStatus.Void, cancellationToken))
                {
                    validation.AddFailure(nameof(CreateInvoiceCommand.NextPeriod), ValidationMessages.PeriodAlreadyInvoiced);
                }
            });
    }
}

public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly TimeProvider _timeProvider;

    public CreateInvoiceCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, TimeProvider timeProvider)
    {
        _context = context;
        _auditLog = auditLog;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var schoolId = await _context.Schools.ResolveIdAsync(request.SchoolId, cancellationToken);

        var subscription = await _context.Subscriptions.FirstOrDefaultAsync(s => s.SchoolId == schoolId && s.Status != SubscriptionStatus.Cancelled, cancellationToken);
        Guard.Against.NotFound(request.SchoolId, subscription);

        // The school's own rate wins over its country's.
        var bookRate = await _context.PriceBooks.Where(b => b.Id == subscription.PriceBookId).Select(b => (decimal?)b.TaxRate).FirstOrDefaultAsync(cancellationToken);
        var schoolRate = await _context.Schools.Where(s => s.Id == schoolId).Select(s => s.TaxRateOverride).FirstOrDefaultAsync(cancellationToken);
        var taxRate = schoolRate ?? bookRate;
        Guard.Against.Null(taxRate);

        var planCode = await _context.Plans.Where(p => p.Id == subscription.PlanId).Select(p => p.Code).FirstOrDefaultAsync(cancellationToken);

        var now = _timeProvider.GetUtcNow();
        var invoice = Invoice.Create(subscription, taxRate.Value, now, request.NextPeriod, Describe(planCode, subscription, request.NextPeriod, now));
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.InvoiceCreated,
            SecurityAuditOutcome.Success,
            targetType: nameof(Invoice),
            targetId: invoice.Id.ToString(CultureInfo.InvariantCulture));

        return invoice.PublicId;
    }

    // Language-neutral on purpose: plan code, cycle and the dates, no sentence to translate.
    private static string Describe(string? planCode, Subscription subscription, bool nextPeriod, DateTimeOffset now)
    {
        // The same period as Invoice.Create: a lapsed subscription restarts on the issue date (ADR-015).
        var start = nextPeriod ? Subscription.RenewalStart(subscription.EndDate, now) : subscription.StartDate;
        var end = nextPeriod ? Subscription.PeriodEnd(start, subscription.BillingCycle) : subscription.EndDate;
        return string.Create(CultureInfo.InvariantCulture, $"{planCode ?? "ADD-ONS"} · {subscription.BillingCycle} · {start:yyyy-MM-dd} – {end:yyyy-MM-dd}");
    }
}
