using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Domain.Entities;
using EducationPortal.Infrastructure.Data;
using EducationPortal.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Tenancy;

/// <summary>
/// The EF Core query filters of the tenant-owned entities (ADR-026): a school sees its own rows inside its own tenant, a
/// master session sees every row, and a request on the master domain without a session sees only its own school.
/// </summary>
public class TenantFilterTests : TestBase
{
    [Test]
    public async Task ShouldShowASchoolOnlyItsOwnRowsEvenWithinTheSameTenant()
    {
        var (one, two, tenantId) = await TwoSchoolsOfOneTenantAsync();

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        TestApp.SetTenant(scope.ServiceProvider, TestApp.Resolve(one.Id));
        var campuses = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Campuses.ToListAsync();

        campuses.ShouldNotBeEmpty();
        campuses.ShouldAllBe(c => c.SchoolId == one.Id);
        campuses.ShouldNotContain(c => c.SchoolId == two.Id);
        tenantId.ShouldBe(TestApp.Resolve(two.Id).TenantId);
    }

    [Test]
    public async Task ShouldShowNothingWithoutAResolvedSchool()
    {
        await TwoSchoolsOfOneTenantAsync();

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var count = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Campuses.CountAsync();

        count.ShouldBe(0);
    }

    [Test]
    public async Task ShouldShowEveryTenantToAMasterSessionOnly()
    {
        await TwoSchoolsOfOneTenantAsync();
        var everything = await CountCampusesAsync(null);

        // The master domain without a session: its own school only, never every school (SEC-AUTHZ-13).
        using (var anonymous = FunctionalTestSetup.ScopeFactory.CreateScope())
        {
            anonymous.ServiceProvider.GetRequiredService<CurrentTenant>().Set(TestApp.Resolve(TestApp.MasterSchoolId, isMaster: true));
            var visible = await anonymous.ServiceProvider.GetRequiredService<ApplicationDbContext>().Campuses.ToListAsync();

            visible.ShouldAllBe(c => c.SchoolId == TestApp.MasterSchoolId);
        }

        using var master = FunctionalTestSetup.ScopeFactory.CreateScope();
        TestApp.SetTenant(master.ServiceProvider, TestApp.Resolve(TestApp.MasterSchoolId, isMaster: true));
        (await master.ServiceProvider.GetRequiredService<ApplicationDbContext>().Campuses.CountAsync()).ShouldBe(everything);
    }

    [Test]
    public async Task ShouldRefuseTheBypassForASchoolTenant()
    {
        var one = await TestApp.AddSchoolAsync("Filter School", "filter-one.localhost");

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var current = scope.ServiceProvider.GetRequiredService<CurrentTenant>();
        current.Set(TestApp.Resolve(one.Id));

        Should.Throw<InvalidOperationException>(() => current.GrantMasterBypass());
        current.BypassesTenantFilters.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldSetTheTenantAndTheBypassOnEveryConnection()
    {
        var one = await TestApp.AddSchoolAsync("Session School", "session-one.localhost");
        var tenantId = TestApp.Resolve(one.Id).TenantId;

        using (var school = FunctionalTestSetup.ScopeFactory.CreateScope())
        {
            TestApp.SetTenant(school.ServiceProvider, TestApp.Resolve(one.Id));
            var context = school.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            (await ReadSettingAsync(context, "app.tenant_id")).ShouldBe(tenantId.ToString());
            (await ReadSettingAsync(context, "app.tenant_bypass")).ShouldBe("off");
        }

        using var master = FunctionalTestSetup.ScopeFactory.CreateScope();
        TestApp.SetTenant(master.ServiceProvider, TestApp.Resolve(TestApp.MasterSchoolId, isMaster: true));
        var masterContext = master.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        (await ReadSettingAsync(masterContext, "app.tenant_bypass")).ShouldBe("on");
    }

    private static async Task<string> ReadSettingAsync(ApplicationDbContext context, string name) =>
        await context.Database.SqlQuery<string>($"SELECT coalesce(current_setting({name}, true), '') AS \"Value\"").SingleAsync();

    private static async Task<int> CountCampusesAsync(int? schoolId)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // compliance-exception: SEC-AUTHZ-03 test counts every campus across tenants to prove the filter hides the others
        var query = context.Campuses.IgnoreQueryFilters();
        return schoolId is null ? await query.CountAsync() : await query.CountAsync(c => c.SchoolId == schoolId);
    }

    /// <summary>Two schools of one new tenant, each with its main campus.</summary>
    private static async Task<(EntityIds One, EntityIds Two, int TenantId)> TwoSchoolsOfOneTenantAsync()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var tenant = Tenant.Create("Shared tenant");
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        var one = School.Create("Shared one", "shared-one.localhost");
        one.AssignToTenant(tenant.Id);
        var two = School.Create("Shared two", "shared-two.localhost");
        two.AssignToTenant(tenant.Id);
        context.Schools.AddRange(one, two);
        await context.SaveChangesAsync();

        return (new EntityIds(one.Id, one.PublicId), new EntityIds(two.Id, two.PublicId), tenant.Id);
    }
}
