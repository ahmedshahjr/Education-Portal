using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Domain.ValueObjects;
using FluentValidation;

namespace EducationPortal.Application.StudentAdmissions.Commands.CreateStudentAdmission;

/// <summary>
/// Admits a student to the current school: the student's name and phone, and the basic fields of a Pakistani school's
/// admission form. The student's person record and the admission form are saved together. Needs the Students module.
/// Audited (SEC-LOG-03) with the ids only, never the name or the family's details.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.Students)]
public record CreateStudentAdmissionCommand
{
    public string FirstName { get; init; } = string.Empty;

    /// <summary>Optional.</summary>
    public string? MiddleName { get; init; }

    public string LastName { get; init; } = string.Empty;

    /// <summary>The student's own phone, when there is one. Optional.</summary>
    public string? Phone { get; init; }

    public DateOnly DateOfBirth { get; init; }

    public Gender Gender { get; init; }

    public string FatherName { get; init; } = string.Empty;

    public string Nationality { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    /// <summary>The class applied for, for example <c>Class 5</c>.</summary>
    public string ClassApplied { get; init; } = string.Empty;

    /// <summary>The school the student left. Optional.</summary>
    public string? PreviousSchool { get; init; }

    /// <summary>The parent or guardian's phone number.</summary>
    public string GuardianPhone { get; init; } = string.Empty;

    public DateOnly AdmissionDate { get; init; }
}

public class CreateStudentAdmissionCommandValidator : AbstractValidator<CreateStudentAdmissionCommand>
{
    public CreateStudentAdmissionCommandValidator(TimeProvider timeProvider)
    {
        // The rules that depend on the date read "today" when the request is checked, not when the validator is built.
        DateOnly Today() => DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);

        RuleFor(c => c.FirstName).NotEmpty().MaximumLength(UserDetails.NameMaxLength);
        RuleFor(c => c.MiddleName).MaximumLength(UserDetails.NameMaxLength);
        RuleFor(c => c.LastName).NotEmpty().MaximumLength(UserDetails.NameMaxLength);
        RuleFor(c => c.Phone).MaximumLength(ContactRules.PhoneMaxLength)
            .Must(value => string.IsNullOrWhiteSpace(value) || ContactRules.IsValidPhone(value.Trim()))
            .WithMessage(_ => ValidationMessages.PhoneInvalid);

        RuleFor(c => c.Gender).IsInEnum();

        RuleFor(c => c.DateOfBirth)
            .Must(dateOfBirth => dateOfBirth <= Today())
            .WithMessage(_ => ValidationMessages.StudentBirthDateInFuture);

        RuleFor(c => c.AdmissionDate)
            .Cascade(CascadeMode.Stop)
            .Must((command, admissionDate) => admissionDate >= command.DateOfBirth)
            .WithMessage(_ => ValidationMessages.StudentAdmissionBeforeBirth)
            .Must(admissionDate => admissionDate <= Today().AddDays(StudentProfile.MaximumAdmissionDaysAhead))
            .WithMessage(_ => string.Format(CultureInfo.CurrentCulture, ValidationMessages.StudentAdmissionTooFarAhead, StudentProfile.MaximumAdmissionDaysAhead));

        // The age is checked only once both dates are in order, so one mistake gives one message.
        RuleFor(c => c.DateOfBirth)
            .Must((command, dateOfBirth) => StudentProfile.AgeOn(dateOfBirth, command.AdmissionDate) is >= StudentProfile.MinimumAgeYears and <= StudentProfile.MaximumAgeYears)
            .When(c => c.AdmissionDate >= c.DateOfBirth)
            .WithMessage(_ => string.Format(CultureInfo.CurrentCulture, ValidationMessages.StudentAgeOutOfRange, StudentProfile.MinimumAgeYears, StudentProfile.MaximumAgeYears));

        RuleFor(c => c.FatherName).NotEmpty().MaximumLength(StudentProfile.FatherNameMaxLength);
        RuleFor(c => c.Nationality).NotEmpty().MaximumLength(StudentProfile.NationalityMaxLength);
        RuleFor(c => c.Address).NotEmpty().MaximumLength(StudentProfile.AddressMaxLength);
        RuleFor(c => c.City).NotEmpty().MaximumLength(StudentProfile.CityMaxLength);
        RuleFor(c => c.ClassApplied).NotEmpty().MaximumLength(StudentProfile.ClassMaxLength);
        RuleFor(c => c.PreviousSchool).MaximumLength(StudentProfile.PreviousSchoolMaxLength);
        RuleFor(c => c.GuardianPhone).NotEmpty().MaximumLength(ContactRules.PhoneMaxLength)
            .Must(value => !string.IsNullOrWhiteSpace(value) && ContactRules.IsValidPhone(value.Trim()))
            .WithMessage(_ => ValidationMessages.PhoneInvalid);
    }
}

public class CreateStudentAdmissionCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ISecurityAuditLog _auditLog;
    private readonly ICurrentTenant _tenant;
    private readonly TimeProvider _timeProvider;

    public CreateStudentAdmissionCommandHandler(IApplicationDbContext context, ISecurityAuditLog auditLog, ICurrentTenant tenant, TimeProvider timeProvider)
    {
        _context = context;
        _auditLog = auditLog;
        _tenant = tenant;
        _timeProvider = timeProvider;
    }

    public async Task<Guid> Handle(CreateStudentAdmissionCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId;
        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);

        var person = UserDetails.Create(schoolId, request.FirstName, request.MiddleName, request.LastName, UserType.Student, request.Phone);
        var profile = StudentProfile.Create(
            person,
            new StudentAdmissionDetails(
                request.DateOfBirth,
                request.Gender,
                request.FatherName,
                request.Nationality,
                request.Address,
                request.City,
                request.ClassApplied,
                request.PreviousSchool,
                request.GuardianPhone,
                request.AdmissionDate),
            today);

        // The profile holds the person, so one save writes both or neither.
        _context.StudentProfiles.Add(profile);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLog.WriteAsync(
            SecurityAuditEventType.UserDetailsCreated,
            SecurityAuditOutcome.Success,
            targetType: nameof(StudentProfile),
            targetId: profile.Id.ToString(CultureInfo.InvariantCulture));

        return person.PublicId;
    }
}
