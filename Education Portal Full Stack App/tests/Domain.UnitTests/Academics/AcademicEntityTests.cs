using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Academics;

/// <summary>The academic entities keep their ids positive, their names and bounds within limits, and their times ordered.</summary>
public class AcademicEntityTests
{
    [Test]
    public void ShouldCreateACourseWithTrimmedName()
    {
        var course = Course.Create(1, 2, "  Mathematics ", 3, null);

        course.Name.ShouldBe("Mathematics");
        course.TeacherUserDetailsId.ShouldBe(3);
    }

    [TestCase(100, true)]
    [TestCase(101, false)]
    public void ShouldLimitACourseNameTo100Characters(int length, bool valid)
    {
        var name = new string('c', length);

        if (valid)
        {
            Course.Create(1, 2, name, 3, null).Name.Length.ShouldBe(length);
        }
        else
        {
            Should.Throw<ArgumentOutOfRangeException>(() => Course.Create(1, 2, name, 3, null));
        }
    }

    [TestCase("")]
    [TestCase("   ")]
    public void ShouldRefuseABlankCourseName(string name)
    {
        Should.Throw<ArgumentException>(() => Course.Create(1, 2, name, 3, null));
    }

    [TestCase(0, 2, 3)]
    [TestCase(1, 0, 3)]
    [TestCase(1, 2, 0)]
    public void ShouldRefuseAnIdThatIsNotASavedRow(int schoolId, int classOfferingId, int teacherId)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Course.Create(schoolId, classOfferingId, "Maths", teacherId, null));
    }

    [Test]
    public void ShouldEnrolAStudentInAClass()
    {
        var enrolment = Enrolment.Create(1, 4, 2);

        enrolment.StudentUserDetailsId.ShouldBe(4);
        enrolment.ClassOfferingId.ShouldBe(2);
    }

    [TestCase(0, true)]
    [TestCase(100, true)]
    [TestCase(-0.01, false)]
    [TestCase(100.01, false)]
    public void ShouldKeepAnAssessmentWeightFromZeroToHundred(decimal weight, bool valid)
    {
        if (valid)
        {
            Assessment.Create(1, 2, "Test", weight, 20m).Weight.ShouldBe(weight);
        }
        else
        {
            Should.Throw<ArgumentOutOfRangeException>(() => Assessment.Create(1, 2, "Test", weight, 20m));
        }
    }

    [Test]
    public void ShouldRefuseAnAssessmentWithNoMaximumScore()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Assessment.Create(1, 2, "Test", 50m, 0m));
    }

    [Test]
    public void ShouldRecordAScoreWithinItsMaximum()
    {
        var score = Score.Record(1, 2, 3, 18m, 20m);

        score.Value.ShouldBe(18m);
    }

    [TestCase(-1, 20)]
    [TestCase(21, 20)]
    public void ShouldRefuseAScoreOutsideTheMaximum(decimal value, decimal maximum)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Score.Record(1, 2, 3, value, maximum));
    }

    [Test]
    public void ShouldChangeAScoreWithTheSameBounds()
    {
        var score = Score.Record(1, 2, 3, 10m, 20m);

        score.Change(20m, 20m);

        score.Value.ShouldBe(20m);
        Should.Throw<ArgumentOutOfRangeException>(() => score.Change(21m, 20m));
    }

    [Test]
    public void ShouldCreateATimetableSlotWithATrimmedRoom()
    {
        var slot = TimetableSlot.Create(1, 2, 3, Weekday.Monday, new TimeOnly(9, 0), new TimeOnly(9, 45), "  Lab 1 ");

        slot.Day.ShouldBe(Weekday.Monday);
        slot.Room.ShouldBe("Lab 1");
    }

    [Test]
    public void ShouldRefuseALessonThatEndsWhenItStarts()
    {
        Should.Throw<ArgumentException>(() => TimetableSlot.Create(1, 2, 3, Weekday.Monday, new TimeOnly(9, 0), new TimeOnly(9, 0), null));
    }

    [Test]
    public void ShouldLeaveAnEmptyRoomEmpty()
    {
        TimetableSlot.Create(1, 2, null, Weekday.Friday, new TimeOnly(9, 0), new TimeOnly(10, 0), "  ").Room.ShouldBeNull();
    }
}
