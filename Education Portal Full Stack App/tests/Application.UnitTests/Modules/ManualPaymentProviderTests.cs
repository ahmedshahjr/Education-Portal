using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Invoices.Common;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Modules;

/// <summary>The manual payment provider (ADR-013): the master tenant has the money already, so it accepts any payment that has a reference.</summary>
public class ManualPaymentProviderTests
{
    private readonly ManualPaymentProvider _provider = new();

    private static PaymentRequest Request(decimal amount = 10m, string reference = "TRX-1") =>
        new(Guid.NewGuid(), amount, "PKR", PaymentMethod.BankTransfer, reference);

    [Test]
    public async Task ShouldAcceptAPaymentAndKeepTheTrimmedReference()
    {
        var result = await _provider.ChargeAsync(Request(reference: " TRX-1 "), CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
        result.Reference.ShouldBe("TRX-1");
        _provider.Name.ShouldBe("Manual");
    }

    [TestCase("")]
    [TestCase("  ")]
    public async Task ShouldRefuseAPaymentWithoutAReference(string reference) =>
        (await _provider.ChargeAsync(Request(reference: reference), CancellationToken.None)).Succeeded.ShouldBeFalse();

    [TestCase(0)]
    [TestCase(-5)]
    public async Task ShouldRefuseAPaymentOfNothing(double amount) =>
        (await _provider.ChargeAsync(Request(amount: (decimal)amount), CancellationToken.None)).Succeeded.ShouldBeFalse();

    [Test]
    public async Task ShouldRefuseANullRequest() =>
        await Should.ThrowAsync<ArgumentNullException>(() => _provider.ChargeAsync(null!, CancellationToken.None));
}
