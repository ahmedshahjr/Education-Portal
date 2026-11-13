using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Web.Jobs;
using EducationPortal.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static EducationPortal.Application.FunctionalTests.Infrastructure.CommercialHttp;

namespace EducationPortal.Application.FunctionalTests.Audit;

/// <summary>
/// Every change to a subscription, its modules, an invoice or a payment leaves a change audit entry (ADR-016): who made
/// it (or the system), when, which row, and each changed value before and after. The entry is saved with the change, so
/// a refused change leaves none, and a school sees only the entries about its own rows (SEC-LOG-03, SEC-AUTHZ-03).
/// </summary>
public class ChangeAuditTests : TestBase
{
    private const string SchoolDomain = "school1.localhost";

    [Test]
    public async Task ShouldRecordWhoCreatedTheSubscriptionAndEveryValue()
    {
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var countryId = await SetUpCatalogueAsync(master);
        var before = DateTimeOffset.UtcNow;

        var school = await AddSubscribedSchoolAsync(master, countryId, "School One", SchoolDomain);

        var entry = (await EntriesAsync(nameof(Subscription))).ShouldHaveSingleItem();
        entry.ShouldSatisfyAllConditions(
            e => e.Action.ShouldBe(ChangeAuditAction.Added),
            e => e.ActorUserId.ShouldBe(masterUserId),
            e => e.SchoolId.ShouldBe(school.Id),
            e => e.OccurredAt.ShouldBeGreaterThanOrEqualTo(before.AddSeconds(-1)));
        var changes = Changes(entry);
        changes["Status"].ShouldBe(new Change("Status", null, nameof(SubscriptionStatus.Active)));
        changes["Amount"].New.ShouldBe("25000");
        changes["CurrencyCode"].New.ShouldBe("PKR");

        // Its modules are recorded too: the plan's Attendance, what it needs and every core module.
        var modules = await EntriesAsync(nameof(TenantModule));
        modules.ShouldNotBeEmpty();
        modules.ShouldAllBe(e => e.Action == ChangeAuditAction.Added && e.ActorUserId == masterUserId && e.SchoolId == school.Id);
    }

    [Test]
    public async Task ShouldRecordTheOldAndNewStatusWhenTheMasterSuspends()
    {
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await AddSubscribedSchoolAsync(master, await SetUpCatalogueAsync(master), "School One", SchoolDomain);

        await ExpectAsync(master.PostAsync($"/api/Schools/{school.PublicId}/subscription/suspend"), HttpStatusCode.NoContent);

        var suspended = (await EntriesAsync(nameof(Subscription))).Where(e => e.Action == ChangeAuditAction.Modified).ShouldHaveSingleItem();
        suspended.ActorUserId.ShouldBe(masterUserId);
        Changes(suspended)["Status"].ShouldBe(new Change("Status", nameof(SubscriptionStatus.Active), nameof(SubscriptionStatus.Suspended)));

        var modules = (await EntriesAsync(nameof(TenantModule))).Where(e => e.Action == ChangeAuditAction.Modified).ToList();
        modules.ShouldNotBeEmpty();
        modules.ShouldAllBe(e => e.ActorUserId == masterUserId);
        modules.Select(e => Changes(e)["Status"]).ShouldAllBe(c =>
            c.Old == nameof(TenantModuleStatus.Active) && c.New == nameof(TenantModuleStatus.Suspended));
    }

    [Test]
    public async Task ShouldRecordAModuleAddedAndThenEnded()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await AddSubscribedSchoolAsync(master, await SetUpCatalogueAsync(master), "School One", SchoolDomain);
        var path = $"/api/Schools/{school.PublicId}/subscription/modules";

        await ExpectAsync(master.PostAsync(path, new { moduleCode = ModuleCodes.Library }), HttpStatusCode.NoContent);
        var added = (await EntriesAsync(nameof(TenantModule))).Last();
        added.Action.ShouldBe(ChangeAuditAction.Added);
        Changes(added).ShouldContainKey("ModuleId");

