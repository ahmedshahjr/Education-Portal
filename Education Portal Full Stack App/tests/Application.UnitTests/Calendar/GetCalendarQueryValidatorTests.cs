using EducationPortal.Application.Calendar.Queries.GetCalendar;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Calendar;

/// <summary>A calendar range ends after it starts and covers at most 366 days (the boundaries are tested).</summary>
public class GetCalendarQueryValidatorTests
{
    private static readonly DateTimeOffset From = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static GetCalendarQuery Range(DateTimeOffset to) => new() { From = From, To = to };

    [Test]
    public void ShouldAcceptARangeOfOneDay()
    {
        new GetCalendarQueryValidator().Validate(Range(From.AddDays(1))).IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldAcceptARangeOfExactlyTheMaximum()
    {
        new GetCalendarQueryValidator().Validate(Range(From.AddDays(GetCalendarQueryValidator.MaximumDays))).IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRefuseARangeOneDayOverTheMaximum()
    {
        new GetCalendarQueryValidator().Validate(Range(From.AddDays(GetCalendarQueryValidator.MaximumDays + 1))).IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldRefuseAnEmptyRange()
    {
        new GetCalendarQueryValidator().Validate(Range(From)).IsValid.ShouldBeFalse();
    }
}
