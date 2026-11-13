using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.Plans.Common;
using EducationPortal.Application.PriceBooks.Common;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Application.FunctionalTests.Pricing;

/// <summary>
/// Plans and regional price books (ADR-010): only master tenant administrators manage them, every change is audited,
/// and a plan or module can be priced once per price book and billing cycle (SEC-AUTHZ-07, -09, SEC-LOG-03).
/// </summary>
public class PricingTests : TestBase
{
    // Enums travel by name (ADR-008).
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private const string SchoolDomain = "school1.localhost";
    private const string SchoolAdminEmail = "admin@school1.local";

    [Test]
    public async Task ShouldAddChangeAndDeleteAPlan()
    {
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();

        var id = await CreatePlanAsync(master, "BASIC", [ModuleCodes.Attendance, ModuleCodes.Exams]);

        var plan = await ReadPlanAsync(master, id);
        plan.ShouldSatisfyAllConditions(
            p => p.Code.ShouldBe("BASIC"),
            p => p.IsActive.ShouldBeTrue(),
            p => p.Modules.ShouldBe([ModuleCodes.Attendance, ModuleCodes.Exams]));

        using var changed = await master.PutAsync($"/api/Plans/{id}", new { name = "Basic plus", description = "More.", isActive = false, modules = new[] { ModuleCodes.Fees } });
        changed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadPlanAsync(master, id)).ShouldSatisfyAllConditions(
            p => p.Name.ShouldBe("Basic plus"),
            p => p.IsActive.ShouldBeFalse(),
            p => p.Modules.ShouldBe([ModuleCodes.Fees]));

        using var deleted = await master.DeleteAsync($"/api/Plans/{id}");
        deleted.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        using var gone = await master.GetAsync($"/api/Plans/{id}");
        gone.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        // The code is free again after a delete.
        await CreatePlanAsync(master, "BASIC", []);

