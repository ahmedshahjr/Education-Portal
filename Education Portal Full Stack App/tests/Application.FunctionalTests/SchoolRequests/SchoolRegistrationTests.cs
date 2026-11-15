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
/// A school applies anonymously; the master tenant approves it, which registers the school with its 30-day free trial and
/// the modules that are on by default. Rejecting creates nothing. Only the master tenant reads and decides the queue.
/// </summary>
public class SchoolRegistrationTests : TestBase
{
    private const string NewSchoolDomain = "approved.localhost";
    private const string AdministratorEmail = "admin@approved.local";

    [Test]
    public async Task ShouldApproveARequestAndRegisterTheSchoolWithAThirtyDayTrial()
    {
        var master = await SetUpPriceBookAsync();
        var requestId = await SubmitRequestAsync("Approved School");

        using var approve = await master.PostAsync($"/api/SchoolRequests/{requestId}/approve", new
        {
            domain = NewSchoolDomain,
            countryId = await CountryIdAsync("PK"),
            administratorEmail = AdministratorEmail,
            administratorPassword = TestApp.AdministratorPassword,
        });
        approve.StatusCode.ShouldBe(HttpStatusCode.OK);
        var schoolId = await approve.Content.ReadFromJsonAsync<Guid>();

        (await ReadRequestsAsync(master)).Single(r => r.Id == requestId).Status.ShouldBe(SchoolRequestStatus.Approved);

        using var subscription = await master.GetAsync($"/api/Schools/{schoolId}/subscription");
        subscription.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await subscription.Content.ReadFromJsonAsync<SubscriptionReadModel>(Json);
        body.ShouldNotBeNull();
        body.Status.ShouldBe(SubscriptionStatus.Trial);
        var trialEnd = body.TrialEndDate.ShouldNotBeNull();
        trialEnd.ShouldBeGreaterThan(DateTimeOffset.UtcNow.AddDays(SchoolRegistration.TrialDays - 1));
        trialEnd.ShouldBeLessThanOrEqualTo(DateTimeOffset.UtcNow.AddDays(SchoolRegistration.TrialDays + 1));
    }

    [Test]
    public async Task ShouldRefuseToApproveARequestTwice()
    {
        var master = await SetUpPriceBookAsync();
        var requestId = await SubmitRequestAsync("Twice School");
        using var first = await Approve(master, requestId, "twice.localhost", "twice@approved.local");
        first.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var second = await Approve(master, requestId, "twice-again.localhost", "twice-again@approved.local");

        second.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseACountryWithoutAPriceBook()
    {
        var master = await SetUpPriceBookAsync();
        var requestId = await SubmitRequestAsync("Unpriced School");
        var unpriced = await TestApp.AddCountryAsync("AE", "United Arab Emirates", ["en", "ar"], "en", "Asia/Dubai", "AED");

        using var response = await master.PostAsync($"/api/SchoolRequests/{requestId}/approve", new
        {
            domain = "unpriced.localhost",
            countryId = unpriced.PublicId,
            administratorEmail = "admin@unpriced.local",
            administratorPassword = TestApp.AdministratorPassword,
        });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadRequestsAsync(master)).Single(r => r.Id == requestId).Status.ShouldBe(SchoolRequestStatus.Pending);
    }

    [Test]
    public async Task ShouldLeaveTheRequestPendingAndCreateNothingWhenTheSchoolCannotBeRegistered()
    {
        var master = await SetUpPriceBookAsync();
        var requestId = await SubmitRequestAsync("Rolled Back School");
        var schoolsBefore = await TestApp.CountAsync<School>();
        var subscriptionsBefore = await TestApp.CountAsync<Subscription>();

        // The password passes the length rule but fails the password policy, which is only checked after the school is saved.
        using var response = await master.PostAsync($"/api/SchoolRequests/{requestId}/approve", new
        {
            domain = "rolled-back.localhost",
            countryId = await CountryIdAsync("PK"),
            administratorEmail = "head@rolled-back.local",
            administratorPassword = "head head head head",
        });

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadRequestsAsync(master)).Single(r => r.Id == requestId).Status.ShouldBe(SchoolRequestStatus.Pending);
        (await TestApp.CountAsync<School>()).ShouldBe(schoolsBefore);
        (await TestApp.CountAsync<Subscription>()).ShouldBe(subscriptionsBefore);
    }

    [Test]
    public async Task ShouldRejectARequestAndCreateNoSchool()
    {
        var master = await SetUpPriceBookAsync();
        var requestId = await SubmitRequestAsync("Rejected School");
        var schoolsBefore = await TestApp.CountAsync<School>();

        using var reject = await master.PostAsync($"/api/SchoolRequests/{requestId}/reject", new { });
        reject.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        (await ReadRequestsAsync(master)).Single(r => r.Id == requestId).Status.ShouldBe(SchoolRequestStatus.Rejected);
        (await TestApp.CountAsync<School>()).ShouldBe(schoolsBefore);

        using var again = await master.PostAsync($"/api/SchoolRequests/{requestId}/reject", new { });
        again.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldKeepTheRequestQueueFromAnonymousVisitors()
    {
        await TestApp.RunAsAdministratorAsync();
        var anonymous = TestHttp.CreateDevelopmentSession();

        using var list = await anonymous.GetAsync("/api/SchoolRequests");
        using var approve = await anonymous.PostAsync($"/api/SchoolRequests/{Guid.NewGuid()}/approve", new { });

        list.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        approve.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    /// <summary>The master tenant, with a country (PK) that has a price book and the Full plan priced for it.</summary>
    private static async Task<HttpSession> SetUpPriceBookAsync()
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

        return master;
    }

    private static async Task<Guid> SubmitRequestAsync(string schoolName)
    {
        var anonymous = TestHttp.CreateDevelopmentSession();
        using var response = await anonymous.PostAsync("/api/SchoolRequests", new
        {
            schoolName,
            contactName = "Ayesha Khan",
            contactEmail = $"contact@{schoolName.Replace(" ", "-").ToLowerInvariant()}.example",
        });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static async Task<HttpResponseMessage> Approve(HttpSession master, Guid requestId, string domain, string email) =>
        await master.PostAsync($"/api/SchoolRequests/{requestId}/approve", new
        {
            domain,
            countryId = await CountryIdAsync("PK"),
            administratorEmail = email,
            administratorPassword = TestApp.AdministratorPassword,
        });

    private static async Task<Guid> CountryIdAsync(string code)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var country = await context.Countries.SingleAsync(c => c.Code == code);
        return country.PublicId;
    }

    private static async Task<List<RequestReadModel>> ReadRequestsAsync(HttpSession master)
    {
        using var response = await master.GetAsync("/api/SchoolRequests");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<RequestReadModel>>(Json))!;
    }

    // The API sends enums by name (ENG-ENUM-01).
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private sealed record RequestReadModel(Guid Id, string SchoolName, string ContactName, string ContactEmail, SchoolRequestStatus Status);

    private sealed record SubscriptionReadModel(Guid Id, SubscriptionStatus Status, DateTimeOffset? TrialEndDate);
}
