using EducationPortal.Application.Common.Secrets;

namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// The environment's secret store (ADR-006): HashiCorp Vault locally, or Azure Key Vault, AWS Secrets Manager or
/// Google Secret Manager when deployed, chosen by <c>Secrets:Provider</c>. One store serves every tenant.
/// Secret values are never stored in the database, logged or put in exception messages (SEC-SECRET-01).
/// </summary>
/// <remarks>
/// Use <see cref="ITenantSecretProvider"/> for a school's own secrets; it takes the school from the request.
/// Failures throw <see cref="Exceptions.SecretProviderException"/>.
/// </remarks>
public interface ISecretProvider
{
    /// <summary>Returns the secret's value, or <c>null</c> when it does not exist.</summary>
    Task<string?> GetSecretAsync(SecretKey key, CancellationToken cancellationToken);

    /// <summary>Creates the secret, or replaces its value.</summary>
    Task SetSecretAsync(SecretKey key, string value, CancellationToken cancellationToken);

    /// <summary>Deletes the secret. Deleting a secret that does not exist does nothing.</summary>
    Task DeleteSecretAsync(SecretKey key, CancellationToken cancellationToken);
}
