using System.Security.Claims;

using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Security;

namespace EducationPortal.Web.Services;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Id => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public bool MustChangePassword => _httpContextAccessor.HttpContext?.User?.HasClaim(TenantClaimTypes.MustChangePassword, "true") ?? false;

    public List<string>? Roles => _httpContextAccessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();

}
