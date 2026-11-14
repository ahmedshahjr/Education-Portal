using EducationPortal.Domain.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class InvoiceTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 31, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Now = new(2026, 2, 1, 8, 0, 0, TimeSpan.Zero);

    private static T Saved<T>(T entity, int id) where T : BaseEntity
    {
        entity.Id = id;
        return entity;
    }

    private static Subscription Subscription(int trialDays = 0, BillingCycle cycle = BillingCycle.Monthly, decimal amount = 100m) =>
        Saved(Domain.Entities.Subscription.Create(9, null, Saved(PriceBook.Create("PK", "PKR"), 3), cycle, amount, Start, trialDays), 5);

    private static Invoice Invoice(decimal taxRate = 15m, bool next = false) => Domain.Entities.Invoice.Create(Subscription(), taxRate, Now, next, "BASIC");

    [Test]
    public void ShouldInvoiceTheCurrentPeriodWithTaxAndACopyOfTheCurrency()
    {
        var invoice = Invoice();

        invoice.ShouldSatisfyAllConditions(
            i => i.SchoolId.ShouldBe(9),
            i => i.SubscriptionId.ShouldBe(5),
            i => i.Status.ShouldBe(InvoiceStatus.Open),
            i => i.CurrencyCode.ShouldBe("PKR"),
            i => i.Subtotal.ShouldBe(100m),
            i => i.TaxRate.ShouldBe(15m),
            i => i.TaxAmount.ShouldBe(15m),
            i => i.Total.ShouldBe(115m),
            i => i.PeriodStart.ShouldBe(Start),
            i => i.PeriodEnd.ShouldBe(new DateTimeOffset(2026, 2, 28, 10, 0, 0, TimeSpan.Zero)),
            i => i.IssueDate.ShouldBe(Now),
            i => i.DueDate.ShouldBe(Now.AddDays(Domain.Entities.Invoice.PaymentTermDays)),
            i => i.PaidDate.ShouldBeNull());
        invoice.Lines.ShouldHaveSingleItem().ShouldSatisfyAllConditions(l => l.Amount.ShouldBe(100m), l => l.Description.ShouldBe("BASIC"), l => l.SchoolId.ShouldBe(9));
    }

    [Test]
    public void ShouldInvoiceTheNextPeriodFromTheCurrentEnd()
    {
        var subscription = Subscription(cycle: BillingCycle.Yearly);

        var invoice = Domain.Entities.Invoice.Create(subscription, 0m, Now, nextPeriod: true, "BASIC");

        invoice.PeriodStart.ShouldBe(subscription.EndDate);
        invoice.PeriodEnd.ShouldBe(subscription.EndDate.AddYears(1));
    }

    [TestCase(0, 100.00, 100.00)]
    [TestCase(15, 33.33, 38.33)] // 4.9995 rounds away from zero to 5.00
    [TestCase(5.5, 100, 105.50)]
    [TestCase(100, 100, 200)]
    public void ShouldRoundTaxToTwoDecimalsAwayFromZero(double rate, double amount, double total)
    {
        var invoice = Domain.Entities.Invoice.Create(Subscription(amount: (decimal)amount), (decimal)rate, Now, false, "X");

        invoice.Total.ShouldBe((decimal)total);
        invoice.Total.ShouldBe(invoice.Subtotal + invoice.TaxAmount);
    }

    [Test]
    public void ShouldRefuseATrialsCurrentPeriodButInvoiceItsNextOne()
    {
        var trial = Subscription(trialDays: 14);

        Should.Throw<InvalidOperationException>(() => Domain.Entities.Invoice.Create(trial, 0m, Now, false, "X"));
        Domain.Entities.Invoice.Create(trial, 0m, Now, true, "X").PeriodStart.ShouldBe(trial.EndDate);
    }

    [TestCase(-0.01)]
    [TestCase(100.01)]
    [TestCase(1.005)]
    public void ShouldRefuseABadTaxRate(double rate) =>
        Should.Throw<ArgumentOutOfRangeException>(() => Domain.Entities.Invoice.Create(Subscription(), (decimal)rate, Now, false, "X"));

    [TestCase("")]
    [TestCase("  ")]
    public void ShouldRefuseAnEmptyDescription(string description) =>
        Should.Throw<ArgumentException>(() => Domain.Entities.Invoice.Create(Subscription(), 0m, Now, false, description));

    [Test]
    public void ShouldRefuseAnUnsavedSubscription() =>
        Should.Throw<ArgumentOutOfRangeException>(() => Domain.Entities.Invoice.Create(Domain.Entities.Subscription.Create(9, null, Saved(PriceBook.Create("PK", "PKR"), 3), BillingCycle.Monthly, 1m, Start, 0), 0m, Now, false, "X"));

    [Test]
    public void ShouldTakeOnePaymentForTheWholeTotalAndBecomePaid()
    {
        var invoice = Invoice();
        var received = Now.AddDays(1);

        var payment = invoice.RecordPayment(115m, PaymentMethod.BankTransfer, " TRX-9 ", received, "Manual");

        invoice.ShouldSatisfyAllConditions(i => i.Status.ShouldBe(InvoiceStatus.Paid), i => i.PaidDate.ShouldBe(received), i => i.Payments.ShouldHaveSingleItem());
        payment.ShouldSatisfyAllConditions(
            p => p.Amount.ShouldBe(115m),
            p => p.CurrencyCode.ShouldBe("PKR"),
            p => p.SchoolId.ShouldBe(9),
            p => p.Reference.ShouldBe("TRX-9"),
            p => p.Method.ShouldBe(PaymentMethod.BankTransfer),
            p => p.Provider.ShouldBe("Manual"));
    }

    [TestCase(114.99)]
    [TestCase(115.01)]
    [TestCase(0)]
    public void ShouldRefuseAnAmountThatIsNotTheTotal(double amount) =>
        Should.Throw<ArgumentException>(() => Invoice().RecordPayment((decimal)amount, PaymentMethod.Cash, "R", Now, "Manual"));

    [Test]
    public void ShouldRefuseASecondPaymentAndPayingAVoidedInvoice()
    {
        var paid = Invoice();
        paid.RecordPayment(115m, PaymentMethod.Cash, "R", Now, "Manual");
        var voided = Invoice();
        voided.Void();

        Should.Throw<InvalidOperationException>(() => paid.RecordPayment(115m, PaymentMethod.Cash, "R", Now, "Manual"));
        Should.Throw<InvalidOperationException>(() => voided.RecordPayment(115m, PaymentMethod.Cash, "R", Now, "Manual"));
        Should.Throw<InvalidOperationException>(paid.Void);
        Should.Throw<InvalidOperationException>(voided.Void);
    }

    [TestCase("")]
    [TestCase(" ")]
    public void ShouldRefuseAPaymentWithoutAReference(string reference) =>
        Should.Throw<ArgumentException>(() => Invoice().RecordPayment(115m, PaymentMethod.Cash, reference, Now, "Manual"));

    [Test]
    public void ShouldLimitTheReferenceAndRefuseAnUnknownMethod()
    {
        Invoice().RecordPayment(115m, PaymentMethod.Cash, new string('r', Payment.ReferenceMaxLength), Now, "Manual");

        Should.Throw<ArgumentOutOfRangeException>(() => Invoice().RecordPayment(115m, PaymentMethod.Cash, new string('r', Payment.ReferenceMaxLength + 1), Now, "Manual"));
        Should.Throw<ArgumentOutOfRangeException>(() => Invoice().RecordPayment(115m, (PaymentMethod)99, "R", Now, "Manual"));
    }

    [Test]
    public void ShouldVoidAnOpenInvoice()
    {
        var invoice = Invoice();

        invoice.Void();

        invoice.Status.ShouldBe(InvoiceStatus.Void);
    }
}

