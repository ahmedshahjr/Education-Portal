using Microsoft.AspNetCore.Identity;

namespace EducationPortal.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// The school the account belongs to. The account can sign in only on that school's domain, and the session carries
    /// it as a claim that must match the domain on every request (AGENTS.md section 2).
    /// </summary>
    public int SchoolId { get; set; }

    /// <summary>
    /// The language the user last chose (ISO 639-1), or null when they never chose one. Saved on the account, so it
    /// follows them to every device; used only while the school offers it (ADR-005).
    /// </summary>
    public string? PreferredLanguage { get; set; }

    /// <summary>
    /// True while the account still has the one-time password it was created with. Until the user sets their own, every
    /// request except reading their account, changing the password and signing out is refused.
    /// </summary>
    public bool MustChangePassword { get; set; }

    /// <summary>
    /// The person this account belongs to (their record in the school's people), or null for an account that is not a
    /// person's own: the school administrator and the master tenant's accounts. One account per person in a school.
    /// </summary>
    public int? UserDetailsId { get; set; }
}
