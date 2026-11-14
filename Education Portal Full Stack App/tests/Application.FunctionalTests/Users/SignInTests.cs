using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.FunctionalTests.Users;

/// <summary>
/// Password sign-in over HTTP: what the client is told, and what reaches the security audit log (SEC-AUTHN-05, SEC-LOG-03).
/// </summary>
public class SignInTests : TestBase
{
    private const string InvalidCredentials = "Invalid email or password.";

    [Test]
    public async Task ShouldSignInWithTheCorrectPassword()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();
        using var session = TestHttp.CreateDevelopmentSession();

        var response = await session.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        session.Cookie("__Host-EducationPortal").ShouldNotBeNullOrEmpty();

        var entry = (await TestApp.FindAuditEventsAsync(userId)).ShouldHaveSingleItem();
        entry.EventType.ShouldBe(SecurityAuditEventType.SignInSucceeded);
        entry.Outcome.ShouldBe(SecurityAuditOutcome.Success);
        entry.TargetType.ShouldBe("User");
        entry.TargetId.ShouldBe(userId);
    }

    [Test]
    public async Task ShouldAnswerAWrongPasswordAndAnUnknownEmailTheSameWay()
    {
        await TestApp.RunAsDefaultUserAsync();
        using var session = TestHttp.CreateDevelopmentSession();

        var wrongPassword = await session.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword + "-wrong");
        var unknownEmail = await session.SignInAsync("nobody@local", TestApp.DefaultUserPassword);

        wrongPassword.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        unknownEmail.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await ReadDetailAsync(wrongPassword)).ShouldBe(InvalidCredentials);
        (await ReadDetailAsync(unknownEmail)).ShouldBe(InvalidCredentials);
        session.Cookie("__Host-EducationPortal").ShouldBeNull();
    }

    [Test]
    public async Task ShouldAuditAFailedSignInAgainstTheAccount()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();
        using var session = TestHttp.CreateDevelopmentSession();

        await session.SignInAsync(TestApp.DefaultUserEmail, "not the password");

        var entry = (await TestApp.FindAuditEventsAsync(userId)).ShouldHaveSingleItem();
        entry.EventType.ShouldBe(SecurityAuditEventType.SignInFailed);
        entry.Outcome.ShouldBe(SecurityAuditOutcome.Failure);
        entry.TargetId.ShouldBe(userId);
    }

    [Test]
    public async Task ShouldLockTheAccountAfterThreeFailedAttempts()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();
        using var session = TestHttp.CreateDevelopmentSession();

        for (var attempt = 1; attempt <= 3; attempt++)
        {
            var failed = await session.SignInAsync(TestApp.DefaultUserEmail, $"wrong password {attempt}");
            failed.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        }

        // While the lockout lasts, even the correct password is refused.
        var locked = await session.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword);

        locked.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await ReadDetailAsync(locked)).ShouldBe(InvalidCredentials);
        session.Cookie("__Host-EducationPortal").ShouldBeNull();

        var eventTypes = (await TestApp.FindAuditEventsAsync(userId)).Select(e => e.EventType).ToArray();
        eventTypes.Last().ShouldBe(SecurityAuditEventType.SignInLockedOut);
        eventTypes.Count(type => type == SecurityAuditEventType.SignInLockedOut).ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task ShouldRejectAMalformedEmailBeforeCheckingAnyPassword()
    {
        using var session = TestHttp.CreateDevelopmentSession();

        var response = await session.SignInAsync("not-an-email", "any password at all");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    private static async Task<string?> ReadDetailAsync(HttpResponseMessage response)
    {
        var problem = await response.Content.ReadFromJsonAsync<JsonElement>();

        return problem.GetProperty("detail").GetString();
    }
}
