using System.ComponentModel.DataAnnotations;
using EducationPortal.Application.Subscriptions.Lifecycle;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Subscriptions;

/// <summary>Which renewal reminder is due, in UTC days, and which lifecycle settings are accepted (ADR-015).</summary>
public class SubscriptionLifecycleScheduleTests
{
    private static readonly int[] Days = [30, 7, 1];

    [TestCase(45, null)]
    [TestCase(31, null)]
    [TestCase(30, 30)]
    [TestCase(8, 30)]
    [TestCase(7, 7)]
    [TestCase(3, 7)]
    [TestCase(1, 1)]
    [TestCase(0, 1)]
    public void ShouldPickTheNearestReminderDayNotBeforeTheDaysLeft(int daysLeft, int? expected) =>
        SubscriptionLifecycle.ReminderDayFor(daysLeft, Days).ShouldBe(expected);

    [Test]
    public void ShouldCountWholeUtcCalendarDays()
    {
        var now = new DateTimeOffset(2026, 5, 1, 23, 30, 0, TimeSpan.Zero);

        SubscriptionLifecycle.DaysLeft(now, new DateTimeOffset(2026, 5, 2, 0, 15, 0, TimeSpan.Zero)).ShouldBe(1);
        SubscriptionLifecycle.DaysLeft(now, new DateTimeOffset(2026, 5, 8, 0, 0, 0, TimeSpan.Zero)).ShouldBe(7);

        // An offset is converted to UTC first: 02:00 at +05:00 is 21:00 UTC the day before.
        SubscriptionLifecycle.DaysLeft(now, new DateTimeOffset(2026, 5, 2, 2, 0, 0, TimeSpan.FromHours(5))).ShouldBe(0);
    }

    [Test]
    public void ShouldUseSafeDefaults()
    {
        var options = new SubscriptionLifecycleOptions();

        options.ShouldSatisfyAllConditions(
            o => o.Enabled.ShouldBeTrue(),
            o => o.Schedule.ShouldBe("0 0 1 * * ?"),
            o => o.GracePeriodDays.ShouldBe(14),
            o => o.EffectiveReminderDays.ShouldBe([30, 7, 1]));
        IsValid(options).ShouldBeTrue();
    }

    [Test]
    public void ShouldReplaceTheDefaultReminderDaysFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ReminderDays:0"] = "3", ["ReminderDays:1"] = "10" })
            .Build();

        var options = configuration.Get<SubscriptionLifecycleOptions>()!;

        options.EffectiveReminderDays.ShouldBe([10, 3]);
    }

    [TestCase(-1, new[] { 7 })]
    [TestCase(91, new[] { 7 })]
    [TestCase(14, new[] { 0 })]
    [TestCase(14, new[] { 366 })]
    [TestCase(14, new[] { 7, 7 })]
    public void ShouldRefuseBadSettings(int graceDays, int[] reminderDays) =>
        IsValid(new SubscriptionLifecycleOptions { GracePeriodDays = graceDays, ReminderDays = reminderDays }).ShouldBeFalse();

    private static bool IsValid(SubscriptionLifecycleOptions options) =>
        Validator.TryValidateObject(options, new ValidationContext(options), null, validateAllProperties: true);
}
