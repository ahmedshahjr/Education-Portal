using EducationPortal.Application.AcademicCalendar.Commands.CreateAcademicYear;
using EducationPortal.Application.AcademicCalendar.Commands.CreateCalendarEvent;
using EducationPortal.Application.AcademicCalendar.Commands.CreateHoliday;
using EducationPortal.Application.AcademicCalendar.Commands.CreateShift;
using EducationPortal.Application.AcademicCalendar.Commands.CreateTerm;
using EducationPortal.Application.AcademicCalendar.Commands.UpdateAcademicYear;
using EducationPortal.Application.AcademicCalendar.Commands.UpdateCalendarEvent;
using EducationPortal.Application.AcademicCalendar.Queries.GetSchoolCalendar;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.AcademicCalendar;

public class AcademicYearValidatorTests
{
    private static readonly DateOnly Start = new(2026, 8, 1);

    private static CreateAcademicYearCommand Valid() => new() { Name = "2026-27", StartDate = Start, EndDate = new DateOnly(2027, 6, 30) };

    [Test]
    public void ShouldAcceptAValidYear() => new CreateAcademicYearCommandValidator().Validate(Valid()).IsValid.ShouldBeTrue();

    [Test]
    public void ShouldAcceptAOneDayYear() =>
        new CreateAcademicYearCommandValidator().Validate(Valid() with { EndDate = Start }).IsValid.ShouldBeTrue();

    [Test]
    public void ShouldRefuseAnEndBeforeTheStart() =>
        new CreateAcademicYearCommandValidator().Validate(Valid() with { EndDate = Start.AddDays(-1) }).IsValid.ShouldBeFalse();

    [TestCase(AcademicYear.MaxLengthInDays - 1, true)]
    [TestCase(AcademicYear.MaxLengthInDays, false)]
    public void ShouldLimitTheLength(int days, bool valid) =>
        new CreateAcademicYearCommandValidator().Validate(Valid() with { EndDate = Start.AddDays(days) }).IsValid.ShouldBe(valid);

    [TestCase("", false)]
    [TestCase("123456789012345678901", false)]
    [TestCase("12345678901234567890", true)]
    public void ShouldCheckTheName(string name, bool valid) =>
        new CreateAcademicYearCommandValidator().Validate(Valid() with { Name = name }).IsValid.ShouldBe(valid);

    [Test]
    public void ShouldRefuseAnUnknownStatusOrAMissingId()
    {
        var validator = new UpdateAcademicYearCommandValidator();
        var command = new UpdateAcademicYearCommand { AcademicYearId = Guid.NewGuid(), Name = "2026-27", StartDate = Start, EndDate = Start, Status = AcademicYearStatus.Active };

        validator.Validate(command).IsValid.ShouldBeTrue();
        validator.Validate(command with { Status = (AcademicYearStatus)9 }).IsValid.ShouldBeFalse();
        validator.Validate(command with { AcademicYearId = Guid.Empty }).IsValid.ShouldBeFalse();
    }
}

public class TermValidatorTests
{
    private static readonly DateOnly Start = new(2026, 8, 1);

    private static CreateTermCommand Valid() => new() { AcademicYearId = Guid.NewGuid(), Name = "First term", Sequence = 1, StartDate = Start, EndDate = Start.AddDays(90) };

    [Test]
    public void ShouldAcceptAValidTerm() => new CreateTermCommandValidator().Validate(Valid()).IsValid.ShouldBeTrue();

    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(Term.MaxSequence, true)]
    [TestCase(Term.MaxSequence + 1, false)]
    public void ShouldCheckTheNumber(int sequence, bool valid) =>
        new CreateTermCommandValidator().Validate(Valid() with { Sequence = sequence }).IsValid.ShouldBe(valid);

    [Test]
    public void ShouldRefuseAnEndBeforeTheStartOrAMissingYear()
    {
        new CreateTermCommandValidator().Validate(Valid() with { EndDate = Start.AddDays(-1) }).IsValid.ShouldBeFalse();
        new CreateTermCommandValidator().Validate(Valid() with { AcademicYearId = Guid.Empty }).IsValid.ShouldBeFalse();
    }
}

public class ShiftValidatorTests
{
    private static CreateShiftCommand Valid() => new() { CampusId = Guid.NewGuid(), Name = "Morning", StartTime = new TimeOnly(7, 30), EndTime = new TimeOnly(13, 0) };

    [Test]
    public void ShouldAcceptAValidShift() => new CreateShiftCommandValidator().Validate(Valid()).IsValid.ShouldBeTrue();

