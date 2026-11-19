namespace EducationPortal.Application.Common.Interfaces;

/// <summary>Finds the school that owns a host name. Unknown hosts give <c>null</c> and must get a 404 (SEC-AUTHZ-10).</summary>
public interface ITenantResolver
{
    Task<ResolvedTenant?> ResolveAsync(string host, CancellationToken cancellationToken);

    /// <summary>
    /// Finds an active school (not the master tenant) that has no domain of its own, so it signs in on the master tenant's
    /// domain. Gives <c>null</c> for a school that has a domain, is inactive or is deleted.
    /// </summary>
    Task<ResolvedTenant?> ResolveSchoolWithoutDomainAsync(int schoolId, CancellationToken cancellationToken);
}

public sealed record ResolvedTenant(int SchoolId, bool IsMaster);
