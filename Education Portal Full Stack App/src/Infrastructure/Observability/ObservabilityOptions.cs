using Microsoft.Extensions.Hosting;

namespace EducationPortal.Infrastructure.Observability;

/// <summary>
/// The <c>Observability</c> configuration section (ADR-007). The app knows one OTLP endpoint only, so the backend can be
/// changed by configuration alone.
/// </summary>
public sealed class ObservabilityOptions
{
    public const string SectionName = "Observability";

    /// <summary>Trace sampling in Development when <see cref="TraceSampleRatio"/> is not set: every trace.</summary>
    public const double DevelopmentSampleRatio = 1.0;

    /// <summary>Trace sampling in other environments when <see cref="TraceSampleRatio"/> is not set.</summary>
    public const double DefaultSampleRatio = 0.1;

    /// <summary>When false, no OpenTelemetry is set up and the app logs to the console only.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>The <c>service.name</c> resource attribute.</summary>
    public string ServiceName { get; set; } = Services.WebApi;

    /// <summary>The OTLP gRPC endpoint, for example <c>http://localhost:4317</c>. Empty means nothing is exported.</summary>
    public string? OtlpEndpoint { get; set; }

    /// <summary>The share of new traces to keep, from 0 to 1. Empty means 1 in Development and 0.1 elsewhere.</summary>
    public double? TraceSampleRatio { get; set; }

    /// <summary>
    /// Adds <c>tenant.id</c> to HTTP server metrics. Off by default: one series per school can make too many series.
    /// </summary>
    public bool TenantIdAsMetricLabel { get; set; }

    public double SampleRatioFor(IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);

        return TraceSampleRatio ?? (environment.IsDevelopment() ? DevelopmentSampleRatio : DefaultSampleRatio);
    }

    /// <summary>The problems with these settings, or none. Read at startup so wrong settings stop the app early.</summary>
    public IReadOnlyList<string> GetErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ServiceName))
        {
            errors.Add($"{SectionName}:ServiceName is required.");
        }

        if (TraceSampleRatio is < 0 or > 1 || (TraceSampleRatio is { } ratio && double.IsNaN(ratio)))
        {
            errors.Add($"{SectionName}:TraceSampleRatio must be between 0 and 1.");
        }

        if (!string.IsNullOrWhiteSpace(OtlpEndpoint)
            && (!Uri.TryCreate(OtlpEndpoint, UriKind.Absolute, out var endpoint)
                || (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps)))
        {
            errors.Add($"{SectionName}:OtlpEndpoint must be an absolute http or https URL.");
        }

        return errors;
    }
}
