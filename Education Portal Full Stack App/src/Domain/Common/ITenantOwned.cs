namespace EducationPortal.Domain.Common;

/// <summary>
/// Marks an entity whose rows belong to one school (AGENTS.md section 2). Every entity with a <c>SchoolId</c> implements it,
/// and <c>ApplicationDbContext</c> gives every implementing entity the tenant query filter in one place, so a new
/// tenant-owned entity cannot be added without isolation (SEC-AUTHZ-03). A functional test fails when an entity has a
/// <c>SchoolId</c> but does not implement this interface, or implements it without a filter.
/// </summary>
/// <remarks>
/// Not for platform reference data (modules, plans, price books, countries), which belongs to no school, nor for the
/// <c>School</c> itself, which is the tenant root.
/// </remarks>
public interface ITenantOwned
{
    /// <summary>The name of the column that holds the tenant of the row (ADR-026). It is a shadow property of EF Core, so no entity class has it.</summary>
    public const string TenantColumn = "TenantId";

    /// <summary>The school that owns the row. Set when the row is created and never changed.</summary>
    int SchoolId { get; }
}
