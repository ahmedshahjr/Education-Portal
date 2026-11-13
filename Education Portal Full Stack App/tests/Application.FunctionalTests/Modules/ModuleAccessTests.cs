using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Constants;
using EducationPortal.Domain.Entities;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Web.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EducationPortal.Application.FunctionalTests.Modules;

/// <summary>
/// Which modules a school may use, read from its subscription (ADR-012, SEC-AUTHZ-01, -06): core modules always, paid
/// modules only while an active grant covers now, for the whole school or the asking branch, and only while the module
/// is active. The answer is cached per school until something that changes it invalidates it.
/// </summary>
public class ModuleAccessTests : TestBase
{
    private const string SchoolDomain = "school1.localhost";

    [Test]
    public async Task ShouldGiveASchoolWithoutASubscriptionOnlyTheCoreModules()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);

        (await HasAsync(school.Id, ModuleCodes.Students)).ShouldBeTrue();
        (await HasAsync(school.Id, ModuleCodes.Fees)).ShouldBeFalse();
        (await HasAsync(school.Id, "NOPE")).ShouldBeFalse(); // unknown: fail closed
        (await EnabledAsync(school.Id)).ShouldBe(ModuleCatalog.CreateModules().Where(m => m.IsCore).OrderBy(m => m.SortOrder).Select(m => m.Code).ToList());
    }

    [Test]
    public async Task ShouldGiveTheGrantedPaidModulesAndNotTheOthers()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var other = await TestApp.AddSchoolAsync("School Two", "school2.localhost");

        await TestApp.SubscribeAsync(school.Id, [ModuleCodes.Fees]);

        (await HasAsync(school.Id, ModuleCodes.Fees)).ShouldBeTrue();
        (await HasAsync(school.Id, ModuleCodes.Exams)).ShouldBeFalse();
        (await HasAsync(other.Id, ModuleCodes.Fees)).ShouldBeFalse();
    }

    [Test]
    public async Task ShouldTurnAModuleOffOncePastItsValidToAndNotBeforeItStarts()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);

        await TestApp.SubscribeAsync(school.Id, [ModuleCodes.Fees], start: DateTimeOffset.UtcNow.AddMonths(-2)); // ended a month ago
        (await HasAsync(school.Id, ModuleCodes.Fees)).ShouldBeFalse();

        var future = await TestApp.AddSchoolAsync("School Three", "school3.localhost");
        await TestApp.SubscribeAsync(future.Id, [ModuleCodes.Fees], start: DateTimeOffset.UtcNow.AddDays(2));
        (await HasAsync(future.Id, ModuleCodes.Fees)).ShouldBeFalse();
    }

    [Test]
    public async Task ShouldTurnAModuleOffWhileItsGrantIsSuspendedAndBackOnWhenReactivated()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var subscription = await TestApp.SubscribeAsync(school.Id, [ModuleCodes.Fees]);

        await ChangeGrantAsync(subscription.Id, school.Id, grant => grant.Suspend());
        (await HasAsync(school.Id, ModuleCodes.Fees)).ShouldBeFalse();

        await ChangeGrantAsync(subscription.Id, school.Id, grant => grant.Reactivate());
        (await HasAsync(school.Id, ModuleCodes.Fees)).ShouldBeTrue();
    }

    [Test]
    public async Task ShouldKeepABranchGrantForThatBranchOnly()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var branch = Branch.Create(school.Id, "North");
        var otherBranch = Branch.Create(school.Id, "South");
        await TestApp.AddAsync(branch);
        await TestApp.AddAsync(otherBranch);

        await TestApp.SubscribeAsync(school.Id, [ModuleCodes.Fees], branchId: branch.Id);

        (await HasAsync(school.Id, ModuleCodes.Fees, branch.Id)).ShouldBeTrue();
        (await HasAsync(school.Id, ModuleCodes.Fees, otherBranch.Id)).ShouldBeFalse();
        (await HasAsync(school.Id, ModuleCodes.Fees)).ShouldBeFalse(); // no branch asked: fail closed
        (await EnabledAsync(school.Id)).ShouldNotContain(ModuleCodes.Fees);
    }

    [Test]
    public async Task ShouldKeepAnAnswerUntilInvalidatedAndReadItAgainAfterwards()
    {
        var school = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var subscription = await TestApp.SubscribeAsync(school.Id, [ModuleCodes.Fees]);
        (await HasAsync(school.Id, ModuleCodes.Fees)).ShouldBeTrue();

        // A change that bypasses the application leaves the cached answer in place...
        await ChangeGrantAsync(subscription.Id, school.Id, grant => grant.Suspend(), invalidate: false);
        (await HasAsync(school.Id, ModuleCodes.Fees)).ShouldBeTrue();

        // ...until the school's entitlements are invalidated, as the use cases do.
        Store().Invalidate(school.Id);
        (await HasAsync(school.Id, ModuleCodes.Fees)).ShouldBeFalse();
    }

    [Test]
    public async Task ShouldDropEverySchoolsEntitlementsWhenTheCatalogueChanges()
    {
        var one = await TestApp.AddSchoolAsync("School One", SchoolDomain);
        var two = await TestApp.AddSchoolAsync("School Two", "school2.localhost");
        await TestApp.SubscribeAsync(one.Id, [ModuleCodes.Fees]);
        await TestApp.SubscribeAsync(two.Id, [ModuleCodes.Fees]);
        (await HasAsync(one.Id, ModuleCodes.Fees)).ShouldBeTrue();
        (await HasAsync(two.Id, ModuleCodes.Fees)).ShouldBeTrue();

        await TestApp.RunAsAdministratorAsync();
        using var master = TestHttp.CreateDevelopmentSession();
        (await master.SignInAsync(TestApp.AdministratorEmail, TestApp.AdministratorPassword)).Dispose();
        (await master.PutAsync($"/api/Modules/{ModuleCodes.Fees}", new { name = "Fees", description = "", isCore = false, isActive = false, sortOrder = 120 })).Dispose();

        (await HasAsync(one.Id, ModuleCodes.Fees)).ShouldBeFalse();
        (await HasAsync(two.Id, ModuleCodes.Fees)).ShouldBeFalse();
    }

    private static IModuleEntitlementStore Store() =>
        FunctionalTestSetup.ScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IModuleEntitlementStore>();

    private static async Task<bool> HasAsync(int schoolId, string code, int? branchId = null)
    {
        using var scope = SchoolScope(schoolId);
        return await scope.ServiceProvider.GetRequiredService<IModuleAccess>().HasModuleAsync(schoolId, code, branchId, CancellationToken.None);
    }

    private static async Task<IReadOnlyList<string>> EnabledAsync(int schoolId)
    {
        using var scope = SchoolScope(schoolId);
        return await scope.ServiceProvider.GetRequiredService<IModuleAccess>().GetEnabledCodesAsync(schoolId, CancellationToken.None);
    }

    /// <summary>A scope that answers as a request of that school would: its tenant is set, so its query filters apply.</summary>
    private static IServiceScope SchoolScope(int schoolId)
    {
        var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        scope.ServiceProvider.GetRequiredService<CurrentTenant>().Set(new ResolvedTenant(schoolId, IsMaster: false));
        return scope;
    }

    /// <summary>Changes the subscription's grants; <paramref name="invalidate"/> drops the cached answer, as the use cases that change modules do.</summary>
    private static async Task ChangeGrantAsync(int subscriptionId, int schoolId, Action<TenantModule> change, bool invalidate = true)
    {
        using var scope = SchoolScope(schoolId);
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        foreach (var grant in context.TenantModules.Where(m => m.SubscriptionId == subscriptionId))
        {
            change(grant);
        }

        await context.SaveChangesAsync();

        if (invalidate)
        {
            Store().Invalidate(schoolId);
        }
    }
}
