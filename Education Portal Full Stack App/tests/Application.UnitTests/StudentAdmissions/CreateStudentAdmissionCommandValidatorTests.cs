using System.Globalization;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.StudentAdmissions.Commands.CreateStudentAdmission;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using FluentValidation;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.UnitTests.StudentAdmissions;

public class CreateStudentAdmissionCommandValidatorTests
{
    private static readonly DateOnly Today = new(2026, 11, 15);

    private readonly CreateStudentAdmissionCommandValidator _validator = new(new FixedTime(Today));

    [OneTimeSetUp]
    public void ConfigureValidation() => ValidationLocalization.Configure();

    [TearDown]
    public void ResetCulture() => CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;

    private static CreateStudentAdmissionCommand Valid() => new()
    {
        FirstName = "Ali",
        LastName = "Khan",
        DateOfBirth = new DateOnly(2018, 4, 1),
        Gender = Gender.Male,
        FatherName = "Imran Khan",
        Nationality = "Pakistani",
        Address = "House 12, Street 4",
        City = "Lahore",
        ClassApplied = "Class 1",
        GuardianPhone = "+92 300 7654321",
        AdmissionDate = Today,
    };

    [Test]
    public void ShouldAcceptAValidAdmissionForm()
    {
        _validator.Validate(Valid()).IsValid.ShouldBeTrue();
    }

    [TestCase("")]
    [TestCase(" ")]
    public void ShouldRequireTheFatherName(string name)
    {
        _validator.Validate(Valid() with { FatherName = name }).Errors.ShouldContain(e => e.PropertyName == nameof(CreateStudentAdmissionCommand.FatherName));
    }

    [TestCase(100, true)]
    [TestCase(101, false)]
    public void ShouldLimitTheFatherNameLength(int length, bool valid)
    {
        _validator.Validate(Valid() with { FatherName = new string('a', length) }).IsValid.ShouldBe(valid);
    }

    [TestCase("")]
    [TestCase("call me")]
    public void ShouldRequireAGuardianPhoneNumber(string phone)
    {
        _validator.Validate(Valid() with { GuardianPhone = phone }).IsValid.ShouldBeFalse();
    }

    [Test]
    public void ShouldAcceptNoStudentPhone()
    {
        _validator.Validate(Valid() with { Phone = null }).IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldRejectAGenderThatIsNotDefined()
    {
        _validator.Validate(Valid() with { Gender = (Gender)99 }).Errors.ShouldContain(e => e.PropertyName == nameof(CreateStudentAdmissionCommand.Gender));
    }

    [Test]
    public void ShouldRequireTheClassApplied()
    {
        _validator.Validate(Valid() with { ClassApplied = "" }).IsValid.ShouldBeFalse();
    }

    [TestCase(3, true)]
    [TestCase(2, false)]
    [TestCase(30, true)]
    [TestCase(31, false)]
    public void ShouldLimitTheAgeOnTheAdmissionDate(int years, bool valid)
    {
        // Admitted today, so the age is exactly `years`.
        var command = Valid() with { DateOfBirth = Today.AddYears(-years) };

        _validator.Validate(command).IsValid.ShouldBe(valid);
    }

    [Test]
    public void ShouldRejectABirthDateInTheFuture()
    {
        var errors = _validator.Validate(Valid() with { DateOfBirth = Today.AddDays(1) }).Errors;

        errors.ShouldContain(e => e.PropertyName == nameof(CreateStudentAdmissionCommand.DateOfBirth) && e.ErrorMessage == "The date of birth cannot be in the future.");
    }

    [Test]
    public void ShouldRejectAnAdmissionDateBeforeTheBirthDate()
    {
        var dateOfBirth = new DateOnly(2018, 4, 1);

        var errors = _validator.Validate(Valid() with { DateOfBirth = dateOfBirth, AdmissionDate = dateOfBirth.AddDays(-1) }).Errors;

        errors.ShouldContain(e => e.ErrorMessage == "The admission date cannot be before the date of birth.");
    }

    [TestCase(366, true)]
    [TestCase(367, false)]
    public void ShouldLimitHowFarAheadTheAdmissionDateIs(int days, bool valid)
    {
        var command = Valid() with { AdmissionDate = Today.AddDays(days) };

        _validator.Validate(command).IsValid.ShouldBe(valid);
    }

    [Test]
    public void ShouldReportAnAdmissionDateTooFarAheadWithTheLimit()
    {
        var errors = _validator.Validate(Valid() with { AdmissionDate = Today.AddDays(367) }).Errors;

        errors.ShouldContain(e => e.ErrorMessage == "The admission date cannot be more than 366 days ahead.");
    }

    [Test]
    public void ShouldReportTheAgeOnlyOnceTheDatesAreInOrder()
    {
        // The admission date is before the birth date, so only that message appears, not an age message too.
        var errors = _validator.Validate(Valid() with { DateOfBirth = Today, AdmissionDate = Today.AddDays(-1) }).Errors;

        errors.Count.ShouldBe(1);
        errors[0].ErrorMessage.ShouldBe("The admission date cannot be before the date of birth.");
    }

    [Test]
    public void ShouldReportTheAgeInTheRequestLanguage()
    {
        var command = Valid() with { DateOfBirth = Today.AddYears(-2) };

        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        var english = _validator.Validate(command).Errors.Single().ErrorMessage;

        CultureInfo.CurrentUICulture = new CultureInfo("ur-PK");
        var urdu = _validator.Validate(command).Errors.Single().ErrorMessage;

        english.ShouldBe("The student must be 3 to 30 years old on the admission date.");
        urdu.ShouldNotBe(english);
        urdu.ShouldContain("3");
        urdu.ShouldContain("30");
    }

    [Test]
    public void ShouldReportAPhoneNumberInTheRequestLanguage()
    {
        var command = Valid() with { GuardianPhone = "call me" };

        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        var english = _validator.Validate(command).Errors.Single(e => e.PropertyName == nameof(CreateStudentAdmissionCommand.GuardianPhone)).ErrorMessage;

        CultureInfo.CurrentUICulture = new CultureInfo("ar");
        var arabic = _validator.Validate(command).Errors.Single(e => e.PropertyName == nameof(CreateStudentAdmissionCommand.GuardianPhone)).ErrorMessage;

        english.ShouldBe("A phone number has 7 to 15 digits and may start with +.");
        arabic.ShouldNotBe(english);
    }

    /// <summary>A clock that always reads the same moment, so the date rules are checked against a fixed day.</summary>
    private sealed class FixedTime(DateOnly today) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
    }
}
