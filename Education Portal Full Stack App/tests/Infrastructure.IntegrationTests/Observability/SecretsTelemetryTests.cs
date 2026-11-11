using System.Diagnostics;
using System.Diagnostics.Metrics;
using EducationPortal.Application.Common.Diagnostics;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Secrets;
using EducationPortal.Infrastructure.IntegrationTests.Secrets;
using EducationPortal.Infrastructure.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;

namespace EducationPortal.Infrastructure.IntegrationTests.Observability;

/// <summary>
/// Cache hit and miss metrics, and one span per secrets call that names the provider, operation and key but never the
/// value (ADR-006, ADR-007).
/// </summary>
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class SecretsTelemetryTests : IDisposable
{
    private const string SecretValue = "s3cr3t-value-never-recorded";

    private static readonly SecretKey TenantKey = SecretKey.ForTenant(4, "sms-api-key");

    private readonly ServiceProvider _services = new ServiceCollection().AddMetrics().BuildServiceProvider();
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());
    private readonly InMemorySecretProvider _store = new();
    private readonly MeterListener _meterListener = new();
    private readonly List<(long Value, Dictionary<string, object?> Tags)> _measurements = [];
    private readonly List<Activity> _spans = [];
    private readonly ActivityListener _activityListener;
    private readonly InstrumentedSecretProvider _provider;

    public SecretsTelemetryTests()
    {
        var meterFactory = _services.GetRequiredService<IMeterFactory>();

        // Only this test's meter: the factory gives each container its own Meter instance.
        _meterListener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == TelemetryNames.Secrets && instrument.Meter.Scope == meterFactory)
            {
                listener.EnableMeasurementEvents(instrument);
            }
        };
        _meterListener.SetMeasurementEventCallback<long>((_, value, tags, _) =>
            _measurements.Add((value, tags.ToArray().ToDictionary(tag => tag.Key, tag => tag.Value))));
        _meterListener.Start();

        _activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == TelemetryNames.Secrets,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
            ActivityStopped = _spans.Add,
        };
        ActivitySource.AddActivityListener(_activityListener);

        var telemetry = new SecretsTelemetry(meterFactory);
        var cached = new CachedSecretProvider(_store, _cache, Options.Create(new SecretsOptions()), telemetry);
        _provider = new InstrumentedSecretProvider(cached, "HashiCorp");
    }

    public void Dispose()
    {
        _activityListener.Dispose();
        _meterListener.Dispose();
        _cache.Dispose();
        _services.Dispose();
    }

    [Test]
    public async Task ShouldCountAMissThenAHit()
    {
        _store.Values[TenantKey.Path] = SecretValue;

        await _provider.GetSecretAsync(TenantKey, CancellationToken.None);
        await _provider.GetSecretAsync(TenantKey, CancellationToken.None);

        _measurements.Select(m => (m.Value, m.Tags[SecretsTelemetry.CacheResultTag])).ShouldBe([(1L, "miss"), (1L, "hit")]);
    }

    [Test]
    public async Task ShouldCountAMissingSecretAsAMissEachTime()
    {
        await _provider.GetSecretAsync(TenantKey, CancellationToken.None);
        await _provider.GetSecretAsync(TenantKey, CancellationToken.None);

        _measurements.Select(m => m.Tags[SecretsTelemetry.CacheResultTag]).ShouldBe(["miss", "miss"]);
    }

    [Test]
    public async Task ShouldRecordOneSpanPerCallWithProviderOperationKeyAndCacheResult()
    {
        _store.Values[TenantKey.Path] = SecretValue;

        await _provider.GetSecretAsync(TenantKey, CancellationToken.None);
        await _provider.GetSecretAsync(TenantKey, CancellationToken.None);

        _spans.Count.ShouldBe(2);
        _spans.ShouldAllBe(span => span.DisplayName == "secrets get");
        _spans.ShouldAllBe(span => (string?)span.GetTagItem(SecretsTelemetry.ProviderTag) == "HashiCorp");
        _spans.ShouldAllBe(span => (string?)span.GetTagItem(SecretsTelemetry.ScopeTag) == "tenant");
        _spans.ShouldAllBe(span => (string?)span.GetTagItem(SecretsTelemetry.NameTag) == "sms-api-key");
        _spans.ShouldAllBe(span => (string?)span.GetTagItem(TelemetryNames.Tags.TenantId) == "4");
        _spans[0].GetTagItem(SecretsTelemetry.CacheHitTag).ShouldBe(false);
        _spans[1].GetTagItem(SecretsTelemetry.CacheHitTag).ShouldBe(true);
    }

    [Test]
    public async Task ShouldNeverRecordTheSecretValue()
    {
        await _provider.SetSecretAsync(TenantKey, SecretValue, CancellationToken.None);
        await _provider.GetSecretAsync(TenantKey, CancellationToken.None);

        _spans.Count.ShouldBe(2);
        _spans.SelectMany(span => span.TagObjects).ShouldAllBe(tag => !Equals(tag.Value, SecretValue));
        _measurements.SelectMany(m => m.Tags.Values).ShouldAllBe(value => !Equals(value, SecretValue));
    }

    [Test]
    public async Task ShouldTagAPlatformKeyWithoutATenant()
    {
        await _provider.GetSecretAsync(SecretKey.Platform("db-connection"), CancellationToken.None);

        _spans.Single().GetTagItem(SecretsTelemetry.ScopeTag).ShouldBe("platform");
        _spans.Single().GetTagItem(TelemetryNames.Tags.TenantId).ShouldBeNull();
    }

    [Test]
    public async Task ShouldMarkTheSpanAsFailedAndRethrowUnchanged()
    {
        var failing = new InstrumentedSecretProvider(new FlakySecretProvider(failures: 1, transient: false), "Aws");

        await Should.ThrowAsync<SecretProviderException>(
            () => failing.GetSecretAsync(TenantKey, CancellationToken.None));

        var span = _spans.Single();
        span.Status.ShouldBe(ActivityStatusCode.Error);
        span.GetTagItem("error.type").ShouldBe(typeof(SecretProviderException).FullName);
    }
}
