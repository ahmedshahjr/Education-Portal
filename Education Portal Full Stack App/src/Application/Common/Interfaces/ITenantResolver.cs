namespace EducationPortal.Application.Common.Interfaces;

/// <summary>Finds the school that owns a host name. Unknown hosts give <c>null</c> and must get a 404 (SEC-AUTHZ-10).</summary>
public interface ITenantResolver
{
    Task<ResolvedTenant?> ResolveAsync(string host, CancellationToken cancellationToken);
}

public sealed record ResolvedTenant(long SchoolId, bool IsMaster);
