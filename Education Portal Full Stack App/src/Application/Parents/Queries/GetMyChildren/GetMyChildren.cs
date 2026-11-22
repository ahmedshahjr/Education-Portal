using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;
using EducationPortal.Application.Parents.Common;
using EducationPortal.Application.Users.Dtos;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Parents.Queries.GetMyChildren;

/// <summary>
/// The signed-in parent's own students and nobody else's (SEC-AUTHZ-04). The person is the one the account belongs to; an
/// account that is not a person's sees no students. Needs the Students module, and the account must have changed its
/// one-time password (the authorization behaviour refuses the request otherwise).
/// </summary>
[Authorize(Module = ModuleCode.Students)]
public record GetMyChildrenQuery;

public class GetMyChildrenQueryHandler
{
    private readonly IApplicationDbContext _context;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public GetMyChildrenQueryHandler(IApplicationDbContext context, IIdentityService identityService, IUser user)
    {
        _context = context;
        _identityService = identityService;
        _user = user;
    }

    public async Task<IReadOnlyList<UserDetailsDto>> Handle(GetMyChildrenQuery request, CancellationToken cancellationToken)
    {
        var personId = await _identityService.FindPersonIdOfAccountAsync(_user.Id ?? string.Empty);
        if (personId is null)
        {
            return [];
        }

        return await ParentChildren.ListAsync(_context, personId.Value, cancellationToken);
    }
}
