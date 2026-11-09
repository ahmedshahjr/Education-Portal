namespace EducationPortal.Web.Infrastructure;

public static class RateLimitPolicies
{
    /// <summary>Sign-in attempts, counted per client address and minute (SEC-WEB-07).</summary>
    public const string SignIn = "sign-in";
}
