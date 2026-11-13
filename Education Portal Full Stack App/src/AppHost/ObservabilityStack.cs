using EducationPortal.Shared;

/// <summary>
/// The local observability stack (ADR-007), as Aspire resources (AGENTS.md section 6): an OpenTelemetry Collector that
/// receives everything from the app, Prometheus for metrics, Loki for logs, Jaeger for traces and Grafana to view all
/// three. Images are pinned; each container keeps its data in a named volume. Run mode only: deployed environments
/// point <c>Observability:OtlpEndpoint</c> at their own backend instead.
/// </summary>
internal static class ObservabilityStack
{
    private const string ConfigDirectory = "./observability";

    // Pinned to the current stable releases (checked 2026-11-11). Update them together and re-test the stack.
    private const string CollectorImage = "otel/opentelemetry-collector";
    private const string CollectorTag = "0.165.0";
    private const string PrometheusImage = "prom/prometheus";
    private const string PrometheusTag = "v3.16.0";
    private const string LokiImage = "grafana/loki";
    private const string LokiTag = "3.7.9";
    private const string JaegerImage = "jaegertracing/jaeger";
    // 2.21.0 removed the v1 HTTP query API that Grafana's Jaeger data source still calls (/api/services,
    // /api/traces), so Jaeger stays on 2.20.0 until Grafana moves to the v3 API.
    private const string JaegerTag = "2.20.0";
    private const string GrafanaImage = "grafana/grafana";
    private const string GrafanaTag = "13.3.1";

    private const string OtlpApiKeyHeader = "x-otlp-api-key=";

