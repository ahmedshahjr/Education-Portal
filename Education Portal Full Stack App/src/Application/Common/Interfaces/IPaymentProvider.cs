namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// What a payment source must do for the billing code: accept or refuse a payment for an invoice and say under which
/// reference it was taken. The only implementation today is <c>ManualPaymentProvider</c> (the master tenant confirms a
/// bank transfer, cash or cheque). A card gateway would be another implementation; it must keep card data off our
/// servers (PCI DSS skill, SEC-PCI-02) and needs a merchant-model decision first.
/// </summary>
public interface IPaymentProvider
{
    /// <summary>A short stable name stored with each payment, for example <c>Manual</c>.</summary>
    string Name { get; }

    Task<PaymentResult> ChargeAsync(PaymentRequest request, CancellationToken cancellationToken);
}

/// <summary>A payment to take: the invoice (by public id), the amount and currency, the method and the reference given.</summary>
public sealed record PaymentRequest(Guid InvoiceId, decimal Amount, string CurrencyCode, PaymentMethod Method, string Reference);

/// <summary>The provider's answer: accepted with the reference to store, or refused.</summary>
public sealed record PaymentResult(bool Succeeded, string Reference)
{
    public static PaymentResult Accepted(string reference) => new(true, reference);

    public static PaymentResult Refused() => new(false, string.Empty);
}
