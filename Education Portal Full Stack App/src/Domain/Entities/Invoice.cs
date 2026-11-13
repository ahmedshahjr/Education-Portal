namespace EducationPortal.Domain.Entities;

/// <summary>
/// A bill for one period of a school's subscription. The amounts, currency and tax rate are copied when it is issued, so
/// a later price or tax change never alters it (money is <c>decimal</c> with its currency next to it). It is tenant-owned
/// (<c>SchoolId</c>, tenant query filter, SEC-AUTHZ-03); only the master tenant issues and settles invoices. The number
/// (<c>INV-000123</c>) comes from a database sequence, so it is unique but not gap-free.
/// </summary>
public class Invoice : BaseAuditableEntity
{
    public const int PaymentTermDays = 14;
    public const int NumberMaxLength = 20;

    private readonly List<InvoiceLine> _lines = [];
    private readonly List<Payment> _payments = [];

    private Invoice()
    {
        // For EF Core.
        Number = string.Empty;
        CurrencyCode = string.Empty;
    }

    private Invoice(Subscription subscription, decimal taxRate, DateTimeOffset issueDate, bool nextPeriod, string description) : this()
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(subscription.Id);

        if (subscription.Status == SubscriptionStatus.Cancelled)
        {
            throw new InvalidOperationException("A cancelled subscription cannot be invoiced.");
        }

        if (subscription.Status == SubscriptionStatus.Trial && !nextPeriod)
        {
            throw new InvalidOperationException("A trial has no paid period yet; invoice the next period.");
        }

        if (!PriceBook.IsValidTaxRate(taxRate))
        {
            throw new ArgumentOutOfRangeException(nameof(taxRate), taxRate, "A tax rate is a percentage from 0 to 100 with at most two decimal places.");
        }

        SchoolId = subscription.SchoolId;
        SubscriptionId = subscription.Id;
        CurrencyCode = subscription.CurrencyCode;
        PeriodStart = nextPeriod ? subscription.EndDate : subscription.StartDate;
        PeriodEnd = nextPeriod ? Subscription.PeriodEnd(subscription.EndDate, subscription.BillingCycle) : subscription.EndDate;
        Subtotal = subscription.Amount;
        TaxRate = taxRate;
        TaxAmount = decimal.Round(Subtotal * taxRate / 100m, PriceBookItem.PriceScale, MidpointRounding.AwayFromZero);
        Total = Subtotal + TaxAmount;
        IssueDate = issueDate;
        DueDate = issueDate.AddDays(PaymentTermDays);
        Status = InvoiceStatus.Open;

        _lines.Add(InvoiceLine.Create(this, description, Subtotal));
    }

    /// <summary>
    /// Issues an invoice for the subscription's current period (<paramref name="nextPeriod"/> false) or for the period
    /// that follows it (true). The line text names what is billed.
    /// </summary>
    public static Invoice Create(Subscription subscription, decimal taxRate, DateTimeOffset issueDate, bool nextPeriod, string description)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new Invoice(subscription, taxRate, issueDate, nextPeriod, description);
    }

    public int SchoolId { get; private set; }

    public int SubscriptionId { get; private set; }

    /// <summary>Assigned by the database when the invoice is saved.</summary>
    public string Number { get; private set; }

    public InvoiceStatus Status { get; private set; }

    public string CurrencyCode { get; private set; }

    public decimal Subtotal { get; private set; }

    /// <summary>The tax rate in percent that was in force when the invoice was issued.</summary>
    public decimal TaxRate { get; private set; }

    public decimal TaxAmount { get; private set; }

    public decimal Total { get; private set; }

    public DateTimeOffset IssueDate { get; private set; }

    public DateTimeOffset DueDate { get; private set; }

    public DateTimeOffset PeriodStart { get; private set; }

    public DateTimeOffset PeriodEnd { get; private set; }

    public DateTimeOffset? PaidDate { get; private set; }

    public IReadOnlyCollection<InvoiceLine> Lines => _lines;

    public IReadOnlyCollection<Payment> Payments => _payments;

    /// <summary>
    /// Records a payment that covers the whole total and marks the invoice paid. Part payments are not supported: the
    /// amount must equal the total and the invoice must be open.
    /// </summary>
    public Payment RecordPayment(decimal amount, PaymentMethod method, string reference, DateTimeOffset receivedDate, string provider)
    {
        if (Status != InvoiceStatus.Open)
        {
            throw new InvalidOperationException("Only an open invoice can be paid.");
        }

        if (amount != Total)
        {
            throw new ArgumentException("A payment covers the whole invoice total.", nameof(amount));
        }

        var payment = Payment.Create(this, amount, method, reference, receivedDate, provider);
        _payments.Add(payment);
        Status = InvoiceStatus.Paid;
        PaidDate = receivedDate;
        return payment;
    }

    /// <summary>Cancels an open invoice. A paid invoice cannot be voided.</summary>
    public void Void()
    {
        if (Status != InvoiceStatus.Open)
        {
            throw new InvalidOperationException("Only an open invoice can be voided.");
        }

        Status = InvoiceStatus.Void;
    }
}
