using EducationPortal.Application.Timetable.Common;
using EducationPortal.Domain.Academics;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Timetable;

/// <summary>The timetable's shared rules: weekdays map to the day of the week, and a clash names the lesson it clashes with.</summary>
public class TimetableRulesTests
{
    [TestCase(Weekday.Monday, DayOfWeek.Monday)]
    [TestCase(Weekday.Tuesday, DayOfWeek.Tuesday)]
    [TestCase(Weekday.Wednesday, DayOfWeek.Wednesday)]
    [TestCase(Weekday.Thursday, DayOfWeek.Thursday)]
    [TestCase(Weekday.Friday, DayOfWeek.Friday)]
    [TestCase(Weekday.Saturday, DayOfWeek.Saturday)]
    [TestCase(Weekday.Sunday, DayOfWeek.Sunday)]
    public void ShouldMapEachWeekdayToItsDayOfWeek(Weekday day, DayOfWeek expected)
    {
        TimetableRules.ToDayOfWeek(day).ShouldBe(expected);
    }

    [Test]
    public void ShouldRefuseAValueThatIsNotAWeekday()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => TimetableRules.ToDayOfWeek((Weekday)99));
    }

    [Test]
    public void ShouldReportEachClashAgainstTheLessonItClashesWith()
    {
        var lesson = Guid.NewGuid();

        var exception = TimetableRules.ToValidationException(
        [
            new TimetableConflictDto(lesson, TimetableConflictKind.Teacher),
            new TimetableConflictDto(lesson, TimetableConflictKind.Room),
        ]);

        exception.Errors["Start"].Length.ShouldBe(2);
        exception.Errors["Start"].ShouldAllBe(message => message.Contains(lesson.ToString()));
    }
}
