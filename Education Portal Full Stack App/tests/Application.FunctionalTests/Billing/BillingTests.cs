using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.Invoices.Common;
using EducationPortal.Application.PriceBooks.Common;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Application.FunctionalTests.Billing;

/// <summary>
/// Invoices and manual payments (ADR-013): the master tenant issues an invoice from a subscription, records the payment,
/// and the subscription is activated or extended. Only master tenant administrators touch billing, a school never sees
/// another's invoices, and every change is audited (SEC-AUTHZ-01, -03, -07, -09, SEC-LOG-03).
/// </summary>
public class BillingTests : TestBase
{
    // Enums travel by name (ADR-008).
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private const string SchoolDomain = "school1.localhost";
    private const string OtherSchoolDomain = "school2.localhost";

    private static Guid _bookId;
    private static int _countryId;

    [Test]
    public async Task ShouldIssueAnInvoiceWithTheSubscriptionPriceAndTheCountrysTax()
    {
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSchoolAsync(master, SchoolDomain, trialDays: 0, taxRate: 15m);

        var id = await CreateInvoiceAsync(master, school.PublicId, nextPeriod: false);

        var invoice = await ReadAsync(master, id);
        var subscription = await ReadSubscriptionAsync(master, school.PublicId);
        invoice.ShouldSatisfyAllConditions(
            i => i.SchoolId.ShouldBe(school.PublicId),
            i => i.Number.ShouldMatch(@"^INV-\d{6}$"),
            i => i.Status.ShouldBe(InvoiceStatus.Open),
            i => i.CurrencyCode.ShouldBe("PKR"),
            i => i.Subtotal.ShouldBe(25000m),
            i => i.TaxRate.ShouldBe(15m),
            i => i.TaxAmount.ShouldBe(3750m),
            i => i.Total.ShouldBe(28750m),
            i => i.PeriodStart.ShouldBe(subscription.StartDate),
            i => i.PeriodEnd.ShouldBe(subscription.EndDate),
            i => i.DueDate.ShouldBe(i.IssueDate.AddDays(Invoice.PaymentTermDays)),
            i => i.PaidDate.ShouldBeNull(),
            i => i.Payments.ShouldBeEmpty(),
            i => i.Lines.ShouldHaveSingleItem().Amount.ShouldBe(25000m));
        invoice.Lines.Single().Description.ShouldStartWith("BASIC");

        (await TestApp.FindAuditEventsAsync(masterUserId)).ShouldContain(e => e.EventType == SecurityAuditEventType.InvoiceCreated);
    }

    [Test]
    public async Task ShouldKeepTheTaxRateOnTheInvoiceWhenThePriceBookChangesLater()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSchoolAsync(master, SchoolDomain, trialDays: 0, taxRate: 10m);
        var id = await CreateInvoiceAsync(master, school.PublicId, nextPeriod: false);

        await SetTaxRateAsync(master, 20m);

