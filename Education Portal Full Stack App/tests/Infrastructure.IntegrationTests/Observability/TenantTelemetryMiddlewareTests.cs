using System.Diagnostics;
using EducationPortal.Application.Common.Diagnostics;
using EducationPortal.Application.Common.Interfaces;
using EducationPortal.Infrastructure.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shouldly;

namespace EducationPortal.Infrastructure.IntegrationTests.Observability;

/// <summary>tenant.id on the request's span, in its log scope, and on HTTP metrics only when opted in (ADR-007).</summary>
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class TenantTelemetryMiddlewareTests : IDisposable
{
    private static readonly ActivitySource TestSource = new("EducationPortal.Tests.TenantTelemetry");

    private readonly ActivityListener _listener = new()
    {
        ShouldListenTo = source => source == TestSource,
        Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllDataAndRecorded,
    };

    private readonly ScopeRecordingLogger _logger = new();

    public TenantTelemetryMiddlewareTests() => ActivitySource.AddActivityListener(_listener);

    public void Dispose() => _listener.Dispose();

    [Test]
    public async Task ShouldTagTheRequestActivityWithTheTenantId()
    {
        using var activity = TestSource.StartActivity("GET /api/Branding")!;
        var context = new DefaultHttpContext();

        await Middleware().InvokeAsync(context, new FakeTenant(7));

        activity.GetTagItem(TelemetryNames.Tags.TenantId).ShouldBe("7");
    }

    [Test]
    public async Task ShouldPreferTheActivityOfTheHttpActivityFeature()
    {
        using var request = TestSource.StartActivity("request")!;
        using var inner = TestSource.StartActivity("inner")!;
        var context = new DefaultHttpContext();
        context.Features.Set<IHttpActivityFeature>(new ActivityFeature(request));

        await Middleware().InvokeAsync(context, new FakeTenant(3));

        request.GetTagItem(TelemetryNames.Tags.TenantId).ShouldBe("3");
        inner.GetTagItem(TelemetryNames.Tags.TenantId).ShouldBeNull();
    }

    [Test]
    public async Task ShouldOpenALoggingScopeWithTheTenantIdAroundTheRestOfThePipeline()
    {
        IReadOnlyDictionary<string, object?>? scopeDuringRequest = null;
        var middleware = Middleware(next: _ =>
        {
            scopeDuringRequest = _logger.ActiveScope;
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(new DefaultHttpContext(), new FakeTenant(5));

        scopeDuringRequest.ShouldNotBeNull();
        scopeDuringRequest[TelemetryNames.Tags.TenantId].ShouldBe("5");
        _logger.ActiveScope.ShouldBeNull("the scope must close when the request ends");
    }

    [Test]
    public async Task ShouldLeaveRequestsWithoutATenantUntouched()
    {
        using var activity = TestSource.StartActivity("GET /health")!;
        var nextCalled = false;
        var middleware = Middleware(next: _ =>
        {
            nextCalled = true;
            _logger.ActiveScope.ShouldBeNull();
            return Task.CompletedTask;
        });

        await middleware.InvokeAsync(new DefaultHttpContext(), new FakeTenant(null));

        nextCalled.ShouldBeTrue();
        activity.GetTagItem(TelemetryNames.Tags.TenantId).ShouldBeNull();
    }

    [Test]
    public async Task ShouldNotAddTheTenantIdToHttpMetricsByDefault()
    {
        var metricsTags = new MetricsTagsFeature();
        var context = new DefaultHttpContext();
        context.Features.Set<IHttpMetricsTagsFeature>(metricsTags);

        await Middleware().InvokeAsync(context, new FakeTenant(9));

        metricsTags.Tags.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldAddTheTenantIdToHttpMetricsWhenOptedIn()
    {
        var metricsTags = new MetricsTagsFeature();
        var context = new DefaultHttpContext();
        context.Features.Set<IHttpMetricsTagsFeature>(metricsTags);

        await Middleware(tenantIdAsMetricLabel: true).InvokeAsync(context, new FakeTenant(9));

        metricsTags.Tags.ShouldBe([new KeyValuePair<string, object?>(TelemetryNames.Tags.TenantId, "9")]);
    }

    private TenantTelemetryMiddleware Middleware(RequestDelegate? next = null, bool tenantIdAsMetricLabel = false) =>
        new(
            next ?? (_ => Task.CompletedTask),
            _logger,
            Options.Create(new ObservabilityOptions { TenantIdAsMetricLabel = tenantIdAsMetricLabel }));

    private sealed record FakeTenant(long? SchoolId) : ICurrentTenant
    {
        public bool IsMaster => false;
    }

    private sealed class ActivityFeature(Activity activity) : IHttpActivityFeature
    {
        public Activity Activity { get; set; } = activity;
    }

    private sealed class MetricsTagsFeature : IHttpMetricsTagsFeature
    {
        public ICollection<KeyValuePair<string, object?>> Tags { get; } = [];

        public bool MetricsDisabled { get; set; }
    }

    /// <summary>Remembers the scope that is open, as a logging provider with IncludeScopes does.</summary>
    private sealed class ScopeRecordingLogger : ILogger<TenantTelemetryMiddleware>
    {
        public IReadOnlyDictionary<string, object?>? ActiveScope { get; private set; }

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull
        {
            var previous = ActiveScope;
            ActiveScope = state as IReadOnlyDictionary<string, object?>;
            return new Scope(() => ActiveScope = previous);
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }

        private sealed class Scope(Action onDispose) : IDisposable
        {
            public void Dispose() => onDispose();
        }
    }
}
