using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.Academics.Commands.EnrolStudent;

/// <summary>Enrols a student of the school in a class offering, once. Needs the Academic setup module.</summary>
[Authorize(Module = ModuleCode.AcademicSetup, Right = AccessRight.Create)]
public record EnrolStudentCommand
{
    public Guid StudentId { get; init; }

    public Guid ClassOfferingId { get; init; }
}

public class EnrolStudentCommandValidator : AbstractValidator<EnrolStudentCommand>
{
    public EnrolStudentCommandValidator()
    {
        RuleFor(c => c.StudentId).NotEmpty();
        RuleFor(c => c.ClassOfferingId).NotEmpty();
    }
}

public class EnrolStudentCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public EnrolStudentCommandHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(EnrolStudentCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId;

        var studentId = await _context.UserDetails.ResolveIdAsync(request.StudentId, cancellationToken);
        var classOfferingId = await _context.ClassOfferings.ResolveIdAsync(request.ClassOfferingId, cancellationToken);

        var studentType = await _context.UserDetails.Where(u => u.Id == studentId).Select(u => u.UserType).SingleAsync(cancellationToken);
        if (studentType != UserType.Student)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new ValidationFailure(nameof(request.StudentId), ValidationMessages.StudentIsNotStudent)]);
        }

        var alreadyEnrolled = await _context.Enrolments.AnyAsync(
            e => e.StudentUserDetailsId == studentId && e.ClassOfferingId == classOfferingId, cancellationToken);
        if (alreadyEnrolled)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new ValidationFailure(nameof(request.StudentId), ValidationMessages.StudentAlreadyEnrolled)]);
        }

        var enrolment = Domain.Entities.Enrolment.Create(schoolId, studentId, classOfferingId);
        _context.Enrolments.Add(enrolment);
        await _context.SaveChangesAsync(cancellationToken);

        return enrolment.PublicId;
    }
}
