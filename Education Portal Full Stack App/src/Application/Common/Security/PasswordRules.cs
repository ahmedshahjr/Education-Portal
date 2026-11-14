namespace EducationPortal.Application.Common.Security;

/// <summary>
/// Password policy from SEC-AUTHN-03 (NIST SP 800-63B): length instead of composition rules. The minimum is
/// 15 because students and parents may sign in with a password as their only factor.
/// </summary>
public static class PasswordRules
{
    public const int MinimumLength = 15;

    /// <summary>At least 64 is required; the cap keeps hashing cost bounded.</summary>
    public const int MaximumLength = 128;
}
