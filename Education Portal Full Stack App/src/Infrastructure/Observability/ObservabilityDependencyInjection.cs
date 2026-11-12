using System.Reflection;
using EducationPortal.Application.Common.Diagnostics;
using EducationPortal.Infrastructure.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Logs, metrics and traces with OpenTelemetry, exported over OTLP to one endpoint (ADR-007).</summary>
public static class ObservabilityDependencyInjection
{
    /// <summary>The Npgsql ActivitySource and Meter: database calls, including every EF Core query.</summary>
    public const string NpgsqlTelemetryName = "Npgsql";

    /// <summary>Probe endpoints that are never traced: they run every few seconds and carry no business work.</summary>
    private static readonly PathString[] UntracedPaths = ["/health", "/alive", "/metrics"];

    /// <summary>
    /// Sets up OpenTelemetry from the <c>Observability</c> section: resource attributes, a parent-based ratio sampler,
    /// ASP.NET Core, HttpClient and Npgsql instrumentation, runtime metrics, the app's own sources and meters
    /// (<see cref="TelemetryNames"/>), and logs through OpenTelemetry with scopes, so each line carries TraceId, SpanId
    /// and <c>tenant.id</c>. Console logging stays. With <c>Enabled</c> false nothing is added. Export is in the
    /// background, so an unreachable endpoint never delays or stops the app.
    /// </summary>
    public static IServiceCollection AddObservability(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var section = configuration.GetSection(ObservabilityOptions.SectionName);
        var options = section.Get<ObservabilityOptions>() ?? new ObservabilityOptions();

        var errors = options.GetErrors();
        if (errors.Count > 0)
        {
            throw new OptionsValidationException(ObservabilityOptions.SectionName, typeof(ObservabilityOptions), errors);
        }

        services.AddOptions<ObservabilityOptions>().Bind(section);

        if (!options.Enabled)
        {
            return services;
        }

        var builder = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(options.ServiceName, serviceVersion: ServiceVersion())
                .AddAttributes(
                [
                    new("deployment.environment", environment.EnvironmentName),
                    new("deployment.environment.name", environment.EnvironmentName),
                ]))
            .WithTracing(tracing => tracing
                .SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(options.SampleRatioFor(environment))))
                .AddAspNetCoreInstrumentation(aspNetCore => aspNetCore.Filter = IsTraced)
                .AddHttpClientInstrumentation()
                .AddSource(NpgsqlTelemetryName)
                .AddSource([.. TelemetryNames.ActivitySources]))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(NpgsqlTelemetryName)
                .AddMeter([.. TelemetryNames.Meters]))
            .WithLogging(configureBuilder: null, configureOptions: logging =>
            {
                logging.IncludeFormattedMessage = true;
                logging.IncludeScopes = true;
            });

        if (!string.IsNullOrWhiteSpace(options.OtlpEndpoint))
        {
            builder.UseOtlpExporter(OtlpExportProtocol.Grpc, new Uri(options.OtlpEndpoint));
        }

        return services;
    }

    private static bool IsTraced(HttpContext context) =>
        !UntracedPaths.Any(path => context.Request.Path.StartsWithSegments(path));

    private static string? ServiceVersion() =>
        Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
}
