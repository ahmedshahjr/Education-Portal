using EducationPortal.Application.Branches.Common;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Branches.Queries.GetCampusSettings;

/// <summary>Reads the current school's campus code settings (whether the prefix is on, and the prefix). Needs the Branches module.</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.Branch)]
public record GetCampusSettingsQuery : IRequest<CampusSettingsDto>;

public class GetCampusSettingsQueryHandler : IRequestHandler<GetCampusSettingsQuery, CampusSettingsDto>
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
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        return await _context.Schools
            .AsNoTracking()
            .Where(s => s.Id == schoolId)
            .Select(s => new CampusSettingsDto(s.CampusCodePrefixEnabled, s.CampusCodePrefix))
            .SingleAsync(cancellationToken);
    }
}
