namespace EducationPortal.Application.Invoices.Common;

public sealed record InvoiceLineDto(string Description, decimal Amount);

public sealed record PaymentDto(Guid Id, decimal Amount, string CurrencyCode, PaymentMethod Method, string Reference, DateTimeOffset ReceivedDate, string Provider);

/// <summary>An invoice with its lines and payments. Amounts are in <paramref name="CurrencyCode"/>; the tax rate is in percent.</summary>
public sealed record InvoiceDto(
    Guid Id,
    Guid SchoolId,
    string Number,
    InvoiceStatus Status,
    string CurrencyCode,
    decimal Subtotal,
    decimal TaxRate,
    decimal TaxAmount,
    decimal Total,
    DateTimeOffset IssueDate,
    DateTimeOffset DueDate,
    DateTimeOffset PeriodStart,
    DateTimeOffset PeriodEnd,
    DateTimeOffset? PaidDate,
    IReadOnlyList<InvoiceLineDto> Lines,
    IReadOnlyList<PaymentDto> Payments);
