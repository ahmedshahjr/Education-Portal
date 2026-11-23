using EducationPortal.Application.Campuses.Common;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Campuses.Queries.GetCampusSettings;

/// <summary>Reads the current school's campus code settings (whether the prefix is on, and the prefix). Needs the Campuses module.</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.Campus)]
public record GetCampusSettingsQuery;

public class GetCampusSettingsQueryHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetCampusSettingsQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<CampusSettingsDto> Handle(GetCampusSettingsQuery request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId;

        return await _context.Schools
            .AsNoTracking()
            .Where(s => s.Id == schoolId)
            .Select(s => new CampusSettingsDto(s.CampusCodePrefixEnabled, s.CampusCodePrefix))
            .SingleAsync(cancellationToken);
    }
}
