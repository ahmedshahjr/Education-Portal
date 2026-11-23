using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class AcademicYearTests
{
    private static readonly DateOnly Start = new(2026, 8, 1);
    private static readonly DateOnly End = new(2027, 6, 30);

    [Test]
    public void ShouldCreateAPlannedYearThatIsNotCurrent()
    {
        var year = AcademicYear.Create(1, " 2026-27 ", Start, End);

        year.Name.ShouldBe("2026-27");
        year.Status.ShouldBe(AcademicYearStatus.Planned);
        year.IsCurrent.ShouldBeFalse();
    }

    [Test]
    public void ShouldAllowAOneDayYear() =>
        AcademicYear.Create(1, "Short", Start, Start).EndDate.ShouldBe(Start);

    [Test]
    public void ShouldRefuseAnEndBeforeTheStart() =>
        Should.Throw<ArgumentException>(() => AcademicYear.Create(1, "2026-27", End, Start));

    [Test]
    public void ShouldAllowAYearOfExactlyTheLongestLength() =>
        AcademicYear.Create(1, "Long", Start, Start.AddDays(AcademicYear.MaxLengthInDays - 1)).ShouldNotBeNull();

    [Test]
    public void ShouldRefuseAYearLongerThanTwoYears() =>
        Should.Throw<ArgumentException>(() => AcademicYear.Create(1, "Long", Start, Start.AddDays(AcademicYear.MaxLengthInDays)));

    [TestCase("")]
    [TestCase("   ")]
    public void ShouldRefuseABlankName(string name) =>
        Should.Throw<ArgumentException>(() => AcademicYear.Create(1, name, Start, End));

    [Test]
    public void ShouldRefuseANameOverTheLimit()
    {
        AcademicYear.Create(1, new string('a', AcademicYear.NameMaxLength), Start, End).ShouldNotBeNull();
        Should.Throw<ArgumentOutOfRangeException>(() => AcademicYear.Create(1, new string('a', AcademicYear.NameMaxLength + 1), Start, End));
    }

    [Test]
    public void ShouldRefuseASchoolIdThatIsNotPositive() =>
        Should.Throw<ArgumentOutOfRangeException>(() => AcademicYear.Create(0, "2026-27", Start, End));

    [Test]
    public void ShouldStopBeingCurrentWhenClosed()
    {
        var year = AcademicYear.Create(1, "2026-27", Start, End);
        year.MakeCurrent();

        year.SetStatus(AcademicYearStatus.Closed);

        year.IsCurrent.ShouldBeFalse();
        year.Status.ShouldBe(AcademicYearStatus.Closed);
    }

    [Test]
    public void ShouldNotChangeOrBecomeCurrentWhenClosed()
    {
        var year = AcademicYear.Create(1, "2026-27", Start, End);
        year.SetStatus(AcademicYearStatus.Closed);

        Should.Throw<InvalidOperationException>(() => year.Change("2027-28", Start, End));
        Should.Throw<InvalidOperationException>(year.MakeCurrent);
    }

    [Test]
    public void ShouldChangeAgainOnceReopened()
    {
        var year = AcademicYear.Create(1, "2026-27", Start, End);
        year.SetStatus(AcademicYearStatus.Closed);

        year.SetStatus(AcademicYearStatus.Active);
        year.Change("2026-27 (revised)", Start, End);

        year.Name.ShouldBe("2026-27 (revised)");
    }

    [Test]
    public void ShouldRefuseAnUnknownStatus() =>
        Should.Throw<ArgumentOutOfRangeException>(() => AcademicYear.Create(1, "2026-27", Start, End).SetStatus((AcademicYearStatus)99));

    [TestCase("2026-07-01", "2026-08-01", true)]
    [TestCase("2027-06-30", "2027-07-31", true)]
    [TestCase("2026-01-01", "2026-07-31", false)]
    [TestCase("2027-07-01", "2027-12-31", false)]
    public void ShouldTellWhetherARangeOverlaps(string from, string to, bool overlaps) =>
        AcademicYear.Create(1, "2026-27", Start, End).Overlaps(DateOnly.Parse(from), DateOnly.Parse(to)).ShouldBe(overlaps);

    [Test]
    public void ShouldNotChangeOnceDeleted()
    {
        var year = AcademicYear.Create(1, "2026-27", Start, End);
        year.Delete(DateTimeOffset.UtcNow, "user");

        Should.Throw<InvalidOperationException>(() => year.Change("x", Start, End));
        Should.Throw<InvalidOperationException>(() => year.SetStatus(AcademicYearStatus.Active));
    }
}

public class TermTests
{
    private static readonly DateOnly Start = new(2026, 8, 1);

    [Test]
    public void ShouldCreateATerm()
    {
        var term = Term.Create(1, 2, " First term ", 1, Start, Start.AddDays(90));

        term.Name.ShouldBe("First term");
        term.Sequence.ShouldBe(1);
        term.AcademicYearId.ShouldBe(2);
    }

    [TestCase(0)]
    [TestCase(Term.MaxSequence + 1)]
    public void ShouldRefuseANumberOutOfRange(int sequence) =>
        Should.Throw<ArgumentOutOfRangeException>(() => Term.Create(1, 2, "Term", sequence, Start, Start));

    [Test]
    public void ShouldAllowTheHighestNumber() =>
        Term.Create(1, 2, "Term", Term.MaxSequence, Start, Start).Sequence.ShouldBe(Term.MaxSequence);

