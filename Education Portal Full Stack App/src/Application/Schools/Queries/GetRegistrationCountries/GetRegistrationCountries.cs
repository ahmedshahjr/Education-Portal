using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Entities;

namespace EducationPortal.Application.Schools.Queries.GetRegistrationCountries;

/// <summary>A country a school can register in.</summary>
public sealed record RegistrationCountryDto(Guid Id, string Name);

/// <summary>
/// The countries the public registration form offers: those with a price book, because the trial is priced from it. Only the
/// name and public id are returned. Open without sign-in, on the master tenant's domain only (like registration itself).
/// </summary>
public record GetRegistrationCountriesQuery : IRequest<IReadOnlyList<RegistrationCountryDto>>;

public class GetRegistrationCountriesQueryHandler : IRequestHandler<GetRegistrationCountriesQuery, IReadOnlyList<RegistrationCountryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;

    public GetRegistrationCountriesQueryHandler(IApplicationDbContext context, ICurrentTenant tenant)
    {
        _context = context;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<RegistrationCountryDto>> Handle(GetRegistrationCountriesQuery request, CancellationToken cancellationToken)
    {
        if (!_tenant.IsMaster)
        {
            throw new NotFoundException("host", nameof(School));
        }

        return await (
                from c in _context.Countries.AsNoTracking()
                join b in _context.PriceBooks.AsNoTracking() on c.Code equals b.CountryCode
                orderby c.Name, c.Id
                select new RegistrationCountryDto(c.PublicId, c.Name))
            .ToListAsync(cancellationToken);
    }
}
