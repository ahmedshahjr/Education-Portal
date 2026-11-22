using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolPublicPage.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.SchoolPublicPage.Queries.GetFacilities;

/// <summary>Lists the facilities a school shows on its public page. Needs the School module.</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.School)]
public record GetFacilitiesQuery
{
    /// <summary>The school's public id. The master tenant must send it; a school user may leave it out.</summary>
    public Guid? SchoolId { get; init; }
}

public class GetFacilitiesQueryHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetFacilitiesQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<SchoolFacilityDto>> Handle(GetFacilitiesQuery request, CancellationToken cancellationToken)
    {
        var schoolId = await SchoolScope.ResolveAsync(_context, _tenant, request.SchoolId, cancellationToken);

        return await _context.SchoolFacilities
            .AsNoTracking()
            .Where(f => f.SchoolId == schoolId)
            .OrderBy(f => f.Kind)
            .Select(f => new SchoolFacilityDto(f.PublicId, f.Kind, f.Description))
            .ToListAsync(cancellationToken);
    }
}
