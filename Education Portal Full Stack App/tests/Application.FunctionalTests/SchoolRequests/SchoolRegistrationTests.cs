using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Application.Schools.Registration;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.SchoolRequests;

/// <summary>
/// The public self-service registration (decided 2026-11-19): a visitor registers a school with its country, administrator
/// email and password, and the school, its 3-month trial and its administrator are created at once, with no master approval.
/// Refused registrations create nothing. The master no longer approves or rejects requests.
/// </summary>
public class SchoolRegistrationTests : TestBase
{
    private const string Password = "violet harbour lantern 47";

    [Test]
    public async Task ShouldRegisterASchoolWithATrialAndLetItsAdministratorSignIn()
    {
        var countryId = await SetUpPriceBookAsync();
        var anonymous = TestHttp.CreateDevelopmentSession();

        using var response = await anonymous.PostAsync("/api/SchoolRequests", new
        {
            schoolName = "Self Service School",
            countryId,
            contactName = "Ayesha Khan",
            contactEmail = "admin@self-service.example",
            administratorPassword = Password,
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var schoolId = await response.Content.ReadFromJsonAsync<Guid>();
        var school = (await TestApp.FindAsync<School>(await TestApp.FindIdAsync<School>(schoolId)))!;
        school.Domain.ShouldBeNull();
        school.DataRegion.ShouldBe(DataRegion.SouthAsia);

        // The administrator signs in straight away on the master domain, because the school has no domain yet.
        var admin = TestHttp.CreateDevelopmentSession();
        using var signIn = await admin.SignInAsync("admin@self-service.example", Password);
        signIn.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var subscription = await admin.GetAsync("/api/Subscriptions/mine");
        subscription.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = (await subscription.Content.ReadFromJsonAsync<MySubscriptionReadModel>(Json))!.Subscription;
        body.ShouldNotBeNull();
        body.Status.ShouldBe(SubscriptionStatus.Trial);
        var trialEnd = body.TrialEndDate.ShouldNotBeNull();
        SchoolRegistration.TrialDays.ShouldBe(90);
        trialEnd.ShouldBeGreaterThan(DateTimeOffset.UtcNow.AddDays(SchoolRegistration.TrialDays - 1));
        trialEnd.ShouldBeLessThanOrEqualTo(DateTimeOffset.UtcNow.AddDays(SchoolRegistration.TrialDays + 1));
    }

    [Test]
    public async Task ShouldRefuseAWeakPasswordAndCreateNothing()
    {
        var countryId = await SetUpPriceBookAsync();
        var schoolsBefore = await TestApp.CountAsync<School>();
        var anonymous = TestHttp.CreateDevelopmentSession();

        // The password passes the length rule but fails the policy (it names the product), which is checked after the school is saved.
        using var response = await anonymous.PostAsync("/api/SchoolRequests", Body("Weak School", countryId, "weak@weak.example", "educationportal 2026"));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, await response.Content.ReadAsStringAsync());
        (await TestApp.CountAsync<School>()).ShouldBe(schoolsBefore);
    }

    [Test]
    public async Task ShouldRefuseACountryWithoutAPriceBookAndCreateNothing()
    {
        await SetUpPriceBookAsync();
        var unpriced = await TestApp.AddCountryAsync("AE", "United Arab Emirates", ["en", "ar"], "en", "Asia/Dubai", "AED");
        var schoolsBefore = await TestApp.CountAsync<School>();
        var anonymous = TestHttp.CreateDevelopmentSession();

        using var response = await anonymous.PostAsync("/api/SchoolRequests", Body("Unpriced School", unpriced.PublicId, "admin@unpriced.example", Password));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await TestApp.CountAsync<School>()).ShouldBe(schoolsBefore);
    }

    [Test]
    public async Task ShouldRefuseAnAdministratorEmailThatAlreadyHasAnAccount()
    {
        var countryId = await SetUpPriceBookAsync();
        var anonymous = TestHttp.CreateDevelopmentSession();
        using (var first = await anonymous.PostAsync("/api/SchoolRequests", Body("First School", countryId, "taken@first.example", Password)))
        {
            first.StatusCode.ShouldBe(HttpStatusCode.Created);
        }

        using var second = await anonymous.PostAsync("/api/SchoolRequests", Body("Second School", countryId, "taken@first.example", Password));

        second.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldListOnlyThePricedCountriesToVisitors()
    {
        await SetUpPriceBookAsync();
        await TestApp.AddCountryAsync("AE", "United Arab Emirates", ["en", "ar"], "en", "Asia/Dubai", "AED");
        var anonymous = TestHttp.CreateDevelopmentSession();

        using var response = await anonymous.GetAsync("/api/SchoolRequests/countries");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var countries = (await response.Content.ReadFromJsonAsync<List<CountryReadModel>>(Json))!;
        countries.Select(c => c.Code).ShouldBe(["PK"]);
    }

    [Test]
    public async Task ShouldNoLongerHaveMasterApprovalOrTheRequestQueue()
    {
        await TestApp.RunAsAdministratorAsync();
        var master = TestHttp.CreateDevelopmentSession();
        using (var signIn = await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword))
        {
            signIn.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        using var approve = await master.PostAsync($"/api/SchoolRequests/{Guid.NewGuid()}/approve", new { });
        using var queue = await master.GetAsync("/api/SchoolRequests");

        approve.IsSuccessStatusCode.ShouldBeFalse();
        queue.StatusCode.ShouldNotBe(HttpStatusCode.OK);
    }

    /// <summary>Adds the country PK with a price book and the Full plan priced for it. Returns the country's public id.</summary>
    private static async Task<Guid> SetUpPriceBookAsync()
    {
        await TestApp.RunAsAdministratorAsync();
        await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");

        var master = TestHttp.CreateDevelopmentSession();
        using (var signIn = await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword))
        {
            signIn.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        (await master.PostAsync("/api/Plans", new { code = PricingCatalog.Full, name = "Full", description = "", modules = new[] { ModuleCode.Attendance, ModuleCode.Exams } })).Dispose();
        using var book = await master.PostAsync("/api/PriceBooks", new { countryCode = "PK", currencyCode = "PKR" });
        var bookId = await book.Content.ReadFromJsonAsync<Guid>();
        using var price = await master.PutAsync($"/api/PriceBooks/{bookId}/prices", new { planCode = PricingCatalog.Full, billingCycle = "Monthly", price = 30000 });
        price.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        return await CountryIdAsync("PK");
    }

    private static object Body(string schoolName, Guid countryId, string email, string password) => new
    {
        schoolName,
        countryId,
        contactName = "Ayesha Khan",
        contactEmail = email,
        administratorPassword = password,
    };

    private static async Task<Guid> CountryIdAsync(string code)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var country = await context.Countries.SingleAsync(c => c.Code == code);
        return country.PublicId;
    }

    // The API sends enums by name (ENG-ENUM-01).
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private sealed record MySubscriptionReadModel(SubscriptionReadModel? Subscription);

    private sealed record SubscriptionReadModel(Guid Id, SubscriptionStatus Status, DateTimeOffset? TrialEndDate);

    private sealed record CountryReadModel(Guid Id, string Name, string Code);
}
