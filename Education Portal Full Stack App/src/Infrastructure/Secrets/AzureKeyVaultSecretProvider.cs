using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Secrets;

namespace EducationPortal.Infrastructure.Secrets;

/// <summary>
/// Azure Key Vault with <see cref="DefaultAzureCredential"/> (managed identity when deployed) (ADR-006). Key Vault
/// names allow only letters, digits and dashes, so <c>tenants/5/sms-api-key</c> becomes <c>tenant-5-sms-api-key</c>
/// and <c>platform/db-connection</c> becomes <c>platform-db-connection</c>.
/// </summary>
public sealed class AzureKeyVaultSecretProvider(SecretClient client) : ISecretProvider
{
    public static SecretClient CreateClient(AzureKeyVaultOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new SecretClient(new Uri(options.VaultUri!), new DefaultAzureCredential());
    }

    public static string ToSecretName(SecretKey key) => DashedSecretName.From(key);

    public Task<string?> GetSecretAsync(SecretKey key, CancellationToken cancellationToken) =>
        SecretCall.RunAsync<string?>("get", key, async () =>
        {
            try
            {
                var response = await client.GetSecretAsync(ToSecretName(key), cancellationToken: cancellationToken);
                return response.Value.Value;
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return null;
            }
        }, IsTransient, cancellationToken);

    public Task SetSecretAsync(SecretKey key, string value, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(value);

        return SecretCall.RunAsync("set", key,
            () => client.SetSecretAsync(ToSecretName(key), value, cancellationToken),
            IsTransient, cancellationToken);
    }

    /// <summary>
    /// Starts Key Vault's soft delete and waits for it. With soft delete, the same name can only be set again after
    /// the deleted secret is purged or recovered (ADR-006).
    /// </summary>
    public Task DeleteSecretAsync(SecretKey key, CancellationToken cancellationToken) =>
        SecretCall.RunAsync("delete", key, async () =>
        {
            try
            {
                var operation = await client.StartDeleteSecretAsync(ToSecretName(key), cancellationToken);
                await operation.WaitForCompletionAsync(cancellationToken);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                // Already gone.
            }
        }, IsTransient, cancellationToken);

    private static bool IsTransient(Exception ex) => ex switch
    {
        RequestFailedException failed => SecretCall.IsTransientStatus(failed.Status),
        HttpRequestException or TimeoutException => true,
        _ => false,
    };
}

/// <summary>
/// The translation for stores whose names allow only letters, digits, dashes (and, for GCP, underscores):
/// <c>platform-{name}</c> and <c>tenant-{tenantId}-{name}</c>. Names start with a letter, so the tenant id always
/// ends at the first dash after it and two keys never get the same name.
/// </summary>
internal static class DashedSecretName
{
    public static string From(SecretKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return key.TenantId is { } tenantId
            ? $"tenant-{tenantId.ToString(System.Globalization.CultureInfo.InvariantCulture)}-{key.Name}"
            : $"platform-{key.Name}";
    }
}
