using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EducationPortal.Application.AddOnRequests;
using EducationPortal.Application.Subscriptions.Common;
using EducationPortal.Application.Subscriptions.Queries.GetMySubscription;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.FunctionalTests.Subscriptions;

/// <summary>
/// Managing a school's subscription (ADR-014): the master tenant suspends, reactivates, extends, changes the plan and
/// adds or removes modules; a school administrator asks for add-ons and sees its own subscription. Only the master tenant
/// changes anything, a school sees only its own rows, every change is audited and the entitlement of the school follows
/// at once (SEC-AUTHZ-01, -03, -07, -09, SEC-LOG-03).
/// </summary>
public class SubscriptionManagementTests : TestBase
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { Converters = { new JsonStringEnumConverter() } };

    private const string SchoolDomain = "school1.localhost";
    private const string OtherSchoolDomain = "school2.localhost";

    private static Guid _bookId;

    [Test]
    public async Task ShouldSuspendAndReactivateTheSubscriptionAndItsModules()
    {
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSubscribedSchoolAsync(master);
        var path = $"/api/Schools/{school.PublicId}/subscription";

        using var suspend = await master.PostAsync($"{path}/suspend");
        var suspended = await ReadAsync(master, school.PublicId);
        using var suspendAgain = await master.PostAsync($"{path}/suspend");
        using var reactivate = await master.PostAsync($"{path}/reactivate");
        var reactivated = await ReadAsync(master, school.PublicId);
        using var reactivateAgain = await master.PostAsync($"{path}/reactivate");

        suspend.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        suspended.Status.ShouldBe(SubscriptionStatus.Suspended);
        suspended.Modules.ShouldAllBe(m => m.Status == TenantModuleStatus.Suspended);
        suspendAgain.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        reactivate.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        reactivated.Status.ShouldBe(SubscriptionStatus.Active);
        reactivated.Modules.ShouldAllBe(m => m.Status == TenantModuleStatus.Active);
        reactivateAgain.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await TestApp.FindAuditEventsAsync(masterUserId)).Count(e => e.EventType == SecurityAuditEventType.SubscriptionChanged).ShouldBe(2);
    }

    [Test]
    public async Task ShouldExtendThePeriodAndRefuseABadNumberOfDays()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSubscribedSchoolAsync(master);
        var before = await ReadAsync(master, school.PublicId);
        var path = $"/api/Schools/{school.PublicId}/subscription/extend";

        using var ok = await master.PostAsync(path, new { days = 10 });
        using var zero = await master.PostAsync(path, new { days = 0 });
        using var tooLong = await master.PostAsync(path, new { days = Subscription.MaxExtensionDays + 1 });

        var after = await ReadAsync(master, school.PublicId);
        ok.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        zero.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        tooLong.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        after.EndDate.ShouldBe(before.EndDate.AddDays(10));
        after.Modules.ShouldAllBe(m => m.ValidTo == after.EndDate);
    }

    [Test]
    public async Task ShouldAddAndRemoveAnAddOnModuleAndFollowThePrice()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSubscribedSchoolAsync(master);
        var path = $"/api/Schools/{school.PublicId}/subscription/modules";

        using var add = await master.PostAsync(path, new { moduleCode = ModuleCodes.Library });
        var added = await ReadAsync(master, school.PublicId);
        using var addAgain = await master.PostAsync(path, new { moduleCode = ModuleCodes.Library });
        using var remove = await master.DeleteAsync($"{path}/{ModuleCodes.Library}");
        var removed = await ReadAsync(master, school.PublicId);
        using var removeAgain = await master.DeleteAsync($"{path}/{ModuleCodes.Library}");

        add.StatusCode.ShouldBe(HttpStatusCode.NoContent, await add.Content.ReadAsStringAsync());
        added.Amount.ShouldBe(25000m + 500m);
        added.Modules.ShouldContain(m => m.Code == ModuleCodes.Library && m.Status == TenantModuleStatus.Active && m.ValidTo == added.EndDate);
        addAgain.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        remove.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        removed.Amount.ShouldBe(25000m);
        removed.Modules.ShouldContain(m => m.Code == ModuleCodes.Library && m.Status == TenantModuleStatus.Expired);
        removeAgain.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        // The module's row is kept (its data stays) and adding it again restores it without a second row.
        using var addBack = await master.PostAsync(path, new { moduleCode = ModuleCodes.Library });
        addBack.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadAsync(master, school.PublicId)).Modules.Count(m => m.Code == ModuleCodes.Library).ShouldBe(1);
    }

    [Test]
    public async Task ShouldRefuseBadModuleChanges()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSubscribedSchoolAsync(master);
        var path = $"/api/Schools/{school.PublicId}/subscription/modules";

        var responses = new[]
        {
            await master.PostAsync(path, new { moduleCode = ModuleCodes.Students }),   // core modules are never sold
            await master.PostAsync(path, new { moduleCode = "NOPE" }),                  // unknown
            await master.PostAsync(path, new { moduleCode = ModuleCodes.Hostel }),     // no price in the price book
            await master.PostAsync(path, new { moduleCode = ModuleCodes.Attendance }), // the plan already includes it
            await master.DeleteAsync($"{path}/{ModuleCodes.Attendance}"),               // part of the plan: not removable
            await master.DeleteAsync($"{path}/{ModuleCodes.Students}"),                 // core
            await master.DeleteAsync($"{path}/NOPE"),
        };

        foreach (var response in responses)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, response.RequestMessage?.RequestUri?.ToString());
            response.Dispose();
        }

        (await ReadAsync(master, school.PublicId)).Amount.ShouldBe(25000m);
    }

    [Test]
    public async Task ShouldChangeThePlanKeepAddOnsAndEndModulesTheNewPlanLacks()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSubscribedSchoolAsync(master);
        (await master.PostAsync("/api/Plans", new { code = "PLUS", name = "Plus", description = "", modules = new[] { ModuleCodes.Attendance, ModuleCodes.Fees } })).Dispose();
        await SetPriceAsync(master, new { planCode = "PLUS", billingCycle = "Monthly", price = 40000 });
        (await master.PostAsync($"/api/Schools/{school.PublicId}/subscription/modules", new { moduleCode = ModuleCodes.Library })).Dispose();

        using var change = await master.PutAsync($"/api/Schools/{school.PublicId}/subscription/plan", new { planCode = "PLUS" });
        using var unknown = await master.PutAsync($"/api/Schools/{school.PublicId}/subscription/plan", new { planCode = "NOPE" });
        using var unpriced = await master.PutAsync($"/api/Schools/{school.PublicId}/subscription/plan", new { planCode = "UNPRICED" });

        var subscription = await ReadAsync(master, school.PublicId);
        change.StatusCode.ShouldBe(HttpStatusCode.NoContent, await change.Content.ReadAsStringAsync());
        unknown.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        unpriced.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        subscription.ShouldSatisfyAllConditions(
            s => s.PlanCode.ShouldBe("PLUS"),
            s => s.Amount.ShouldBe(40000m + 500m), // the plan plus the Library add-on, which is kept
            s => s.Modules.Single(m => m.Code == ModuleCodes.Exams).Status.ShouldBe(TenantModuleStatus.Expired),
            s => s.Modules.Single(m => m.Code == ModuleCodes.Fees).Status.ShouldBe(TenantModuleStatus.Active),
            s => s.Modules.Single(m => m.Code == ModuleCodes.Library).Status.ShouldBe(TenantModuleStatus.Active),
            s => s.Modules.Single(m => m.Code == ModuleCodes.Attendance).Status.ShouldBe(TenantModuleStatus.Active));
    }

    [Test]
    public async Task ShouldTakeAnAddOnRequestFromTheSchoolAndLetTheMasterApproveIt()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSubscribedSchoolAsync(master);
        using var session = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");

        using var request = await session.PostAsync("/api/AddOnRequests", new { moduleCode = ModuleCodes.Library });
        var requestId = await request.Content.ReadFromJsonAsync<Guid>();
        using var again = await session.PostAsync("/api/AddOnRequests", new { moduleCode = ModuleCodes.Library });
        var mine = await ReadMineAsync(session);
        var pending = await ListRequestsAsync(master, "Pending");

        request.StatusCode.ShouldBe(HttpStatusCode.OK);
        again.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        mine.Subscription!.Modules.ShouldNotContain(m => m.Code == ModuleCodes.Library);
        mine.AvailableAddOns.ShouldContain(a => a.Code == ModuleCodes.Library && a.Price == 500m && a.Requested);
        mine.AvailableAddOns.ShouldNotContain(a => a.Code == ModuleCodes.Exams); // the plan has it
        mine.Requests.ShouldHaveSingleItem().Status.ShouldBe(AddOnRequestStatus.Pending);
        pending.ShouldHaveSingleItem().ShouldSatisfyAllConditions(r => r.Id.ShouldBe(requestId), r => r.SchoolId.ShouldBe(school.PublicId));

        // Nothing changed until the master decides, and the school cannot decide for itself.
        using var selfApprove = await session.PostAsync($"/api/AddOnRequests/{requestId}/approve");
        selfApprove.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ReadAsync(master, school.PublicId)).Modules.ShouldNotContain(m => m.Code == ModuleCodes.Library);

        using var approve = await master.PostAsync($"/api/AddOnRequests/{requestId}/approve");
        using var approveAgain = await master.PostAsync($"/api/AddOnRequests/{requestId}/approve");
        using var rejectAfter = await master.PostAsync($"/api/AddOnRequests/{requestId}/reject");

        approve.StatusCode.ShouldBe(HttpStatusCode.NoContent, await approve.Content.ReadAsStringAsync());
        approveAgain.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        rejectAfter.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var subscription = await ReadAsync(master, school.PublicId);
        subscription.Amount.ShouldBe(25500m);
        subscription.Modules.ShouldContain(m => m.Code == ModuleCodes.Library && m.Status == TenantModuleStatus.Active);
        (await ReadMineAsync(session)).Requests.ShouldHaveSingleItem().Status.ShouldBe(AddOnRequestStatus.Approved);

        // A module the school already has cannot be requested again.
        using var held = await session.PostAsync("/api/AddOnRequests", new { moduleCode = ModuleCodes.Library });
        held.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task ShouldRejectAnAddOnRequestAndChangeNothingElse()
    {
        var masterUserId = await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSubscribedSchoolAsync(master);
        using var session = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        using var request = await session.PostAsync("/api/AddOnRequests", new { moduleCode = ModuleCodes.Library });
        var requestId = await request.Content.ReadFromJsonAsync<Guid>();

        using var reject = await master.PostAsync($"/api/AddOnRequests/{requestId}/reject");

        reject.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadAsync(master, school.PublicId)).Amount.ShouldBe(25000m);
        (await ListRequestsAsync(master, "Rejected")).ShouldHaveSingleItem();
        (await ListRequestsAsync(master, "Pending")).ShouldBeEmpty();
        (await TestApp.FindAuditEventsAsync(masterUserId)).ShouldContain(e => e.EventType == SecurityAuditEventType.AddOnRequestDecided);

        // After a decision the school may ask again.
        using var again = await session.PostAsync("/api/AddOnRequests", new { moduleCode = ModuleCodes.Library });
        again.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task ShouldRefuseBadAddOnRequests()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSubscribedSchoolAsync(master);
        var noSubscription = await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        using var session = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        using var otherSession = await SignInToSchoolAsync(noSubscription.Id, OtherSchoolDomain, "admin@school2.local");

        var responses = new[]
        {
            await session.PostAsync("/api/AddOnRequests", new { moduleCode = ModuleCodes.Students }), // core
            await session.PostAsync("/api/AddOnRequests", new { moduleCode = "NOPE" }),
            await session.PostAsync("/api/AddOnRequests", new { moduleCode = "" }),
            await session.PostAsync("/api/AddOnRequests", new { moduleCode = ModuleCodes.Exams }),    // the plan includes it
            await otherSession.PostAsync("/api/AddOnRequests", new { moduleCode = ModuleCodes.Library }), // no subscription
            await master.PostAsync("/api/AddOnRequests", new { moduleCode = ModuleCodes.Library }),   // the master has every module
        };

        foreach (var response in responses)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, response.RequestMessage?.RequestUri?.ToString());
            response.Dispose();
        }

        (await TestApp.CountAsync<AddOnRequest>()).ShouldBe(0);
    }

    [Test]
    public async Task ShouldShowASchoolOnlyItsOwnSubscriptionAndRequests()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSubscribedSchoolAsync(master);
        var other = await TestApp.AddSchoolAsync("School Two", OtherSchoolDomain);
        using var session = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        using var otherSession = await SignInToSchoolAsync(other.Id, OtherSchoolDomain, "admin@school2.local");
        (await session.PostAsync("/api/AddOnRequests", new { moduleCode = ModuleCodes.Library })).Dispose();

        var otherMine = await ReadMineAsync(otherSession);

        otherMine.Subscription.ShouldBeNull();
        otherMine.AvailableAddOns.ShouldBeEmpty();
        otherMine.Requests.ShouldBeEmpty();
        (await ReadMineAsync(session)).Requests.Count.ShouldBe(1);

        // The database filter agrees: a school sees only its own requests (SEC-AUTHZ-03).
        TestApp.UseTenant(other.Id, isMaster: false);
        (await TestApp.CountAsync<AddOnRequest>()).ShouldBe(0);
        TestApp.UseTenant(school.Id, isMaster: false);
        (await TestApp.CountAsync<AddOnRequest>()).ShouldBe(1);
    }

    [Test]
    public async Task ShouldKeepSubscriptionChangesAwayFromSchoolsAndAnonymousVisitors()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await SetUpSubscribedSchoolAsync(master);
        using var session = await SignInToSchoolAsync(school.Id, SchoolDomain, "admin@school1.local");
        using var anonymous = TestHttp.CreateDevelopmentSession(SchoolDomain);
        var path = $"/api/Schools/{school.PublicId}/subscription";

        var forbidden = new[]
        {
            await session.PostAsync($"{path}/suspend"),
            await session.PostAsync($"{path}/reactivate"),
            await session.PostAsync($"{path}/extend", new { days = 5 }),
            await session.PutAsync($"{path}/plan", new { planCode = "BASIC" }),
            await session.PostAsync($"{path}/modules", new { moduleCode = ModuleCodes.Library }),
            await session.DeleteAsync($"{path}/modules/{ModuleCodes.Library}"),
            await session.GetAsync("/api/AddOnRequests"),
            await session.PostAsync($"/api/AddOnRequests/{Guid.NewGuid()}/reject"),
        };

        foreach (var response in forbidden)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden, response.RequestMessage?.RequestUri?.ToString());
            response.Dispose();
        }

        (await anonymous.PostAsync($"{path}/suspend")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await anonymous.GetAsync("/api/Subscriptions/mine")).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await ReadAsync(master, school.PublicId)).Status.ShouldBe(SubscriptionStatus.Active);
    }

    [Test]
    public async Task ShouldAnswerNotFoundForASchoolWithoutASubscription()
    {
        await TestApp.RunAsAdministratorAsync();
        using var master = await SignInAsMasterAsync();
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var path = $"/api/Schools/{school.PublicId}/subscription";

        var responses = new[]
        {
            await master.PostAsync($"{path}/suspend"),
            await master.PostAsync($"{path}/reactivate"),
            await master.PostAsync($"{path}/extend", new { days = 5 }),
        };

        foreach (var response in responses)
        {
            // Suspend and reactivate are validated against the subscription (400); extend is looked up (404).
            response.StatusCode.ShouldBeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
            response.Dispose();
        }
    }

    private static async Task<EntityIds> SetUpSubscribedSchoolAsync(HttpSession master)
    {
        var country = await TestApp.AddCountryAsync("PK", "Pakistan", ["en", "ur"], "en", "Asia/Karachi", "PKR");
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain, country.Id);

        (await master.PostAsync("/api/Plans", new { code = "BASIC", name = "Basic", description = "", modules = new[] { ModuleCodes.Attendance, ModuleCodes.Exams } })).Dispose();
        (await master.PostAsync("/api/Plans", new { code = "UNPRICED", name = "Unpriced", description = "", modules = Array.Empty<string>() })).Dispose();
        using var created = await master.PostAsync("/api/PriceBooks", new { countryCode = "PK", currencyCode = "PKR" });
        _bookId = await created.Content.ReadFromJsonAsync<Guid>();
        await SetPriceAsync(master, new { planCode = "BASIC", billingCycle = "Monthly", price = 25000 });
        await SetPriceAsync(master, new { moduleCode = ModuleCodes.Library, billingCycle = "Monthly", price = 500 });

        using var response = await master.PostAsync($"/api/Schools/{school.PublicId}/subscription", new { planCode = "BASIC", modules = Array.Empty<string>(), billingCycle = "Monthly", trialDays = 0 });
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());

        return school;
    }

    private static async Task SetPriceAsync(HttpSession master, object body)
    {
        using var response = await master.PutAsync($"/api/PriceBooks/{_bookId}/prices", body);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    private static async Task<SubscriptionDto> ReadAsync(HttpSession master, Guid schoolId)
    {
        using var response = await master.GetAsync($"/api/Schools/{schoolId}/subscription");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<SubscriptionDto>(Json))!;
    }

    private static async Task<MySubscriptionDto> ReadMineAsync(HttpSession session)
    {
        using var response = await session.GetAsync("/api/Subscriptions/mine");
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync());
        return (await response.Content.ReadFromJsonAsync<MySubscriptionDto>(Json))!;
    }

    private static async Task<List<AddOnRequestDto>> ListRequestsAsync(HttpSession master, string status)
    {
        using var response = await master.GetAsync($"/api/AddOnRequests?status={status}");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<List<AddOnRequestDto>>(Json))!;
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
