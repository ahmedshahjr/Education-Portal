using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EducationPortal.Application.FunctionalTests.Hosting;

/// <summary>
/// Protections that apply to every response, to unsafe requests, and to the API reference, which is Development-only
/// (SEC-WEB-03, SEC-WEB-04, SEC-WEB-05, SEC-TEST-02, SEC-TEST-03).
/// </summary>
public class HardeningTests : TestBase
{
    [Test]
    public async Task ShouldSendTheSecurityHeadersOnAnApiResponse()
    {
        await TestApp.RunAsDefaultUserAsync();
        using var session = TestHttp.CreateDevelopmentSession();

        // The authorization step writes this 401 itself, so it must carry the headers too.
        var anonymous = await session.GetCurrentUserAsync();
        anonymous.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        AssertSecurityHeaders(anonymous);

        var signIn = await session.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword);
        signIn.StatusCode.ShouldBe(HttpStatusCode.OK);

        var signedIn = await session.GetCurrentUserAsync();
        signedIn.StatusCode.ShouldBe(HttpStatusCode.OK);
        AssertSecurityHeaders(signedIn);
    }

    [Test]
    public async Task ShouldNotServeTheApiDocumentOutsideDevelopmentEvenToASignedInUser()
    {
        await TestApp.RunAsDefaultUserAsync();
        using var session = TestHttp.CreateProductionSession();

        // Anonymous callers get only the authorization answer, not the document.
        (await session.GetAsync("/openapi/v1.json")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var signIn = await session.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword);
        signIn.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Signed in, the caller gets past authorization and finds no such route.
        (await session.GetAsync("/openapi/v1.json")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ShouldServeTheApiDocumentToASignedInUserInDevelopment()
    {
        await TestApp.RunAsDefaultUserAsync();
        using var session = TestHttp.CreateDevelopmentSession();

        (await session.GetAsync("/openapi/v1.json")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        var signIn = await session.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword);
        signIn.StatusCode.ShouldBe(HttpStatusCode.OK);

        // The same route answers in Development, so the 404 in the test above shows that the mapping is Development-only.
        (await session.GetAsync("/openapi/v1.json")).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task ShouldRejectASignInWithoutTheAntiforgeryTokenBeforeAnyPasswordIsChecked()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();
        using var session = TestHttp.CreateDevelopmentSession();

        // A browser has loaded the app before it sends anything, so the antiforgery cookie is there. The request still
        // lacks the header that proves the page sent it, and the correct password must not get through without it.
        (await session.GetAsync("/")).Dispose();

        var response = await session.PostAsync(
            HttpSession.SignInPath,
            new { email = TestApp.DefaultUserEmail, password = TestApp.DefaultUserPassword },
            withAntiforgeryToken: false);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // The title shows that the antiforgery check refused the request, not some other failure.
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
        problem.GetProperty("title").GetString().ShouldBe("The antiforgery token is missing or invalid.");

        session.Cookie("__Host-EducationPortal").ShouldBeNull();
        (await TestApp.FindAuditEventsAsync(userId)).ShouldBeEmpty();
    }

    private static void AssertSecurityHeaders(HttpResponseMessage response)
    {
        var headers = response.Headers.Concat(response.Content.Headers)
            .ToDictionary(header => header.Key, header => string.Join(", ", header.Value), StringComparer.OrdinalIgnoreCase);

        headers.ShouldContainKey("Content-Security-Policy");
        headers["Content-Security-Policy"].ShouldContain("default-src 'self'");
        headers["Content-Security-Policy"].ShouldContain("object-src 'none'");
        headers["Content-Security-Policy"].ShouldContain("frame-ancestors 'none'");
        headers["X-Content-Type-Options"].ShouldBe("nosniff");
        headers["X-Frame-Options"].ShouldBe("DENY");
        headers["Referrer-Policy"].ShouldBe("strict-origin-when-cross-origin");
        headers["Permissions-Policy"].ShouldContain("camera=()");
        headers["Cache-Control"].ShouldBe("no-store");
    }
}
