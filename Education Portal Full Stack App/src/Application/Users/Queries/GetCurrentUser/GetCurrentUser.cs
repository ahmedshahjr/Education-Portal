using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Models;
using EducationPortal.Application.Common.Security;

namespace EducationPortal.Application.Users.Queries.GetCurrentUser;

/// <summary>Returns the signed-in user's own account.</summary>
[Authorize(AllowWhilePasswordChangeRequired = true)]
public record GetCurrentUserQuery : IRequest<CurrentUserDto>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserDto>
{
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public GetCurrentUserQueryHandler(IUser user, IIdentityService identityService)
    {
        _user = user;
        _identityService = identityService;
    }

    public async Task<CurrentUserDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var userId = _user.Id ?? throw new UnauthorizedAccessException();

        return await _identityService.GetCurrentUserAsync(userId) ?? throw new UnauthorizedAccessException();
    }
}
