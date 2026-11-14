using System.Net;
using System.Net.Http.Json;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Application.Subscriptions.Queries.GetMySubscription;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static EducationPortal.Application.FunctionalTests.Infrastructure.CommercialHttp;

namespace EducationPortal.Application.FunctionalTests.Tenancy;

/// <summary>
/// One school can never read another school's subscription, modules, invoices, payments, add-on requests or change audit
/// entries: not through the API, where only the master tenant reads billing and a school reads only its own
/// subscription, and not through the database, where the tenant query filters hide the rows (SEC-AUTHZ-03, -07, -09).
/// </summary>
public class CommercialIsolationTests : TestBase
{
    private const string DomainOne = "school1.localhost";
    private const string DomainTwo = "school2.localhost";
    private const string SessionCookieName = "__Host-EducationPortal";

    [Test]
    public async Task ShouldShowASchoolOnlyItsOwnSubscription()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var (one, two, _) = await SetUpTwoPaidSchoolsAsync(master);
        var oneSubscription = await ReadSubscriptionAsync(master, one.PublicId);
        var twoSubscription = await ReadSubscriptionAsync(master, two.PublicId);
        using var session = await SignInToSchoolAsync(one.Id, DomainOne, "admin@school1.local");

        using var response = await session.GetAsync("/api/Subscriptions/mine");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var mine = (await response.Content.ReadFromJsonAsync<MySubscriptionDto>(Json))!;
        mine.Subscription.ShouldNotBeNull().Id.ShouldBe(oneSubscription.Id);
        mine.Subscription.Id.ShouldNotBe(twoSubscription.Id);
    }

    [Test]
    public async Task ShouldRefuseASchoolAnotherSchoolsSubscriptionAndInvoicesThroughTheApi()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var (one, two, twoInvoice) = await SetUpTwoPaidSchoolsAsync(master);
        using var session = await SignInToSchoolAsync(one.Id, DomainOne, "admin@school1.local");

        (await session.GetAsync($"/api/Schools/{two.PublicId}/subscription")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await session.GetAsync($"/api/Schools/{two.PublicId}/modules")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await session.GetAsync($"/api/Invoices/{twoInvoice}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await session.GetAsync($"/api/Invoices?schoolId={two.PublicId}")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await session.GetAsync("/api/AddOnRequests")).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldNotLetASchoolsSessionReadAnotherSchoolsSubscriptionOnItsDomain()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var (one, _, _) = await SetUpTwoPaidSchoolsAsync(master);
        using var session = await SignInToSchoolAsync(one.Id, DomainOne, "admin@school1.local");

        // The same cookie sent to school two's domain belongs to another tenant and is refused (SEC-AUTHZ-10).
        using var onOtherDomain = TestHttp.CreateDevelopmentSession(DomainTwo);
        onOtherDomain.SetCookie(SessionCookieName, session.Cookie(SessionCookieName).ShouldNotBeNull());

        (await onOtherDomain.GetAsync("/api/Subscriptions/mine")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldHideEveryCommercialRowOfAnotherSchoolInTheDatabase()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var (one, two, _) = await SetUpTwoPaidSchoolsAsync(master);
        foreach (var (school, domain) in new[] { (one, DomainOne), (two, DomainTwo) })
        {
            using var session = await SignInToSchoolAsync(school.Id, domain, $"admin@{domain}");
            await ExpectAsync(session.PostAsync("/api/AddOnRequests", new { moduleCode = ModuleCodes.Library }), HttpStatusCode.OK);
        }

        var asOne = await SchoolIdsAsync(new ResolvedTenant(one.Id, IsMaster: false));
        var asMaster = await SchoolIdsAsync(new ResolvedTenant(TestApp.MasterSchoolId, IsMaster: true));

        foreach (var (table, ids) in asOne)
        {
            ids.ShouldNotBeEmpty(table);
            ids.ShouldAllBe(id => id == one.Id, table);
            asMaster[table].ShouldContain(two.Id, table); // the rows exist; the filter hides them
        }
    }

    /// <summary>Two schools on BASIC; each has an invoice, and school one's is paid. Returns school two's invoice.</summary>
    private static async Task<(EntityIds One, EntityIds Two, Guid TwoInvoice)> SetUpTwoPaidSchoolsAsync(HttpSession master)
    {
        var countryId = await SetUpCatalogueAsync(master);
        var one = await AddSubscribedSchoolAsync(master, countryId, "School One", DomainOne);
        var two = await AddSubscribedSchoolAsync(master, countryId, "School Two", DomainTwo);
        await PayAsync(master, await CreateInvoiceAsync(master, one.PublicId), PlanPrice);
        var twoInvoice = await CreateInvoiceAsync(master, two.PublicId);
        await PayAsync(master, twoInvoice, PlanPrice);
        return (one, two, twoInvoice);
    }

    /// <summary>The SchoolId of every commercial row a tenant can read, per table.</summary>
    private static async Task<Dictionary<string, List<int?>>> SchoolIdsAsync(ResolvedTenant tenant)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(tenant);
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return new Dictionary<string, List<int?>>
        {
            [nameof(Subscription)] = await context.Subscriptions.Select(r => (int?)r.SchoolId).ToListAsync(),
            [nameof(TenantModule)] = await context.TenantModules.Select(r => (int?)r.SchoolId).ToListAsync(),
            [nameof(Invoice)] = await context.Invoices.Select(r => (int?)r.SchoolId).ToListAsync(),
            [nameof(InvoiceLine)] = await context.InvoiceLines.Select(r => (int?)r.SchoolId).ToListAsync(),
            [nameof(Payment)] = await context.Payments.Select(r => (int?)r.SchoolId).ToListAsync(),
            [nameof(AddOnRequest)] = await context.AddOnRequests.Select(r => (int?)r.SchoolId).ToListAsync(),
            [nameof(ChangeAuditEntry)] = await context.ChangeAuditEntries.Select(r => r.SchoolId).ToListAsync(),
        };
    }

    private static async Task<SubscriptionDto> ReadSubscriptionAsync(HttpSession master, Guid schoolId)
    {
        using var response = await master.GetAsync($"/api/Schools/{schoolId}/subscription");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<SubscriptionDto>(Json))!;
    }
}
