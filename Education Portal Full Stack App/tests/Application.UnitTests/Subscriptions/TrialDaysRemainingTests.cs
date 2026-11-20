using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Application.Subscriptions.Queries.GetMySubscription;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Subscriptions;

/// <summary>The banner's "days left" of a trial: whole days, rounded up, never negative, and only while on the trial.</summary>
public class TrialDaysRemainingTests
{
    private static readonly DateTimeOffset Now = new(2026, 6, 15, 12, 0, 0, TimeSpan.Zero);

    private static SubscriptionDto Subscription(SubscriptionStatus status, DateTimeOffset? trialEnd) => new(
        Guid.NewGuid(),
        "FULL",
        "PK",
        "PKR",
        BillingCycle.Monthly,
        30000,
        status,
        Now.AddDays(-1),
        trialEnd ?? Now.AddDays(29),
        trialEnd,
        []);

    [Test]
    public void ShouldGiveTheWholeDaysLeftOfTheTrial() =>
        GetMySubscriptionQueryHandler.TrialDaysRemaining(Subscription(SubscriptionStatus.Trial, Now.AddDays(30)), Now).ShouldBe(30);

    [Test]
    public void ShouldRoundAPartDayUp()
    {
        GetMySubscriptionQueryHandler.TrialDaysRemaining(Subscription(SubscriptionStatus.Trial, Now.AddHours(36)), Now).ShouldBe(2);
        GetMySubscriptionQueryHandler.TrialDaysRemaining(Subscription(SubscriptionStatus.Trial, Now.AddMinutes(1)), Now).ShouldBe(1);
    }

    [Test]
    public void ShouldNeverGoBelowZero() =>
        GetMySubscriptionQueryHandler.TrialDaysRemaining(Subscription(SubscriptionStatus.Trial, Now.AddDays(-3)), Now).ShouldBe(0);

    [TestCase(SubscriptionStatus.Active)]
    [TestCase(SubscriptionStatus.PastDue)]
    [TestCase(SubscriptionStatus.Suspended)]
    [TestCase(SubscriptionStatus.Cancelled)]
    public void ShouldHaveNoTrialDaysOnceTheSubscriptionIsNotOnTrial(SubscriptionStatus status) =>
        GetMySubscriptionQueryHandler.TrialDaysRemaining(Subscription(status, Now.AddDays(10)), Now).ShouldBeNull();

    [Test]
    public void ShouldHaveNoTrialDaysWithoutASubscription() =>
        GetMySubscriptionQueryHandler.TrialDaysRemaining(null, Now).ShouldBeNull();
}
