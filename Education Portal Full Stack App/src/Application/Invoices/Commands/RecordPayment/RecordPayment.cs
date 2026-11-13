using System.Globalization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using FluentValidation.Results;

namespace EducationPortal.Application.Invoices.Commands.RecordPayment;

/// <summary>
/// Records a payment for an open invoice, marks the invoice paid and applies it to the school's subscription: a trial
/// or renewal becomes Active and runs to the end of the invoiced period (its modules' validity is extended), a past-due or
/// suspended subscription becomes Active and its suspended modules come back. The payment must equal the invoice total
/// (no part payments) and goes through the <see cref="IPaymentProvider"/>. Everything is saved in one transaction.
/// Master tenant administrators only (SEC-AUTHZ-07); audited (SEC-LOG-03).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record RecordPaymentCommand : IRequest<Guid>
{
    /// <summary>The invoice's public id, taken from the route (ENG-ID-02).</summary>
    public Guid InvoiceId { get; init; }

    public decimal Amount { get; init; }

    public PaymentMethod Method { get; init; }

    /// <summary>The bank transfer id, cheque number or receipt number.</summary>
    public string Reference { get; init; } = string.Empty;

    /// <summary>When the money was received (UTC).</summary>
    public DateTimeOffset ReceivedDate { get; init; }
}

public class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator(IApplicationDbContext context, TimeProvider timeProvider)
    {
        RuleFor(c => c.InvoiceId).NotEmpty();
        RuleFor(c => c.Method).IsInEnum();
        RuleFor(c => c.Reference).NotEmpty().MaximumLength(Payment.ReferenceMaxLength);
        RuleFor(c => c.Amount).Must(a => a > 0 && PriceBookItem.IsValidPrice(a));

        RuleFor(c => c.ReceivedDate)
            .NotEmpty()
            .Must(date => date <= timeProvider.GetUtcNow())
            .WithMessage(_ => ValidationMessages.PaymentDateInFuture);

        RuleFor(c => c)
            .CustomAsync(async (command, validation, cancellationToken) =>
            {
                var invoice = await context.Invoices.AsNoTracking()
                    .Where(i => i.PublicId == command.InvoiceId)
                    .Select(i => new { i.Status, i.Total })
                    .FirstOrDefaultAsync(cancellationToken);

                if (invoice is null)
                {
                    return; // the handler answers 404
                }

                if (invoice.Status != InvoiceStatus.Open)
                {
                    validation.AddFailure(nameof(RecordPaymentCommand.InvoiceId), ValidationMessages.InvoiceNotOpen);
                }
                else if (command.Amount != invoice.Total)
                {
                    validation.AddFailure(nameof(RecordPaymentCommand.Amount), ValidationMessages.PaymentMustCoverTotal);
                }
            });
    }
}

public class RecordPaymentCommandHandler : IRequestHandler<RecordPaymentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly IPaymentProvider _provider;
    private readonly ISecurityAuditLog _auditLog;
    private readonly IModuleEntitlementStore _entitlements;

    public RecordPaymentCommandHandler(IApplicationDbContext context, IPaymentProvider provider, ISecurityAuditLog auditLog, IModuleEntitlementStore entitlements)
    {
        _context = context;
        _provider = provider;
        _auditLog = auditLog;
        _entitlements = entitlements;
    }

    public async Task<Guid> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.PublicId == request.InvoiceId, cancellationToken);
        Guard.Against.NotFound(request.InvoiceId, invoice);

        var result = await _provider.ChargeAsync(
            new PaymentRequest(invoice.PublicId, request.Amount, invoice.CurrencyCode, request.Method, request.Reference), cancellationToken);
        if (!result.Succeeded)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException([new ValidationFailure(nameof(RecordPaymentCommand.Reference), ValidationMessages.PaymentRejected)]);
        }

        var payment = invoice.RecordPayment(request.Amount, request.Method, result.Reference, request.ReceivedDate, _provider.Name);

        var subscription = await _context.Subscriptions.Include(s => s.Modules).FirstOrDefaultAsync(s => s.Id == invoice.SubscriptionId, cancellationToken);
        Guard.Against.NotFound(invoice.SubscriptionId, subscription);
        subscription.ApplyPayment(invoice);

        // One save: the payment, the paid invoice and the subscription change are stored together or not at all.
        await _context.SaveChangesAsync(cancellationToken);

        // The school's modules may have changed, so its cached entitlements are stale.
        _entitlements.Invalidate(invoice.SchoolId);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.PaymentRecorded,
            SecurityAuditOutcome.Success,
            targetType: nameof(Payment),
            targetId: payment.Id.ToString(CultureInfo.InvariantCulture));

        return payment.PublicId;
    }
}
