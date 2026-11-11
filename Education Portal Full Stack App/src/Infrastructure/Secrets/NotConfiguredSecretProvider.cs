using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Secrets;

namespace EducationPortal.Infrastructure.Secrets;

/// <summary>
/// Used when <c>Secrets:Provider</c> is empty: the app runs on normal configuration, and any use of a secret fails
/// closed with a clear message instead of returning nothing.
/// </summary>
public sealed class NotConfiguredSecretProvider : ISecretProvider
{
    public const string Message =
        "No secret store is configured. Set Secrets:Provider to HashiCorp, AzureKeyVault, Aws or Gcp.";

    public Task<string?> GetSecretAsync(SecretKey key, CancellationToken cancellationToken) =>
        throw new InvalidOperationException(Message);

    public Task SetSecretAsync(SecretKey key, string value, CancellationToken cancellationToken) =>
        throw new InvalidOperationException(Message);

    public Task DeleteSecretAsync(SecretKey key, CancellationToken cancellationToken) =>
        throw new InvalidOperationException(Message);
}
