using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.FunctionalTests.Subscriptions;

/// <summary>
/// A school's subscription (ADR-011): the master tenant starts it, the school gets core modules, the plan's modules,
/// add-ons and what they require, priced from its country's price book, all saved together. Only master tenant
/// administrators touch it, every change is audited, and a school never sees another's (SEC-AUTHZ-01, -03, -07, -09,
/// SEC-LOG-03).
/// </summary>
public class SubscriptionTests : TestBase
{
    // Enums travel by name (ADR-008).
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private const string SchoolDomain = "school1.localhost";
    private const string OtherSchoolDomain = "school2.localhost";

    [Test]
    public async Task ShouldGiveTheSchoolCoreModulesThePlanAndAddOnsAtThePriceBookPrice()
    {
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var schoolId = await AddPakistaniSchoolAsync("School One", SchoolDomain);
        await SetUpPricesAsync(master);

        var before = DateTimeOffset.UtcNow;
        var id = await SubscribeAsync(master, schoolId.PublicId, new { planCode = "BASIC", modules = new[] { ModuleCodes.Fees }, billingCycle = "Monthly", trialDays = 0 });

        var subscription = await ReadAsync(master, schoolId.PublicId);
        subscription.ShouldSatisfyAllConditions(
            s => s.Id.ShouldBe(id),
            s => s.PlanCode.ShouldBe("BASIC"),
            s => s.CountryCode.ShouldBe("PK"),
            s => s.CurrencyCode.ShouldBe("PKR"),
            s => s.BillingCycle.ShouldBe(BillingCycle.Monthly),
            s => s.Amount.ShouldBe(25000m + 8000m), // the plan plus Fees, which it does not include
            s => s.Status.ShouldBe(SubscriptionStatus.Active),
            s => s.TrialEndDate.ShouldBeNull(),
            s => s.StartDate.ShouldBeGreaterThanOrEqualTo(before.AddSeconds(-5)),
            s => s.EndDate.ShouldBe(s.StartDate.AddMonths(1)));

        var core = ModuleCatalog.CreateModules().Where(m => m.IsCore).Select(m => m.Code);
        subscription.Modules.Select(m => m.Code).OrderBy(c => c).ShouldBe(
            core.Concat([ModuleCodes.Attendance, ModuleCodes.Exams, ModuleCodes.Fees]).OrderBy(c => c).ToList());
        subscription.Modules.ShouldAllBe(m => m.Status == TenantModuleStatus.Active && m.ValidFrom == subscription.StartDate && m.ValidTo == subscription.EndDate);

        var audit = await TestApp.FindAuditEventsAsync(masterUserId);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.SubscriptionCreated);
    }

    [Test]
    public async Task ShouldStartATrialThatEndsAfterTheTrialDays()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var schoolId = await AddPakistaniSchoolAsync("School One", SchoolDomain);
        await SetUpPricesAsync(master);

        await SubscribeAsync(master, schoolId.PublicId, new { planCode = "BASIC", modules = Array.Empty<string>(), billingCycle = "Yearly", trialDays = 14 });

        var subscription = await ReadAsync(master, schoolId.PublicId);
        subscription.ShouldSatisfyAllConditions(
            s => s.Status.ShouldBe(SubscriptionStatus.Trial),
            s => s.TrialEndDate.ShouldBe(s.StartDate.AddDays(14)),
            s => s.EndDate.ShouldBe(s.TrialEndDate!.Value),
            s => s.Amount.ShouldBe(250000m));
    }

    [Test]
    public async Task ShouldPriceAndGrantTheModulesAnAddOnRequires()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var schoolId = await AddPakistaniSchoolAsync("School One", SchoolDomain);
        await SetUpPricesAsync(master);
        (await master.PostAsync("/api/Modules", new { code = "KIOSK", name = "Kiosk", description = "", isCore = false, sortOrder = 300 })).Dispose();
        (await master.PostAsync("/api/Modules", new { code = "CANTEEN", name = "Canteen", description = "", isCore = false, sortOrder = 301 })).Dispose();
        (await master.PostAsync("/api/Modules/CANTEEN/dependencies", new { requiresCode = "KIOSK" })).Dispose();
        await SetPriceAsync(master, new { moduleCode = "CANTEEN", billingCycle = "Monthly", price = 100 });
        await SetPriceAsync(master, new { moduleCode = "KIOSK", billingCycle = "Monthly", price = 40 });

        await SubscribeAsync(master, schoolId.PublicId, new { modules = new[] { "CANTEEN" }, billingCycle = "Monthly", trialDays = 0 });

        var subscription = await ReadAsync(master, schoolId.PublicId);
        subscription.PlanCode.ShouldBeNull();
        subscription.Amount.ShouldBe(140m);
        subscription.Modules.Select(m => m.Code).ShouldContain("KIOSK");
        subscription.Modules.Select(m => m.Code).ShouldContain("CANTEEN");
    }

    [Test]
    public async Task ShouldRefuseASecondSubscriptionAndTheMasterAndASchoolWithoutACountry()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var schoolId = await AddPakistaniSchoolAsync("School One", SchoolDomain);
        var noCountry = await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        await SetUpPricesAsync(master);
        var body = new { planCode = "BASIC", modules = Array.Empty<string>(), billingCycle = "Monthly", trialDays = 0 };
        await SubscribeAsync(master, schoolId.PublicId, body);

        using var second = await master.PostAsync($"/api/Schools/{schoolId.PublicId}/subscription", body);
        using var masterSchool = await master.PostAsync($"/api/Schools/{TestApp.MasterSchoolPublicId}/subscription", body);
        using var none = await master.PostAsync($"/api/Schools/{noCountry.PublicId}/subscription", body);

        second.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        masterSchool.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        none.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await TestApp.CountAsync<Subscription>()).ShouldBe(1);
    }

    [Test]
    public async Task ShouldRefuseBadChoicesAndWriteNothing()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var schoolId = await AddPakistaniSchoolAsync("School One", SchoolDomain);
        await SetUpPricesAsync(master);
        (await master.PostAsync("/api/Plans", new { code = "UNPRICED", name = "Unpriced", description = "", modules = Array.Empty<string>() })).Dispose();
        var path = $"/api/Schools/{schoolId.PublicId}/subscription";
        string[] none = [];

        var responses = new[]
        {
            await master.PostAsync(path, new { modules = none, billingCycle = "Monthly", trialDays = 0 }),                              // nothing chosen
            await master.PostAsync(path, new { planCode = "NOPE", modules = none, billingCycle = "Monthly", trialDays = 0 }),           // unknown plan
            await master.PostAsync(path, new { planCode = "UNPRICED", modules = none, billingCycle = "Monthly", trialDays = 0 }),       // no price
            await master.PostAsync(path, new { modules = new[] { ModuleCodes.Students }, billingCycle = "Monthly", trialDays = 0 }),    // core add-on
            await master.PostAsync(path, new { modules = new[] { "NOPE" }, billingCycle = "Monthly", trialDays = 0 }),                  // unknown module
            await master.PostAsync(path, new { modules = new[] { ModuleCodes.Fees, ModuleCodes.Fees }, billingCycle = "Monthly", trialDays = 0 }),
            await master.PostAsync(path, new { modules = new[] { ModuleCodes.Library }, billingCycle = "Monthly", trialDays = 0 }),          // Library has no price in the price book
            await master.PostAsync(path, new { planCode = "BASIC", modules = none, billingCycle = "Weekly", trialDays = 0 }),
            await master.PostAsync(path, new { planCode = "BASIC", modules = none, billingCycle = "Monthly", trialDays = 91 }),
            await master.PostAsync(path, new { planCode = "BASIC", modules = none, billingCycle = "Monthly", trialDays = -1 }),
        };

        foreach (var response in responses)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, response.RequestMessage?.RequestUri?.ToString());
            response.Dispose();
        }

        (await TestApp.CountAsync<Subscription>()).ShouldBe(0);
        (await TestApp.CountAsync<TenantModule>()).ShouldBe(0);
    }

    [Test]
    public async Task ShouldSetAndClearTheDataRegion()
    {
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);

        using var set = await master.PutAsync($"/api/Schools/{schoolId.PublicId}/data-region", new { dataRegion = "MiddleEast" });
        (await TestApp.FindAsync<School>(schoolId.Id))!.DataRegion.ShouldBe(DataRegion.MiddleEast);
        using var cleared = await master.PutAsync($"/api/Schools/{schoolId.PublicId}/data-region", new { dataRegion = (string?)null });
        using var bad = await master.PutAsync($"/api/Schools/{schoolId.PublicId}/data-region", new { dataRegion = "Mars" });
        using var missing = await master.PutAsync($"/api/Schools/{Guid.NewGuid()}/data-region", new { dataRegion = "MiddleEast" });

        set.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        cleared.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await TestApp.FindAsync<School>(schoolId.Id))!.DataRegion.ShouldBeNull();
        bad.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        missing.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await TestApp.FindAuditEventsAsync(masterUserId)).Count(e => e.EventType == SecurityAuditEventType.DataRegionChanged).ShouldBe(2);
    }

    [Test]
    public async Task ShouldAnswerNotFoundForASchoolWithoutASubscription()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);

        using var none = await master.GetAsync($"/api/Schools/{schoolId.PublicId}/subscription");
        using var unknown = await master.GetAsync($"/api/Schools/{Guid.NewGuid()}/subscription");

        none.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task ShouldKeepSubscriptionsAwayFromSchoolAdministratorsAndHideThemFromOtherSchools()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var schoolId = await AddPakistaniSchoolAsync("School One", SchoolDomain);
        var other = await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        await SetUpPricesAsync(master);
        await SubscribeAsync(master, schoolId.PublicId, new { planCode = "BASIC", modules = Array.Empty<string>(), billingCycle = "Monthly", trialDays = 0 });

        using var otherSession = await SignInToSchoolAsync(other.Id, OtherSchoolDomain, "admin@school2.local");
        var responses = new[]
        {
            await otherSession.GetAsync($"/api/Schools/{schoolId.PublicId}/subscription"),
            await otherSession.PostAsync($"/api/Schools/{other.PublicId}/subscription", new { planCode = "BASIC", modules = Array.Empty<string>(), billingCycle = "Monthly", trialDays = 0 }),
            await otherSession.PutAsync($"/api/Schools/{other.PublicId}/data-region", new { dataRegion = "MiddleEast" }),
        };

        foreach (var response in responses)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden, response.RequestMessage?.RequestUri?.ToString());
            response.Dispose();
        }

        TestApp.UseTenant(TestApp.MasterSchoolId, isMaster: true);
        (await TestApp.CountAsync<Subscription>()).ShouldBe(1);
        (await TestApp.FindAsync<School>(other.Id))!.DataRegion.ShouldBeNull();
    }

    [Test]
    public async Task ShouldRequireSignIn()
    {
        var schoolId = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var session = TestHttp.CreateDevelopmentSession(SchoolDomain);

        using var read = await session.GetAsync($"/api/Schools/{schoolId.PublicId}/subscription");
        using var create = await session.PostAsync($"/api/Schools/{schoolId.PublicId}/subscription", new { planCode = "BASIC", modules = Array.Empty<string>(), billingCycle = "Monthly", trialDays = 0 });
        using var region = await session.PutAsync($"/api/Schools/{schoolId.PublicId}/data-region", new { dataRegion = "MiddleEast" });

        read.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        create.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        region.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private static async Task<EntityIds> AddPakistaniSchoolAsync(string name, string domain)
    {
        var country = await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");
        return await TestApp.AddSchoolAsync(name, domain, country.Id);
    }

    /// <summary>A BASIC plan (Attendance, Exams) at 25,000 monthly and 250,000 yearly, Fees at 8,000 monthly, in PKR.</summary>
    private static async Task SetUpPricesAsync(HttpSession master)
    {
        (await master.PostAsync("/api/Plans", new { code = "BASIC", name = "Basic", description = "", modules = new[] { ModuleCodes.Attendance, ModuleCodes.Exams } })).Dispose();
        using var created = await master.PostAsync("/api/PriceBooks", new { countryCode = "PK", currencyCode = "PKR" });
        var bookId = await created.Content.ReadFromJsonAsync<Guid>();
        _bookId = bookId;
        await SetPriceAsync(master, new { planCode = "BASIC", billingCycle = "Monthly", price = 25000 });
        await SetPriceAsync(master, new { planCode = "BASIC", billingCycle = "Yearly", price = 250000 });
        await SetPriceAsync(master, new { moduleCode = ModuleCodes.Fees, billingCycle = "Monthly", price = 8000 });
    }

    private static Guid _bookId;

    private static async Task SetPriceAsync(HttpSession master, object body)
    {
        using var response = await master.PutAsync($"/api/PriceBooks/{_bookId}/prices", body);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    private static async Task<Guid> SubscribeAsync(HttpSession master, Guid schoolId, object body)
    {
        using var response = await master.PostAsync($"/api/Schools/{schoolId}/subscription", body);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static async Task<SubscriptionDto> ReadAsync(HttpSession master, Guid schoolId)
    {
        using var response = await master.GetAsync($"/api/Schools/{schoolId}/subscription");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<SubscriptionDto>(Json))!;
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
