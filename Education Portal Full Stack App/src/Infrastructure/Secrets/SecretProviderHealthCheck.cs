using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Secrets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EducationPortal.Infrastructure.Secrets;

/// <summary>
/// Reads the probe key <c>platform/health-check</c> straight from the store, past the cache. A missing secret is
/// healthy: the check only proves the store answers and the app's identity can read it. No value is reported.
/// </summary>
public sealed class SecretProviderHealthCheck(ISecretProvider uncachedProvider) : IHealthCheck
{
    public const string Name = "secrets";

    public static readonly SecretKey ProbeKey = SecretKey.Platform("health-check");

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            await uncachedProvider.GetSecretAsync(ProbeKey, cancellationToken);
            return HealthCheckResult.Healthy("The secret store answered.");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "The secret store did not answer.", ex);
        }
    }
}
