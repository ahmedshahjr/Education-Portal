using EducationPortal.Application.Campuses.Common;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Campuses.Queries.GetCampuses;

/// <summary>
/// Lists the current school's campuses, the main campus first. The school is the one the request's host resolves to
/// (SEC-AUTHZ-04), so a school sees only its own campuses. Needs the Campuses module (AGENTS.md section 2).
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.Campus)]
public record GetCampusesQuery : IRequest<IReadOnlyList<CampusDto>>;

public class GetCampusesQueryHandler : IRequestHandler<GetCampusesQuery, IReadOnlyList<CampusDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetCampusesQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<CampusDto>> Handle(GetCampusesQuery request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var school = await _context.Schools
            .AsNoTracking()
            .Where(s => s.Id == schoolId)
            .Select(s => new { s.CampusCodePrefixEnabled, s.CampusCodePrefix })
            .SingleAsync(cancellationToken);

        var campuses = await _context.Campuses
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
                b.Address,
                StateId = b.State == null ? (Guid?)null : b.State.PublicId,
                StateName = b.State == null ? null : b.State.Name,
                CityId = b.City == null ? (Guid?)null : b.City.PublicId,
                CityName = b.City == null ? null : b.City.Name,
                b.PostalCode,
                b.Website,
                b.SchoolNumber,
                b.ShortName,
                b.Latitude,
                b.Longitude,
                PrincipalId = b.PrincipalUserDetails == null ? (Guid?)null : b.PrincipalUserDetails.PublicId,
                PrincipalName = b.PrincipalUserDetails == null
                    ? null
                    : b.PrincipalUserDetails.FirstName
                        + (b.PrincipalUserDetails.MiddleName == null ? "" : " " + b.PrincipalUserDetails.MiddleName)
                        + " " + b.PrincipalUserDetails.LastName,
            })
            .ToListAsync(cancellationToken);

        // The phone numbers of all the campuses in one query, in the order each campus gave them.
        var campusIds = campuses.Select(c => c.PublicId).ToList();
        var phones = await (
                from p in _context.CampusPhones.AsNoTracking()
                join c in _context.Campuses.AsNoTracking() on p.CampusId equals c.Id
                where c.SchoolId == schoolId && campusIds.Contains(c.PublicId)
                orderby p.Position
                select new { c.PublicId, p.Number })
            .ToListAsync(cancellationToken);
        var phonesByCampus = phones
            .GroupBy(p => p.PublicId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<string>)g.Select(p => p.Number).ToList());

        var prefix = school.CampusCodePrefixEnabled ? school.CampusCodePrefix ?? string.Empty : string.Empty;

        return campuses
            .Select(c => new CampusDto(
                c.PublicId,
                c.Name,
                c.Code,
                prefix + c.Code,
                c.Kind,
                new CampusDetailsDto(
                    c.Address,
                    c.StateId,
                    c.StateName,
                    c.CityId,
                    c.CityName,
                    c.PostalCode,
                    phonesByCampus.TryGetValue(c.PublicId, out var numbers) ? numbers : [],
                    c.Website,
                    c.SchoolNumber,
                    c.ShortName,
                    c.PrincipalId,
                    c.PrincipalName,
                    c.Latitude,
                    c.Longitude)))
            .ToList();
    }
}
