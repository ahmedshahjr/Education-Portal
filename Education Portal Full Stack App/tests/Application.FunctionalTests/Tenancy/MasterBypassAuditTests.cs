using EducationPortal.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Tenancy;

/// <summary>
/// The master bypass of the tenant filters is granted only to a signed-in master session, and every API call under it is
/// written to the security audit log (SEC-AUTHZ-13, SEC-LOG-03, ADR-026).
/// </summary>
public class MasterBypassAuditTests : TestBase
{
    [Test]
    public async Task ShouldAuditAnApiCallMadeUnderTheMasterBypass()
    {
        var userId = await TestApp.RunAsDefaultUserAsync(); // a user of the master tenant
        using var session = TestHttp.CreateDevelopmentSession(TestApp.MasterDomain);
        (await session.SignInAsync(TestApp.DefaultUserEmail, TestApp.DefaultUserPassword)).StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);

        using var response = await session.GetCurrentUserAsync();

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var entries = await TestApp.FindAuditEventsAsync(userId);
        entries.ShouldContain(e => e.EventType == SecurityAuditEventType.MasterTenantAccess && e.Outcome == SecurityAuditOutcome.Success);
    }

    [Test]
    public async Task ShouldNotGrantTheMasterBypassToASchoolSession()
    {
        var school = await TestApp.AddSchoolAsync("Audit School", "audit-school.localhost");
        var userId = await TestApp.RunAsUserAsync(school.Id, "audit.teacher@school.local", TestApp.DefaultUserPassword, []);
        using var session = TestHttp.CreateDevelopmentSession("audit-school.localhost");
        (await session.SignInAsync("audit.teacher@school.local", TestApp.DefaultUserPassword)).StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);

        using var response = await session.GetCurrentUserAsync();

        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        (await TestApp.FindAuditEventsAsync(userId)).ShouldNotContain(e => e.EventType == SecurityAuditEventType.MasterTenantAccess);
    }
}
