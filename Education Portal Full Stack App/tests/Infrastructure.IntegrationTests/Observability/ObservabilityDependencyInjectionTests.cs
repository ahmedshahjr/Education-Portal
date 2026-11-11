using EducationPortal.Infrastructure.Observability;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Shouldly;

namespace EducationPortal.Infrastructure.IntegrationTests.Observability;

/// <summary>Settings, sampling defaults and the Enabled switch of AddObservability (ADR-007).</summary>
public class ObservabilityDependencyInjectionTests
{
    [Test]
    public void ShouldAddNoOpenTelemetryWhenDisabled()
    {
        using var services = Build(new() { ["Observability:Enabled"] = "false" });

        services.GetService<TracerProvider>().ShouldBeNull();
        services.GetService<MeterProvider>().ShouldBeNull();
        services.GetRequiredService<IOptions<ObservabilityOptions>>().Value.Enabled.ShouldBeFalse();
    }

    [Test]
    public void ShouldAddTracingAndMetricsWhenEnabledEvenWithoutAnEndpoint()
    {
        using var services = Build(new() { ["Observability:Enabled"] = "true" });

        services.GetService<TracerProvider>().ShouldNotBeNull();
        services.GetService<MeterProvider>().ShouldNotBeNull();
    }

    [TestCase("1.5", "TraceSampleRatio")]
    [TestCase("-0.1", "TraceSampleRatio")]
    [TestCase("", "ServiceName")]
    public void ShouldRejectInvalidSettingsAtStartup(string value, string setting)
    {
        var settings = new Dictionary<string, string?> { [$"Observability:{setting}"] = value };

        var ex = Should.Throw<OptionsValidationException>(() => Build(settings));

        ex.Message.ShouldContain(setting);
    }

    [TestCase("localhost:4317")]
    [TestCase("ftp://collector:4317")]
    public void ShouldRejectAnEndpointThatIsNotAnAbsoluteHttpUrl(string endpoint)
    {
        Should.Throw<OptionsValidationException>(() => Build(new() { ["Observability:OtlpEndpoint"] = endpoint }))
            .Message.ShouldContain("OtlpEndpoint");
    }

    [TestCase("Development", 1.0)]
    [TestCase("Production", 0.1)]
    [TestCase("Staging", 0.1)]
    public void ShouldSampleEveryTraceInDevelopmentAndATenthElsewhereByDefault(string environment, double expected)
    {
        new ObservabilityOptions().SampleRatioFor(new TestEnvironment(environment)).ShouldBe(expected);
    }

    [Test]
    public void ShouldUseTheConfiguredSampleRatioInAnyEnvironment()
    {
        new ObservabilityOptions { TraceSampleRatio = 0.25 }
            .SampleRatioFor(new TestEnvironment("Development"))
            .ShouldBe(0.25);
    }

    [Test]
    public void ShouldKeepTheTenantIdOutOfMetricsByDefault()
    {
        new ObservabilityOptions().TenantIdAsMetricLabel.ShouldBeFalse();
    }

    private static ServiceProvider Build(Dictionary<string, string?> settings)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        return new ServiceCollection()
            .AddLogging()
            .AddObservability(configuration, new TestEnvironment(Environments.Production))
            .BuildServiceProvider();
    }

    private sealed class TestEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;

        public string ApplicationName { get; set; } = "EducationPortal.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
