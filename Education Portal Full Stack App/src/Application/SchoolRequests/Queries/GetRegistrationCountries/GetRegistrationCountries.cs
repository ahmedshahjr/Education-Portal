using EducationPortal.Application.Common.Interfaces;

namespace EducationPortal.Application.SchoolRequests.Queries.GetRegistrationCountries;

/// <summary>A country a visitor can register a school in. Only the public id, name and code are shown.</summary>
public sealed record RegistrationCountryDto(Guid Id, string Name, string Code);

/// <summary>
/// The countries a school can register in: those with a price book, since the trial is priced from it. Anonymous (the public
/// registration form needs it), and it returns no settings, only what the form shows.
/// </summary>
public record GetRegistrationCountriesQuery : IRequest<IReadOnlyList<RegistrationCountryDto>>;

public class GetRegistrationCountriesQueryHandler(IApplicationDbContext context) : IRequestHandler<GetRegistrationCountriesQuery, IReadOnlyList<RegistrationCountryDto>>
{
    public async Task<IReadOnlyList<RegistrationCountryDto>> Handle(GetRegistrationCountriesQuery request, CancellationToken cancellationToken)
    {
        return await context.Countries
            .AsNoTracking()
            .Where(c => context.PriceBooks.Any(b => b.CountryCode == c.Code))
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .Select(c => new RegistrationCountryDto(c.PublicId, c.Name, c.Code))
            .ToListAsync(cancellationToken);
    }
}