        var audit = await TestApp.FindAuditEventsAsync(masterUserId);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.PlanCreated);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.PlanChanged);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.PlanDeleted);
    }

    [Test]
    public async Task ShouldRefuseABadPlan()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        await CreatePlanAsync(master, "BASIC", []);

        using var duplicate = await master.PostAsync("/api/Plans", new { code = "BASIC", name = "x", description = "", modules = Array.Empty<string>() });
        using var badCode = await master.PostAsync("/api/Plans", new { code = "basic plan", name = "x", description = "", modules = Array.Empty<string>() });
        using var core = await master.PostAsync("/api/Plans", new { code = "A", name = "x", description = "", modules = new[] { ModuleCodes.Students } });
        using var unknown = await master.PostAsync("/api/Plans", new { code = "B", name = "x", description = "", modules = new[] { "NOPE" } });
        using var twice = await master.PostAsync("/api/Plans", new { code = "C", name = "x", description = "", modules = new[] { ModuleCodes.Fees, ModuleCodes.Fees } });
        using var noName = await master.PostAsync("/api/Plans", new { code = "D", name = "", description = "", modules = Array.Empty<string>() });

        new[] { duplicate, badCode, core, unknown, twice, noName }
            .ShouldAllBe(r => r.StatusCode == HttpStatusCode.BadRequest);
        (await TestApp.CountAsync<Plan>()).ShouldBe(1);
    }

    [Test]
    public async Task ShouldAddAPriceBookAndSetReplaceAndRemovePrices()
    {
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        await CreatePlanAsync(master, "BASIC", [ModuleCodes.Attendance]);

        var bookId = await CreatePriceBookAsync(master, "pk", "PKR");

        (await SetPriceAsync(master, bookId, new { planCode = "BASIC", billingCycle = "Monthly", price = 25000 })).ShouldBe(HttpStatusCode.NoContent);
        (await SetPriceAsync(master, bookId, new { planCode = "BASIC", billingCycle = "Yearly", price = 250000 })).ShouldBe(HttpStatusCode.NoContent);
        (await SetPriceAsync(master, bookId, new { moduleCode = ModuleCodes.Fees, billingCycle = "Monthly", price = 8000.5 })).ShouldBe(HttpStatusCode.NoContent);
        (await SetPriceAsync(master, bookId, new { planCode = "BASIC", billingCycle = "Monthly", price = 26000 })).ShouldBe(HttpStatusCode.NoContent);

        var book = await ReadPriceBookAsync(master, bookId);
        book.CountryCode.ShouldBe("PK");
        book.Prices.ShouldBe(
        [
            new PriceDto("BASIC", null, BillingCycle.Monthly, 26000m),
            new PriceDto("BASIC", null, BillingCycle.Yearly, 250000m),
            new PriceDto(null, ModuleCodes.Fees, BillingCycle.Monthly, 8000.5m),
        ]);

        using var removed = await master.DeleteAsync($"/api/PriceBooks/{bookId}/prices?planCode=BASIC&billingCycle=Yearly");
        using var removedAgain = await master.DeleteAsync($"/api/PriceBooks/{bookId}/prices?planCode=BASIC&billingCycle=Yearly");
        removed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        removedAgain.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadPriceBookAsync(master, bookId)).Prices.Count.ShouldBe(2);

        using var changed = await master.PutAsync($"/api/PriceBooks/{bookId}", new { currencyCode = "USD", isActive = false });
        changed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadPriceBookAsync(master, bookId)).ShouldSatisfyAllConditions(b => b.CurrencyCode.ShouldBe("USD"), b => b.IsActive.ShouldBeFalse());

        var audit = await TestApp.FindAuditEventsAsync(masterUserId);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.PriceBookCreated);
        audit.ShouldContain(e => e.EventType == SecurityAuditEventType.PriceBookChanged);
        audit.Count(e => e.EventType == SecurityAuditEventType.PriceChanged).ShouldBe(5);
    }

    [Test]
    public async Task ShouldRefuseBadPricesAndPriceBooks()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        await CreatePlanAsync(master, "BASIC", []);
        var bookId = await CreatePriceBookAsync(master, "PK", "PKR");

        using var secondBook = await master.PostAsync("/api/PriceBooks", new { countryCode = "pk", currencyCode = "PKR" });
        using var badCountry = await master.PostAsync("/api/PriceBooks", new { countryCode = "PAK", currencyCode = "PKR" });
        using var badCurrency = await master.PostAsync("/api/PriceBooks", new { countryCode = "SA", currencyCode = "sar" });

        var results = new[]
        {
            await SetPriceAsync(master, bookId, new { planCode = "BASIC", billingCycle = "Monthly", price = -1 }),
            await SetPriceAsync(master, bookId, new { planCode = "BASIC", billingCycle = "Monthly", price = 1.234 }),
            await SetPriceAsync(master, bookId, new { planCode = "BASIC", moduleCode = ModuleCodes.Fees, billingCycle = "Monthly", price = 1 }),
            await SetPriceAsync(master, bookId, new { billingCycle = "Monthly", price = 1 }),
            await SetPriceAsync(master, bookId, new { moduleCode = ModuleCodes.Students, billingCycle = "Monthly", price = 1 }),
            await SetPriceAsync(master, bookId, new { moduleCode = "NOPE", billingCycle = "Monthly", price = 1 }),
            await SetPriceAsync(master, bookId, new { planCode = "NOPE", billingCycle = "Monthly", price = 1 }),
            await SetPriceAsync(master, bookId, new { planCode = "BASIC", billingCycle = "Weekly", price = 1 }),
        };
        using var missing = await master.PutAsync($"/api/PriceBooks/{Guid.NewGuid()}/prices", new { planCode = "BASIC", billingCycle = "Monthly", price = 1 });

        new[] { secondBook, badCountry, badCurrency }.ShouldAllBe(r => r.StatusCode == HttpStatusCode.BadRequest);
        results.ShouldAllBe(r => r == HttpStatusCode.BadRequest);
        missing.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await TestApp.CountAsync<PriceBookItem>()).ShouldBe(0);
    }

    [Test]
    public async Task ShouldDeleteAPriceBookWithItsPricesAndFreeTheCountry()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        await CreatePlanAsync(master, "BASIC", []);
        var bookId = await CreatePriceBookAsync(master, "PK", "PKR");
        await SetPriceAsync(master, bookId, new { planCode = "BASIC", billingCycle = "Monthly", price = 1 });

        using var deleted = await master.DeleteAsync($"/api/PriceBooks/{bookId}");
        deleted.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await TestApp.CountAsync<PriceBookItem>()).ShouldBe(0);
        using var gone = await master.GetAsync($"/api/PriceBooks/{bookId}");
        gone.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await CreatePriceBookAsync(master, "PK", "PKR");
    }

    [Test]
    public async Task ShouldRemoveADeletedPlanFromEveryPriceBook()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var planId = await CreatePlanAsync(master, "BASIC", []);
        var bookId = await CreatePriceBookAsync(master, "PK", "PKR");
        await SetPriceAsync(master, bookId, new { planCode = "BASIC", billingCycle = "Monthly", price = 1 });

        (await master.DeleteAsync($"/api/Plans/{planId}")).Dispose();

        (await ReadPriceBookAsync(master, bookId)).Prices.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldNotDeleteAModuleThatAPlanOrAPriceUses()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        (await master.PostAsync("/api/Modules", new { code = "CANTEEN", name = "Canteen", description = "", isCore = false, sortOrder = 300 })).Dispose();
        (await master.PostAsync("/api/Modules", new { code = "KIOSK", name = "Kiosk", description = "", isCore = false, sortOrder = 301 })).Dispose();
        await CreatePlanAsync(master, "BASIC", ["CANTEEN"]);
        var bookId = await CreatePriceBookAsync(master, "PK", "PKR");
        await SetPriceAsync(master, bookId, new { moduleCode = "KIOSK", billingCycle = "Monthly", price = 1 });

        using var inPlan = await master.DeleteAsync("/api/Modules/CANTEEN");
        using var priced = await master.DeleteAsync("/api/Modules/KIOSK");

        inPlan.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        priced.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        (await master.DeleteAsync($"/api/PriceBooks/{bookId}/prices?moduleCode=KIOSK&billingCycle=Monthly")).Dispose();
        using var free = await master.DeleteAsync("/api/Modules/KIOSK");
        free.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task ShouldKeepPlansAndPricesAwayFromSchoolAdministrators()
    {
        await TestApp.AddSchoolAsync("School One", SchoolDomain);
        using var session = await SignInToSchoolAsync();
        var id = Guid.NewGuid();

        var responses = new[]
        {
            await session.GetAsync("/api/Plans"),
            await session.GetAsync($"/api/Plans/{id}"),
            await session.PostAsync("/api/Plans", new { code = "BASIC", name = "Basic", description = "", modules = Array.Empty<string>() }),
            await session.PutAsync($"/api/Plans/{id}", new { name = "Basic", description = "", isActive = true, modules = Array.Empty<string>() }),
            await session.DeleteAsync($"/api/Plans/{id}"),
            await session.GetAsync("/api/PriceBooks"),
            await session.GetAsync($"/api/PriceBooks/{id}"),
            await session.PostAsync("/api/PriceBooks", new { countryCode = "PK", currencyCode = "PKR" }),
            await session.PutAsync($"/api/PriceBooks/{id}", new { currencyCode = "PKR", isActive = true }),
            await session.DeleteAsync($"/api/PriceBooks/{id}"),
            await session.PutAsync($"/api/PriceBooks/{id}/prices", new { planCode = "BASIC", billingCycle = "Monthly", price = 1 }),
            await session.DeleteAsync($"/api/PriceBooks/{id}/prices?planCode=BASIC&billingCycle=Monthly"),
        };

        foreach (var response in responses)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden, response.RequestMessage?.RequestUri?.ToString());
            response.Dispose();
        }

        (await TestApp.CountAsync<Plan>()).ShouldBe(0);
        (await TestApp.CountAsync<PriceBook>()).ShouldBe(0);
    }

    [Test]
    public async Task ShouldRequireSignIn()
    {
        using var session = TestHttp.CreateDevelopmentSession(SchoolDomain);
        await TestApp.AddSchoolAsync("School One", SchoolDomain);

        using var plans = await session.GetAsync("/api/Plans");
        using var books = await session.GetAsync("/api/PriceBooks");
        using var create = await session.PostAsync("/api/PriceBooks", new { countryCode = "PK", currencyCode = "PKR" });

        plans.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        books.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        create.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task ShouldSeedThePlansAndThePriceBooksOfExistingCountriesOnceOnly()
    {
        await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");
        await TestApp.AddCountryAsync("US", "United States", ["en"], "en", "America/New_York", "USD");

        for (var run = 0; run < 2; run++)
        {
            using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
            await scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>().SeedPricingAsync();
        }

        var paid = ModuleCatalog.CreateModules().Count(m => !m.IsCore);
        (await TestApp.CountAsync<Plan>()).ShouldBe(PricingCatalog.Plans.Count);
        (await TestApp.CountAsync<PriceBook>()).ShouldBe(2); // SA and AE have no country yet
        (await TestApp.CountAsync<PriceBookItem>()).ShouldBe(2 * 2 * (PricingCatalog.Plans.Count + paid));
        (await TestApp.CountAsync<PlanModule>()).ShouldBe(PricingCatalog.Plans.Sum(p => p.Modules.Count));
    }

    private static async Task<Guid> CreatePlanAsync(HttpSession master, string code, string[] modules)
    {
        using var response = await master.PostAsync("/api/Plans", new { code, name = code, description = "", modules });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static async Task<PlanDto> ReadPlanAsync(HttpSession master, Guid id)
    {
        using var response = await master.GetAsync($"/api/Plans/{id}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<PlanDto>())!;
    }

    private static async Task<Guid> CreatePriceBookAsync(HttpSession master, string countryCode, string currencyCode)
    {
        using var response = await master.PostAsync("/api/PriceBooks", new { countryCode, currencyCode });
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static async Task<PriceBookDto> ReadPriceBookAsync(HttpSession master, Guid id)
    {
        using var response = await master.GetAsync($"/api/PriceBooks/{id}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<PriceBookDto>(Json))!;
    }

    private static async Task<HttpStatusCode> SetPriceAsync(HttpSession master, Guid bookId, object body)
    {
        using var response = await master.PutAsync($"/api/PriceBooks/{bookId}/prices", body);
        return response.StatusCode;
    }

    private static async Task<HttpSession> SignInToSchoolAsync()
    {
        var schoolId = (await TestApp.FindSchoolIdAsync(SchoolDomain))!;
        await TestApp.RunAsUserAsync(schoolId.Id, SchoolAdminEmail, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(SchoolDomain);
        using var response = await session.SignInAsync(SchoolAdminEmail, TestApp.AdministratorPassword);
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
