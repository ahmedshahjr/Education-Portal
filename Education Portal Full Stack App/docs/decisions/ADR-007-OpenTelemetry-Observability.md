# ADR-007: Open-Source Observability with OpenTelemetry

## Status

Accepted

## Date

2026-11-11

## Context

The template's `ServiceDefaults` sent OpenTelemetry data to the Aspire dashboard only, and only when Aspire injected `OTEL_EXPORTER_OTLP_ENDPOINT`. The dashboard keeps data in memory and has no queries, dashboards or links between logs and traces. Hosting is still undecided (AGENTS.md section 9), so the backend must be swappable, for example to Application Insights later, without code changes. Logs must carry the trace and the tenant so one school's problem can be followed end to end, and nothing recorded may contain secrets or personal data (SEC-LOG-02, SEC-PII-02).

## Decision

- **One endpoint.** The app exports logs, metrics and traces over OTLP gRPC to `Observability:OtlpEndpoint` only. It does not know which backend is behind it.
- **Local stack as Aspire resources** (AGENTS.md section 6, no docker-compose): an OpenTelemetry Collector (core distribution) receives OTLP on 4317 and 4318, runs `memory_limiter` and `batch`, and exports metrics to Prometheus 3 (native OTLP receiver), logs to Loki 3 (native `/otlp` endpoint; the Loki exporter is deprecated), traces to Jaeger v2 (native OTLP), and a copy of everything to the Aspire dashboard. Grafana is provisioned with the three data sources, trace-to-logs and logs-to-trace links by trace id, anonymous admin (local only) and a starter dashboard. Images are pinned; Prometheus, Loki, Jaeger and Grafana keep data in named volumes.
- **Layers.** Domain has no observability code. Application names its ActivitySources and Meters in one static class, `TelemetryNames`, and uses only `ILogger<T>` and `System.Diagnostics`. Infrastructure owns the OpenTelemetry packages and `AddObservability(IServiceCollection, IConfiguration, IHostEnvironment)`. Web calls `AddObservability` and `UseTenantTelemetry` only. OpenTelemetry setup moves out of `ServiceDefaults`.
- **Instrumentation.** Traces: ASP.NET Core (health and metrics endpoints excluded), HttpClient, Npgsql (covers EF Core queries with a stable package already brought in by Aspire, instead of the prerelease EF Core instrumentation), and the app's own sources. Metrics: ASP.NET Core, HttpClient, .NET runtime, Npgsql and the app's own meters. Logs go through the OpenTelemetry logging provider with scopes, alongside the console provider, so each line has its TraceId and SpanId.
- **Tenant.** `TenantTelemetryMiddleware` runs right after tenant resolution and puts `tenant.id` (the SchoolId from `ICurrentTenant`) on the request span and in a log scope. It is a metric label only when `Observability:TenantIdAsMetricLabel` is true, because a label per school multiplies series.
- **Sampling.** Parent-based trace-id ratio. `TraceSampleRatio` defaults to 1.0 in Development and 0.1 elsewhere.
- **Secrets layer.** `InstrumentedSecretProvider` wraps the cached provider: one span per call with the provider, operation, key scope, logical name and `tenant.id`; `CachedSecretProvider` counts cache hits and misses (`secrets.cache.lookups`) and marks the span. Values are never recorded.
- **Email outbox.** Not instrumented yet because the email layer does not exist. When it is built it adds its spans and the metrics `emails_sent_total`, `emails_failed_total`, `email_send_duration` and `outbox_pending`, plus dashboard panels (decided by the maintainer, 2026-11-11).
- **Failure behaviour.** `Enabled=false` adds no OpenTelemetry. An unreachable endpoint never blocks startup or requests: export runs in the background. Invalid settings stop startup with a clear message.

## Rationale

- A Collector between the app and the backends is the OpenTelemetry-recommended pattern: the app stays backend-neutral, and routing, batching, memory limits and later redaction live in configuration.
- Prometheus, Loki and Jaeger now all ingest OTLP natively, so the core Collector distribution is enough and no backend-specific exporters are needed.
- Keeping the Aspire dashboard fed through the Collector preserves the existing developer workflow while the app still has a single endpoint.
- No new NuGet packages: the OpenTelemetry packages were already pinned for `ServiceDefaults`; tests use the BCL `ActivityListener` and `MeterListener`.

## Consequences

**Easier:**
- Switching to another backend, including Application Insights through a Collector, is a configuration change.
- A trace, its database queries and its log lines are linked by trace id and carry the school, in Grafana and in the Aspire dashboard.

**Harder:**
- Five more local containers start with `aspire run` and use memory and disk.
- Jaeger is pinned to 2.20.0: 2.21.0 removed the v1 HTTP query API that Grafana's Jaeger data source uses. Upgrade both together once Grafana uses the v3 API.
- Production still needs a Collector or OTLP backend, retention and alerting (SEC-LOG-06) as part of the hosting decision; `appsettings.json` exports nothing until a deployment sets `OtlpEndpoint`.
- Every new log, tag and metric must follow the never-record list in `docs/observability.md`.
