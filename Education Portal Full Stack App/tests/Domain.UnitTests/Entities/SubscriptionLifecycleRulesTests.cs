using EducationPortal.Domain.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

/// <summary>The domain rules the daily lifecycle job uses (ADR-015): each changes state once and then does nothing.</summary>
public class SubscriptionLifecycleRulesTests
{
    private static readonly DateTimeOffset Start = new(2026, 3, 1, 10, 0, 0, TimeSpan.Zero);

    private static T Saved<T>(T entity, int id) where T : BaseEntity
    {
        entity.Id = id;
        return entity;
    }

    private static Subscription NewSubscription(int trialDays = 0)
    {
        var subscription = Saved(Subscription.Create(9, null, Saved(PriceBook.Create("PK", "PKR"), 3), BillingCycle.Monthly, 100m, Start, trialDays), 5);
        subscription.AddModule(Saved(PortalModule.Create("STUDENTS", "Students", "", isCore: false, sortOrder: 0), 7), TestBranches.MainBranch());
        return subscription;
    }

    [Test]
    public void ShouldEndATrialOnlyOnceItsEndDateHasPassed()
    {
        var subscription = NewSubscription(trialDays: 14);

        subscription.EndTrial(Start.AddDays(13)).ShouldBeFalse();
        subscription.Status.ShouldBe(SubscriptionStatus.Trial);

        subscription.EndTrial(Start.AddDays(14)).ShouldBeTrue();
        subscription.ShouldSatisfyAllConditions(
            s => s.Status.ShouldBe(SubscriptionStatus.PastDue),
            s => s.PastDueSince.ShouldBe(Start.AddDays(14)),
            s => s.Modules.ShouldAllBe(m => m.Status == TenantModuleStatus.Active));

        subscription.EndTrial(Start.AddDays(15)).ShouldBeFalse();
    }

    [Test]
    public void ShouldNotEndAnActiveSubscriptionAsATrial() =>
        NewSubscription().EndTrial(Start.AddYears(1)).ShouldBeFalse();

    [Test]
    public void ShouldMarkOnlyAnActiveSubscriptionPastDue()
    {
        var subscription = NewSubscription();

        subscription.MarkPastDue(Start.AddDays(30)).ShouldBeTrue();
        subscription.PastDueSince.ShouldBe(Start.AddDays(30));

        // Already past due: the first date stays, so the grace period is not restarted.
        subscription.MarkPastDue(Start.AddDays(40)).ShouldBeFalse();
        subscription.PastDueSince.ShouldBe(Start.AddDays(30));
    }

    [Test]
    public void ShouldSuspendOnlyAfterTheGracePeriodAndKeepTheModules()
    {
        var subscription = NewSubscription();
        var lapsed = Start.AddDays(31);
        subscription.MarkPastDue(lapsed);

        subscription.SuspendAfterGrace(lapsed.AddDays(30).AddTicks(-1), graceDays: 30).ShouldBeFalse();
        subscription.Status.ShouldBe(SubscriptionStatus.PastDue);

        subscription.SuspendAfterGrace(lapsed.AddDays(30), graceDays: 30).ShouldBeTrue();
        subscription.ShouldSatisfyAllConditions(
            s => s.Status.ShouldBe(SubscriptionStatus.Suspended),
            s => s.Modules.ShouldHaveSingleItem().Status.ShouldBe(TenantModuleStatus.Suspended));

        subscription.SuspendAfterGrace(lapsed.AddDays(40), graceDays: 30).ShouldBeFalse();
    }

    [Test]
    public void ShouldNotSuspendASubscriptionInGoodStanding() =>
        NewSubscription().SuspendAfterGrace(Start.AddYears(1), graceDays: 0).ShouldBeFalse();

    [Test]
    public void ShouldClearThePastDueDateWhenReactivated()
    {
        var subscription = NewSubscription();
        subscription.MarkPastDue(Start);

        subscription.Reactivate();

        subscription.ShouldSatisfyAllConditions(s => s.Status.ShouldBe(SubscriptionStatus.Active), s => s.PastDueSince.ShouldBeNull());
    }

    [Test]
    public void ShouldContinueARenewalFromTheEndUnlessThatEndHasPassed()
    {
        var end = new DateTimeOffset(2026, 4, 1, 0, 0, 0, TimeSpan.Zero);

        Subscription.RenewalStart(end, end.AddDays(-5)).ShouldBe(end);
        Subscription.RenewalStart(end, end).ShouldBe(end);
        Subscription.RenewalStart(end, end.AddDays(20)).ShouldBe(end.AddDays(20));
    }

    [Test]
    public void ShouldInvoiceALapsedRenewalFromTheIssueDate()
    {
        var subscription = NewSubscription();
        var issued = subscription.EndDate.AddDays(20);

        var invoice = Invoice.Create(subscription, 0m, issued, nextPeriod: true, "renewal");

        invoice.ShouldSatisfyAllConditions(
            i => i.PeriodStart.ShouldBe(issued),
            i => i.PeriodEnd.ShouldBe(issued.AddMonths(1)));
    }

    [Test]
    public void ShouldMarkAnOpenInvoiceOverdueOnceAfterItsDueDate()
    {
        var invoice = Invoice.Create(NewSubscription(), 0m, Start, nextPeriod: false, "first");

        invoice.MarkOverdue(invoice.DueDate.AddTicks(-1)).ShouldBeFalse();
        invoice.IsOverdue.ShouldBeFalse();

        invoice.MarkOverdue(invoice.DueDate.AddHours(1)).ShouldBeTrue();
        invoice.ShouldSatisfyAllConditions(
            i => i.IsOverdue.ShouldBeTrue(),
            i => i.Status.ShouldBe(InvoiceStatus.Open),
            i => i.OverdueSince.ShouldBe(invoice.DueDate.AddHours(1)));

        invoice.MarkOverdue(invoice.DueDate.AddDays(2)).ShouldBeFalse();
        invoice.OverdueSince.ShouldBe(invoice.DueDate.AddHours(1));
    }

    [Test]
    public void ShouldNotMarkAVoidInvoiceOverdue()
    {
        var invoice = Invoice.Create(NewSubscription(), 0m, Start, nextPeriod: false, "first");
        invoice.Void();

        invoice.MarkOverdue(invoice.DueDate.AddDays(1)).ShouldBeFalse();
    }

    [Test]
    public void ShouldQueueAReminderForTheCurrentPeriodEnd()
    {
        var subscription = NewSubscription();

        var reminder = RenewalReminder.Queue(subscription, 7, Start);

        reminder.ShouldSatisfyAllConditions(
            r => r.SchoolId.ShouldBe(9),
            r => r.SubscriptionId.ShouldBe(5),
            r => r.PeriodEnd.ShouldBe(subscription.EndDate),
            r => r.DaysBefore.ShouldBe(7),
            r => r.SentAt.ShouldBeNull());
    }

    [TestCase(0)]
    [TestCase(RenewalReminder.MaxDaysBefore + 1)]
    public void ShouldRefuseAReminderDayOutOfRange(int days) =>
        Should.Throw<ArgumentOutOfRangeException>(() => RenewalReminder.Queue(NewSubscription(), days, Start));
}
