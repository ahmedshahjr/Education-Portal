using System.Diagnostics;
using System.Diagnostics.Metrics;
using EducationPortal.Application.Common.Diagnostics;

namespace EducationPortal.Infrastructure.Secrets;

/// <summary>
/// Spans and metrics of the secrets layer (ADR-006, ADR-007). Tags name the provider, the operation and the key's
/// scope and name, never a value (SEC-SECRET-01, SEC-LOG-02).
/// </summary>
public sealed class SecretsTelemetry
{
    public const string CacheLookupsName = "secrets.cache.lookups";
    public const string CacheResultTag = "secrets.cache.result";
    public const string CacheHitTag = "secrets.cache_hit";
    public const string ProviderTag = "secrets.provider";
    public const string OperationTag = "secrets.operation";
    public const string ScopeTag = "secrets.key_scope";
    public const string NameTag = "secrets.name";

    public static readonly ActivitySource ActivitySource = new(TelemetryNames.Secrets);

    private readonly Counter<long> _cacheLookups;

    public SecretsTelemetry(IMeterFactory meterFactory)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);

        var meter = meterFactory.Create(TelemetryNames.Secrets);
        _cacheLookups = meter.CreateCounter<long>(
            CacheLookupsName,
            unit: "{lookup}",
            description: "Secret reads answered from the cache (hit) or the store (miss).");
    }

    /// <summary>Counts a cache lookup and marks the current secrets span as a hit or a miss.</summary>
    public void RecordCacheLookup(bool hit)
    {
        _cacheLookups.Add(1, new KeyValuePair<string, object?>(CacheResultTag, hit ? "hit" : "miss"));

        if (Activity.Current is { } activity && activity.Source == ActivitySource)
        {
            activity.SetTag(CacheHitTag, hit);
        }
    }
}
