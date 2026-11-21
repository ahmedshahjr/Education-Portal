using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.FunctionalTests.Infrastructure;
using EducationPortal.Application.FunctionalTests.SchoolRequests;
using EducationPortal.Application.Schools.Registration;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;
using EducationPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Schools;

/// <summary>
/// Self-service registration (ADR-018): anyone on the master tenant's domain registers a school with an administrator and
/// a password they choose, and gets a 90-day trial at once. No other host takes registrations.
/// </summary>
public class RegisterSchoolTests : TestBase
{
    private const string Email = "founder@new-school.example";
    private const string Password = "a brand new passphrase 2026";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    [Test]
    public async Task ShouldRegisterASchoolWithItsAdministratorAndANinetyDayTrial()
    {
        var master = await SchoolRegistrationTests.SetUpPriceBookAsync();
        var anonymous = TestHttp.CreateDevelopmentSession(); // the master tenant's domain, nobody signed in

        using var countries = await anonymous.GetAsync("/api/SchoolRegistrations/countries");
        countries.StatusCode.ShouldBe(HttpStatusCode.OK);
        var offered = (await countries.Content.ReadFromJsonAsync<List<CountryReadModel>>(Json))!;
        offered.Select(c => c.Name).ShouldContain("Pakistan");

        using var register = await anonymous.PostAsync("/api/SchoolRegistrations", Registration(offered.Single(c => c.Name == "Pakistan").Id));
        register.StatusCode.ShouldBe(HttpStatusCode.Created);
        var schoolId = await register.Content.ReadFromJsonAsync<Guid>();

        // The trial is 90 days and starts now.
        using var subscription = await master.GetAsync($"/api/Schools/{schoolId}/subscription");
        subscription.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = (await subscription.Content.ReadFromJsonAsync<SubscriptionReadModel>(Json))!;
        body.Status.ShouldBe(SubscriptionStatus.Trial);
        var trialEnd = body.TrialEndDate.ShouldNotBeNull();
        trialEnd.ShouldBeGreaterThan(DateTimeOffset.UtcNow.AddDays(SchoolRegistration.TrialDays - 1));
        trialEnd.ShouldBeLessThanOrEqualTo(DateTimeOffset.UtcNow.AddDays(SchoolRegistration.TrialDays + 1));
    }

