using System.Globalization;
using System.Security.Claims;
using EducationPortal.Application.Common.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace EducationPortal.Infrastructure.Identity;

/// <summary>
/// Adds the user's school (and saved language) to the session, so the server can check on every request that the session is used on its own
/// school's domain (AGENTS.md section 2). The principal is rebuilt from the database on each request, because the
/// security stamp is validated every time.
/// </summary>
internal sealed class ApplicationUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);
        identity.AddClaim(new Claim(TenantClaimTypes.SchoolId, user.SchoolId.ToString(CultureInfo.InvariantCulture)));

        // The saved language follows the user to every device (ADR-005). The principal is rebuilt on each request, so a
        // change applies on the next one.
        if (!string.IsNullOrEmpty(user.PreferredLanguage))
        {
            identity.AddClaim(new Claim(TenantClaimTypes.Language, user.PreferredLanguage));
        }

        return identity;
    }
}
