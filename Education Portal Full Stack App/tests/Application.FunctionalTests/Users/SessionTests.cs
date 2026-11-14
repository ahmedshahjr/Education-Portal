using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EducationPortal.Application.FunctionalTests.Users;

/// <summary>
/// Sessions over HTTP: who is signed in, how the cookies are issued, and how sessions end (SEC-AUTHN-06, SEC-AUTHN-07).
/// </summary>
public class SessionTests : TestBase
{
    private const string SessionCookieName = "__Host-EducationPortal";

    [Test]
    public async Task ShouldReturnUnauthorizedForTheCurrentUserWithoutASession()
    {
        using var session = TestHttp.CreateDevelopmentSession();

        var response = await session.GetCurrentUserAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldReturnOnlyTheSignedInUsersOwnAccount()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();
        using var session = await SignInAsDefaultUserAsync();

        var response = await session.GetCurrentUserAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var account = await response.Content.ReadFromJsonAsync<JsonElement>();
        account.EnumerateObject().Select(property => property.Name).ToArray().ShouldBe(new[] { "id", "email", "roles", "mustChangePassword" });
        account.GetProperty("id").GetString().ShouldBe(userId);
        account.GetProperty("email").GetString().ShouldBe(TestApp.DefaultUserEmail);
        account.GetProperty("roles").GetArrayLength().ShouldBe(0);
        account.GetProperty("mustChangePassword").GetBoolean().ShouldBeFalse();
    }

    [Test]
    public async Task ShouldIssueAHostOnlyHttpOnlySecureCookieForTheSession()
    {
        await TestApp.RunAsDefaultUserAsync();
        using var session = TestHttp.CreateDevelopmentSession();

        var response = await session.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword);

        var cookie = SetCookieHeader(response, SessionCookieName);
        cookie.ShouldContain("; path=/");
        cookie.ShouldContain("; secure");
        cookie.ShouldContain("; httponly");
        cookie.ShouldContain("; samesite=strict");
        cookie.ShouldNotContain("; domain=");
    }

    [Test]
    public async Task ShouldIssueAnAntiforgeryTokenCookieThatTheAppCanRead()
    {
        using var session = TestHttp.CreateDevelopmentSession();

        var response = await session.GetAsync("/");

        // Angular copies this cookie into the X-XSRF-TOKEN header, so script must be able to read it.
        var cookie = SetCookieHeader(response, "__Host-XSRF-TOKEN");
        cookie.ShouldContain("; secure");
        cookie.ShouldContain("; samesite=strict");
        cookie.ShouldNotContain("; httponly");
    }

    [Test]
    public async Task ShouldRejectASignOutWithoutTheAntiforgeryToken()
    {
        await TestApp.RunAsDefaultUserAsync();
        using var session = await SignInAsDefaultUserAsync();

        var response = await session.PostAsync(HttpSession.SignOutPath, withAntiforgeryToken: false);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // The title shows that the antiforgery check refused the request, not some other failure.
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("title").GetString().ShouldBe("The antiforgery token is missing or invalid.");

        (await session.GetCurrentUserAsync()).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task ShouldEndEverySessionOfTheUserOnSignOut()
    {
        await TestApp.RunAsDefaultUserAsync();
        using var laptop = await SignInAsDefaultUserAsync();
        using var phone = await SignInAsDefaultUserAsync();

        var response = await laptop.SignOutAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await laptop.GetCurrentUserAsync()).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await phone.GetCurrentUserAsync()).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldRejectACookieThatWasChanged()
    {
        await TestApp.RunAsDefaultUserAsync();
        using var session = await SignInAsDefaultUserAsync();
        var cookie = session.Cookie(SessionCookieName)!;
        var changedLastCharacter = cookie[^1] == 'A' ? 'B' : 'A';
        session.SetCookie(SessionCookieName, cookie[..^1] + changedLastCharacter);

        var response = await session.GetCurrentUserAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static async Task<HttpSession> SignInAsDefaultUserAsync()
    {
        var session = TestHttp.CreateDevelopmentSession();

        var response = await session.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        return session;
    }

    private static string SetCookieHeader(HttpResponseMessage response, string name)
    {
        return response.Headers.GetValues("Set-Cookie")
            .Single(header => header.StartsWith(name + "=", StringComparison.Ordinal))
            .ToLowerInvariant();
    }
}
