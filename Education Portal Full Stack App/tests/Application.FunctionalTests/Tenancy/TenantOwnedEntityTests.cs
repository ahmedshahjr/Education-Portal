using EducationPortal.Domain.Common;
using EducationPortal.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace EducationPortal.Application.FunctionalTests.Tenancy;

/// <summary>
/// The architecture rule behind tenant isolation (SEC-AUTHZ-03): a school-owned entity says so with ITenantOwned, and the
/// database context then gives it the tenant query filter. An entity with a SchoolId that does not implement the interface
/// would silently skip the filter, so this test fails the build for it. Reads the Domain types only, no database.
/// </summary>
public class TenantOwnedEntityTests
{
    /// <summary>Entities whose SchoolId is on purpose not a plain int. Each needs a reason.</summary>
    private static readonly Type[] NullableSchoolIdExceptions =
    [
        // The audit trail: entries the system writes belong to no school. It has its own filter in ApplicationDbContext.
        typeof(ChangeAuditEntry),
    ];

    private static IEnumerable<Type> EntityTypes() =>
        typeof(BaseEntity).Assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(BaseEntity).IsAssignableFrom(t) || t == typeof(ChangeAuditEntry));

    [Test]
    public void EveryEntityWithASchoolIdShouldImplementITenantOwned()
    {
        var offenders = EntityTypes()
            .Where(t => t.GetProperty("SchoolId")?.PropertyType == typeof(int))
            .Where(t => !typeof(ITenantOwned).IsAssignableFrom(t))
            .Select(t => t.Name)
            .ToList();

        offenders.ShouldBeEmpty("These entities have a SchoolId but do not implement ITenantOwned, so they would get no tenant filter");
    }

    [Test]
    public void ShouldFindTheTenantOwnedEntities()
    {
        // Guards the test above against passing because nothing was found.
        EntityTypes().Count(t => typeof(ITenantOwned).IsAssignableFrom(t)).ShouldBeGreaterThanOrEqualTo(20);
    }

    [Test]
    public void OnlyTheListedEntitiesMayHaveANullableSchoolId()
    {
        var nullable = EntityTypes()
            .Where(t => t.GetProperty("SchoolId")?.PropertyType == typeof(int?))
            .ToList();

        nullable.ShouldBe(NullableSchoolIdExceptions, ignoreOrder: true);
    }

    [Test]
    public void ITenantOwnedShouldOnlyExposeAReadableSchoolId()
    {
        typeof(ITenantOwned).GetProperties().Select(p => p.Name).ShouldBe(["SchoolId"]);
        typeof(ITenantOwned).GetProperty("SchoolId")!.CanWrite.ShouldBeFalse();
    }
}
