using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Timetable.Common;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Timetable.Queries.GetTimetableSlots;

/// <summary>
/// The lessons of the timetable by day and start time, optionally for one course. A user sees the lessons of the campuses they
/// are assigned to and the lessons of the whole school (<see cref="ICampusAccess"/>). Needs the Timetable module and the
/// View right.
/// </summary>
[Authorize(Module = ModuleCode.Timetable, Right = AccessRight.View)]
public record GetTimetableSlotsQuery : IRequest<IReadOnlyList<TimetableSlotDto>>
{
    public Guid? CourseId { get; init; }
}

public class GetTimetableSlotsQueryHandler : IRequestHandler<GetTimetableSlotsQuery, IReadOnlyList<TimetableSlotDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICampusAccess _campusAccess;

    public GetTimetableSlotsQueryHandler(IApplicationDbContext context, ICampusAccess campusAccess)
    {
        _context = context;
        _campusAccess = campusAccess;
    }

    public async Task<IReadOnlyList<TimetableSlotDto>> Handle(GetTimetableSlotsQuery request, CancellationToken cancellationToken)
    {
        int? courseId = request.CourseId is { } publicId
            ? await _context.Courses.ResolveIdAsync(publicId, cancellationToken)
            : null;
        var allowed = await _campusAccess.GetAllowedCampusIdsAsync(ModuleCode.Timetable, AccessRight.View, cancellationToken);
        var allowedIds = allowed?.ToList();

        return await (
            from slot in _context.TimetableSlots.AsNoTracking()
            join course in _context.Courses.AsNoTracking() on slot.CourseId equals course.Id
            where (!courseId.HasValue || slot.CourseId == courseId.Value)
                && (allowedIds == null || slot.CampusId == null || allowedIds.Contains(slot.CampusId.Value))
            orderby slot.Day, slot.Start, slot.Id
            select new TimetableSlotDto(
                slot.PublicId,
                course.PublicId,
                course.Name,
                slot.Day,
                slot.Start,
                slot.End,
                _context.Campuses.Where(c => c.Id == slot.CampusId).Select(c => (Guid?)c.PublicId).FirstOrDefault(),
                slot.Room))
            .ToListAsync(cancellationToken);
    }
}
