using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Secrets;

namespace EducationPortal.Infrastructure.Secrets;

/// <summary>
/// The current school's secrets under <c>tenants/{schoolId}/{name}</c>. The school comes only from
/// <see cref="ICurrentTenant"/>, which the request's host sets (SEC-AUTHZ-04); there is no way to pass another one.
/// </summary>
public sealed class TenantSecretProvider(ICurrentTenant tenant, ISecretProvider secrets) : ITenantSecretProvider
{
    public const string NoTenantMessage = "No tenant is resolved for this request, so tenant secrets cannot be used.";

    public Task<string?> GetAsync(string name, CancellationToken cancellationToken) =>
        secrets.GetSecretAsync(KeyFor(name), cancellationToken);

    public Task SetAsync(string name, string value, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(value);
        return secrets.SetSecretAsync(KeyFor(name), value, cancellationToken);
    }

    public Task DeleteAsync(string name, CancellationToken cancellationToken) =>
        secrets.DeleteSecretAsync(KeyFor(name), cancellationToken);

    private SecretKey KeyFor(string name) => tenant.SchoolId is { } schoolId
        ? SecretKey.ForTenant(schoolId, name)
        : throw new InvalidOperationException(NoTenantMessage);
}