public class SubscriptionPaymentTests
{
    private static readonly DateTimeOffset Start = new(2026, 1, 15, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Now = new(2026, 1, 16, 8, 0, 0, TimeSpan.Zero);

    private static T Saved<T>(T entity, int id) where T : BaseEntity
    {
        entity.Id = id;
        return entity;
    }

    private static Subscription Subscription(int trialDays)
    {
        var subscription = Saved(Domain.Entities.Subscription.Create(9, null, Saved(PriceBook.Create("PK", "PKR"), 3), BillingCycle.Monthly, 100m, Start, trialDays), 5);
        subscription.AddModule(Saved(PortalModule.Create("FEES", "Fees", "", false, 0), 7), TestBranches.MainBranch());
        return subscription;
    }

    private static Invoice Paid(Subscription subscription, bool next)
    {
        var invoice = Saved(Invoice.Create(subscription, 0m, Now, next, "X"), 1);
        invoice.RecordPayment(100m, PaymentMethod.Cash, "R", Now, "Manual");
        return invoice;
    }

    [Test]
    public void ShouldActivateATrialAndExtendEveryModuleToTheEndOfThePaidPeriod()
    {
        var subscription = Subscription(trialDays: 14);
        var invoice = Paid(subscription, next: true);

        subscription.ApplyPayment(invoice);

        subscription.ShouldSatisfyAllConditions(s => s.Status.ShouldBe(SubscriptionStatus.Active), s => s.EndDate.ShouldBe(invoice.PeriodEnd));
        subscription.Modules.Single().ValidTo.ShouldBe(invoice.PeriodEnd);
    }

    [Test]
    public void ShouldChangeNoDateWhenTheCurrentPeriodIsPaid()
    {
        var subscription = Subscription(trialDays: 0);
        var end = subscription.EndDate;

        subscription.ApplyPayment(Paid(subscription, next: false));

        subscription.ShouldSatisfyAllConditions(s => s.Status.ShouldBe(SubscriptionStatus.Active), s => s.EndDate.ShouldBe(end));
        subscription.Modules.Single().ValidTo.ShouldBe(end);
    }

    [Test]
    public void ShouldReactivateSuspendedModulesOfALapsedSubscription()
    {
        var subscription = Subscription(trialDays: 0);
        typeof(Subscription).GetProperty(nameof(Domain.Entities.Subscription.Status))!.SetValue(subscription, SubscriptionStatus.Suspended);
        subscription.Modules.Single().Suspend();

        subscription.ApplyPayment(Paid(subscription, next: false));

        subscription.Status.ShouldBe(SubscriptionStatus.Active);
        subscription.Modules.Single().Status.ShouldBe(TenantModuleStatus.Active);
    }

    [Test]
    public void ShouldLeaveASuspendedModuleOffWhenAnActiveSubscriptionIsRenewed()
    {
        var subscription = Subscription(trialDays: 0);
        subscription.Modules.Single().Suspend();

        subscription.ApplyPayment(Paid(subscription, next: true));

        subscription.Modules.Single().Status.ShouldBe(TenantModuleStatus.Suspended);
    }

    [Test]
    public void ShouldNeverChangeACancelledSubscription()
    {
        var subscription = Subscription(trialDays: 0);
        var invoice = Paid(subscription, next: true);
        typeof(Subscription).GetProperty(nameof(Domain.Entities.Subscription.Status))!.SetValue(subscription, SubscriptionStatus.Cancelled);
        var end = subscription.EndDate;

        subscription.ApplyPayment(invoice);

        subscription.ShouldSatisfyAllConditions(s => s.Status.ShouldBe(SubscriptionStatus.Cancelled), s => s.EndDate.ShouldBe(end));
    }

    [Test]
    public void ShouldRefuseAnOpenOrForeignInvoice()
    {
        var subscription = Subscription(trialDays: 0);
        var open = Saved(Invoice.Create(subscription, 0m, Now, false, "X"), 1);
        var other = Saved(Domain.Entities.Subscription.Create(9, null, Saved(PriceBook.Create("PK", "PKR"), 3), BillingCycle.Monthly, 100m, Start, 0), 6);
        var foreign = Saved(Invoice.Create(other, 0m, Now, false, "X"), 2);
        foreign.RecordPayment(100m, PaymentMethod.Cash, "R", Now, "Manual");

        Should.Throw<ArgumentException>(() => subscription.ApplyPayment(open));
        Should.Throw<ArgumentException>(() => subscription.ApplyPayment(foreign));
    }

    [Test]
    public void ShouldNeverShortenAModulesValidity()
    {
        var subscription = Subscription(trialDays: 0);
        var module = subscription.Modules.Single();
        var end = module.ValidTo;

        module.ExtendTo(end!.Value.AddDays(-1));

        module.ValidTo.ShouldBe(end);
    }
}

public class PriceBookTaxRateTests
{
    [Test]
    public void ShouldStartWithNoTax() => PriceBook.Create("PK", "PKR").TaxRate.ShouldBe(0m);

