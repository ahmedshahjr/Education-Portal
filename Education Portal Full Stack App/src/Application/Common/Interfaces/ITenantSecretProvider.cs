namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// Secrets of the current school, stored under <c>tenants/{schoolId}/{name}</c> (ADR-006). The school always comes
/// from <see cref="ICurrentTenant"/>, which the request's host sets; a caller can never pass a tenant id
/// (SEC-AUTHZ-04). Every method throws <see cref="InvalidOperationException"/> when no tenant is resolved.
/// </summary>
public interface ITenantSecretProvider
{
    /// <summary>Returns the current school's secret, or <c>null</c> when it does not exist.</summary>
    Task<string?> GetAsync(string name, CancellationToken cancellationToken);

    Task SetAsync(string name, string value, CancellationToken cancellationToken);

    Task DeleteAsync(string name, CancellationToken cancellationToken);
}
