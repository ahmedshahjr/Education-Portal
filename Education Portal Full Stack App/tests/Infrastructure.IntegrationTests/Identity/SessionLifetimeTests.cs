using EducationPortal.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Infrastructure.IntegrationTests.Identity;

/// <summary>The 12-hour absolute session lifetime (SEC-AUTHN-07).</summary>
public class SessionLifetimeTests
{
    private static readonly DateTimeOffset SignedInAt = new(2026, 11, 9, 8, 0, 0, TimeSpan.Zero);

    [Test]
    public void ShouldAcceptASessionUntilTwelveHoursAfterSignIn()
    {
        var properties = new AuthenticationProperties();
        SessionLifetime.MarkSignedIn(properties, SignedInAt);

        SessionLifetime.IsWithinAbsoluteLifetime(properties, SignedInAt.AddHours(12).AddSeconds(-1)).ShouldBeTrue();
        SessionLifetime.IsWithinAbsoluteLifetime(properties, SignedInAt.AddHours(12)).ShouldBeFalse();
    }

    [Test]
    public void ShouldKeepTheOriginalSignInTimeWhenTheCookieIsRenewed()
    {
        var properties = new AuthenticationProperties();
        SessionLifetime.MarkSignedIn(properties, SignedInAt);

        // The idle timeout renews the cookie at each request. The renewal must not restart the 12 hours.
        SessionLifetime.MarkSignedIn(properties, SignedInAt.AddHours(5));

        SessionLifetime.IsWithinAbsoluteLifetime(properties, SignedInAt.AddHours(11)).ShouldBeTrue();
        SessionLifetime.IsWithinAbsoluteLifetime(properties, SignedInAt.AddHours(12)).ShouldBeFalse();
    }

    [Test]
    public void ShouldRejectASessionWithoutASignInTime()
    {
        var properties = new AuthenticationProperties();

        SessionLifetime.IsWithinAbsoluteLifetime(properties, SignedInAt).ShouldBeFalse();
    }

    [Test]
    public void ShouldRejectASessionWhoseSignInTimeCannotBeRead()
    {
        var properties = new AuthenticationProperties();
        properties.Items[SessionLifetime.SignedInAtKey] = "not a time";

        SessionLifetime.IsWithinAbsoluteLifetime(properties, SignedInAt).ShouldBeFalse();
    }
}
