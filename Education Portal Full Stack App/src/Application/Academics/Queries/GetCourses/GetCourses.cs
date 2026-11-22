using EducationPortal.Application.Academics.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Academics.Queries.GetCourses;

/// <summary>The school's courses, by name, optionally for one class offering. Needs the Academic setup module and the View right.</summary>
[Authorize(Module = ModuleCode.AcademicSetup, Right = AccessRight.View)]
public record GetCoursesQuery
{
    public Guid? ClassOfferingId { get; init; }
}

public class GetCoursesQueryHandler
{
    private readonly IApplicationDbContext _context;

    public GetCoursesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CourseDto>> Handle(GetCoursesQuery request, CancellationToken cancellationToken)
    {
        int? classOfferingId = request.ClassOfferingId is { } publicId
            ? await _context.ClassOfferings.ResolveIdAsync(publicId, cancellationToken)
            : null;

        return await (
            from course in _context.Courses.AsNoTracking()
            join offering in _context.ClassOfferings.AsNoTracking() on course.ClassOfferingId equals offering.Id
            join teacher in _context.UserDetails.AsNoTracking() on course.TeacherUserDetailsId equals teacher.Id
            where !classOfferingId.HasValue || course.ClassOfferingId == classOfferingId.Value
            orderby course.Name
            select new CourseDto(course.PublicId, course.Name, offering.PublicId, teacher.PublicId))
            .ToListAsync(cancellationToken);
    }
}
