using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Modules.Queries.GetSchoolModules;

/// <summary>
/// Lists every module as it stands for one school: core modules are always on, paid modules are on when the school's
/// subscription gives them now. Read only; modules change through the subscription. A master-tenant use case, so the
/// school id may come from the route (SEC-AUTHZ-04, SEC-AUTHZ-07).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetSchoolModulesQuery(Guid SchoolId) : IRequest<IReadOnlyList<SchoolModuleDto>>;

public class GetSchoolModulesQueryHandler : IRequestHandler<GetSchoolModulesQuery, IReadOnlyList<SchoolModuleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IModuleAccess _moduleAccess;

    public GetSchoolModulesQueryHandler(IApplicationDbContext context, IModuleAccess moduleAccess)
    {
        _context = context;
        _moduleAccess = moduleAccess;
    }

    public async Task<IReadOnlyList<SchoolModuleDto>> Handle(GetSchoolModulesQuery request, CancellationToken cancellationToken)
    {
        var school = await _context.Schools
            .AsNoTracking()
            .Where(s => s.PublicId == request.SchoolId)
            .Select(s => new { s.Id, s.IsMaster })
            .FirstOrDefaultAsync(cancellationToken);
        Guard.Against.NotFound(request.SchoolId, school);

        var modules = await _context.Modules
            .AsNoTracking()
            .OrderBy(m => m.SortOrder)
            .ThenBy(m => m.Code)
            .Select(m => new { m.Code, m.Name, m.Description, m.IsCore, m.IsActive })
            .ToListAsync(cancellationToken);

        var enabled = (await _moduleAccess.GetEnabledCodesAsync(school.Id, cancellationToken)).ToHashSet(StringComparer.Ordinal);

        // The master tenant is never limited by modules (AGENTS.md section 2: it oversees every school).
        return modules
            .Select(m => new SchoolModuleDto(m.Code, m.Name, m.Description, m.IsCore, m.IsActive, school.IsMaster || enabled.Contains(m.Code)))
            .ToList();
    }
}
