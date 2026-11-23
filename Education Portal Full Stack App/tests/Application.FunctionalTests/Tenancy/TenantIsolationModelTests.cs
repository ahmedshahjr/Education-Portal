using EducationPortal.Domain.Common;
using EducationPortal.Domain.Entities;
using EducationPortal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Tenancy;

/// <summary>
/// Reads the EF Core model, not the rows: every school-owned entity (ITenantOwned) has a query filter, so a new entity cannot
/// reach production without the tenant isolation of SEC-AUTHZ-03. The behaviour of the filter itself is covered by the
/// isolation tests of each feature.
/// </summary>
public class TenantIsolationModelTests : TestBase
{
    [Test]
    public void EveryTenantOwnedEntityShouldHaveAQueryFilter()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var model = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().GetService<Microsoft.EntityFrameworkCore.Metadata.IDesignTimeModel>().Model;

        var tenantOwned = model.GetEntityTypes().Where(t => typeof(ITenantOwned).IsAssignableFrom(t.ClrType)).ToList();
        tenantOwned.Count.ShouldBeGreaterThanOrEqualTo(20);

        var withoutFilter = tenantOwned.Where(t => !t.GetDeclaredQueryFilters().Any()).Select(t => t.ClrType.Name).ToList();

        withoutFilter.ShouldBeEmpty("These school-owned entities have no query filter");
    }

    [Test]
    public void EveryTenantOwnedEntityShouldBeFilteredBySchoolId()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var model = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().GetService<Microsoft.EntityFrameworkCore.Metadata.IDesignTimeModel>().Model;

        var notBySchool = model.GetEntityTypes()
            .Where(t => typeof(ITenantOwned).IsAssignableFrom(t.ClrType))
            .Where(t => !t.GetDeclaredQueryFilters().Any(f => f.Expression?.ToString().Contains("SchoolId") == true))
            .Select(t => t.ClrType.Name)
            .ToList();

        notBySchool.ShouldBeEmpty("These entities have a query filter that does not look at SchoolId");
    }

    [Test]
    public void EveryTenantOwnedEntityShouldHaveATenantColumnThatReferencesItsTenant()
    {
        var model = ModelOf();

        var withoutTenant = model.GetEntityTypes()
            .Where(t => typeof(ITenantOwned).IsAssignableFrom(t.ClrType))
            .Where(t => t.FindProperty(ITenantOwned.TenantColumn) is null
                || !t.GetForeignKeys().Any(fk => fk.PrincipalEntityType.ClrType == typeof(Tenant)
                    && fk.Properties.Any(p => p.Name == ITenantOwned.TenantColumn)))
            .Select(t => t.ClrType.Name)
            .ToList();

        withoutTenant.ShouldBeEmpty("These tenant-owned entities have no TenantId column that references Tenants (ADR-026)");
    }

    /// <summary>
    /// Entities with a SchoolId that are not tenant-owned, each with the reason. Any other entity with a SchoolId fails the test,
    /// so a school-linked table cannot skip the tenant isolation by accident (SEC-AUTHZ-03, ADR-026).
    /// </summary>
    private static readonly Dictionary<string, string> SchoolIdExemptions = new(StringComparer.Ordinal)
    {
        // The login account. Sign-in finds an account by email across the platform (an account of a school without a domain
        // signs in on the master domain), so the account table is read before any tenant is known. Its school-owned data
        // (UserDetails, roles, campuses) is tenant-owned and filtered.
        ["ApplicationUser"] = "login account, read across tenants by email at sign-in (ADR-020, ADR-026)",

        // The audit trail: entries the system writes belong to no school, so the SchoolId is nullable. It keeps its own filter
        // in ApplicationDbContext and is not tenant-owned.
        ["ChangeAuditEntry"] = "append-only audit trail with a nullable school (ADR-016, ADR-026)",
    };

    [Test]
    public void EveryEntityWithASchoolIdShouldBeTenantOwnedOrListedWithAReason()
    {
        var model = ModelOf();

        var offenders = model.GetEntityTypes()
            .Where(t => t.FindProperty("SchoolId") is { } property && (property.ClrType == typeof(int) || property.ClrType == typeof(int?)))
            .Where(t => !typeof(ITenantOwned).IsAssignableFrom(t.ClrType))
            .Where(t => !SchoolIdExemptions.ContainsKey(t.ClrType.Name))
            .Select(t => t.ClrType.Name)
            .ToList();

        offenders.ShouldBeEmpty("These entities have a SchoolId but are neither tenant-owned nor listed as exempt with a reason");
    }

    [Test]
    public void TheExemptionsShouldStillBeNeeded()
    {
        var model = ModelOf();

        var stale = SchoolIdExemptions.Keys
            .Where(name => !model.GetEntityTypes().Any(t => t.ClrType.Name == name && t.FindProperty("SchoolId") is not null && !typeof(ITenantOwned).IsAssignableFrom(t.ClrType)))
            .ToList();

        stale.ShouldBeEmpty("These exemptions no longer match an entity with a SchoolId that is not tenant-owned; remove them");
    }

    private static Microsoft.EntityFrameworkCore.Metadata.IModel ModelOf()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().GetService<Microsoft.EntityFrameworkCore.Metadata.IDesignTimeModel>().Model;
    }
}
