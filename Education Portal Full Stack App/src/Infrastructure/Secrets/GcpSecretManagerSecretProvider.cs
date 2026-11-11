using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Secrets;
using Google.Api.Gax.ResourceNames;
using Google.Cloud.SecretManager.V1;
using Google.Protobuf;
using Grpc.Core;

namespace EducationPortal.Infrastructure.Secrets;

/// <summary>
/// Google Secret Manager with application default credentials (a service account when deployed) (ADR-006). Secret
/// ids allow letters, digits, dashes and underscores, so <c>tenants/5/sms-api-key</c> becomes
/// <c>tenant-5-sms-api-key</c>. Reads return the latest version; a set adds a version.
/// </summary>
public sealed class GcpSecretManagerSecretProvider(SecretManagerServiceClient client, string projectId) : ISecretProvider
{
    private const string LatestVersion = "latest";

    public static SecretManagerServiceClient CreateClient() => SecretManagerServiceClient.Create();

    public static string ToSecretId(SecretKey key) => DashedSecretName.From(key);

    public Task<string?> GetSecretAsync(SecretKey key, CancellationToken cancellationToken) =>
        SecretCall.RunAsync<string?>("get", key, async () =>
        {
            try
            {
                var response = await client.AccessSecretVersionAsync(
                    new SecretVersionName(projectId, ToSecretId(key), LatestVersion), cancellationToken);
                return response.Payload.Data.ToStringUtf8();
            }
            catch (RpcException ex) when (ex.StatusCode is StatusCode.NotFound or StatusCode.FailedPrecondition)
            {
                // Not found, or no enabled version left.
                return null;
            }
        }, IsTransient, cancellationToken);

    public Task SetSecretAsync(SecretKey key, string value, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(value);

        return SecretCall.RunAsync("set", key, async () =>
        {
            var secretName = new SecretName(projectId, ToSecretId(key));
            var payload = new SecretPayload { Data = ByteString.CopyFromUtf8(value) };

            try
            {
                await client.AddSecretVersionAsync(secretName, payload, cancellationToken);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                await CreateSecretAsync(secretName, cancellationToken);
                await client.AddSecretVersionAsync(secretName, payload, cancellationToken);
            }
        }, IsTransient, cancellationToken);
    }

    public Task DeleteSecretAsync(SecretKey key, CancellationToken cancellationToken) =>
        SecretCall.RunAsync("delete", key, async () =>
        {
            try
            {
                await client.DeleteSecretAsync(new SecretName(projectId, ToSecretId(key)), cancellationToken);
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
            {
                // Already gone.
            }
        }, IsTransient, cancellationToken);

    private async Task CreateSecretAsync(SecretName secretName, CancellationToken cancellationToken)
    {
        try
        {
            var secret = new Secret { Replication = new Replication { Automatic = new Replication.Types.Automatic() } };
            await client.CreateSecretAsync(new ProjectName(projectId), secretName.SecretId, secret, cancellationToken);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.AlreadyExists)
        {
            // Another request created it first.
        }
    }

    private static bool IsTransient(Exception ex) => ex switch
    {
        RpcException rpc => rpc.StatusCode is StatusCode.Unavailable or StatusCode.DeadlineExceeded
            or StatusCode.ResourceExhausted or StatusCode.Internal or StatusCode.Aborted,
        HttpRequestException or TimeoutException => true,
        _ => false,
    };
}
