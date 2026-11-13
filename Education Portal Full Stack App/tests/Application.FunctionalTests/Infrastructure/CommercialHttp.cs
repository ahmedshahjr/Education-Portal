using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Domain.Constants;

namespace EducationPortal.Application.FunctionalTests.Infrastructure;

/// <summary>
/// Sets up subscriptions and invoices through the real API, as the master tenant's administrator does, for tests that
/// check what happens around them (change audit, tenant isolation). The use cases themselves have their own tests.
/// </summary>
internal static class CommercialHttp
{
    // Enums travel by name (ADR-008).
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    public const string PlanCode = "BASIC";
    public const decimal PlanPrice = 25000m;

    /// <summary>A Pakistani price book with the BASIC plan (Attendance, monthly 25,000 PKR) and Library as a 500 PKR add-on.</summary>
    public static async Task<int> SetUpCatalogueAsync(HttpSession master)
    {
        var country = await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");

        await ExpectAsync(master.PostAsync("/api/Plans", new { code = PlanCode, name = "Basic", description = "", modules = new[] { ModuleCodes.Attendance } }), HttpStatusCode.OK);
        using var created = await master.PostAsync("/api/PriceBooks", new { countryCode = "PK", currencyCode = "PKR" });
        created.StatusCode.ShouldBe(HttpStatusCode.OK, await created.Content.ReadAsStringAsync());
        var bookId = await created.Content.ReadFromJsonAsync<Guid>();
        await ExpectAsync(master.PutAsync($"/api/PriceBooks/{bookId}/prices", new { planCode = PlanCode, billingCycle = "Monthly", price = PlanPrice }), HttpStatusCode.NoContent);
        await ExpectAsync(master.PutAsync($"/api/PriceBooks/{bookId}/prices", new { moduleCode = ModuleCodes.Library, billingCycle = "Monthly", price = 500 }), HttpStatusCode.NoContent);

        return country.Id;
    }

    /// <summary>Adds a school in the catalogue's country and gives it the BASIC plan, monthly, without a trial.</summary>
    public static async Task<EntityIds> AddSubscribedSchoolAsync(HttpSession master, int countryId, string name, string domain)
    {
        var school = await TestApp.AddSchoolAsync(name, domain, countryId);
        await ExpectAsync(
            master.PostAsync($"/api/Schools/{school.PublicId}/subscription", new { planCode = PlanCode, modules = Array.Empty<string>(), billingCycle = "Monthly", trialDays = 0 }),
            HttpStatusCode.OK);
        return school;
    }

    public static async Task<Guid> CreateInvoiceAsync(HttpSession master, Guid schoolId)
    {
        using var response = await master.PostAsync("/api/Invoices", new { schoolId, nextPeriod = false });
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    public static Task PayAsync(HttpSession master, Guid invoiceId, decimal amount) =>
        ExpectAsync(
            master.PostAsync($"/api/Invoices/{invoiceId}/payments", new { amount, method = "BankTransfer", reference = "TRX-1", receivedDate = DateTimeOffset.UtcNow.AddMinutes(-1) }),
            HttpStatusCode.OK);

    public static async Task<HttpSession> SignInAsMasterAsync()
    {
        var master = TestHttp.CreateDevelopmentSession();
        using var response = await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return master;
    }

    /// <summary>Signs in an administrator of a school on the school's own domain.</summary>
    public static async Task<HttpSession> SignInToSchoolAsync(int schoolId, string domain, string email)
    {
        await TestApp.RunAsUserAsync(schoolId, email, TestApp.AdministratorPassword, [Roles.Administrator]);
        var session = TestHttp.CreateDevelopmentSession(domain);
        using var response = await session.SignInAsync(email, TestApp.AdministratorPassword);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return session;
    }

    public static async Task ExpectAsync(Task<HttpResponseMessage> request, HttpStatusCode status)
    {
        using var response = await request;
        response.StatusCode.ShouldBe(status, await response.Content.ReadAsStringAsync());
    }
}
