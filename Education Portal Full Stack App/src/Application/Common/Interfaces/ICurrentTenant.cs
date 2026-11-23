namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// The school and tenant that the current request's host resolves to (AGENTS.md section 2). They are set by the tenant
/// resolution middleware before any endpoint runs and are the only source of the current <c>SchoolId</c> and
/// <c>TenantId</c> (SEC-AUTHZ-04).
/// </summary>
public interface ICurrentTenant
{
    /// <summary>
    /// True once the tenant is known: a request whose host resolved, or a scope that set the master tenant (a scheduled job).
    /// Read <see cref="SchoolId"/> and <see cref="TenantId"/> only when it is true; reading them before that throws, so no
    /// code runs as "no school" by accident (SEC-AUTHZ-04, fail closed).
    /// </summary>
    bool IsResolved { get; }

    /// <summary>The resolved school. Required: reading it before <see cref="IsResolved"/> throws.</summary>
    int SchoolId { get; }

    /// <summary>The tenant that owns <see cref="SchoolId"/>. Required: reading it before <see cref="IsResolved"/> throws.</summary>
    int TenantId { get; }

    /// <summary>True when the host is the master tenant's domain.</summary>
    bool IsMaster { get; }

    /// <summary>
    /// True only for a signed-in master tenant user on the master tenant's domain, after the session check. It lets the query
    /// filters and the database's row-level security see every tenant. A request without a session never has it, so an
    /// anonymous request on the master domain sees its own school only (SEC-AUTHZ-13).
    /// </summary>
    bool BypassesTenantFilters { get; }
}
