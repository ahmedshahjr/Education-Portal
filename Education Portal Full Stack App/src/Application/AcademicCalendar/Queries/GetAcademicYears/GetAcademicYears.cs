using EducationPortal.Application.AcademicCalendar.Common;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.AcademicCalendar.Queries.GetAcademicYears;

/// <summary>Lists a school's academic years, newest first, each with its terms in order. Needs the Academic setup module.</summary>
[Authorize(Module = ModuleCode.AcademicSetup, Right = AccessRight.View)]
public record GetAcademicYearsQuery
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }
}

public class GetAcademicYearsQueryHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetAcademicYearsQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<AcademicYearDto>> Handle(GetAcademicYearsQuery request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var years = await _context.AcademicYears
            .AsNoTracking()
            .Where(y => y.SchoolId == schoolId)
            .OrderByDescending(y => y.StartDate)
            .Select(y => new { y.Id, y.PublicId, y.Name, y.StartDate, y.EndDate, y.IsCurrent, y.Status })
            .ToListAsync(cancellationToken);

        // One query for the terms of all the years, not one per year (ENG-EF-04).
        var terms = await _context.Terms
            .AsNoTracking()
            .Where(t => t.SchoolId == schoolId)
            .OrderBy(t => t.Sequence)
            .Select(t => new { t.AcademicYearId, Dto = new TermDto(t.PublicId, t.Name, t.Sequence, t.StartDate, t.EndDate) })
            .ToListAsync(cancellationToken);
        var termsByYear = terms.ToLookup(t => t.AcademicYearId, t => t.Dto);

        return years
            .Select(y => new AcademicYearDto(y.PublicId, y.Name, y.StartDate, y.EndDate, y.IsCurrent, y.Status, termsByYear[y.Id].ToList()))
            .ToList();
    }
}
