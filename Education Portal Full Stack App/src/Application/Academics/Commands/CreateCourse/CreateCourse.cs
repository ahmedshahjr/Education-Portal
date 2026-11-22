using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.Academics.Commands.CreateCourse;

/// <summary>Adds a course to a class offering, taught by a teacher of the school. Needs the Academic setup module. Audited.</summary>
[Authorize(Module = ModuleCode.AcademicSetup, Right = AccessRight.Create)]
public record CreateCourseCommand
{
    public Guid ClassOfferingId { get; init; }

    public string Name { get; init; } = string.Empty;

    public Guid TeacherId { get; init; }

    public Guid? CampusId { get; init; }
}

public class CreateCourseCommandValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseCommandValidator()
    {
        RuleFor(c => c.ClassOfferingId).NotEmpty();
        RuleFor(c => c.TeacherId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(Domain.Entities.Course.NameMaxLength);
    }
}

public class CreateCourseCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public CreateCourseCommandHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<Guid> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var classOfferingId = await _context.ClassOfferings.ResolveIdAsync(request.ClassOfferingId, cancellationToken);
        var teacherId = await _context.UserDetails.ResolveIdAsync(request.TeacherId, cancellationToken);
        var teacherType = await _context.UserDetails.Where(u => u.Id == teacherId).Select(u => u.UserType).SingleAsync(cancellationToken);
        if (teacherType != UserType.Teacher)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new ValidationFailure(nameof(request.TeacherId), ValidationMessages.PersonIsNotTeacher)]);
        }

        int? campusId = request.CampusId is { } campus ? await _context.Campuses.ResolveIdAsync(campus, cancellationToken) : null;

        var course = Domain.Entities.Course.Create(schoolId, classOfferingId, request.Name, teacherId, campusId);
        _context.Courses.Add(course);
        await _context.SaveChangesAsync(cancellationToken);

        return course.PublicId;
    }
}
