using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Locations.Common;

namespace EducationPortal.Application.Locations.Queries.GetStates;

/// <summary>The active states of an active country, by name, for the address dropdowns. Any signed-in user (see <see cref="Queries.GetLocationCountries.GetLocationCountriesQuery"/>).</summary>
[Authorize]
public record GetStatesQuery(Guid CountryId) : IRequest<IReadOnlyList<StateDto>>;

public class GetStatesQueryValidator : AbstractValidator<GetStatesQuery>
{
    public GetStatesQueryValidator()
    {
        RuleFor(q => q.CountryId).NotEmpty();
    }
}

public class GetStatesQueryHandler : IRequestHandler<GetStatesQuery, IReadOnlyList<StateDto>>
{
    private readonly IApplicationDbContext _context;

    public GetStatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<StateDto>> Handle(GetStatesQuery request, CancellationToken cancellationToken)
    {
        // An unknown or inactive country is 404, so the caller can tell it from a country with no states.
        var countryId = await _context.Countries
            .Where(c => c.PublicId == request.CountryId && c.IsActive)
            .Select(c => (int?)c.Id)
            .SingleOrDefaultAsync(cancellationToken);
        Guard.Against.NotFound(request.CountryId, countryId);

        return await _context.States
            .AsNoTracking()
            .Where(s => s.CountryId == countryId && s.IsActive)
            .OrderBy(s => s.Name)
            .ThenBy(s => s.Id)
            .Select(s => new StateDto(s.PublicId, s.Name, s.NativeName, s.IsoCode, s.Type))
            .ToListAsync(cancellationToken);
    }
}
