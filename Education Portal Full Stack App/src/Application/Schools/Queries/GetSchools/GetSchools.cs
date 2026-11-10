using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Schools.Queries.GetSchools;

/// <summary>Lists every school, for the master portal. Master tenant administrators only (SEC-AUTHZ-07).</summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetSchoolsQuery : IRequest<PaginatedList<SchoolSummaryDto>>
{
    public int PageNumber { get; init; } = 1;

    public int PageSize { get; init; } = 25;
}

/// <param name="CountryId">The school's country, or <c>null</c> for the platform defaults.</param>
public sealed record SchoolSummaryDto(int Id, string Name, string Domain, bool IsMaster, int? CountryId);

public class GetSchoolsQueryValidator : AbstractValidator<GetSchoolsQuery>
{
    public GetSchoolsQueryValidator()
    {
        RuleFor(q => q.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(q => q.PageSize).InclusiveBetween(1, PaginatedList<SchoolSummaryDto>.MaxPageSize);
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
        // The master tenant comes first, then the schools by name; the id keeps the order stable (ENG-EF-05).
        var schools = _context.Schools
            .AsNoTracking()
            .OrderByDescending(s => s.IsMaster)
            .ThenBy(s => s.Name)
            .ThenBy(s => s.Id)
            .Select(s => new SchoolSummaryDto(s.Id, s.Name, s.Domain, s.IsMaster, s.CountryId));

        return await PaginatedList<SchoolSummaryDto>.CreateAsync(schools, request.PageNumber, request.PageSize, cancellationToken);
    }
}