    [Test]
    public void ShouldRefuseAnEndBeforeTheStart() =>
        Should.Throw<ArgumentException>(() => Term.Create(1, 2, "Term", 1, Start, Start.AddDays(-1)));

    [Test]
    public void ShouldRefuseAMissingYear() =>
        Should.Throw<ArgumentOutOfRangeException>(() => Term.Create(1, 0, "Term", 1, Start, Start));
}

public class ShiftTests
{
    [Test]
    public void ShouldCreateAShift()
    {
        var shift = Shift.Create(1, 3, " Morning ", new TimeOnly(7, 30), new TimeOnly(13, 0));

        shift.Name.ShouldBe("Morning");
        shift.CampusId.ShouldBe(3);
    }

    [TestCase(13, 0, 13, 0)]
    [TestCase(13, 0, 7, 30)]
    public void ShouldRefuseAnEndThatIsNotAfterTheStart(int startHour, int startMinute, int endHour, int endMinute) =>
        Should.Throw<ArgumentException>(() => Shift.Create(1, 3, "Morning", new TimeOnly(startHour, startMinute), new TimeOnly(endHour, endMinute)));

    [Test]
    public void ShouldAllowAOneMinuteShift() =>
        Shift.Create(1, 3, "Short", new TimeOnly(7, 0), new TimeOnly(7, 1)).ShouldNotBeNull();

    [Test]
    public void ShouldRefuseAMissingCampus() =>
        Should.Throw<ArgumentOutOfRangeException>(() => Shift.Create(1, 0, "Morning", new TimeOnly(7, 0), new TimeOnly(8, 0)));
}

public class HolidayTests
{
    private static readonly DateOnly Start = new(2026, 12, 25);

    private static Holiday Create(int campusId = 1, DateOnly? end = null) =>
        Holiday.Create(1, campusId, " Winter break ", Start, end ?? Start, HolidayType.Vacation, HolidayAudience.Both, isTentative: false);

    [Test]
    public void ShouldCreateAHolidayForItsCampus()
    {
        var holiday = Create();

        holiday.Title.ShouldBe("Winter break");
        holiday.CampusId.ShouldBe(1);
        holiday.WholeSchoolKey.ShouldBeNull();
    }

    [Test]
    public void ShouldRefuseAHolidayWithoutACampus() => Should.Throw<ArgumentOutOfRangeException>(() => Create(campusId: 0));

    [Test]
    public void ShouldJoinAWholeSchoolKeyOnce()
    {
        var key = Guid.CreateVersion7();
        var holiday = Create();

        holiday.JoinWholeSchool(key);

        holiday.WholeSchoolKey.ShouldBe(key);
        Should.Throw<ArgumentException>(() => holiday.JoinWholeSchool(Guid.Empty));
    }

    [Test]
    public void ShouldCreateACampusHoliday() => Create(campusId: 4).CampusId.ShouldBe(4);

    [Test]
    public void ShouldRefuseACampusIdThatIsNotPositive() =>
        Should.Throw<ArgumentOutOfRangeException>(() => Create(campusId: 0));

    [Test]
    public void ShouldRefuseAnEndBeforeTheStart() =>
        Should.Throw<ArgumentException>(() => Create(end: Start.AddDays(-1)));

    [Test]
    public void ShouldAllowTheLongestHolidayAndRefuseLonger()
    {
        Create(end: Start.AddDays(Holiday.MaxLengthInDays - 1)).ShouldNotBeNull();
        Should.Throw<ArgumentException>(() => Create(end: Start.AddDays(Holiday.MaxLengthInDays)));
    }

    [Test]
    public void ShouldRefuseUnknownTypesAndAudiences()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => Holiday.Create(1, 1, "x", Start, Start, (HolidayType)0, HolidayAudience.Both, false));
        Should.Throw<ArgumentOutOfRangeException>(() => Holiday.Create(1, 1, "x", Start, Start, HolidayType.Public, (HolidayAudience)0, false));
    }
}

public class CalendarEventTests
{
    private static readonly DateOnly Day = new(2026, 10, 15);

    private static CalendarEvent Create(TimeOnly? start, TimeOnly? end, string? description = null) =>
        CalendarEvent.Create(1, 1, " Sports day ", description, Day, start, end, CalendarEventType.Sports, CalendarEventAudience.All);

    [Test]
    public void ShouldCreateAnAllDayEvent()
    {
        var calendarEvent = Create(null, null, "   ");

        calendarEvent.Title.ShouldBe("Sports day");
        calendarEvent.Description.ShouldBeNull();
        calendarEvent.StartTime.ShouldBeNull();
    }

    [Test]
    public void ShouldAllowAStartWithoutAnEnd() => Create(new TimeOnly(9, 0), null).StartTime.ShouldBe(new TimeOnly(9, 0));

    [Test]
    public void ShouldRefuseAnEndWithoutAStart() =>
        Should.Throw<ArgumentException>(() => Create(null, new TimeOnly(10, 0)));

    [TestCase(10, 10)]
    [TestCase(10, 9)]
    public void ShouldRefuseAnEndThatIsNotAfterTheStart(int startHour, int endHour) =>
        Should.Throw<ArgumentException>(() => Create(new TimeOnly(startHour, 0), new TimeOnly(endHour, 0)));

    [Test]
    public void ShouldRefuseADescriptionOverTheLimit()
    {
        Create(null, null, new string('a', CalendarEvent.DescriptionMaxLength)).ShouldNotBeNull();
        Should.Throw<ArgumentOutOfRangeException>(() => Create(null, null, new string('a', CalendarEvent.DescriptionMaxLength + 1)));
    }
}
