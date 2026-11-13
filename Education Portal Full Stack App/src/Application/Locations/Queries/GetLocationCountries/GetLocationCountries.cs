using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Locations.Common;

namespace EducationPortal.Application.Locations.Queries.GetLocationCountries;

/// <summary>
/// The active countries for the address dropdowns, by name. Location data is platform reference data, not owned by a
/// school and not part of a module, so any signed-in user on a school or the master domain may read it.
/// </summary>
[Authorize]
public record GetLocationCountriesQuery : IRequest<IReadOnlyList<LocationCountryDto>>;

public class GetLocationCountriesQueryHandler : IRequestHandler<GetLocationCountriesQuery, IReadOnlyList<LocationCountryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLocationCountriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LocationCountryDto>> Handle(GetLocationCountriesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Countries
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .Select(c => new LocationCountryDto(c.PublicId, c.Code, c.Iso3, c.Name, c.NativeName, c.PhoneCode, c.CurrencyCode))
            .ToListAsync(cancellationToken);
    }
}