    [TestCase(0, true)]
    [TestCase(100, true)]
    [TestCase(15.25, true)]
    [TestCase(-0.01, false)]
    [TestCase(100.01, false)]
    [TestCase(1.234, false)]
    public void ShouldAcceptAPercentageFromZeroToOneHundredWithTwoDecimals(double rate, bool valid)
    {
        var book = PriceBook.Create("PK", "PKR");

        if (valid)
        {
            book.SetTaxRate((decimal)rate);
            book.TaxRate.ShouldBe((decimal)rate);
        }
        else
        {
            Should.Throw<ArgumentOutOfRangeException>(() => book.SetTaxRate((decimal)rate));
            book.TaxRate.ShouldBe(0m);
        }
    }
}

public class SchoolTaxRateOverrideTests
{
    [Test]
    public void ShouldStartWithoutAnOverride() => School.Create("School", "school.example").TaxRateOverride.ShouldBeNull();

    [TestCase(0, true)]
    [TestCase(100, true)]
    [TestCase(7.5, true)]
    [TestCase(-0.01, false)]
    [TestCase(100.01, false)]
    [TestCase(1.234, false)]
    public void ShouldAcceptAPercentageFromZeroToOneHundredWithTwoDecimals(double rate, bool valid)
    {
        var school = School.Create("School", "school.example");

        if (valid)
        {
            school.SetTaxRateOverride((decimal)rate);
            school.TaxRateOverride.ShouldBe((decimal)rate);
        }
        else
        {
            Should.Throw<ArgumentOutOfRangeException>(() => school.SetTaxRateOverride((decimal)rate));
            school.TaxRateOverride.ShouldBeNull();
        }
    }

    [Test]
    public void ShouldClearTheOverride()
    {
        var school = School.Create("School", "school.example");
        school.SetTaxRateOverride(5m);

        school.SetTaxRateOverride(null);

        school.TaxRateOverride.ShouldBeNull();
    }
}
