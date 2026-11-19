using EducationPortal.Application.Academics.Common;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Localization;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using FluentValidation.Results;

namespace EducationPortal.Application.Academics.Commands.RecordScore;

/// <summary>
/// Records a student's score in an assessment, or changes it. The student must be enrolled in the course's class, the score
/// must lie within the assessment's maximum, and only the course's teacher (or an administrator) grades it. Audited (ADR-016).
/// </summary>
[Authorize(Module = ModuleCode.Exams, Right = AccessRight.Edit)]
public record RecordScoreCommand : IRequest
{
    public Guid AssessmentId { get; init; }

    public Guid StudentId { get; init; }

    public decimal Value { get; init; }
}

public class RecordScoreCommandValidator : AbstractValidator<RecordScoreCommand>
{
    public RecordScoreCommandValidator()
    {
        RuleFor(c => c.AssessmentId).NotEmpty();
        RuleFor(c => c.StudentId).NotEmpty();
        RuleFor(c => c.Value).GreaterThanOrEqualTo(0m);
    }
}

public class RecordScoreCommandHandler : IRequestHandler<RecordScoreCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public RecordScoreCommandHandler(IApplicationDbContext context, ICurrentTenant tenant, IIdentityService identityService, IUser user)
    {
        _context = context;
        _tenant = tenant;
        _identityService = identityService;
        _user = user;
    }

    public async Task Handle(RecordScoreCommand request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var assessmentId = await _context.Assessments.ResolveIdAsync(request.AssessmentId, cancellationToken);
        var assessment = await _context.Assessments.AsNoTracking().FirstAsync(a => a.Id == assessmentId, cancellationToken);
        var course = await _context.Courses.AsNoTracking().FirstAsync(c => c.Id == assessment.CourseId, cancellationToken);
        await CourseOwnership.EnsureCanGradeAsync(_identityService, _user, course.TeacherUserDetailsId, cancellationToken);

        if (request.Value > assessment.MaximumScore)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new ValidationFailure(nameof(request.Value), ValidationMessages.ScoreOutOfRange)]);
        }

        var studentId = await _context.UserDetails.ResolveIdAsync(request.StudentId, cancellationToken);
        var enrolmentId = await _context.Enrolments
            .Where(e => e.StudentUserDetailsId == studentId && e.ClassOfferingId == course.ClassOfferingId)
            .Select(e => (int?)e.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (enrolmentId is not { } enrolment)
        {
            throw new EducationPortal.Application.Common.Exceptions.ValidationException(
                [new ValidationFailure(nameof(request.StudentId), ValidationMessages.StudentNotInCourseClass)]);
        }

        var existing = await _context.Scores.FirstOrDefaultAsync(s => s.AssessmentId == assessmentId && s.EnrolmentId == enrolment, cancellationToken);
        if (existing is null)
        {
            _context.Scores.Add(Score.Record(schoolId, assessmentId, enrolment, request.Value, assessment.MaximumScore));
        }
        else
        {
            existing.Change(request.Value, assessment.MaximumScore);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
