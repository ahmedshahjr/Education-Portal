using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Users.Queries.GetAccountRoles;

/// <summary>The public ids of the school roles a person's login account holds. Needs the Users &amp; Roles module.</summary>
[Authorize(Roles = Roles.Administrator, Module = ModuleCode.UsersRoles)]
public record GetAccountRolesQuery
{
    public Guid PersonId { get; init; }
}

public class GetAccountRolesQueryHandler
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly ICurrentTenant _tenant;

    public GetAccountRolesQueryHandler(IApplicationDbContext context, IIdentityService identityService, ICurrentTenant tenant)
    {
        _context = context;
        _identityService = identityService;
        _tenant = tenant;
    }

    public async Task<IReadOnlyList<Guid>> Handle(GetAccountRolesQuery request, CancellationToken cancellationToken)
    {
        var schoolId = _tenant.SchoolId ?? throw new ForbiddenAccessException();

        var personId = await _context.UserDetails.ResolveIdAsync(request.PersonId, cancellationToken);
        var accountId = await _identityService.FindAccountIdOfPersonAsync(schoolId, personId);
        if (accountId is null)
        {
            return [];
        }

        return await _context.AccountRoles
            .AsNoTracking()
            .Where(a => a.UserId == accountId)
            .Join(_context.SchoolRoles.AsNoTracking(), a => a.SchoolRoleId, r => r.Id, (a, r) => r.PublicId)
            .ToListAsync(cancellationToken);
    }
}
