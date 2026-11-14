using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Schools.Queries.GetSchools;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Schools.Queries.GetSchool;

/// <summary>Reads one school (name, domain, country). A master-tenant use case, so the school id may come from the route (SEC-AUTHZ-04, SEC-AUTHZ-07).</summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetSchoolQuery(Guid SchoolId) : IRequest<SchoolSummaryDto>;

public class GetSchoolQueryHandler : IRequestHandler<GetSchoolQuery, SchoolSummaryDto>
{
    private readonly IApplicationDbContext _context;

    public GetSchoolQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SchoolSummaryDto> Handle(GetSchoolQuery request, CancellationToken cancellationToken)
    {
        var school = await _context.Schools
            .AsNoTracking()
            .Where(s => s.PublicId == request.SchoolId)
            .Select(s => new SchoolSummaryDto(
                s.PublicId,
                s.Name,
                s.Domain,
                s.IsMaster,
                _context.Countries.Where(c => c.Id == s.CountryId).Select(c => (Guid?)c.PublicId).FirstOrDefault()))
            .FirstOrDefaultAsync(cancellationToken);
        Guard.Against.NotFound(request.SchoolId, school);

        return school;
    }
}