    [Test]
    public async Task ShouldLetTheNewAdministratorSignInOnTheMasterDomainWithTheirOwnPasswordAndActAsTheirSchool()
    {
        await SchoolRegistrationTests.SetUpPriceBookAsync();
        var anonymous = TestHttp.CreateDevelopmentSession();
        using var register = await anonymous.PostAsync("/api/SchoolRegistrations", Registration(await PakistanIdAsync()));
        register.StatusCode.ShouldBe(HttpStatusCode.Created);

        var session = TestHttp.CreateDevelopmentSession();
        using var signIn = await session.SignInAsync(Email, Password);
        signIn.StatusCode.ShouldBe(HttpStatusCode.OK);

        // The administrator chose the password, so nothing is blocked behind a forced password change.
        using var modules = await session.GetAsync("/api/Modules/enabled");
        modules.StatusCode.ShouldBe(HttpStatusCode.OK);

        // The request is the school's, never the master's.
        using var schools = await session.GetAsync("/api/Schools");
        schools.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task ShouldShowTheTrialToASchoolWithoutADomainOnceItIsSignedInOnTheMasterDomain()
    {
        await SchoolRegistrationTests.SetUpPriceBookAsync();
        using var register = await TestHttp.CreateDevelopmentSession().PostAsync("/api/SchoolRegistrations", Registration(await PakistanIdAsync()));
        register.StatusCode.ShouldBe(HttpStatusCode.Created);
        var session = TestHttp.CreateDevelopmentSession(); // the master tenant's domain
        (await session.SignInAsync(Email, Password)).Dispose();

        // The banner inside the app reads the school's own subscription: the days left of its trial.
        using var mine = await session.GetAsync("/api/Subscriptions/mine");
        mine.StatusCode.ShouldBe(HttpStatusCode.OK);
        var subscription = (await mine.Content.ReadFromJsonAsync<MySubscriptionReadModel>(Json))!;
        subscription.TrialDaysRemaining.ShouldNotBeNull().ShouldBeInRange(SchoolRegistration.TrialDays - 1, SchoolRegistration.TrialDays);

        // Signed in, the trial notice answers for the school the session acts as, not for the master tenant.
        using var notice = await session.GetAsync("/api/Subscriptions/trial-notice");
        (await notice.Content.ReadFromJsonAsync<TrialNoticeReadModel>(Json))!.IsTrial.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldRefuseWhenThePasswordsDoNotMatch()
    {
        await SchoolRegistrationTests.SetUpPriceBookAsync();
        var anonymous = TestHttp.CreateDevelopmentSession();
        var schoolsBefore = await TestApp.CountAsync<School>();

        using var response = await anonymous.PostAsync("/api/SchoolRegistrations", Registration(await PakistanIdAsync(), confirmPassword: "something else entirely 2026"));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await TestApp.CountAsync<School>()).ShouldBe(schoolsBefore);
    }

    [Test]
    public async Task ShouldRefuseATooShortPasswordAnInvalidEmailAndAMissingName()
    {
        await SchoolRegistrationTests.SetUpPriceBookAsync();
        var anonymous = TestHttp.CreateDevelopmentSession();
        var countryId = await PakistanIdAsync();

        using var shortPassword = await anonymous.PostAsync("/api/SchoolRegistrations", Registration(countryId, password: "short", confirmPassword: "short"));
        using var badEmail = await anonymous.PostAsync("/api/SchoolRegistrations", Registration(countryId, email: "not-an-email"));
        using var noName = await anonymous.PostAsync("/api/SchoolRegistrations", Registration(countryId, schoolName: ""));

        shortPassword.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        badEmail.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        noName.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseAnEmailThatAnotherAccountUses()
    {
        await SchoolRegistrationTests.SetUpPriceBookAsync();
        var anonymous = TestHttp.CreateDevelopmentSession();
        var countryId = await PakistanIdAsync();
        (await anonymous.PostAsync("/api/SchoolRegistrations", Registration(countryId))).StatusCode.ShouldBe(HttpStatusCode.Created);

        using var again = await anonymous.PostAsync("/api/SchoolRegistrations", Registration(countryId, schoolName: "Second School"));

        again.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRefuseACountryWithoutAPriceBook()
    {
        await SchoolRegistrationTests.SetUpPriceBookAsync();
        var unpriced = await TestApp.AddCountryAsync("AE", "United Arab Emirates", ["en", "ar"], "en", "Asia/Dubai", "AED");
        var anonymous = TestHttp.CreateDevelopmentSession();

        using var response = await anonymous.PostAsync("/api/SchoolRegistrations", Registration(unpriced.PublicId));
        using var countries = await anonymous.GetAsync("/api/SchoolRegistrations/countries");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await countries.Content.ReadFromJsonAsync<List<CountryReadModel>>(Json))!.Select(c => c.Name).ShouldNotContain("United Arab Emirates");
    }

    [Test]
    public async Task ShouldRollBackTheSchoolWhenThePasswordFailsThePolicy()
    {
        await SchoolRegistrationTests.SetUpPriceBookAsync();
        var anonymous = TestHttp.CreateDevelopmentSession();
        var schoolsBefore = await TestApp.CountAsync<School>();
        var subscriptionsBefore = await TestApp.CountAsync<Subscription>();

        // Long enough for the length rule, but it repeats a word of the email, which the password policy refuses only after the school is saved.
        using var response = await anonymous.PostAsync("/api/SchoolRegistrations", Registration(await PakistanIdAsync(), password: "founder founder founder founder", confirmPassword: "founder founder founder founder"));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await TestApp.CountAsync<School>()).ShouldBe(schoolsBefore);
        (await TestApp.CountAsync<Subscription>()).ShouldBe(subscriptionsBefore);
    }

    [Test]
    public async Task ShouldTakeRegistrationsOnTheMasterDomainOnly()
    {
        await SchoolRegistrationTests.SetUpPriceBookAsync();
        await TestApp.AddSchoolAsync("Existing School", "existing.localhost");
        var onSchoolDomain = TestHttp.CreateDevelopmentSession("existing.localhost");
        var schoolsBefore = await TestApp.CountAsync<School>();

        using var register = await onSchoolDomain.PostAsync("/api/SchoolRegistrations", Registration(await PakistanIdAsync()));
        using var countries = await onSchoolDomain.GetAsync("/api/SchoolRegistrations/countries");

        register.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        countries.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await TestApp.CountAsync<School>()).ShouldBe(schoolsBefore);
    }

    private static object Registration(
        Guid countryId,
        string schoolName = "New School",
        string email = Email,
        string password = Password,
        string? confirmPassword = null) => new
        {
            schoolName,
            countryId,
            administratorEmail = email,
            password,
            confirmPassword = confirmPassword ?? password,
        };

    private static async Task<Guid> PakistanIdAsync()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return (await context.Countries.SingleAsync(c => c.Code == "PK")).PublicId;
    }

    private sealed record CountryReadModel(Guid Id, string Name);

    private sealed record MySubscriptionReadModel(int? TrialDaysRemaining);

    private sealed record TrialNoticeReadModel(bool IsTrial, int DaysRemaining);

    private sealed record SubscriptionReadModel(Guid Id, SubscriptionStatus Status, DateTimeOffset? TrialEndDate);
}