    /// <summary>Adds the stack and returns the Collector, the only endpoint the app sends to.</summary>
    public static IResourceBuilder<ContainerResource> AddObservabilityStack(this IDistributedApplicationBuilder builder)
    {
        var prometheus = builder.AddContainer(Services.Prometheus, PrometheusImage, PrometheusTag)
            .WithLifetime(ContainerLifetime.Persistent)
            .WithBindMount($"{ConfigDirectory}/prometheus.yml", "/etc/prometheus/prometheus.yml", isReadOnly: true)
            .WithVolume("educationportal-prometheus-data", "/prometheus")
            .WithArgs(
                "--config.file=/etc/prometheus/prometheus.yml",
                "--storage.tsdb.path=/prometheus",
                "--storage.tsdb.retention.time=7d",
                "--web.enable-otlp-receiver")
            .WithHttpEndpoint(port: LocalPorts.Prometheus, targetPort: 9090, name: "http")
            .WithHttpHealthCheck("/-/ready");

        var loki = builder.AddContainer(Services.Loki, LokiImage, LokiTag)
            .WithLifetime(ContainerLifetime.Persistent)
            .WithBindMount($"{ConfigDirectory}/loki.yaml", "/etc/loki/loki.yaml", isReadOnly: true)
            .WithVolume("educationportal-loki-data", "/loki")
            .WithArgs("-config.file=/etc/loki/loki.yaml")
            .WithHttpEndpoint(port: LocalPorts.Loki, targetPort: 3100, name: "http")
            .WithHttpHealthCheck("/ready");

        // The Jaeger image declares /tmp as its data volume; Badger keeps traces there.
        var jaeger = builder.AddContainer(Services.Jaeger, JaegerImage, JaegerTag)
            .WithLifetime(ContainerLifetime.Persistent)
            .WithBindMount($"{ConfigDirectory}/jaeger.yaml", "/etc/jaeger/config.yaml", isReadOnly: true)
            .WithVolume("educationportal-jaeger-data", "/tmp")
            .WithArgs("--config", "/etc/jaeger/config.yaml")
            .WithHttpEndpoint(port: LocalPorts.JaegerUi, targetPort: 16686, name: "http")
            .WithEndpoint(port: LocalPorts.JaegerOtlp, targetPort: 4317, name: "otlp", scheme: "http")
            .WithHttpHealthCheck("/", endpointName: "http");

        // The dashboard's OTLP endpoint is plain HTTP with the "http" launch profile and HTTPS with "https".
        var dashboardOtlpUrl = builder.Configuration["ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL"]
            ?? builder.Configuration["DOTNET_DASHBOARD_OTLP_ENDPOINT_URL"];
        var dashboardOtlpInsecure = dashboardOtlpUrl?.StartsWith("http://", StringComparison.OrdinalIgnoreCase) == true;

        var collector = builder.AddContainer(Services.OtelCollector, CollectorImage, CollectorTag)
            .WithBindMount($"{ConfigDirectory}/otel-collector.yaml", "/etc/otelcol/config.yaml", isReadOnly: true)
            .WithArgs("--config=/etc/otelcol/config.yaml")
            .WithEndpoint(port: LocalPorts.CollectorGrpc, targetPort: 4317, name: "grpc", scheme: "http")
            .WithEndpoint(port: LocalPorts.CollectorHttp, targetPort: 4318, name: "http", scheme: "http")
            .WithHttpEndpoint(port: LocalPorts.CollectorHealth, targetPort: 13133, name: "health")
            .WithHttpHealthCheck("/", endpointName: "health")
            .WithEnvironment("PROMETHEUS_OTLP_ENDPOINT", ReferenceExpression.Create($"{prometheus.GetEndpoint("http")}/api/v1/otlp"))
            .WithEnvironment("LOKI_OTLP_ENDPOINT", ReferenceExpression.Create($"{loki.GetEndpoint("http")}/otlp"))
            .WithEnvironment("JAEGER_OTLP_ENDPOINT", jaeger.GetEndpoint("otlp"))
            // Copies everything to the Aspire dashboard too. Aspire sets its endpoint and API key; the Collector reads
            // the key from its own variable. The dashboard uses the ASP.NET Core development certificate.
            .WithOtlpExporter(OtlpProtocol.Grpc)
            .WithDeveloperCertificateTrust(true)
            .WithEnvironment(context =>
            {
                var headers = context.EnvironmentVariables.TryGetValue("OTEL_EXPORTER_OTLP_HEADERS", out var value)
                    ? value as string
                    : null;
                context.EnvironmentVariables["ASPIRE_OTLP_API_KEY"] =
                    headers is not null && headers.StartsWith(OtlpApiKeyHeader, StringComparison.Ordinal)
                        ? headers[OtlpApiKeyHeader.Length..]
                        : string.Empty;
                context.EnvironmentVariables["ASPIRE_OTLP_INSECURE"] = dashboardOtlpInsecure ? "true" : "false";
            })
            .WaitFor(prometheus)
            .WaitFor(loki)
            .WaitFor(jaeger);

        // Host port 13000, not Grafana's default 3000: a persistent container binds its host port directly, so when
        // another tool on the machine already holds 3000 the container cannot start (Aspire shows "Failed to start").
        // Anonymous admin with no sign-in form: safe only because this container runs locally (SEC-CFG-01).
        builder.AddContainer(Services.Grafana, GrafanaImage, GrafanaTag)
            .WithLifetime(ContainerLifetime.Persistent)
            .WithBindMount($"{ConfigDirectory}/grafana/provisioning", "/etc/grafana/provisioning", isReadOnly: true)
            .WithBindMount($"{ConfigDirectory}/grafana/dashboards", "/etc/grafana/dashboards", isReadOnly: true)
            .WithVolume("educationportal-grafana-data", "/var/lib/grafana")
            .WithEnvironment("GF_AUTH_ANONYMOUS_ENABLED", "true")
            .WithEnvironment("GF_AUTH_ANONYMOUS_ORG_ROLE", "Admin")
            .WithEnvironment("GF_AUTH_DISABLE_LOGIN_FORM", "true")
            .WithEnvironment("GF_ANALYTICS_REPORTING_ENABLED", "false")
            .WithEnvironment("GF_ANALYTICS_CHECK_FOR_UPDATES", "false")
            .WithEnvironment("PROMETHEUS_URL", prometheus.GetEndpoint("http"))
            .WithEnvironment("LOKI_URL", loki.GetEndpoint("http"))
            .WithEnvironment("JAEGER_URL", jaeger.GetEndpoint("http"))
            .WithHttpEndpoint(port: LocalPorts.Grafana, targetPort: 3000, name: "http")
            .WithHttpHealthCheck("/api/health")
            .WaitFor(prometheus)
            .WaitFor(loki)
            .WaitFor(jaeger);

        return collector;
    }
}
