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
/// The master portal's school list shows whether a school is on its free trial or has a paid subscription, and the sign-in page
/// of a school shows a trial notice (a flag and a day count) for the school of its host, open without sign-in.
/// </summary>
public class SchoolPlanTests : TestBase
{
    private const string Email = "founder@plan-school.example";
    private const string SchoolDomain = "plan-school.localhost";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    [Test]
    public async Task ShouldShowARegisteredSchoolAsOnItsTrialInTheMasterList()
    {
        var master = await SchoolRegistrationTests.SetUpPriceBookAsync();
        var withoutPlan = await TestApp.AddSchoolAsync("Unsubscribed School", "unsubscribed.localhost");
        await RegisterAsync();

        using var response = await master.GetAsync("/api/Schools?pageSize=100");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var schools = (await response.Content.ReadFromJsonAsync<SchoolPage>(Json))!.Items;

        var trial = schools.Single(s => s.Name == "Plan School");
        trial.SubscriptionStatus.ShouldBe(SubscriptionStatus.Trial);
        var trialEnd = trial.TrialEndDate.ShouldNotBeNull();
        trialEnd.ShouldBeGreaterThan(DateTimeOffset.UtcNow.AddDays(SchoolRegistration.TrialDays - 1));

        // The master tenant and a school with no subscription have no plan to show.
        schools.Single(s => s.IsMaster).SubscriptionStatus.ShouldBeNull();
        schools.Single(s => s.Name == "Unsubscribed School").SubscriptionStatus.ShouldBeNull();
        withoutPlan.Id.ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task ShouldShowTheTrialNoticeOnTheSchoolsOwnDomainWithoutSignIn()
    {
        await SchoolRegistrationTests.SetUpPriceBookAsync();
        var schoolPublicId = await RegisterAsync();
        await GiveSchoolADomainAsync(schoolPublicId, SchoolDomain);

        var anonymous = TestHttp.CreateDevelopmentSession(SchoolDomain);
        using var response = await anonymous.GetAsync("/api/Subscriptions/trial-notice");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var notice = (await response.Content.ReadFromJsonAsync<TrialNoticeReadModel>(Json))!;
        notice.IsTrial.ShouldBeTrue();
        notice.DaysRemaining.ShouldBeInRange(SchoolRegistration.TrialDays - 1, SchoolRegistration.TrialDays);
    }

    [Test]
    public async Task ShouldShowNoTrialNoticeOnTheMasterDomainOrForASchoolWithoutATrial()
    {
        await SchoolRegistrationTests.SetUpPriceBookAsync();
        await TestApp.AddSchoolAsync("Untrialled School", "untrialled.localhost");

        using var onMaster = await TestHttp.CreateDevelopmentSession().GetAsync("/api/Subscriptions/trial-notice");
        using var onSchool = await TestHttp.CreateDevelopmentSession("untrialled.localhost").GetAsync("/api/Subscriptions/trial-notice");

        (await onMaster.Content.ReadFromJsonAsync<TrialNoticeReadModel>(Json))!.IsTrial.ShouldBeFalse();
        (await onSchool.Content.ReadFromJsonAsync<TrialNoticeReadModel>(Json))!.IsTrial.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldNotShowAnotherSchoolsTrialOnAHostWithoutOne()
    {
        await SchoolRegistrationTests.SetUpPriceBookAsync();
        await GiveSchoolADomainAsync(await RegisterAsync(), SchoolDomain);
        await TestApp.AddSchoolAsync("Other School", "other-school.localhost");

        using var response = await TestHttp.CreateDevelopmentSession("other-school.localhost").GetAsync("/api/Subscriptions/trial-notice");

        (await response.Content.ReadFromJsonAsync<TrialNoticeReadModel>(Json))!.IsTrial.ShouldBeFalse();
    }

    private static async Task<Guid> RegisterAsync()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var countryId = (await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Countries.SingleAsync(c => c.Code == "PK")).PublicId;

        using var response = await TestHttp.CreateDevelopmentSession().PostAsync("/api/SchoolRegistrations", new
        {
            schoolName = "Plan School",
            countryId,
            administratorEmail = Email,
            password = "a brand new passphrase 2026",
            confirmPassword = "a brand new passphrase 2026",
        });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return await response.Content.ReadFromJsonAsync<Guid>();
    }

    private static async Task GiveSchoolADomainAsync(Guid schoolPublicId, string domain)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var school = await context.Schools.SingleAsync(s => s.PublicId == schoolPublicId);
        school.UpdateProfile(school.Name, domain);
        await context.SaveChangesAsync();
    }

    private sealed record SchoolReadModel(string Name, bool IsMaster, SubscriptionStatus? SubscriptionStatus, DateTimeOffset? TrialEndDate);

    private sealed record SchoolPage(List<SchoolReadModel> Items);

    private sealed record TrialNoticeReadModel(bool IsTrial, int DaysRemaining);
}
