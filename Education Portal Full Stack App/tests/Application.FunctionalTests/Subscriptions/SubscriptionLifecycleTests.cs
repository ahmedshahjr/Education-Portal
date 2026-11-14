using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Invoices.Commands.CreateInvoice;
using EducationPortal.Application.Invoices.Commands.RecordPayment;
using EducationPortal.Application.Subscriptions.Lifecycle;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Web.Jobs;
using EducationPortal.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Application.FunctionalTests.Subscriptions;

/// <summary>
/// The daily subscription lifecycle job (ADR-015) against a real database, run by hand at a chosen UTC time with the
/// default settings (14 days of grace, reminders 30, 7 and 1 day ahead). Each rule is checked on its own, then a full run
/// twice on the same day must change nothing the second time. The job acts as the master tenant only (SEC-AUTHZ-07).
/// </summary>
public class SubscriptionLifecycleTests : TestBase
{
    private const int GraceDays = 30;

    // Today at 09:00 UTC: module access is also checked against the real clock, so the periods must be current.
    private static readonly DateTimeOffset Start = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero).AddHours(9);

    [Test]
    public async Task ShouldEndATrialAfterItsEndDateWithoutSuspendingItsModules()
    {
        var school = await TestApp.AddSchoolAsync("Trial School", "trial.localhost");
        var subscription = await TestApp.SubscribeAsync(school.Id, [ModuleCodes.Fees], Start, trialDays: 14);

        (await RunAsync(Start.AddDays(13))).TrialsEnded.ShouldBe(0);
        (await LoadAsync(subscription.Id)).Status.ShouldBe(SubscriptionStatus.Trial);

        (await RunAsync(Start.AddDays(14).AddHours(1))).TrialsEnded.ShouldBe(1);

        var ended = await LoadAsync(subscription.Id);
        ended.ShouldSatisfyAllConditions(
            s => s.Status.ShouldBe(SubscriptionStatus.PastDue),
            s => s.PastDueSince.ShouldBe(Start.AddDays(14)),
            s => s.Modules.ShouldAllBe(m => m.Status == TenantModuleStatus.Active));
    }

    [Test]
    public async Task ShouldMarkAnUnpaidInvoiceOverdueAndItsSubscriptionPastDue()
    {
        var school = await TestApp.AddSchoolAsync("Billed School", "billed.localhost");
        var subscription = await TestApp.SubscribeAsync(school.Id, [ModuleCodes.Fees], Start);
        var invoice = await IssueInvoiceAsync(subscription.Id, Start);

        (await RunAsync(invoice.DueDate.AddMinutes(-1))).InvoicesMarkedOverdue.ShouldBe(0);

        var result = await RunAsync(invoice.DueDate.AddHours(2));

        result.ShouldSatisfyAllConditions(r => r.InvoicesMarkedOverdue.ShouldBe(1), r => r.MarkedPastDue.ShouldBe(1));
        var overdue = await LoadInvoiceAsync(invoice.Id);
        overdue.ShouldSatisfyAllConditions(i => i.IsOverdue.ShouldBeTrue(), i => i.Status.ShouldBe(InvoiceStatus.Open));
        (await LoadAsync(subscription.Id)).ShouldSatisfyAllConditions(
            s => s.Status.ShouldBe(SubscriptionStatus.PastDue),
            s => s.PastDueSince.ShouldBe(invoice.DueDate));
        (await TestApp.FindAuditEventsAsync(SubscriptionLifecycle.SystemActor)).ShouldContain(e =>
            e.EventType == SecurityAuditEventType.InvoiceMarkedOverdue && e.TargetId == invoice.Id.ToString());
    }

    [Test]
    public async Task ShouldMarkAnActiveSubscriptionPastDueWhenItsPeriodEndsUnpaid()
    {
        var school = await TestApp.AddSchoolAsync("Lapsing School", "lapsing.localhost");
        var subscription = await TestApp.SubscribeAsync(school.Id, [ModuleCodes.Fees], Start);
        var end = subscription.EndDate;

        (await RunAsync(end.AddMinutes(-1))).MarkedPastDue.ShouldBe(0);
        (await RunAsync(end)).MarkedPastDue.ShouldBe(1);

        (await LoadAsync(subscription.Id)).ShouldSatisfyAllConditions(
            s => s.Status.ShouldBe(SubscriptionStatus.PastDue),
            s => s.PastDueSince.ShouldBe(end));
    }

    [Test]
    public async Task ShouldSuspendAfterTheGracePeriodAndKeepTheSchoolsData()
    {
        var school = await TestApp.AddSchoolAsync("Late School", "late.localhost");
        var subscription = await TestApp.SubscribeAsync(school.Id, [ModuleCodes.Fees], Start);
        var lapsed = subscription.EndDate;
        await RunAsync(lapsed);
        (await HasModuleAsync(school.Id, ModuleCodes.Fees)).ShouldBeTrue();

        (await RunAsync(lapsed.AddDays(GraceDays).AddMinutes(-1))).Suspended.ShouldBe(0);
        (await RunAsync(lapsed.AddDays(GraceDays))).Suspended.ShouldBe(1);

        var suspended = await LoadAsync(subscription.Id);
        suspended.ShouldSatisfyAllConditions(
            s => s.Status.ShouldBe(SubscriptionStatus.Suspended),
            s => s.Modules.ShouldNotBeEmpty(),
            s => s.Modules.ShouldAllBe(m => m.Status == TenantModuleStatus.Suspended));

        // Access is blocked at once (the cached entitlements were dropped); nothing was deleted.
        (await HasModuleAsync(school.Id, ModuleCodes.Fees)).ShouldBeFalse();
        (await TestApp.FindAsync<School>(school.Id)).ShouldNotBeNull();
        (await TestApp.FindAuditEventsAsync(SubscriptionLifecycle.SystemActor)).ShouldContain(e =>
            e.EventType == SecurityAuditEventType.SubscriptionChanged && e.TargetId == subscription.Id.ToString());
    }

    [Test]
    public async Task ShouldQueueOneReminderForTheNearestConfiguredDay()
    {
        var school = await TestApp.AddSchoolAsync("Renewing School", "renewing.localhost");
        var subscription = await TestApp.SubscribeAsync(school.Id, [ModuleCodes.Fees], Start);
        var end = subscription.EndDate;

        (await RunAsync(end.AddDays(-31))).RemindersQueued.ShouldBe(0);
        (await RunAsync(end.AddDays(-20))).RemindersQueued.ShouldBe(1);
        (await RunAsync(end.AddDays(-19))).RemindersQueued.ShouldBe(0);

        // The job missed the 7-day run: only the nearest reminder is queued, not a burst of old ones.
        (await RunAsync(end.AddDays(-1))).RemindersQueued.ShouldBe(1);

        var reminders = await RemindersAsync(subscription.Id);
        reminders.Select(r => r.DaysBefore).ShouldBe([30, 1], ignoreOrder: true);
        reminders.ShouldAllBe(r => r.PeriodEnd == end && r.SchoolId == school.Id && r.SentAt == null);
    }

    [Test]
    public async Task ShouldChangeNothingWhenRunTwiceOnTheSameDay()
    {
        var trial = await TestApp.AddSchoolAsync("Trial School", "trial.localhost");
        var billed = await TestApp.AddSchoolAsync("Billed School", "billed.localhost");
        var late = await TestApp.AddSchoolAsync("Late School", "late.localhost");
        var renewing = await TestApp.AddSchoolAsync("Renewing School", "renewing.localhost");

        var now = Start.AddDays(60);
        var trialSubscription = await TestApp.SubscribeAsync(trial.Id, [ModuleCodes.Fees], now.AddDays(-15), trialDays: 14);
        var billedSubscription = await TestApp.SubscribeAsync(billed.Id, [ModuleCodes.Fees], now.AddDays(-25));
        var invoice = await IssueInvoiceAsync(billedSubscription.Id, now.AddDays(-Invoice.PaymentTermDays - 1));
        var lateSubscription = await TestApp.SubscribeAsync(late.Id, [ModuleCodes.Fees], now.AddMonths(-1).AddDays(-GraceDays - 5));
        await TestApp.SubscribeAsync(renewing.Id, [ModuleCodes.Fees], now.AddDays(-25));

        var first = await RunAsync(now);
        var auditCount = (await TestApp.FindAuditEventsAsync(SubscriptionLifecycle.SystemActor)).Count;
        var before = await SnapshotAsync();

        // Later the same day.
        var second = await RunAsync(now.AddHours(3));

        first.ShouldSatisfyAllConditions(
            r => r.TrialsEnded.ShouldBe(1),
            r => r.InvoicesMarkedOverdue.ShouldBe(1),
            r => r.MarkedPastDue.ShouldBe(2),
            r => r.Suspended.ShouldBe(1),
            r => r.RemindersQueued.ShouldBe(1));
        second.ShouldBe(new SubscriptionLifecycleResult(0, 0, 0, 0, 0));
        (await SnapshotAsync()).ShouldBe(before);
        (await TestApp.FindAuditEventsAsync(SubscriptionLifecycle.SystemActor)).Count.ShouldBe(auditCount);

        (await LoadAsync(trialSubscription.Id)).Status.ShouldBe(SubscriptionStatus.PastDue);
        (await LoadInvoiceAsync(invoice.Id)).IsOverdue.ShouldBeTrue();
        (await LoadAsync(lateSubscription.Id)).Status.ShouldBe(SubscriptionStatus.Suspended);
    }

    [Test]
    public async Task ShouldNotMarkAPaidInvoiceOrItsSubscriptionOverdue()
    {
        var school = await TestApp.AddSchoolAsync("Good School", "good.localhost");
        var subscription = await TestApp.SubscribeAsync(school.Id, [ModuleCodes.Fees], Start, amount: 100m);
        // Issued a few days ahead, so it falls due well before the period ends.
        var invoice = await IssueInvoiceAsync(subscription.Id, Start.AddDays(-3));
        await PayAsync(invoice.Id, invoice.Total, Start.AddDays(1));

        // After the due date, still inside the period: only the 1-day renewal reminder is due.
        var result = await RunAsync(invoice.DueDate.AddHours(1));

        result.ShouldBe(new SubscriptionLifecycleResult(0, 0, 0, 0, 1));
        (await LoadAsync(subscription.Id)).Status.ShouldBe(SubscriptionStatus.Active);
    }

    [Test]
    public async Task ShouldRestartALapsedSubscriptionOnTheRenewalInvoiceDate()
    {
        await TestApp.RunAsAdministratorAsync();
        var school = await TestApp.AddSchoolAsync("Returning School", "returning.localhost");
        var subscription = await TestApp.SubscribeAsync(school.Id, [ModuleCodes.Fees], DateTimeOffset.UtcNow.AddMonths(-2), amount: 100m);
        await RunAsync(DateTimeOffset.UtcNow);
        (await LoadAsync(subscription.Id)).Status.ShouldBe(SubscriptionStatus.Suspended);

        var issuedAfter = DateTimeOffset.UtcNow;
        var invoiceId = await TestApp.SendAsync(new CreateInvoiceCommand { SchoolId = school.PublicId, NextPeriod = true });
        var invoice = await LoadInvoiceAsync(invoiceId);
        await TestApp.SendAsync(new RecordPaymentCommand { InvoiceId = invoiceId, Amount = invoice.Total, Method = PaymentMethod.BankTransfer, Reference = "TRX-7", ReceivedDate = DateTimeOffset.UtcNow });

        invoice.PeriodStart.ShouldBeGreaterThanOrEqualTo(issuedAfter.AddSeconds(-1));
        (await LoadAsync(subscription.Id)).ShouldSatisfyAllConditions(
            s => s.Status.ShouldBe(SubscriptionStatus.Active),
            s => s.PastDueSince.ShouldBeNull(),
            s => s.EndDate.ShouldBe(Subscription.PeriodEnd(invoice.PeriodStart, BillingCycle.Monthly)),
            s => s.Modules.ShouldAllBe(m => m.Status == TenantModuleStatus.Active));
        (await HasModuleAsync(school.Id, ModuleCodes.Fees)).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldRefuseToRunAsASchool()
    {
        var school = await TestApp.AddSchoolAsync("Some School", "some.localhost");

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(school.Id, IsMaster: false));

        await Should.ThrowAsync<ForbiddenAccessException>(() =>
            scope.ServiceProvider.GetRequiredService<SubscriptionLifecycle>().RunAsync(Start, CancellationToken.None));
    }

    private static async Task<SubscriptionLifecycleResult> RunAsync(DateTimeOffset now) =>
        (await SubscriptionLifecycleJob.RunAsMasterAsync(FunctionalTestSetup.ScopeFactory, now, CancellationToken.None)).ShouldNotBeNull();

    private static async Task<Invoice> IssueInvoiceAsync(int subscriptionId, DateTimeOffset issueDate)
    {
        using var scope = MasterScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var subscription = await context.Subscriptions.SingleAsync(s => s.Id == subscriptionId);

        var invoice = Invoice.Create(subscription, 0m, issueDate, nextPeriod: false, "test");
        context.Invoices.Add(invoice);
        await context.SaveChangesAsync();
        return invoice;
    }

    private static async Task PayAsync(int invoiceId, decimal amount, DateTimeOffset receivedDate)
    {
        using var scope = MasterScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var invoice = await context.Invoices.SingleAsync(i => i.Id == invoiceId);
        var subscription = await context.Subscriptions.Include(s => s.Modules).SingleAsync(s => s.Id == invoice.SubscriptionId);

        invoice.RecordPayment(amount, PaymentMethod.BankTransfer, "TRX-1", receivedDate, "Manual");
        subscription.ApplyPayment(invoice);
        await context.SaveChangesAsync();
    }

    private static async Task<Subscription> LoadAsync(int id)
    {
        using var scope = MasterScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Subscriptions
            .AsNoTracking().Include(s => s.Modules).SingleAsync(s => s.Id == id);
    }

    private static async Task<Invoice> LoadInvoiceAsync(int id)
    {
        using var scope = MasterScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Invoices.AsNoTracking().SingleAsync(i => i.Id == id);
    }

    private static async Task<Invoice> LoadInvoiceAsync(Guid publicId)
    {
        using var scope = MasterScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Invoices.AsNoTracking().SingleAsync(i => i.PublicId == publicId);
    }

    private static async Task<List<RenewalReminder>> RemindersAsync(int subscriptionId)
    {
        using var scope = MasterScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().RenewalReminders
            .AsNoTracking().Where(r => r.SubscriptionId == subscriptionId).ToListAsync();
    }

    /// <summary>Everything the job can change, as text, to compare two runs.</summary>
    private static async Task<string> SnapshotAsync()
    {
        using var scope = MasterScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var subscriptions = await context.Subscriptions.AsNoTracking().OrderBy(s => s.Id).Select(s => $"{s.Id}:{s.Status}:{s.PastDueSince}:{s.EndDate}").ToListAsync();
        var modules = await context.TenantModules.AsNoTracking().OrderBy(m => m.Id).Select(m => $"{m.Id}:{m.Status}:{m.ValidTo}").ToListAsync();
        var invoices = await context.Invoices.AsNoTracking().OrderBy(i => i.Id).Select(i => $"{i.Id}:{i.Status}:{i.OverdueSince}").ToListAsync();
        var reminders = await context.RenewalReminders.AsNoTracking().OrderBy(r => r.Id).Select(r => $"{r.Id}:{r.SubscriptionId}:{r.DaysBefore}").ToListAsync();
        return string.Join('|', subscriptions.Concat(modules).Concat(invoices).Concat(reminders));
    }

    private static async Task<bool> HasModuleAsync(int schoolId, string code)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(schoolId, IsMaster: false));
        return await scope.ServiceProvider.GetRequiredService<IModuleAccess>().HasModuleAsync(schoolId, code, null, CancellationToken.None);
    }

    private static IServiceScope MasterScope()
    {
        var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(TestApp.MasterSchoolId, IsMaster: true));
        return scope;
    }
}
