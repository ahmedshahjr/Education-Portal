using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.SchoolRoles.Common;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.SchoolRoles.Queries.GetRoles;

/// <summary>
/// Lists the current school's roles with their rights in the modules the school has now. The school comes from the
/// host, never from the caller (SEC-AUTHZ-04). Refused with MODULE_NOT_ENABLED when the school does not have Users &amp; Roles.
/// </summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCodes.UsersRoles)]
public record GetRolesQuery : IRequest<IReadOnlyList<SchoolRoleDto>>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, IReadOnlyList<SchoolRoleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IModuleAccess _moduleAccess;
    private readonly ICurrentTenant _tenant;

    public GetRolesQueryHandler(IApplicationDbContext context, IModuleAccess moduleAccess, ICurrentTenant tenant)
    {
        _context = context;
        _moduleAccess = moduleAccess;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<SchoolRoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();
        return await SchoolRoleReader.LoadAsync(_context, _moduleAccess, schoolId, null, cancellationToken);
    }
}
