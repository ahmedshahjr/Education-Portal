using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Academics;

/// <summary>Moving a lesson follows the same rules as creating it: it ends after it starts, its room is trimmed and limited.</summary>
public class TimetableSlotRescheduleTests
{
    private static TimetableSlot NewLesson() =>
        TimetableSlot.Create(1, 1, null, Weekday.Monday, new TimeOnly(9, 0), new TimeOnly(10, 0), null);

    [Test]
    public void ShouldMoveTheLessonAndTrimItsRoom()
    {
        var lesson = NewLesson();

        lesson.Reschedule(2, Weekday.Friday, new TimeOnly(13, 0), new TimeOnly(14, 30), "  Lab 2 ");

        lesson.Day.ShouldBe(Weekday.Friday);
        lesson.Start.ShouldBe(new TimeOnly(13, 0));
        lesson.End.ShouldBe(new TimeOnly(14, 30));
        lesson.CampusId.ShouldBe(2);
        lesson.Room.ShouldBe("Lab 2");
    }

    [Test]
    public void ShouldRefuseAMoveThatEndsWhenItStarts()
    {
        var lesson = NewLesson();

        Should.Throw<ArgumentException>(() => lesson.Reschedule(null, Weekday.Monday, new TimeOnly(9, 0), new TimeOnly(9, 0), null));
    }

    [Test]
    public void ShouldRefuseARoomOneCharacterOverTheLimit()
    {
        var lesson = NewLesson();

        Should.Throw<ArgumentOutOfRangeException>(() =>
            lesson.Reschedule(null, Weekday.Monday, new TimeOnly(9, 0), new TimeOnly(10, 0), new string('R', TimetableSlot.RoomMaxLength + 1)));
    }

    [Test]
    public void ShouldRefuseAZeroCampus()
    {
        var lesson = NewLesson();

        Should.Throw<ArgumentOutOfRangeException>(() => lesson.Reschedule(0, Weekday.Monday, new TimeOnly(9, 0), new TimeOnly(10, 0), null));
    }
}
