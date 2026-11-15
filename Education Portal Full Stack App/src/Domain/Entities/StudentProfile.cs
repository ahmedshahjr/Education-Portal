using EducationPortal.Domain.Common;
using EducationPortal.Domain.Enums;
using EducationPortal.Domain.ValueObjects;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A student's admission form (the table <c>StudentProfiles</c>): the basic fields of a Pakistani school's admission form,
/// on top of the student's name and phone in <see cref="UserDetails"/>. One profile per student. Owned by the school
/// (<see cref="SchoolId"/>) and covered by the tenant query filter (SEC-AUTHZ-03).
/// </summary>
/// <remarks>
/// Not in this profile, on purpose: the B-Form or CNIC number, blood group, religion and medical notes. They are personal
/// data of a special category or government identifiers, and they wait for the column-encryption decision
/// (SEC-CRYPTO-04, student-data-privacy).
/// </remarks>
public class StudentProfile : BaseAuditableEntity, ITenantOwned
{
    public const int FatherNameMaxLength = 100;
    public const int NationalityMaxLength = 60;
    public const int AddressMaxLength = 300;
    public const int CityMaxLength = 100;
    public const int ClassMaxLength = 50;
    public const int PreviousSchoolMaxLength = 200;

    /// <summary>The youngest and oldest age, in whole years on the admission date, that the portal accepts.</summary>
    public const int MinimumAgeYears = 3;
    public const int MaximumAgeYears = 30;

    /// <summary>How many days after today an admission date may be (a student can be admitted for next year).</summary>
    public const int MaximumAdmissionDaysAhead = 366;

    private StudentProfile()
    {
        // For EF Core.
        FatherName = string.Empty;
        Nationality = string.Empty;
        Address = string.Empty;
        City = string.Empty;
        ClassApplied = string.Empty;
        GuardianPhone = string.Empty;
        UserDetails = null!;
    }

    private StudentProfile(UserDetails person, StudentAdmissionDetails details)
    {
        SchoolId = person.SchoolId;
        UserDetails = person;
        DateOfBirth = details.DateOfBirth;
        Gender = details.Gender;
        FatherName = details.FatherName;
        Nationality = details.Nationality;
        Address = details.Address;
        City = details.City;
        ClassApplied = details.ClassApplied;
        PreviousSchool = details.PreviousSchool;
        GuardianPhone = details.GuardianPhone;
        AdmissionDate = details.AdmissionDate;
    }

    /// <summary>
    /// Creates the admission form of a student. The person must be of type <see cref="UserType.Student"/>. The student is
    /// <see cref="MinimumAgeYears"/> to <see cref="MaximumAgeYears"/> years old on the admission date; the birth date is
    /// not after <paramref name="today"/>; the admission date is not before the birth date and at most
    /// <see cref="MaximumAdmissionDaysAhead"/> days after <paramref name="today"/>. Text is trimmed; a blank previous school
    /// is stored as <c>null</c>.
    /// </summary>
    public static StudentProfile Create(UserDetails person, StudentAdmissionDetails details, DateOnly today)
    {
        ArgumentNullException.ThrowIfNull(person);
        ArgumentNullException.ThrowIfNull(details);

        if (person.UserType != UserType.Student)
        {
            throw new ArgumentException("Only a person of type Student can have an admission form.", nameof(person));
        }

        if (!Enum.IsDefined(details.Gender))
        {
            throw new ArgumentOutOfRangeException(nameof(details));
        }

        var values = new StudentAdmissionDetails(
            details.DateOfBirth,
            details.Gender,
            Required(details.FatherName, FatherNameMaxLength, nameof(details.FatherName)),
            Required(details.Nationality, NationalityMaxLength, nameof(details.Nationality)),
            Required(details.Address, AddressMaxLength, nameof(details.Address)),
            Required(details.City, CityMaxLength, nameof(details.City)),
            Required(details.ClassApplied, ClassMaxLength, nameof(details.ClassApplied)),
            Optional(details.PreviousSchool, PreviousSchoolMaxLength, nameof(details.PreviousSchool)),
            Phone(details.GuardianPhone),
            details.AdmissionDate);

        if (values.DateOfBirth > today)
        {
            throw new ArgumentOutOfRangeException(nameof(details), "The date of birth cannot be in the future.");
        }

        if (values.AdmissionDate < values.DateOfBirth)
        {
            throw new ArgumentOutOfRangeException(nameof(details), "The admission date cannot be before the date of birth.");
        }

        if (values.AdmissionDate > today.AddDays(MaximumAdmissionDaysAhead))
        {
            throw new ArgumentOutOfRangeException(nameof(details), "The admission date is too far in the future.");
        }

        var age = AgeOn(values.DateOfBirth, values.AdmissionDate);
        if (age < MinimumAgeYears || age > MaximumAgeYears)
        {
            throw new ArgumentOutOfRangeException(nameof(details), $"The student must be {MinimumAgeYears} to {MaximumAgeYears} years old on the admission date.");
        }

        return new StudentProfile(person, values);
    }

    /// <summary>The age in whole years on <paramref name="on"/>.</summary>
    public static int AgeOn(DateOnly dateOfBirth, DateOnly on)
    {
        var age = on.Year - dateOfBirth.Year;
        if (on < dateOfBirth.AddYears(age))
        {
            age--;
        }

        return age;
    }

    private static string Required(string value, int maxLength, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        var trimmed = value.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, maxLength, name);
        return trimmed;
    }

    private static string? Optional(string? value, int maxLength, string name)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return null;
        }

        ArgumentOutOfRangeException.ThrowIfGreaterThan(trimmed.Length, maxLength, name);
        return trimmed;
    }

    private static string Phone(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(StudentAdmissionDetails.GuardianPhone));
        var trimmed = value.Trim();
        if (!ContactRules.IsValidPhone(trimmed))
        {
            throw new ArgumentException("A phone number has 7 to 15 digits and an optional leading +.", nameof(StudentAdmissionDetails.GuardianPhone));
        }

        return trimmed;
    }

    public int SchoolId { get; private set; }

    /// <summary>The student's person record: the name and the student's own phone. One student, one profile.</summary>
    public int UserDetailsId { get; private set; }

    public UserDetails UserDetails { get; private set; }

    public DateOnly DateOfBirth { get; private set; }

    public Gender Gender { get; private set; }

    public string FatherName { get; private set; }

    public string Nationality { get; private set; }

    public string Address { get; private set; }

    public string City { get; private set; }

    /// <summary>The class the student is applying for, for example <c>Class 5</c>.</summary>
    public string ClassApplied { get; private set; }

    /// <summary>The school the student left, when the form gives one.</summary>
    public string? PreviousSchool { get; private set; }

    /// <summary>The parent or guardian's phone number.</summary>
    public string GuardianPhone { get; private set; }

    public DateOnly AdmissionDate { get; private set; }
}

/// <summary>The fields of an admission form, as the administrator enters them.</summary>
public sealed record StudentAdmissionDetails(
    DateOnly DateOfBirth,
    Gender Gender,
    string FatherName,
    string Nationality,
    string Address,
    string City,
    string ClassApplied,
    string? PreviousSchool,
    string GuardianPhone,
    DateOnly AdmissionDate);
