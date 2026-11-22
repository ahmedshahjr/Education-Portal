using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolPublicPage.Queries.GetClassOfferings;

/// <summary>Lists a school's class offerings (not deleted). Needs the School module.</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record GetClassOfferingsQuery
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }
}

public class GetClassOfferingsQueryHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetClassOfferingsQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<ClassOfferingDto>> Handle(GetClassOfferingsQuery request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        var offerings = await _context.ClassOfferings
            .AsNoTracking()
            .Where(o => o.SchoolId == schoolId)
            .OrderBy(o => o.Level)
            .ThenBy(o => o.Id)
            .Select(o => new
            {
                o.PublicId,
                o.CampusId,
                o.Level,
                o.AcademicYear,
                o.MonthlyFee,
                o.AdmissionFee,
                o.CurrencyCode,
                o.SeatsTotal,
                o.SeatsAvailable,
                o.Status,
                o.ShowFee,
            })
            .ToListAsync(cancellationToken);

        var campusIds = await CampusPublicIds.ForSchoolAsync(_context, schoolId, cancellationToken);

        return offerings
            .Select(o => new ClassOfferingDto(
                o.PublicId,
                o.CampusId is { } campus && campusIds.TryGetValue(campus, out var publicId) ? publicId : null,
                o.Level,
                o.AcademicYear,
                o.MonthlyFee,
                o.AdmissionFee,
                o.CurrencyCode,
                o.SeatsTotal,
                o.SeatsAvailable,
                o.Status,
                o.ShowFee))
            .ToList();
    }
}
