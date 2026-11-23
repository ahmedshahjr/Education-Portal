using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Modules.Common;

namespace EducationPortal.Application.Modules.Queries.GetEnabledModules;

/// <summary>
/// Returns the codes of the modules the current school may use, so the app can show only those. Any signed-in user of
/// the school. The school comes from the request host, never from the client (SEC-AUTHZ-04). The server still checks the
/// module on every request (SEC-AUTHZ-01); this list is only for the menu.
/// </summary>
[Authorize]
public record GetEnabledModulesQuery;

public class GetEnabledModulesQueryHandler
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;
    private readonly IModuleAccess _moduleAccess;

    public GetEnabledModulesQueryHandler(IApplicationDbContext context, ICurrentTenant tenant, IModuleAccess moduleAccess)
    {
        _context = context;
        _tenant = tenant;
        _moduleAccess = moduleAccess;
    }

    public async Task<IReadOnlyList<ModuleCode>> Handle(GetEnabledModulesQuery request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId;

        return await EnabledModules.ForTenantAsync(_context, _moduleAccess, schoolId, _tenant.IsMaster, cancellationToken);
    }
}
