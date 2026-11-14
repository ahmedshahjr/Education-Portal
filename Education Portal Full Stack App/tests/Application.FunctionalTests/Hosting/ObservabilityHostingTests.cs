using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EducationPortal.Application.FunctionalTests.Hosting;

/// <summary>
/// The app runs normally whether observability is off or its OTLP endpoint cannot be reached (ADR-007).
/// </summary>
public class ObservabilityHostingTests : TestBase
{
    private const string OpenTelemetryLoggerProvider = "OpenTelemetryLoggerProvider";

    [Test]
    public async Task ShouldStartAndServeRequestsWithObservabilityDisabled()
    {
        await using var factory = new ProductionWebApiFactory(FunctionalTestSetup.ConnectionString)
            .WithWebHostBuilder(builder => builder.UseSetting("Observability:Enabled", "false"));
        using var session = TestHttp.CreateSession(factory);

        (await session.GetAsync("/api/Branding")).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await session.GetCurrentUserAsync()).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        LoggerProviderNames(factory.Services).ShouldNotContain(OpenTelemetryLoggerProvider);
    }

    [Test]
    public async Task ShouldServeRequestsWhenTheOtlpEndpointIsUnreachable()
    {
        // Nothing listens on this port, so every export fails in the background.
        await using var factory = new ProductionWebApiFactory(FunctionalTestSetup.ConnectionString)
            .WithWebHostBuilder(builder => builder.UseSetting("Observability:OtlpEndpoint", "http://127.0.0.1:9"));
        using var session = TestHttp.CreateSession(factory);

        (await session.GetAsync("/api/Branding")).StatusCode.ShouldBe(HttpStatusCode.OK);

        LoggerProviderNames(factory.Services).ShouldContain(OpenTelemetryLoggerProvider);
    }

    private static IEnumerable<string> LoggerProviderNames(IServiceProvider services) =>
        services.GetServices<ILoggerProvider>().Select(provider => provider.GetType().Name);
}
