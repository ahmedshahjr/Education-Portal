using Microsoft.AspNetCore.Identity;

namespace EducationPortal.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// The school the account belongs to. The account can sign in only on that school's domain, and the session carries
    /// it as a claim that must match the domain on every request (AGENTS.md section 2).
    /// </summary>
    public int SchoolId { get; set; }
}