        (await ReadAsync(master, id)).ShouldSatisfyAllConditions(i => i.TaxRate.ShouldBe(10m), i => i.Total.ShouldBe(27500m));
    }

    [Test]
    public async Task ShouldUseTheSchoolsOwnTaxRateInsteadOfTheCountrysUntilItIsCleared()
    {
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSchoolAsync(master, SchoolDomain, trialDays: 0, taxRate: 15m);

        using var set = await master.PutAsync($"/api/Schools/{school.PublicId}/tax-rate", new { taxRate = 5m });
        set.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var overridden = await ReadAsync(master, await CreateInvoiceAsync(master, school.PublicId, nextPeriod: false));
        overridden.ShouldSatisfyAllConditions(i => i.TaxRate.ShouldBe(5m), i => i.Total.ShouldBe(26250m));
        (await master.PostAsync($"/api/Invoices/{overridden.Id}/void")).Dispose();

        // A zero override is a rate too: it does not fall back to the country's.
        (await master.PutAsync($"/api/Schools/{school.PublicId}/tax-rate", new { taxRate = 0m })).Dispose();
        (await ReadAsync(master, await CreateInvoiceAsync(master, school.PublicId, nextPeriod: false))).TaxRate.ShouldBe(0m);
        var latest = (await ListAsync(master, school.PublicId)).First(i => i.Status == InvoiceStatus.Open);
        (await master.PostAsync($"/api/Invoices/{latest.Id}/void")).Dispose();

        (await master.PutAsync($"/api/Schools/{school.PublicId}/tax-rate", new { taxRate = (decimal?)null })).Dispose();
        (await ReadAsync(master, await CreateInvoiceAsync(master, school.PublicId, nextPeriod: false))).TaxRate.ShouldBe(15m);

        (await TestApp.FindAuditEventsAsync(masterUserId)).ShouldContain(e => e.EventType == SecurityAuditEventType.SchoolTaxRateChanged);
        (await ReadAsync(master, overridden.Id)).TaxRate.ShouldBe(5m); // an issued invoice keeps its rate
    }

    [TestCase(-1)]
    [TestCase(100.5)]
    [TestCase(1.234)]
    public async Task ShouldRefuseABadSchoolTaxRate(double rate)
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSchoolAsync(master, SchoolDomain, trialDays: 0, taxRate: 15m);

        using var response = await master.PutAsync($"/api/Schools/{school.PublicId}/tax-rate", new { taxRate = (decimal)rate });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldKeepSchoolsFromSettingTheirOwnTaxRate()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSchoolAsync(master, SchoolDomain, trialDays: 0, taxRate: 15m);
        using var schoolSession = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");

        using var response = await schoolSession.PutAsync($"/api/Schools/{school.PublicId}/tax-rate", new { taxRate = 0m });

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldGiveEachInvoiceItsOwnNumber()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSchoolAsync(master, SchoolDomain, trialDays: 0, taxRate: 0m);
        var first = await CreateInvoiceAsync(master, school.PublicId, nextPeriod: false);
        (await master.PostAsync($"/api/Invoices/{first}/void")).Dispose();

        var second = await CreateInvoiceAsync(master, school.PublicId, nextPeriod: false);

        (await ReadAsync(master, second)).Number.ShouldNotBe((await ReadAsync(master, first)).Number);
    }

    [Test]
    public async Task ShouldActivateATrialAndExtendItsModulesWhenTheFirstPaidPeriodIsPaid()
    {
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSchoolAsync(master, SchoolDomain, trialDays: 14, taxRate: 0m);

        // A trial has no paid period yet: only the next period can be invoiced.
        using var current = await master.PostAsync("/api/Invoices", new { schoolId = school.PublicId, nextPeriod = false });
        current.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var id = await CreateInvoiceAsync(master, school.PublicId, nextPeriod: true);
        var invoice = await ReadAsync(master, id);
        var trial = await ReadSubscriptionAsync(master, school.PublicId);
        invoice.PeriodStart.ShouldBe(trial.EndDate);
        invoice.PeriodEnd.ShouldBe(trial.EndDate.AddMonths(1));

        await PayAsync(master, invoice);

        var paid = await ReadSubscriptionAsync(master, school.PublicId);
        paid.ShouldSatisfyAllConditions(
            s => s.Status.ShouldBe(SubscriptionStatus.Active),
            s => s.EndDate.ShouldBe(invoice.PeriodEnd));
        paid.Modules.ShouldAllBe(m => m.ValidTo == invoice.PeriodEnd && m.Status == TenantModuleStatus.Active);
        (await TestApp.FindAuditEventsAsync(masterUserId)).ShouldContain(e => e.EventType == SecurityAuditEventType.PaymentRecorded);
    }

    [Test]
    public async Task ShouldExtendAnActiveSubscriptionByARenewalInvoice()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSchoolAsync(master, SchoolDomain, trialDays: 0, taxRate: 0m);
        var first = await ReadAsync(master, await CreateInvoiceAsync(master, school.PublicId, nextPeriod: false));
        await PayAsync(master, first);
        var before = await ReadSubscriptionAsync(master, school.PublicId);
        before.EndDate.ShouldBe(first.PeriodEnd); // paying the current period changes no date

        var renewal = await ReadAsync(master, await CreateInvoiceAsync(master, school.PublicId, nextPeriod: true));
        await PayAsync(master, renewal);

        var after = await ReadSubscriptionAsync(master, school.PublicId);
        after.EndDate.ShouldBe(before.EndDate.AddMonths(1));
        after.Modules.ShouldAllBe(m => m.ValidTo == after.EndDate);
    }

    [Test]
    public async Task ShouldBringAPastDueSubscriptionAndItsSuspendedModulesBackWhenItsInvoiceIsPaid()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSchoolAsync(master, SchoolDomain, trialDays: 0, taxRate: 0m);
        var invoice = await ReadAsync(master, await CreateInvoiceAsync(master, school.PublicId, nextPeriod: false));
        await LapseAsync(school.Id);
        (await ReadSubscriptionAsync(master, school.PublicId)).Status.ShouldBe(SubscriptionStatus.Suspended);

        await PayAsync(master, invoice);

        var subscription = await ReadSubscriptionAsync(master, school.PublicId);
        subscription.Status.ShouldBe(SubscriptionStatus.Active);
        subscription.Modules.ShouldAllBe(m => m.Status == TenantModuleStatus.Active);
    }

    [Test]
    public async Task ShouldRefuseAPaymentThatIsNotTheWholeTotalOrHasNoReference()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSchoolAsync(master, SchoolDomain, trialDays: 0, taxRate: 0m);
        var invoice = await ReadAsync(master, await CreateInvoiceAsync(master, school.PublicId, nextPeriod: false));

        using var part = await master.PostAsync($"/api/Invoices/{invoice.Id}/payments", Payment(invoice.Total - 1));
        using var over = await master.PostAsync($"/api/Invoices/{invoice.Id}/payments", Payment(invoice.Total + 1));
        using var noReference = await master.PostAsync($"/api/Invoices/{invoice.Id}/payments", new { amount = invoice.Total, method = "Cash", reference = "", receivedDate = DateTimeOffset.UtcNow });
        using var future = await master.PostAsync($"/api/Invoices/{invoice.Id}/payments", new { amount = invoice.Total, method = "Cash", reference = "R1", receivedDate = DateTimeOffset.UtcNow.AddDays(2) });

        part.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        over.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        noReference.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        future.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var unchanged = await ReadAsync(master, invoice.Id);
        unchanged.Status.ShouldBe(InvoiceStatus.Open);
        unchanged.Payments.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldNotTakeASecondPaymentOrPayAVoidedInvoice()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSchoolAsync(master, SchoolDomain, trialDays: 0, taxRate: 0m);
        var invoice = await ReadAsync(master, await CreateInvoiceAsync(master, school.PublicId, nextPeriod: false));
        await PayAsync(master, invoice);

        using var again = await master.PostAsync($"/api/Invoices/{invoice.Id}/payments", Payment(invoice.Total));
        using var voidPaid = await master.PostAsync($"/api/Invoices/{invoice.Id}/void");

        again.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        voidPaid.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadAsync(master, invoice.Id)).Payments.Count.ShouldBe(1);

        var next = await ReadAsync(master, await CreateInvoiceAsync(master, school.PublicId, nextPeriod: true));
        (await master.PostAsync($"/api/Invoices/{next.Id}/void")).Dispose();
        using var payVoided = await master.PostAsync($"/api/Invoices/{next.Id}/payments", Payment(next.Total));
        payVoided.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldAllowOneOpenInvoiceAndOneInvoicePerPeriodButReissueAfterAVoid()
    {
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSchoolAsync(master, SchoolDomain, trialDays: 0, taxRate: 0m);
        var first = await CreateInvoiceAsync(master, school.PublicId, nextPeriod: false);

        using var secondOpen = await master.PostAsync("/api/Invoices", new { schoolId = school.PublicId, nextPeriod = true });
        secondOpen.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        using var voided = await master.PostAsync($"/api/Invoices/{first}/void");
        voided.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await CreateInvoiceAsync(master, school.PublicId, nextPeriod: false); // the voided period can be invoiced again
        (await TestApp.FindAuditEventsAsync(masterUserId)).ShouldContain(e => e.EventType == SecurityAuditEventType.InvoiceVoided);

        var invoices = await ListAsync(master, school.PublicId);
        invoices.Select(i => i.Status).ShouldBe([InvoiceStatus.Open, InvoiceStatus.Void], ignoreOrder: true);
    }

    [Test]
    public async Task ShouldRefuseToInvoiceASchoolWithoutASubscription()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var country = await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain, country.Id);

        using var response = await master.PostAsync("/api/Invoices", new { schoolId = school.PublicId, nextPeriod = false });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [TestCase(-0.01)]
    [TestCase(100.01)]
    [TestCase(12.345)]
    public async Task ShouldRefuseATaxRateOutsideZeroToOneHundredWithTwoDecimals(double rate)
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        await SetUpSchoolAsync(master, SchoolDomain, trialDays: 0, taxRate: 5m);

        using var response = await master.PutAsync($"/api/PriceBooks/{_bookId}", new { currencyCode = "PKR", isActive = true, taxRate = (decimal)rate });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadPriceBookAsync(master)).TaxRate.ShouldBe(5m);
    }

    [Test]
    public async Task ShouldKeepTheTaxRateWhenAnUpdateLeavesItOut()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        await SetUpSchoolAsync(master, SchoolDomain, trialDays: 0, taxRate: 5m);

        (await master.PutAsync($"/api/PriceBooks/{_bookId}", new { currencyCode = "PKR", isActive = true })).Dispose();

        (await ReadPriceBookAsync(master)).TaxRate.ShouldBe(5m);
    }

    [Test]
    public async Task ShouldKeepBillingAwayFromSchoolsAndAnonymousVisitors()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSchoolAsync(master, SchoolDomain, trialDays: 0, taxRate: 0m);
        var invoice = await ReadAsync(master, await CreateInvoiceAsync(master, school.PublicId, nextPeriod: false));
        using var schoolSession = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        using var anonymous = TestHttp.CreateDevelopmentSession(SchoolDomain);

        // A school administrator cannot read, issue, pay or void invoices, not even its own.
        (await schoolSession.GetAsync("/api/Invoices")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await schoolSession.GetAsync($"/api/Invoices/{invoice.Id}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await schoolSession.PostAsync("/api/Invoices", new { schoolId = school.PublicId, nextPeriod = true })).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await schoolSession.PostAsync($"/api/Invoices/{invoice.Id}/payments", Payment(invoice.Total))).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await schoolSession.PostAsync($"/api/Invoices/{invoice.Id}/void")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await anonymous.GetAsync("/api/Invoices")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await ReadAsync(master, invoice.Id)).Status.ShouldBe(InvoiceStatus.Open);
    }

    [Test]
    public async Task ShouldShowEachSchoolOnlyItsOwnInvoicesInTheDatabase()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var one = await SetUpSchoolAsync(master, SchoolDomain, trialDays: 0, taxRate: 0m);
        var two = await AddSubscribedSchoolAsync(master, OtherSchoolDomain);
        await CreateInvoiceAsync(master, one.PublicId, nextPeriod: false);
        await CreateInvoiceAsync(master, two.PublicId, nextPeriod: false);

        (await TestApp.CountAsync<Invoice>()).ShouldBe(2); // the master tenant sees every school's

        TestApp.UseTenant(one.Id);
        (await TestApp.CountAsync<Invoice>()).ShouldBe(1);
        (await TestApp.CountAsync<InvoiceLine>()).ShouldBe(1);
        TestApp.UseTenant(two.Id);
        (await TestApp.CountAsync<Invoice>()).ShouldBe(1);
        TestApp.UseTenant(TestApp.MasterSchoolId, isMaster: true);
    }

    [Test]
    public async Task ShouldListOnlyTheRequestedSchoolsInvoices()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var one = await SetUpSchoolAsync(master, SchoolDomain, trialDays: 0, taxRate: 0m);
        var two = await AddSubscribedSchoolAsync(master, OtherSchoolDomain);
        await CreateInvoiceAsync(master, one.PublicId, nextPeriod: false);
        await CreateInvoiceAsync(master, two.PublicId, nextPeriod: false);

        (await ListAsync(master, one.PublicId)).ShouldHaveSingleItem().SchoolId.ShouldBe(one.PublicId);
        (await ListAsync(master, null)).Count.ShouldBe(2);
    }

    [Test]
    public async Task ShouldAnswerNotFoundForAnUnknownInvoice()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        (await master.GetAsync($"/api/Invoices/{Guid.NewGuid()}")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await master.PostAsync($"/api/Invoices/{Guid.NewGuid()}/void")).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await master.PostAsync($"/api/Invoices/{Guid.NewGuid()}/payments", Payment(10m))).StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static object Payment(decimal amount) => new { amount, method = "BankTransfer", reference = "TRX-1", receivedDate = DateTimeOffset.UtcNow.AddMinutes(-1) };

    /// <summary>A Pakistani school with a BASIC subscription (monthly, 25,000 PKR) and the country's tax rate.</summary>
    private static async Task<EntityIds> SetUpSchoolAsync(HttpSession master, string domain, int trialDays, decimal taxRate)
    {
        var country = await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");
        _countryId = country.Id;
        var school = await TestApp.AddSchoolAsync("School One", domain, country.Id);

        (await master.PostAsync("/api/Plans", new { code = "BASIC", name = "Basic", description = "", modules = new[] { ModuleCodes.Attendance } })).Dispose();
        using var created = await master.PostAsync("/api/PriceBooks", new { countryCode = "PK", currencyCode = "PKR" });
        _bookId = await created.Content.ReadFromJsonAsync<Guid>();
        using var price = await master.PutAsync($"/api/PriceBooks/{_bookId}/prices", new { planCode = "BASIC", billingCycle = "Monthly", price = 25000 });
        price.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await SetTaxRateAsync(master, taxRate);

        await SubscribeAsync(master, school.PublicId, trialDays);
        return school;
    }

    /// <summary>A second Pakistani school with a subscription, sharing the first school's price book.</summary>
    private static async Task<EntityIds> AddSubscribedSchoolAsync(HttpSession master, string domain)
    {
        var school = await TestApp.AddSchoolAsync("School Two", domain, _countryId);
        await SubscribeAsync(master, school.PublicId, trialDays: 0);
        return school;
    }

    private static async Task SubscribeAsync(HttpSession master, Guid schoolId, int trialDays)
    {
        using var response = await master.PostAsync($"/api/Schools/{schoolId}/subscription", new { planCode = "BASIC", modules = Array.Empty<string>(), billingCycle = "Monthly", trialDays });
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
    }

    private static async Task SetTaxRateAsync(HttpSession master, decimal taxRate)
    {
        using var response = await master.PutAsync($"/api/PriceBooks/{_bookId}", new { currencyCode = "PKR", isActive = true, taxRate });
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, await response.Content.ReadAsStringAsync());
    }

    private static async Task<Guid> CreateInvoiceAsync(HttpSession master, Guid schoolId, bool nextPeriod)
    {
        using var response = await master.PostAsync("/api/Invoices", new { schoolId, nextPeriod });
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static async Task PayAsync(HttpSession master, InvoiceDto invoice)
    {
        using var response = await master.PostAsync($"/api/Invoices/{invoice.Id}/payments", Payment(invoice.Total));
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());

        var paid = await ReadAsync(master, invoice.Id);
        paid.ShouldSatisfyAllConditions(
            i => i.Status.ShouldBe(InvoiceStatus.Paid),
            i => i.PaidDate.ShouldNotBeNull(),
            i => i.Payments.ShouldHaveSingleItem().Provider.ShouldBe("Manual"));
        paid.Payments.Single().ShouldSatisfyAllConditions(
            p => p.Amount.ShouldBe(invoice.Total),
            p => p.CurrencyCode.ShouldBe(invoice.CurrencyCode),
            p => p.Method.ShouldBe(PaymentMethod.BankTransfer),
            p => p.Reference.ShouldBe("TRX-1"));
    }

    /// <summary>Stands in for the lifecycle job (phase 7): the subscription is suspended and its modules with it.</summary>
    private static async Task LapseAsync(int schoolId)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var subscriptionStatus = (int)SubscriptionStatus.Suspended;
        var moduleStatus = (int)TenantModuleStatus.Suspended;
        await context.Database.ExecuteSqlAsync($"UPDATE \"Subscriptions\" SET \"Status\" = {subscriptionStatus} WHERE \"SchoolId\" = {schoolId}");
        await context.Database.ExecuteSqlAsync($"UPDATE \"TenantModules\" SET \"Status\" = {moduleStatus} WHERE \"SchoolId\" = {schoolId}");
    }

    private static async Task<InvoiceDto> ReadAsync(HttpSession master, Guid id)
    {
        using var response = await master.GetAsync($"/api/Invoices/{id}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<InvoiceDto>(Json))!;
    }

    private static async Task<List<InvoiceDto>> ListAsync(HttpSession master, Guid? schoolId)
    {
        using var response = await master.GetAsync(schoolId is null ? "/api/Invoices" : $"/api/Invoices?schoolId={schoolId}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<InvoiceDto>>(Json))!;
    }

    private static async Task<SubscriptionDto> ReadSubscriptionAsync(HttpSession master, Guid schoolId)
    {
        using var response = await master.GetAsync($"/api/Schools/{schoolId}/subscription");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<SubscriptionDto>(Json))!;
    }

    private static async Task<PriceBookDto> ReadPriceBookAsync(HttpSession master)
    {
        using var response = await master.GetAsync($"/api/PriceBooks/{_bookId}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<PriceBookDto>(Json))!;
    }

    private static async Task<HttpSession> SignInToSchoolAsync(int schoolId, string domain, string email)
    {
        await TestApp.RunAsUserAsync(schoolId, email, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(domain);
        using var response = await session.SignInAsync(email, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return session;
    }

    private static async Task<HttpSession> SignInAsMasterAsync()
    {
        var master = TestHttp.CreateDevelopmentSession();
        using var response = await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return master;
    }
}
