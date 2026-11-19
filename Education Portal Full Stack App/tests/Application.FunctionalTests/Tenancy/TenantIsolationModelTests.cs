using EducationPortal.Domain.Common;
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
}
