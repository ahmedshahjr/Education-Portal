using EducationPortal.Application.Branches.Common;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Branches.Queries.GetBranches;

/// <summary>
/// Lists the current school's campuses, the main campus first. The school is the one the request's host resolves to
/// (SEC-AUTHZ-04), so a school sees only its own campuses. Needs the Branches module (AGENTS.md section 2).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.Branch)]
public record GetBranchesQuery : IRequest<IReadOnlyList<BranchDto>>;

public class GetBranchesQueryHandler : IRequestHandler<GetBranchesQuery, IReadOnlyList<BranchDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetBranchesQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<BranchDto>> Handle(GetBranchesQuery request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var school = await _context.Schools
            .AsNoTracking()
            .Where(s => s.Id == schoolId)
            .Select(s => new { s.CampusCodePrefixEnabled, s.CampusCodePrefix })
            .SingleAsync(cancellationToken);

        var campuses = await _context.Branches
            .AsNoTracking()
            .Where(b => b.SchoolId == schoolId)
            .OrderBy(b => b.Kind)
            .ThenBy(b => b.Name)
            .Select(b => new
            {
                b.PublicId,
                b.Name,
                b.Code,
                b.Kind,
                b.StateId,
                b.CityId,
                b.AddressLine,
                b.Latitude,
                b.Longitude,
                b.GooglePlaceId,
                b.GoogleMapsUrl,
                Shifts = b.Shifts.OrderBy(s => s.StartTime).Select(s => new CampusShiftDto(s.Name, s.StartTime, s.EndTime)),
            })
            .ToListAsync(cancellationToken);

        // The campus keeps the province and city as internal ids; the API shows their public ids.
        var stateIds = campuses.Where(c => c.StateId is not null).Select(c => c.StateId!.Value).Distinct().ToList();
        var cityIds = campuses.Where(c => c.CityId is not null).Select(c => c.CityId!.Value).Distinct().ToList();

        var statePublicIds = await _context.States
            .AsNoTracking()
            .Where(s => stateIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.PublicId, cancellationToken);

        var cityPublicIds = await _context.Cities
            .AsNoTracking()
            .Where(c => cityIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id, c => c.PublicId, cancellationToken);

        var prefix = school.CampusCodePrefixEnabled ? school.CampusCodePrefix ?? string.Empty : string.Empty;

        return campuses
            .Select(c => new BranchDto(
                c.PublicId,
                c.Name,
                c.Code,
                prefix + c.Code,
                c.Kind,
                new CampusLocationDto(
                    c.StateId is { } stateId ? statePublicIds[stateId] : null,
                    c.CityId is { } cityId ? cityPublicIds[cityId] : null,
                    c.AddressLine,
                    c.Latitude,
                    c.Longitude,
                    c.GooglePlaceId,
                    c.GoogleMapsUrl),
                c.Shifts.ToList()))
            .ToList();
    }
}
