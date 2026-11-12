namespace EducationPortal.Application.Common.Diagnostics;

/// <summary>
/// The one place that names the app's own <see cref="System.Diagnostics.ActivitySource"/>s and
/// <see cref="System.Diagnostics.Metrics.Meter"/>s, and the attribute names shared between them (ADR-007).
/// <c>AddObservability</c> registers every source and meter listed here, so a new one only needs an entry in
/// <see cref="ActivitySources"/> or <see cref="Meters"/>.
/// </summary>
/// <remarks>
/// Never put passwords, tokens, secret values, email bodies, request bodies or personal data in a tag, a metric label
/// or a log message. Record ids only (SEC-LOG-02, SEC-PII-02, docs/observability.md).
/// </remarks>
public static class TelemetryNames
{
    /// <summary>Spans and metrics of use cases in the Application layer.</summary>
    public const string Application = "EducationPortal.Application";

    /// <summary>Spans and metrics of the secrets layer (ADR-006).</summary>
    public const string Secrets = "EducationPortal.Secrets";

    /// <summary>Every ActivitySource the app owns.</summary>
    public static readonly IReadOnlyList<string> ActivitySources = [Application, Secrets];

    /// <summary>Every Meter the app owns.</summary>
    public static readonly IReadOnlyList<string> Meters = [Application, Secrets];

    /// <summary>Attribute names used on spans, logs and metrics.</summary>
    public static class Tags
    {
        /// <summary>The SchoolId of the request's tenant (AGENTS.md section 2).</summary>
        public const string TenantId = "tenant.id";
    }
}
