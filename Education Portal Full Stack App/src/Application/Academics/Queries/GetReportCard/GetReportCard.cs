using EducationPortal.Application.Academics.Common;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Academics;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Academics.Queries.GetReportCard;

/// <summary>
/// A student's report card for one class, graded with one grading scale. Only the assessments that have a recorded score count;
/// a course without any has no percent yet (ADR-019). Needs the Exams module and the View right; access is decided by
/// <see cref="ReportCardAccess"/>.
/// </summary>
[Authorize(Module = ModuleCode.Exams, Right = AccessRight.View)]
public record GetReportCardQuery
{
    public Guid StudentId { get; init; }

    public Guid ClassOfferingId { get; init; }

    public Guid GradingScaleId { get; init; }
}

public class GetReportCardQueryHandler
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public GetReportCardQueryHandler(IApplicationDbContext context, IIdentityService identityService, IUser user)
    {
        _context = context;
        _identityService = identityService;
        _user = user;
    }

    public async Task<ReportCardDto> Handle(GetReportCardQuery request, CancellationToken cancellationToken)
    {
        var studentId = await _context.UserDetails.ResolveIdAsync(request.StudentId, cancellationToken);
        var classOfferingId = await _context.ClassOfferings.ResolveIdAsync(request.ClassOfferingId, cancellationToken);
        var scaleId = await _context.GradingScales.ResolveIdAsync(request.GradingScaleId, cancellationToken);

        var view = await ReportCardAccess.ResolveAsync(_context, _identityService, _user, studentId, classOfferingId, cancellationToken);

        var enrolmentRow = await _context.Enrolments.AsNoTracking()
            .Where(e => e.StudentUserDetailsId == studentId && e.ClassOfferingId == classOfferingId)
            .Select(e => new { e.Id })
            .FirstOrDefaultAsync(cancellationToken);
        Guard.Against.NotFound(request.StudentId, enrolmentRow);
        var enrolment = enrolmentRow.Id;

        var bands = await _context.GradingBands.AsNoTracking()
            .Where(b => b.GradingScaleId == scaleId)
            .Select(b => new GradeBand(b.MinPercent, b.MaxPercent, b.Letter, b.Points))
            .ToListAsync(cancellationToken);

        var coursesQuery = _context.Courses.AsNoTracking().Where(c => c.ClassOfferingId == classOfferingId);
        if (!view.SeesAllCourses)
        {
            coursesQuery = coursesQuery.Where(c => c.TeacherUserDetailsId == view.TeacherPersonId);
        }

        var courses = await coursesQuery.OrderBy(c => c.Name).Select(c => new { c.Id, c.Name }).ToListAsync(cancellationToken);

        var scores = await (
            from score in _context.Scores.AsNoTracking()
            join assessment in _context.Assessments.AsNoTracking() on score.AssessmentId equals assessment.Id
            where score.EnrolmentId == enrolment
            select new { assessment.CourseId, score.Value, assessment.MaximumScore, assessment.Weight })
            .ToListAsync(cancellationToken);

        var results = courses
            .Select(course => new CourseResult(
                course.Name,
                scores
                    .Where(s => s.CourseId == course.Id)
                    .Select(s => new AssessmentScore(s.Value, s.MaximumScore, s.Weight))
                    .ToList()))
            .ToList();

        var card = ReportCard.Build(results, bands);

        return new ReportCardDto(
            card.Lines.Select(l => new ReportCardLineDto(l.CourseName, l.Percent, l.Letter, l.Points)).ToList(),
            card.Gpa);
    }
}
