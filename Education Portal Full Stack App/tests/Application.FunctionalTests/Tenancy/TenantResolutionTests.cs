using System.Net;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EducationPortal.Application.FunctionalTests.Tenancy;

/// <summary>
/// The request's domain picks the school, and a session belongs to one school's domain (AGENTS.md section 2,
/// SEC-AUTHZ-10, SEC-LOG-03).
/// </summary>
public class TenantResolutionTests : TestBase
{
    private const string SchoolDomain = "school1.localhost";

    [Test]
    public async Task ShouldAnswer404ForAnUnknownHostWithoutServingAnything()
    {
        using var session = TestHttp.CreateDevelopmentSession("unknown.localhost");

        foreach (var path in new[] { "/", "/api/Branding", HttpSession.CurrentUserPath })
        {
            using var response = await session.GetAsync(path);

            response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            (await response.Content.ReadAsByteArrayAsync()).ShouldBeEmpty();
        }
    }

    [Test]
    public async Task ShouldSignInOnTheUsersOwnSchoolDomain()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsUserAsync(schoolId, "teacher@school1.local", TestApp.DefaultUserPassword, []);
        using var session = TestHttp.CreateDevelopmentSession(SchoolDomain);

        using var response = await session.SignInAsync("teacher@school1.local", TestApp.DefaultUserPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task ShouldRefuseASignInOnAnotherSchoolsDomainAndAuditIt()
    {
        await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var userId = await TestApp.RunAsDefaultUserAsync(); // a user of the master tenant
        using var session = TestHttp.CreateDevelopmentSession(SchoolDomain);

        using var response = await session.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        session.Cookie("__Host-EducationPortal").ShouldBeNull();
        var entry = (await TestApp.FindAuditEventsAsync(userId)).ShouldHaveSingleItem();
        entry.EventType.ShouldBe(SecurityAuditEventType.TenantMismatch);
        entry.Outcome.ShouldBe(SecurityAuditOutcome.Failure);
    }

    [Test]
    public async Task ShouldNotCountSignInsOnAnotherSchoolsDomainTowardsLockout()
    {
        await TestApp.AddSchoolAsync("School One", SchoolDomain);
        await TestApp.RunAsDefaultUserAsync();
        using var otherSchool = TestHttp.CreateDevelopmentSession(SchoolDomain);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            (await otherSchool.SignInAsync(TestApp.DefaultUserEmail, "a wrong password for this test")).Dispose();
        }

        using var ownSchool = TestHttp.CreateDevelopmentSession();
        using var response = await ownSchool.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task ShouldRejectASessionCookieCopiedToAnotherSchoolsDomain()
    {
        await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var userId = await TestApp.RunAsDefaultUserAsync();
        using var master = TestHttp.CreateDevelopmentSession();
        (await master.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword)).Dispose();

        using var otherSchool = TestHttp.CreateDevelopmentSession(SchoolDomain);
        otherSchool.SetCookie("__Host-EducationPortal", master.Cookie("__Host-EducationPortal")!);
        using var response = await otherSchool.GetCurrentUserAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await TestApp.FindAuditEventsAsync(userId))
            .ShouldContain(e => e.EventType == SecurityAuditEventType.TenantMismatch);
    }

    [Test]
    public async Task ShouldAllowOnlyOneMasterTenant()
    {
        // The master tenant already exists; the database refuses a second one.
        await Should.ThrowAsync<DbUpdateException>(() =>
            TestApp.AddAsync(School.CreateMaster("Second master", "second-master.localhost")));

        (await TestApp.CountAsync<School>()).ShouldBe(1);
    }

    [Test]
    public async Task ShouldAllowOnlyOneSchoolPerDomain()
    {
        await TestApp.AddSchoolAsync("School One", SchoolDomain);

        await Should.ThrowAsync<DbUpdateException>(() => TestApp.AddSchoolAsync("Same domain", SchoolDomain));
    }
}
