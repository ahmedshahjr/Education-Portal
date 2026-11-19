using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Domain.UnitTests.Entities;

public class StudentProfileTests
{
    private static readonly DateOnly Today = new(2026, 11, 15);

    private static UserDetails Student() => UserDetails.Create(1, "Ali", null, "Khan", UserType.Student, "+92 300 1234567");

    private static StudentAdmissionDetails Details(
        DateOnly? dateOfBirth = null,
        DateOnly? admissionDate = null,
        string fatherName = "Imran Khan",
        string guardianPhone = "+92 300 7654321",
        string? previousSchool = null) => new(
            dateOfBirth ?? new DateOnly(2018, 4, 1),
            Gender.Male,
            fatherName,
            "Pakistani",
            "House 12, Street 4",
            "Lahore",
            "Class 1",
            previousSchool,
            guardianPhone,
            admissionDate ?? Today);

    [Test]
    public void ShouldCreateAnAdmissionFormForAStudent()
    {
        var student = Student();

        var profile = StudentProfile.Create(student, Details(), Today);

        profile.UserDetails.ShouldBeSameAs(student);
        profile.SchoolId.ShouldBe(1);
        profile.FatherName.ShouldBe("Imran Khan");
        profile.ClassApplied.ShouldBe("Class 1");
        profile.PreviousSchool.ShouldBeNull();
    }

    [Test]
    public void ShouldRejectAPersonWhoIsNotAStudent()
    {
        var teacher = UserDetails.Create(1, "Sana", null, "Qureshi", UserType.Teacher, null);

        Should.Throw<ArgumentException>(() => StudentProfile.Create(teacher, Details(), Today));
    }

    [Test]
    public void ShouldTrimTextAndStoreABlankPreviousSchoolAsNull()
    {
        var profile = StudentProfile.Create(Student(), Details(fatherName: "  Imran Khan  ", previousSchool: "   "), Today);

        profile.FatherName.ShouldBe("Imran Khan");
        profile.PreviousSchool.ShouldBeNull();
    }

    [TestCase(3, true)]
    [TestCase(2, false)]
    [TestCase(30, true)]
    [TestCase(31, false)]
    public void ShouldLimitTheAgeOnTheAdmissionDate(int years, bool valid)
    {
        // Admitted today, so the age is exactly `years` (the birth date is `years` years before today, not yet a birthday).
        var dateOfBirth = Today.AddYears(-years);

        var act = () => StudentProfile.Create(Student(), Details(dateOfBirth: dateOfBirth), Today);

        if (valid)
        {
            Should.NotThrow(act);
        }
        else
        {
            Should.Throw<ArgumentOutOfRangeException>(act);
        }
    }

    [Test]
    public void ShouldCountTheAgeOnTheDayBeforeABirthdayAsOneYearLess()
    {
        // Turns 3 tomorrow: on the admission date the student is still 2.
        var dateOfBirth = Today.AddYears(-3).AddDays(1);

        Should.Throw<ArgumentOutOfRangeException>(() => StudentProfile.Create(Student(), Details(dateOfBirth: dateOfBirth), Today));
    }

    [Test]
    public void ShouldRejectABirthDateInTheFuture()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => StudentProfile.Create(Student(), Details(dateOfBirth: Today.AddDays(1)), Today));
    }

    [Test]
    public void ShouldRejectAnAdmissionDateBeforeTheBirthDate()
    {
        var dateOfBirth = new DateOnly(2018, 4, 1);

        Should.Throw<ArgumentOutOfRangeException>(() => StudentProfile.Create(Student(), Details(dateOfBirth: dateOfBirth, admissionDate: dateOfBirth.AddDays(-1)), Today));
    }

    [TestCase(366, true)]
    [TestCase(367, false)]
    public void ShouldLimitHowFarAheadTheAdmissionDateIs(int days, bool valid)
    {
        var act = () => StudentProfile.Create(Student(), Details(admissionDate: Today.AddDays(days)), Today);

        if (valid)
        {
            Should.NotThrow(act);
        }
        else
        {
            Should.Throw<ArgumentOutOfRangeException>(act);
        }
    }

    [Test]
    public void ShouldRejectAFatherNameLongerThanTheLimit()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => StudentProfile.Create(Student(), Details(fatherName: new string('a', StudentProfile.FatherNameMaxLength + 1)), Today));
    }

    [Test]
    public void ShouldRejectAFatherNameThatIsBlank()
    {
        Should.Throw<ArgumentException>(() => StudentProfile.Create(Student(), Details(fatherName: "   "), Today));
    }

    [Test]
    public void ShouldRejectAGuardianPhoneThatIsNotAPhone()
    {
        Should.Throw<ArgumentException>(() => StudentProfile.Create(Student(), Details(guardianPhone: "call me"), Today));
    }

    [Test]
    public void ShouldRejectAGenderThatIsNotDefined()
    {
        var details = Details() with { Gender = (Gender)99 };

        Should.Throw<ArgumentOutOfRangeException>(() => StudentProfile.Create(Student(), details, Today));
    }
}
