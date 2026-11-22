using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Campuses.Queries.GetCampusLocations;

/// <summary>A city a campus can be in.</summary>
public sealed record CampusCityDto(Guid Id, string Name);

/// <summary>A state or province a campus can be in, with its cities.</summary>
public sealed record CampusStateDto(Guid Id, string Name, IReadOnlyList<CampusCityDto> Cities);

/// <summary>The school's country (its name, for display) and the states and cities of that country, for a campus's address.</summary>
public sealed record CampusLocationsDto(string? CountryName, IReadOnlyList<CampusStateDto> States);

/// <summary>
/// The states of the current school's country, each with its cities, for the address of a campus. The school comes from the
/// host (SEC-AUTHZ-04). Needs the Campuses module. A school without a country has no states.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.Campus)]
public record GetCampusLocationsQuery;

public class GetCampusLocationsQueryHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetCampusLocationsQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<CampusLocationsDto> Handle(GetCampusLocationsQuery request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var countryId = await _context.Schools
            .AsNoTracking()
            .Where(s => s.Id == schoolId)
            .Select(s => s.CountryId)
            .SingleAsync(cancellationToken);

        if (countryId is null)
        {
            return new CampusLocationsDto(null, []);
        }

        var countryName = await _context.Countries
            .AsNoTracking()
            .Where(c => c.Id == countryId)
            .Select(c => c.Name)
            .SingleAsync(cancellationToken);

        var states = await _context.States
            .AsNoTracking()
            .Where(s => s.CountryId == countryId && s.IsActive)
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.PublicId, s.Name })
            .ToListAsync(cancellationToken);

        var stateIds = states.Select(s => s.Id).ToList();
        var cities = await _context.Cities
            .AsNoTracking()
            .Where(c => stateIds.Contains(c.StateId) && c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new { c.StateId, c.PublicId, c.Name })
            .ToListAsync(cancellationToken);

        var citiesByState = cities.ToLookup(c => c.StateId);

        return new CampusLocationsDto(countryName, states
            .Select(s => new CampusStateDto(
                s.PublicId,
                s.Name,
                citiesByState[s.Id].Select(c => new CampusCityDto(c.PublicId, c.Name)).ToList()))
            .ToList());
    }
}
