using EducationPortal.Application.Timetable.Commands.CreateTimetableSlot;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.Timetable;

/// <summary>A lesson ends after it starts, and its room is at most the stored length (the boundaries are tested).</summary>
public class TimetableValidatorTests
{
    private static CreateTimetableSlotCommand Lesson(TimeOnly start, TimeOnly end, string? room = null) => new()
    {
        CourseId = Guid.NewGuid(),
        Day = Weekday.Monday,
        Start = start,
        End = end,
        Room = room,
    };

    [Test]
    public void ShouldAcceptALessonThatEndsAfterItStarts()
    {
        var result = new CreateTimetableSlotCommandValidator().Validate(Lesson(new TimeOnly(9, 0), new TimeOnly(9, 1)));

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRefuseALessonThatEndsWhenItStarts()
    {
        var result = new CreateTimetableSlotCommandValidator().Validate(Lesson(new TimeOnly(9, 0), new TimeOnly(9, 0)));

        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldRefuseALessonThatEndsBeforeItStarts()
    {
        var result = new CreateTimetableSlotCommandValidator().Validate(Lesson(new TimeOnly(10, 0), new TimeOnly(9, 0)));

        result.IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldAcceptARoomAtTheStoredLimit()
    {
        var result = new CreateTimetableSlotCommandValidator().Validate(
            Lesson(new TimeOnly(9, 0), new TimeOnly(10, 0), new string('R', TimetableSlot.RoomMaxLength)));

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRefuseARoomOneCharacterOverTheLimit()
    {
        var result = new CreateTimetableSlotCommandValidator().Validate(
            Lesson(new TimeOnly(9, 0), new TimeOnly(10, 0), new string('R', TimetableSlot.RoomMaxLength + 1)));

        result.IsValid.ShouldBeFalse();
    }
}
