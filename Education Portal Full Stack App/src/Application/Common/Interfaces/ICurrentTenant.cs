namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// The school that the current request's host resolves to (AGENTS.md section 2). It is set by the tenant resolution
/// middleware before any endpoint runs and is the only source of the current <c>SchoolId</c> (SEC-AUTHZ-04).
/// </summary>
public interface ICurrentTenant
{
    /// <summary>The resolved school, or <c>null</c> outside a request (for example in a health check).</summary>
    long? SchoolId { get; }

    /// <summary>True when the host is the master tenant's domain.</summary>
    bool IsMaster { get; }
}
