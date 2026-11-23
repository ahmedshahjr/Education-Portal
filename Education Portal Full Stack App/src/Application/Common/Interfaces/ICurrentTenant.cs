namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// The school and tenant that the current request's host resolves to (AGENTS.md section 2). They are set by the tenant
/// resolution middleware before any endpoint runs and are the only source of the current <c>SchoolId</c> and
/// <c>TenantId</c> (SEC-AUTHZ-04).
/// </summary>
public interface ICurrentTenant
{
    /// <summary>The resolved school, or <c>null</c> outside a request (for example in a health check).</summary>
    int? SchoolId { get; }

    /// <summary>The tenant that owns <see cref="SchoolId"/>, or <c>null</c> outside a request.</summary>
    int? TenantId { get; }

    /// <summary>True when the host is the master tenant's domain.</summary>
    bool IsMaster { get; }

    /// <summary>
    /// True only for a signed-in master tenant user on the master tenant's domain, after the session check. It lets the query
    /// filters and the database's row-level security see every tenant. A request without a session never has it, so an
    /// anonymous request on the master domain sees its own school only (SEC-AUTHZ-13).
    /// </summary>
    bool BypassesTenantFilters { get; }
}