    [TestCase(13, 0, false)]
    [TestCase(7, 0, false)]
    [TestCase(13, 1, true)]
    public void ShouldNeedTheEndAfterTheStart(int endHour, int endMinute, bool valid) =>
        new CreateShiftCommandValidator().Validate(Valid() with { StartTime = new TimeOnly(13, 0), EndTime = new TimeOnly(endHour, endMinute) }).IsValid.ShouldBe(valid);

    [Test]
    public void ShouldRefuseAMissingCampusOrName()
    {
        new CreateShiftCommandValidator().Validate(Valid() with { CampusId = Guid.Empty }).IsValid.ShouldBeFalse();
        new CreateShiftCommandValidator().Validate(Valid() with { Name = " " }).IsValid.ShouldBeFalse();
    }
}

public class HolidayValidatorTests
{
    private static readonly DateOnly Start = new(2026, 12, 25);

    private static CreateHolidayCommand Valid() => new()
    {
        Title = "Winter break",
        StartDate = Start,
        EndDate = Start.AddDays(7),
        Type = HolidayType.Vacation,
        AppliesTo = HolidayAudience.Both,
    };

    [Test]
    public void ShouldAcceptAValidHoliday() => new CreateHolidayCommandValidator().Validate(Valid()).IsValid.ShouldBeTrue();

    [TestCase(-1, false)]
    [TestCase(0, true)]
    [TestCase(Holiday.MaxLengthInDays - 1, true)]
    [TestCase(Holiday.MaxLengthInDays, false)]
    public void ShouldCheckTheDates(int days, bool valid) =>
        new CreateHolidayCommandValidator().Validate(Valid() with { EndDate = Start.AddDays(days) }).IsValid.ShouldBe(valid);

    [Test]
    public void ShouldRefuseUnknownValuesAndAnEmptyCampusId()
    {
        var validator = new CreateHolidayCommandValidator();

        validator.Validate(Valid() with { Type = (HolidayType)0 }).IsValid.ShouldBeFalse();
        validator.Validate(Valid() with { AppliesTo = (HolidayAudience)9 }).IsValid.ShouldBeFalse();
        validator.Validate(Valid() with { CampusId = Guid.Empty }).IsValid.ShouldBeFalse();
        validator.Validate(Valid() with { Title = "" }).IsValid.ShouldBeFalse();
    }
}

public class CalendarEventValidatorTests
{
    private static CreateCalendarEventCommand Valid() => new()
    {
        Title = "Sports day",
        EventDate = new DateOnly(2026, 10, 15),
        Type = CalendarEventType.Sports,
        Audience = CalendarEventAudience.All,
    };

    [Test]
    public void ShouldAcceptAnAllDayEvent() => new CreateCalendarEventCommandValidator().Validate(Valid()).IsValid.ShouldBeTrue();

    [Test]
    public void ShouldCheckTheTimes()
    {
        var validator = new CreateCalendarEventCommandValidator();

        validator.Validate(Valid() with { StartTime = new TimeOnly(9, 0) }).IsValid.ShouldBeTrue();
        validator.Validate(Valid() with { StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(11, 0) }).IsValid.ShouldBeTrue();
        validator.Validate(Valid() with { EndTime = new TimeOnly(11, 0) }).IsValid.ShouldBeFalse();
        validator.Validate(Valid() with { StartTime = new TimeOnly(11, 0), EndTime = new TimeOnly(11, 0) }).IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldLimitTheDescriptionAndNeedAnIdOnUpdate()
    {
        new CreateCalendarEventCommandValidator().Validate(Valid() with { Description = new string('a', CalendarEvent.DescriptionMaxLength + 1) }).IsValid.ShouldBeFalse();
        new UpdateCalendarEventCommandValidator().Validate(new UpdateCalendarEventCommand
        {
            Title = "Sports day",
            EventDate = new DateOnly(2026, 10, 15),
            Type = CalendarEventType.Sports,
            Audience = CalendarEventAudience.All,
        }).IsValid.ShouldBeFalse();
    }
}

public class SchoolCalendarQueryValidatorTests
{
    private static readonly DateOnly From = new(2026, 1, 1);

    [TestCase(0, true)]
    [TestCase(GetSchoolCalendarQuery.MaxRangeInDays - 1, true)]
    [TestCase(GetSchoolCalendarQuery.MaxRangeInDays, false)]
    [TestCase(-1, false)]
    public void ShouldLimitTheRange(int days, bool valid) =>
        new GetSchoolCalendarQueryValidator().Validate(new GetSchoolCalendarQuery { From = From, To = From.AddDays(days) }).IsValid.ShouldBe(valid);
}