        await ExpectAsync(master.DeleteAsync($"{path}/{ModuleCodes.Library}"), HttpStatusCode.NoContent);
        var removed = (await EntriesAsync(nameof(TenantModule))).Last();
        removed.EntityId.ShouldBe(added.EntityId);
        removed.Action.ShouldBe(ChangeAuditAction.Modified); // the row is kept and expired, never deleted
        Changes(removed)["ValidTo"].New.ShouldNotBeNull();
    }

    [Test]
    public async Task ShouldRecordThePaymentAndTheInvoiceItPaid()
    {
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await AddSubscribedSchoolAsync(master, await SetUpCatalogueAsync(master), "School One", SchoolDomain);
        var invoiceId = await CreateInvoiceAsync(master, school.PublicId);

        await PayAsync(master, invoiceId, PlanPrice);

        var payment = (await EntriesAsync(nameof(Payment))).ShouldHaveSingleItem();
        payment.ShouldSatisfyAllConditions(
            p => p.Action.ShouldBe(ChangeAuditAction.Added),
            p => p.ActorUserId.ShouldBe(masterUserId),
            p => p.SchoolId.ShouldBe(school.Id));
        Changes(payment)["Amount"].New.ShouldBe("25000");
        Changes(payment)["Method"].New.ShouldBe(nameof(PaymentMethod.BankTransfer));

        var invoice = (await EntriesAsync(nameof(Invoice))).Where(e => e.EntityId == invoiceId && e.Action == ChangeAuditAction.Modified).ShouldHaveSingleItem();
        Changes(invoice)["Status"].ShouldBe(new Change("Status", nameof(InvoiceStatus.Open), nameof(InvoiceStatus.Paid)));

        // The payment and what it changed are one save, so their entries carry the same time.
        invoice.OccurredAt.ShouldBe(payment.OccurredAt);
    }

    [Test]
    public async Task ShouldRecordNothingForARefusedChange()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await AddSubscribedSchoolAsync(master, await SetUpCatalogueAsync(master), "School One", SchoolDomain);
        await ExpectAsync(master.PostAsync($"/api/Schools/{school.PublicId}/subscription/suspend"), HttpStatusCode.NoContent);
        var count = (await EntriesAsync(null)).Count;

        await ExpectAsync(master.PostAsync($"/api/Schools/{school.PublicId}/subscription/suspend"), HttpStatusCode.BadRequest);
        await ExpectAsync(master.PostAsync($"/api/Schools/{school.PublicId}/subscription/extend", new { days = 0 }), HttpStatusCode.BadRequest);

        (await EntriesAsync(null)).Count.ShouldBe(count);
    }

    [Test]
    public async Task ShouldRecordTheLifecycleJobAsTheSystem()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var countryId = await SetUpCatalogueAsync(master);
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain, countryId);
        await ExpectAsync(
            master.PostAsync($"/api/Schools/{school.PublicId}/subscription", new { planCode = PlanCode, modules = Array.Empty<string>(), billingCycle = "Monthly", trialDays = 14 }),
            HttpStatusCode.OK);

        TestApp.RunAsSystem();
        var result = await SubscriptionLifecycleJob.RunAsMasterAsync(FunctionalTestSetup.ScopeFactory, DateTimeOffset.UtcNow.AddDays(15), CancellationToken.None);

        result.ShouldNotBeNull().TrialsEnded.ShouldBe(1);
        var ended = (await EntriesAsync(nameof(Subscription))).Where(e => e.Action == ChangeAuditAction.Modified).ShouldHaveSingleItem();
        ended.ActorUserId.ShouldBeNull();
        Changes(ended)["Status"].ShouldBe(new Change("Status", nameof(SubscriptionStatus.Trial), nameof(SubscriptionStatus.PastDue)));
    }

    [Test]
    public async Task ShouldShowASchoolOnlyTheEntriesAboutItsOwnRows()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var countryId = await SetUpCatalogueAsync(master);
        var one = await AddSubscribedSchoolAsync(master, countryId, "School One", SchoolDomain);
        var two = await AddSubscribedSchoolAsync(master, countryId, "School Two", "school2.localhost");

        var seen = await ReadAsync(new ResolvedTenant(one.Id, IsMaster: false));

        seen.ShouldNotBeEmpty();
        seen.ShouldAllBe(e => e.SchoolId == one.Id); // no other school's, and no platform rows such as plans or prices
        (await ReadAsync(new ResolvedTenant(TestApp.MasterSchoolId, IsMaster: true))).ShouldContain(e => e.SchoolId == two.Id);
    }

    /// <summary>The entries for one entity type (all when null), oldest first, as the master tenant reads them.</summary>
    private static async Task<List<ChangeAuditEntry>> EntriesAsync(string? entityType) =>
        (await ReadAsync(new ResolvedTenant(TestApp.MasterSchoolId, IsMaster: true)))
            .Where(e => entityType == null || e.EntityType == entityType)
            .ToList();

    private static async Task<List<ChangeAuditEntry>> ReadAsync(ResolvedTenant tenant)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(tenant);
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.ChangeAuditEntries.AsNoTracking().OrderBy(e => e.Id).ToListAsync();
    }

    private static Dictionary<string, Change> Changes(ChangeAuditEntry entry) =>
        JsonSerializer.Deserialize<List<Change>>(entry.Changes)!.ToDictionary(c => c.Property);

    private sealed record Change(
        [property: JsonPropertyName("property")] string Property,
        [property: JsonPropertyName("old")] string? Old,
        [property: JsonPropertyName("new")] string? New);
}
