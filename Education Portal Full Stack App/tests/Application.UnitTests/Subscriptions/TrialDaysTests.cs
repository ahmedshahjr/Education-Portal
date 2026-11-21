using EducationPortal.Application.Subscriptions.Common;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Subscriptions;

public class TrialDaysTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 21, 12, 0, 0, TimeSpan.Zero);

    [TestCase(90, 90)]
    [TestCase(1, 1)]
    [TestCase(0, 0)]
    public void ShouldCountWholeDays(int daysAhead, int expected) =>
        TrialDays.Remaining(Now.AddDays(daysAhead), Now).ShouldBe(expected);

    [Test]
    public void ShouldRoundAPartOfADayUp() =>
        TrialDays.Remaining(Now.AddHours(1), Now).ShouldBe(1);

    [Test]
    public void ShouldNeverGoBelowZeroOnceTheTrialHasEnded()
    {
        TrialDays.Remaining(Now.AddSeconds(-1), Now).ShouldBe(0);
        TrialDays.Remaining(Now.AddDays(-30), Now).ShouldBe(0);
    }
}
