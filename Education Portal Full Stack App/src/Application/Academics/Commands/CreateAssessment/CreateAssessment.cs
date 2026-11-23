using EducationPortal.Application.Academics.Common;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.Academics.Commands.CreateAssessment;

/// <summary>
/// Adds an assessment to a course: its name, its weight in the course percent and the most it gives. The weights of a course's
/// assessments cannot add up to more than 100. The teacher of the course (or an administrator) adds it. Needs the Exams module.
/// </summary>
[Authorize(Module = ModuleCode.Exams, Right = AccessRight.Create)]
public record CreateAssessmentCommand
{
    public Guid CourseId { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal Weight { get; init; }

    public decimal MaximumScore { get; init; }
}

public class CreateAssessmentCommandValidator : AbstractValidator<CreateAssessmentCommand>
{
    public CreateAssessmentCommandValidator()
    {
        RuleFor(c => c.CourseId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(Domain.Entities.Assessment.NameMaxLength);
        RuleFor(c => c.Weight).InclusiveBetween(0m, 100m);
        RuleFor(c => c.MaximumScore).GreaterThan(0m);
    }
}

public class CreateAssessmentCommandHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public CreateAssessmentCommandHandler(IApplicationDbContext context, ICurrentTenant tenant, IIdentityService identityService, IUser user)
    {
        _context = context;
        _tenant = tenant;
        _identityService = identityService;
        _user = user;
    }

    public async Task<Guid> Handle(CreateAssessmentCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId;

        var courseId = await _context.Courses.ResolveIdAsync(request.CourseId, cancellationToken);
        var course = await _context.Courses.AsNoTracking().FirstAsync(c => c.Id == courseId, cancellationToken);
        await CourseOwnership.EnsureCanGradeAsync(_identityService, _user, course.TeacherUserDetailsId, cancellationToken);

        var weightSoFar = await _context.Assessments.Where(a => a.CourseId == courseId).SumAsync(a => a.Weight, cancellationToken);
        if (weightSoFar + request.Weight > 100m)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new ValidationFailure(nameof(request.Weight), ValidationMessages.AssessmentWeightsTooHigh)]);
        }

        var assessment = Domain.Entities.Assessment.Create(schoolId, courseId, request.Name, request.Weight, request.MaximumScore);
        _context.Assessments.Add(assessment);
        await _context.SaveChangesAsync(cancellationToken);

        return assessment.PublicId;
    }
}
