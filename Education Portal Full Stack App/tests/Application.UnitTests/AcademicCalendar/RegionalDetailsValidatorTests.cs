using EducationPortal.Application.Campuses.Commands.UpdateCampus;
using EducationPortal.Application.Countries.Commands.UpdateCountryRegionalSettings;
using EducationPortal.Application.Schools.Commands.UpdateSchoolDetails;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.AcademicCalendar;

public class UpdateCountryRegionalSettingsValidatorTests
{
    private static UpdateCountryRegionalSettingsCommand Valid() => new()
    {
        Id = Guid.NewGuid(),
        CurrencyDecimals = 2,
        PhonePattern = @"^03\d{9}$",
        WeekStartDay = DayOfWeek.Monday,
        WeekendDays = [DayOfWeek.Sunday],
    };

    [Test]
    public void ShouldAcceptValidSettings() => new UpdateCountryRegionalSettingsCommandValidator().Validate(Valid()).IsValid.ShouldBeTrue();

    [TestCase(-1, false)]
    [TestCase(0, true)]
    [TestCase(Country.MaxCurrencyDecimals, true)]
    [TestCase(Country.MaxCurrencyDecimals + 1, false)]
    public void ShouldCheckTheDecimals(int decimals, bool valid) =>
        new UpdateCountryRegionalSettingsCommandValidator().Validate(Valid() with { CurrencyDecimals = decimals }).IsValid.ShouldBe(valid);

    [TestCase(null, true)]
    [TestCase("", true)]
    [TestCase("[bad", false)]
    public void ShouldCheckThePattern(string? pattern, bool valid) =>
        new UpdateCountryRegionalSettingsCommandValidator().Validate(Valid() with { PhonePattern = pattern }).IsValid.ShouldBe(valid);

    [Test]
    public void ShouldCheckTheWeekend()
    {
        var validator = new UpdateCountryRegionalSettingsCommandValidator();

        validator.Validate(Valid() with { WeekendDays = [] }).IsValid.ShouldBeTrue();
        validator.Validate(Valid() with { WeekendDays = [DayOfWeek.Friday, DayOfWeek.Friday] }).IsValid.ShouldBeFalse();
        validator.Validate(Valid() with { WeekendDays = [DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday] }).IsValid.ShouldBeFalse();
        validator.Validate(Valid() with { WeekendDays = [(DayOfWeek)7] }).IsValid.ShouldBeFalse();
        validator.Validate(Valid() with { WeekStartDay = (DayOfWeek)7 }).IsValid.ShouldBeFalse();
    }
}

public class UpdateSchoolDetailsValidatorTests
{
    private static UpdateSchoolDetailsCommand Valid() => new()
    {
        LocalName = "مدرسہ",
        Phone = "+92 21 1234567",
        Email = "office@school.example",
        Website = "https://school.example",
        AcademicYearStartMonth = 4,
    };

    [Test]
    public void ShouldAcceptValidDetailsAndAnEmptyForm()
    {
        new UpdateSchoolDetailsCommandValidator().Validate(Valid()).IsValid.ShouldBeTrue();
        new UpdateSchoolDetailsCommandValidator().Validate(new UpdateSchoolDetailsCommand()).IsValid.ShouldBeTrue();
    }

    [TestCase(0, false)]
    [TestCase(1, true)]
    [TestCase(12, true)]
    [TestCase(13, false)]
    public void ShouldCheckTheMonth(int month, bool valid) =>
        new UpdateSchoolDetailsCommandValidator().Validate(Valid() with { AcademicYearStartMonth = month }).IsValid.ShouldBe(valid);

    [Test]
    public void ShouldRefuseBadContactDetails()
    {
        var validator = new UpdateSchoolDetailsCommandValidator();

        validator.Validate(Valid() with { Phone = "12" }).IsValid.ShouldBeFalse();
        validator.Validate(Valid() with { Email = "nope" }).IsValid.ShouldBeFalse();
        validator.Validate(Valid() with { Website = "javascript:alert(1)" }).IsValid.ShouldBeFalse();
        validator.Validate(Valid() with { CityId = Guid.Empty }).IsValid.ShouldBeFalse();
        validator.Validate(Valid() with { LocalName = new string('a', 201) }).IsValid.ShouldBeFalse();
    }
}

public class UpdateCampusProfileValidatorTests
{
    [Test]
    public void ShouldCheckTheLocalNameAndGenderPolicy()
    {
        var validator = new UpdateCampusCommandValidator();
        var command = new UpdateCampusCommand { CampusId = Guid.NewGuid(), Name = "North", LocalName = "شمالی", GenderPolicy = SchoolGenderPolicy.BoysOnly };

        validator.Validate(command).IsValid.ShouldBeTrue();
        validator.Validate(command with { GenderPolicy = (SchoolGenderPolicy)0 }).IsValid.ShouldBeFalse();
        validator.Validate(command with { LocalName = new string('a', Campus.LocalNameMaxLength + 1) }).IsValid.ShouldBeFalse();
    }
}
