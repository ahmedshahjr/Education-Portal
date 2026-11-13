namespace EducationPortal.Domain.Entities;

/// <summary>
/// A payment the master tenant recorded against an invoice: the amount and currency (copied from the invoice), how it
/// arrived, the bank or receipt reference and when it was received. Tenant-owned (SEC-AUTHZ-03). No card data is ever
/// stored (PCI scope stays out; there is no gateway in this phase).
/// </summary>
public class Payment : BaseAuditableEntity
{
    public const int ReferenceMaxLength = 100;
    public const int ProviderMaxLength = 50;

    private Payment()
    {
        // For EF Core.
        CurrencyCode = string.Empty;
        Reference = string.Empty;
        Provider = string.Empty;
    }

    internal static Payment Create(Invoice invoice, decimal amount, PaymentMethod method, string reference, DateTimeOffset receivedDate, string provider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reference);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(reference.Trim().Length, ReferenceMaxLength, nameof(reference));
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(provider.Length, ProviderMaxLength, nameof(provider));

        if (!Enum.IsDefined(method))
        {
            throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown payment method.");
        }

        if (amount <= 0 || !PriceBookItem.IsValidPrice(amount))
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "A payment is more than zero, with at most two decimal places.");
        }

        return new Payment
        {
            Invoice = invoice,
            SchoolId = invoice.SchoolId,
            CurrencyCode = invoice.CurrencyCode,
            Amount = amount,
            Method = method,
            Reference = reference.Trim(),
            ReceivedDate = receivedDate,
            Provider = provider,
        };
    }

    public int SchoolId { get; private set; }

    public int InvoiceId { get; private set; }

    public Invoice? Invoice { get; private set; }

    public decimal Amount { get; private set; }

    public string CurrencyCode { get; private set; }

    public PaymentMethod Method { get; private set; }

    /// <summary>The bank transfer id, cheque number or receipt number the master tenant typed in.</summary>
    public string Reference { get; private set; }

    public DateTimeOffset ReceivedDate { get; private set; }

    /// <summary>The name of the <c>IPaymentProvider</c> that accepted the payment, for example <c>Manual</c>.</summary>
    public string Provider { get; private set; }
}
