using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Queries.GetShifts;

/// <summary>Lists the shifts of the school's campuses, or of one campus, by start time. Needs the Academic setup module.</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.AcademicSetup)]
public record GetShiftsQuery : IRequest<IReadOnlyList<ShiftDto>>
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }

    /// <summary>Only this campus's shifts, or every campus when left out.</summary>
    public Guid? CampusId { get; init; }
}

public class GetShiftsQueryHandler : IRequestHandler<GetShiftsQuery, IReadOnlyList<ShiftDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetShiftsQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<ShiftDto>> Handle(GetShiftsQuery request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);
        var campusId = await CalendarRules.ResolveOptionalCampusAsync(_context, schoolId, request.CampusId, cancellationToken);

        return await _context.Shifts
            .AsNoTracking()
            .Where(s => s.SchoolId == schoolId && (campusId == null || s.CampusId == campusId))
            .Join(_context.Campuses, s => s.CampusId, c => c.Id, (s, c) => new { Shift = s, CampusPublicId = c.PublicId })
            .OrderBy(x => x.Shift.StartTime)
            .ThenBy(x => x.Shift.Name)
            .Select(x => new ShiftDto(x.Shift.PublicId, x.CampusPublicId, x.Shift.Name, x.Shift.StartTime, x.Shift.EndTime))
            .ToListAsync(cancellationToken);
    }
}
