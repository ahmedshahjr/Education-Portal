using System.Diagnostics;
using System.Globalization;
using EducationPortal.Application.Common.Diagnostics;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Application.Common.Secrets;

namespace EducationPortal.Infrastructure.Secrets;

/// <summary>
/// One span per secrets call, with the provider, the operation, the key's scope (platform or tenant), its logical name
/// and <c>tenant.id</c> for a tenant secret. Values are never recorded (SEC-SECRET-01). It only observes: results and
/// exceptions pass through unchanged.
/// </summary>
public sealed class InstrumentedSecretProvider(ISecretProvider inner, string providerName) : ISecretProvider
{
    public ISecretProvider Inner => inner;

    public async Task<string?> GetSecretAsync(SecretKey key, CancellationToken cancellationToken)
    {
        using var activity = Start("get", key);
        try
        {
            return await inner.GetSecretAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            Fail(activity, ex);
            throw;
        }
    }

    public async Task SetSecretAsync(SecretKey key, string value, CancellationToken cancellationToken)
    {
        using var activity = Start("set", key);
        try
        {
            await inner.SetSecretAsync(key, value, cancellationToken);
        }
        catch (Exception ex)
        {
            Fail(activity, ex);
            throw;
        }
    }

    public async Task DeleteSecretAsync(SecretKey key, CancellationToken cancellationToken)
    {
        using var activity = Start("delete", key);
        try
        {
            await inner.DeleteSecretAsync(key, cancellationToken);
        }
        catch (Exception ex)
        {
            Fail(activity, ex);
            throw;
        }
    }

    private Activity? Start(string operation, SecretKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        var activity = SecretsTelemetry.ActivitySource.StartActivity($"secrets {operation}", ActivityKind.Internal);
        if (activity is null)
        {
            return null;
        }

        activity.SetTag(SecretsTelemetry.ProviderTag, providerName);
        activity.SetTag(SecretsTelemetry.OperationTag, operation);
        activity.SetTag(SecretsTelemetry.ScopeTag, key.IsPlatform ? "platform" : "tenant");
        activity.SetTag(SecretsTelemetry.NameTag, key.Name);

        if (key.TenantId is { } tenantId)
        {
            activity.SetTag(TelemetryNames.Tags.TenantId, tenantId.ToString(CultureInfo.InvariantCulture));
        }

        return activity;
    }

    // Only the exception's type is recorded: its message could name more than we want in a span.
    private static void Fail(Activity? activity, Exception ex)
    {
        activity?.SetStatus(ActivityStatusCode.Error);
        activity?.SetTag("error.type", ex.GetType().FullName);
    }
}
