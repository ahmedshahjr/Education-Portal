using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Countries.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Countries.Queries.GetCountries;

/// <summary>Lists the countries and their regional settings, for the master portal. Master tenant administrators only (SEC-AUTHZ-07).</summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetCountriesQuery : IRequest<PaginatedList<CountryDto>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 25;
}

public class GetCountriesQueryValidator : AbstractValidator<GetCountriesQuery>
{
    public GetCountriesQueryValidator()
    {
        RuleFor(q => q.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(q => q.PageSize).InclusiveBetween(1, PaginatedList<CountryDto>.MaxPageSize);
    }
}

public class GetCountriesQueryHandler : IRequestHandler<GetCountriesQuery, PaginatedList<CountryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCountriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<CountryDto>> Handle(GetCountriesQuery request, CancellationToken cancellationToken)
    {
        // By name, then by id so the order is stable across pages (ENG-EF-05).
        var countries = _context.Countries
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .Select(c => new CountryDto(c.Id, c.Code, c.Name, c.Languages, c.DefaultLanguage, c.TimeZoneId, c.CurrencyCode));

        return await PaginatedList<CountryDto>.CreateAsync(countries, request.PageNumber, request.PageSize, cancellationToken);
    }
}
