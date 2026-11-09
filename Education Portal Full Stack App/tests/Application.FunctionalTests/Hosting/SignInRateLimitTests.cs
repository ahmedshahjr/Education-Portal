using System.Net;

namespace EducationPortal.Application.FunctionalTests.Hosting;

/// <summary>
/// The configured sign-in limit (SEC-WEB-07). It runs on a host of its own, so no other test uses up its attempts. Each
/// account has its own lockout, which <see cref="Users.SignInTests"/> covers.
/// </summary>
public class SignInRateLimitTests : TestBase
{
    private ProductionWebApiFactory _factory = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _factory = new ProductionWebApiFactory(FunctionalTestSetup.ConnectionString);
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _factory.DisposeAsync();
    }

    [Test]
    public async Task ShouldAllowFiveSignInAttemptsAMinuteFromOneAddressThenRejectTheNextOne()
    {
        using var session = TestHttp.CreateSession(_factory);

        for (var attempt = 1; attempt <= 5; attempt++)
        {
            var response = await session.SignInAsync("nobody@local", "a wrong password");

            response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized, $"attempt {attempt} should reach the password check");
        }

        var limited = await session.SignInAsync("nobody@local", "a wrong password");

        limited.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);
    }
}
