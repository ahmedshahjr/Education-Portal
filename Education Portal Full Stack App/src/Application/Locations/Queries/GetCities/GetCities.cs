using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Locations.Common;

namespace EducationPortal.Application.Locations.Queries.GetCities;

/// <summary>The active cities of an active state, by name, for the address dropdowns. Any signed-in user (see <see cref="Queries.GetLocationCountries.GetLocationCountriesQuery"/>).</summary>
[Authorize]
public record GetCitiesQuery(Guid StateId) : IRequest<IReadOnlyList<CityDto>>;

public class GetCitiesQueryValidator : AbstractValidator<GetCitiesQuery>
{
    public GetCitiesQueryValidator()
    {
        RuleFor(q => q.StateId).NotEmpty();
    }
}

public class GetCitiesQueryHandler : IRequestHandler<GetCitiesQuery, IReadOnlyList<CityDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCitiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CityDto>> Handle(GetCitiesQuery request, CancellationToken cancellationToken)
    {
        // An unknown or inactive state (or one whose country is inactive) is 404.
        var stateId = await _context.States
            .Where(s => s.PublicId == request.StateId && s.IsActive)
            .Join(_context.Countries.Where(c => c.IsActive), s => s.CountryId, c => c.Id, (s, _) => (int?)s.Id)
            .SingleOrDefaultAsync(cancellationToken);
        Guard.Against.NotFound(request.StateId, stateId);

        return await _context.Cities
            .AsNoTracking()
            .Where(c => c.StateId == stateId && c.IsActive)
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .Select(c => new CityDto(c.PublicId, c.Name, c.NativeName, c.Latitude, c.Longitude))
            .ToListAsync(cancellationToken);
    }
}
