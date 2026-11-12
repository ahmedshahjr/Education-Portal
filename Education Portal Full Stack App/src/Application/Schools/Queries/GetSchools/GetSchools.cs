using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Schools.Queries.GetSchools;

/// <summary>
/// Lists the schools, for the master portal, optionally only those whose name contains <see cref="Search"/> and those in
/// one country. Master tenant administrators only (SEC-AUTHZ-07).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetSchoolsQuery : IRequest<PaginatedList<SchoolSummaryDto>>
{
    /// <summary>The longest search text accepted (SEC-INPUT-01). School names are at most 200 characters.</summary>
    public const int MaxSearchLength = 100;

    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 25;

    /// <summary>Only schools whose name contains this text, ignoring case. Blank means every school.</summary>
    public string? Search { get; init; }

    /// <summary>Only schools in this country. <c>null</c> means every school.</summary>
    public Guid? CountryId { get; init; }
}

/// <param name="CountryId">The school's country, or <c>null</c> for the platform defaults.</param>
public sealed record SchoolSummaryDto(Guid Id, string Name, string Domain, bool IsMaster, Guid? CountryId);

public class GetSchoolsQueryValidator : AbstractValidator<GetSchoolsQuery>
{
    public GetSchoolsQueryValidator()
    {
        RuleFor(q => q.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(q => q.PageSize).InclusiveBetween(1, PaginatedList<SchoolSummaryDto>.MaxPageSize);
        RuleFor(q => q.Search).MaximumLength(GetSchoolsQuery.MaxSearchLength);
        RuleFor(q => q.CountryId).NotEqual(Guid.Empty);
    }
}

public class GetSchoolsQueryHandler : IRequestHandler<GetSchoolsQuery, PaginatedList<SchoolSummaryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSchoolsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<SchoolSummaryDto>> Handle(GetSchoolsQuery request, CancellationToken cancellationToken)
    {
        IQueryable<School> schools = _context.Schools.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            // A parameterised, case-insensitive "contains"; EF Core escapes LIKE wildcards in the text (SEC-INPUT-02).
            var search = request.Search.Trim().ToLowerInvariant();
            schools = schools.Where(s => s.Name.ToLower().Contains(search));
        }

        if (request.CountryId is { } countryId)
        {
            schools = schools.Where(s => _context.Countries.Any(c => c.Id == s.CountryId && c.PublicId == countryId));
        }

        // The master tenant comes first, then the schools by name; the id keeps the order stable (ENG-EF-05).
        var page = schools
            .OrderByDescending(s => s.IsMaster)
            .ThenBy(s => s.Name)
            .ThenBy(s => s.Id)
            .Select(s => new SchoolSummaryDto(
                s.PublicId,
                s.Name,
                s.Domain,
                s.IsMaster,
                _context.Countries.Where(c => c.Id == s.CountryId).Select(c => (Guid?)c.PublicId).FirstOrDefault()));

        return await PaginatedList<SchoolSummaryDto>.CreateAsync(page, request.PageNumber, request.PageSize, cancellationToken);
    }
}
