using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Modules.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.Modules.Queries.GetSchoolModules;

/// <summary>
/// Lists every module as it stands for one school: on or off, and whether that comes from the module's default or the
/// master's choice. A master-tenant use case, so the school id may come from the route (SEC-AUTHZ-04, SEC-AUTHZ-07).
/// </summary>
[Authorize(Roles = Roles.Administrator, MasterTenant = true)]
public record GetSchoolModulesQuery(Guid SchoolId) : IRequest<IReadOnlyList<SchoolModuleDto>>;

public class GetSchoolModulesQueryHandler : IRequestHandler<GetSchoolModulesQuery, IReadOnlyList<SchoolModuleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSchoolModulesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SchoolModuleDto>> Handle(GetSchoolModulesQuery request, CancellationToken cancellationToken)
    {
        var school = await _context.Schools
            .AsNoTracking()
            .Where(s => s.PublicId == request.SchoolId)
            .Select(s => new { s.Id, s.IsMaster })
            .FirstOrDefaultAsync(cancellationToken);
        Guard.Against.NotFound(request.SchoolId, school);

        var modules = await ModuleStates.ForSchool(_context, school.Id).ToListAsync(cancellationToken);

        // The master tenant is never limited by modules (AGENTS.md section 2: it oversees every school).
        return modules
            .Select(m => new SchoolModuleDto(
                m.Key,
                m.Name,
                m.Description,
                m.Type,
                m.IsActive,
                school.IsMaster || PortalModule.IsEnabled(m.IsActive, m.Type, m.SchoolChoice),
                FollowsDefault: m.SchoolChoice is null))
            .ToList();
    }
}
