using EducationPortal.Application.Academics.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Academics.Queries.GetAssessments;

/// <summary>
/// The assessments of one course, by name. The course is named by its public id and read through the tenant filter, so a
/// course of another school is not found; the user must be allowed on the course's campus. Needs the Exams module and the
/// View right.
/// </summary>
[Authorize(Module = ModuleCode.Exams, Right = AccessRight.View)]
public record GetAssessmentsQuery(Guid CourseId) : IRequest<IReadOnlyList<AssessmentDto>>;

public class GetAssessmentsQueryValidator : AbstractValidator<GetAssessmentsQuery>
{
    public GetAssessmentsQueryValidator()
    {
        RuleFor(q => q.CourseId).NotEmpty();
    }
}

public class GetAssessmentsQueryHandler : IRequestHandler<GetAssessmentsQuery, IReadOnlyList<AssessmentDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICampusAccess _campusAccess;

    public GetAssessmentsQueryHandler(IApplicationDbContext context, ICampusAccess campusAccess)
    {
        _context = context;
        _campusAccess = campusAccess;
    }

    public async Task<IReadOnlyList<AssessmentDto>> Handle(GetAssessmentsQuery request, CancellationToken cancellationToken)
    {
        var courseId = await _context.Courses.ResolveIdAsync(request.CourseId, cancellationToken);
        var campusId = await _context.Courses.Where(c => c.Id == courseId).Select(c => c.CampusId).SingleAsync(cancellationToken);
        await _campusAccess.EnsureCampusAsync(campusId, ModuleCode.Exams, AccessRight.View, cancellationToken);

        return await _context.Assessments
            .AsNoTracking()
            .Where(a => a.CourseId == courseId)
            .OrderBy(a => a.Name)
            .ThenBy(a => a.Id)
            .Select(a => new AssessmentDto(a.PublicId, a.Name, a.Weight, a.MaximumScore))
            .ToListAsync(cancellationToken);
    }
}
