using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.Invoices.Common;

/// <summary>
/// Manual payments: the master tenant's administrator has already received the money (bank transfer, cash, cheque,
/// wallet) and types in the reference. Nothing is charged here, so the provider accepts any payment that has a
/// reference.
/// </summary>
public sealed class ManualPaymentProvider : IPaymentProvider
{
    public const string ProviderName = "Manual";

    public string Name => ProviderName;

    public Task<PaymentResult> ChargeAsync(PaymentRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = string.IsNullOrWhiteSpace(request.Reference) || request.Amount <= 0
            ? PaymentResult.Refused()
            : PaymentResult.Accepted(request.Reference.Trim());

        return Task.FromResult(result);
    }
}
