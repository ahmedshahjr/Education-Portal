using EducationPortal.Domain.Academics;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Academics;

/// <summary>The timetable rules: a teacher, a room or a class cannot be in two places at once; touching slots are fine.</summary>
public class TimetableConflictsTests
{
    private static readonly TimeOnly Nine = new(9, 0);
    private static readonly TimeOnly Ten = new(10, 0);
    private static readonly TimeOnly Eleven = new(11, 0);

    private static TimetableSlotSpec Slot(int id, DayOfWeek day, TimeOnly start, TimeOnly end, int teacher = 1, int? campus = 1, string? room = "Lab 1", int classOffering = 1) =>
        new(id, day, start, end, teacher, campus, room, classOffering);

    [Test]
    public void ShouldFindNothingWhenTheSlotFits()
    {
        var existing = new[] { Slot(1, DayOfWeek.Monday, Nine, Ten, teacher: 1, room: "A", classOffering: 1) };

        var conflicts = TimetableConflicts.Find(existing, Slot(2, DayOfWeek.Monday, Ten, Eleven, teacher: 2, room: "B", classOffering: 2));

        conflicts.ShouldBeEmpty();
    }

    [Test]
    public void ShouldFlagATeacherWhoIsAlreadyTeaching()
    {
        var existing = new[] { Slot(1, DayOfWeek.Monday, Nine, Ten, teacher: 5, room: "A", classOffering: 1) };

        var conflicts = TimetableConflicts.Find(existing, Slot(2, DayOfWeek.Monday, Nine, Ten, teacher: 5, room: "B", classOffering: 2));

        conflicts.ShouldBe([new TimetableConflict(1, TimetableConflictKind.Teacher)]);
    }

    [Test]
    public void ShouldFlagARoomThatIsAlreadyBooked()
    {
        var existing = new[] { Slot(1, DayOfWeek.Monday, Nine, Ten, teacher: 1, room: "Lab 1", classOffering: 1) };

        var conflicts = TimetableConflicts.Find(existing, Slot(2, DayOfWeek.Monday, Nine, Ten, teacher: 2, room: " lab 1 ", classOffering: 2));

        conflicts.ShouldBe([new TimetableConflict(1, TimetableConflictKind.Room)]);
    }

    [Test]
    public void ShouldNotFlagTheSameRoomNameOnAnotherCampus()
    {
        var existing = new[] { Slot(1, DayOfWeek.Monday, Nine, Ten, teacher: 1, campus: 1, room: "Lab 1", classOffering: 1) };

        var conflicts = TimetableConflicts.Find(existing, Slot(2, DayOfWeek.Monday, Nine, Ten, teacher: 2, campus: 2, room: "Lab 1", classOffering: 2));

        conflicts.ShouldBeEmpty();
    }

    [Test]
    public void ShouldNotFlagAnUnnamedRoom()
    {
        var existing = new[] { Slot(1, DayOfWeek.Monday, Nine, Ten, teacher: 1, room: null, classOffering: 1) };

        var conflicts = TimetableConflicts.Find(existing, Slot(2, DayOfWeek.Monday, Nine, Ten, teacher: 2, room: null, classOffering: 2));

        conflicts.ShouldBeEmpty();
    }

    [Test]
    public void ShouldFlagAClassThatIsAlreadyInAnotherLesson()
    {
        var existing = new[] { Slot(1, DayOfWeek.Monday, Nine, Ten, teacher: 1, room: "A", classOffering: 7) };

        var conflicts = TimetableConflicts.Find(existing, Slot(2, DayOfWeek.Monday, Nine, Ten, teacher: 2, room: "B", classOffering: 7));

        conflicts.ShouldBe([new TimetableConflict(1, TimetableConflictKind.ClassOffering)]);
    }

    [Test]
    public void ShouldAllowSlotsThatTouchAtTheBoundary()
    {
        var existing = new[] { Slot(1, DayOfWeek.Monday, Nine, Ten, teacher: 1, room: "A", classOffering: 1) };

        var conflicts = TimetableConflicts.Find(existing, Slot(2, DayOfWeek.Monday, Ten, Eleven, teacher: 1, room: "A", classOffering: 1));

        conflicts.ShouldBeEmpty();
    }

    [Test]
    public void ShouldNotFlagTheSameDayAtAnotherTime()
    {
        var existing = new[] { Slot(1, DayOfWeek.Monday, Nine, Ten, teacher: 1, room: "A", classOffering: 1) };

        TimetableConflicts.Find(existing, Slot(2, DayOfWeek.Tuesday, Nine, Ten, teacher: 1, room: "A", classOffering: 1)).ShouldBeEmpty();
    }

    [Test]
    public void ShouldIgnoreTheSlotItself()
    {
        var existing = new[] { Slot(1, DayOfWeek.Monday, Nine, Ten, teacher: 1, room: "A", classOffering: 1) };

        TimetableConflicts.Find(existing, Slot(1, DayOfWeek.Monday, Nine, Ten, teacher: 1, room: "A", classOffering: 1)).ShouldBeEmpty();
    }

    [Test]
    public void ShouldReportEveryKindOfClashWithOneSlot()
    {
        var existing = new[] { Slot(1, DayOfWeek.Monday, Nine, Ten, teacher: 1, room: "A", classOffering: 1) };

        var conflicts = TimetableConflicts.Find(existing, Slot(2, DayOfWeek.Monday, Nine, Ten, teacher: 1, room: "A", classOffering: 1));

        conflicts.Select(c => c.Kind).ShouldBe([TimetableConflictKind.Teacher, TimetableConflictKind.Room, TimetableConflictKind.ClassOffering], ignoreOrder: true);
    }

    [Test]
    public void ShouldRefuseASlotThatEndsWhenItStarts()
    {
        Should.Throw<ArgumentException>(() => TimetableConflicts.Find([], Slot(2, DayOfWeek.Monday, Nine, Nine)));
    }
}
