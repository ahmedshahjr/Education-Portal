using System.Globalization;
using Microsoft.AspNetCore.Authentication;

namespace EducationPortal.Infrastructure.Identity;

/// <summary>
/// The absolute session lifetime (SEC-AUTHN-07). The idle timeout is the cookie's 15-minute expiry, which slides with
/// each request. The time of sign-in is kept in the cookie's properties, which survive that renewal, so a session
/// still ends 12 hours after the user signed in.
/// </summary>
public static class SessionLifetime
{
    public const string SignedInAtKey = "edu.signed_in_at";

    public static readonly TimeSpan AbsoluteLifetime = TimeSpan.FromHours(12);

    public static void MarkSignedIn(AuthenticationProperties properties, DateTimeOffset now)
    {
        // Only the first sign-in of a session counts. A renewal keeps the original value.
        if (!properties.Items.ContainsKey(SignedInAtKey))
        {
            properties.Items[SignedInAtKey] = now.ToString("O", CultureInfo.InvariantCulture);
        }
    }

    public static bool IsWithinAbsoluteLifetime(AuthenticationProperties properties, DateTimeOffset now)
    {
        // A session without a sign-in time is rejected, so a missing value fails closed (SEC-ERR-01).
        return properties.Items.TryGetValue(SignedInAtKey, out var value)
            && DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var signedInAt)
            && now - signedInAt < AbsoluteLifetime;
    }
}
