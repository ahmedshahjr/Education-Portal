using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Overview.Queries.GetSchoolCampusOverview;

/// <summary>One campus in the master's overview: its code, its kind and its place (province and city) when it has one.</summary>
public sealed record CampusOverviewDto(Guid Id, string Name, string Code, CampusKind Kind, string? StateName, string? CityName);

/// <summary>One school in the master's overview, with its campuses in order (main campus first).</summary>
public sealed record SchoolCampusOverviewDto(Guid Id, string Name, string? Domain, string? CountryName, bool IsActive, IReadOnlyList<CampusOverviewDto> Campuses);

/// <summary>
/// Lists every school with its campuses, for the master portal's dashboard. The master tenant is the software seller and owns
/// no school, so it is not listed. Master tenant administrators only (SEC-AUTHZ-07); the tenant filters do not apply to the
/// master, so this is the one place that reads every school's campuses.
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetSchoolCampusOverviewQuery : IRequest<IReadOnlyList<SchoolCampusOverviewDto>>;

public class GetSchoolCampusOverviewQueryHandler : IRequestHandler<GetSchoolCampusOverviewQuery, IReadOnlyList<SchoolCampusOverviewDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSchoolCampusOverviewQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SchoolCampusOverviewDto>> Handle(GetSchoolCampusOverviewQuery request, CancellationToken cancellationToken)
    {
        var schools = await _context.Schools
            .AsNoTracking()
            .Where(s => !s.IsMaster)
            .OrderBy(s => s.Name)
            .ThenBy(s => s.Id)
            .Select(s => new
            {
                s.Id,
                s.PublicId,
                s.Name,
                s.Domain,
                s.IsActive,
                CountryName = _context.Countries.Where(c => c.Id == s.CountryId).Select(c => c.Name).FirstOrDefault(),
            })
            .ToListAsync(cancellationToken);

        var campuses = await _context.Campuses
            .AsNoTracking()
            .Where(c => _context.Schools.Any(s => s.Id == c.SchoolId && !s.IsMaster))
            .OrderBy(c => c.Kind)
            .ThenBy(c => c.Name)
            .ThenBy(c => c.Id)
            .Select(c => new
            {
                c.SchoolId,
                c.PublicId,
                c.Name,
                c.Code,
                c.Kind,
                StateName = c.State == null ? null : c.State.Name,
                CityName = c.City == null ? null : c.City.Name,
            })
            .ToListAsync(cancellationToken);

        var bySchool = campuses.ToLookup(c => c.SchoolId);

        return schools
            .Select(s => new SchoolCampusOverviewDto(
                s.PublicId,
                s.Name,
                s.Domain,
                s.CountryName,
                s.IsActive,
                bySchool[s.Id]
                    .Select(c => new CampusOverviewDto(c.PublicId, c.Name, c.Code, c.Kind, c.StateName, c.CityName))
                    .ToList()))
            .ToList();
    }
}
