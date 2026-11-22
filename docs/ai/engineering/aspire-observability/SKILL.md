---
name: aspire-observability
description: Rules for local orchestration and telemetry (every container is an Aspire resource in src/AppHost with fixed ports and WaitFor, no docker-compose, secrets only through ISecretProvider, OpenTelemetry in Infrastructure only, Application uses ILogger and System.Diagnostics with names in TelemetryNames, tenant.id never a metric label by default, and a never-record list for logs, spans and metrics). Use when adding or changing an AppHost resource, a container, a port, a log, a span, a metric, a health check or an observability setting. Part of the mandatory Education Portal engineering rules.
---

# Aspire and observability (MUST follow)

Part of the mandatory engineering rules. Also follow the [overview skill](../SKILL.md): its precedence, exceptions and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose an alternative. This skill points at decisions that already exist (ADR-002, ADR-006, ADR-007, `docs/observability.md`) and the security rules SEC-LOG and SEC-PII; it does not replace them.

## Orchestration (Aspire)

| ID | Rule |
|---|---|
| ENG-OPS-01 | **Containers are Aspire resources.** Every container the app needs locally (PostgreSQL, HashiCorp Vault, the observability stack, anything added later) is declared in `src/AppHost` (`Program.cs` or a helper such as `ObservabilityStack.cs`) with the Aspire hosting APIs (`AddPostgres`, `AddContainer`...). Credentials are Aspire parameters, start order uses `WaitFor`. No `docker-compose` file and no instruction to run `docker run` by hand. |
| ENG-OPS-02 | **Fixed local ports.** Every local resource gets a fixed port in `src/AppHost/LocalPorts.cs` (the Angular app is `http://localhost:14200`, PostgreSQL `15432`). A new resource adds its port there and in the README configuration table. |
| ENG-OPS-03 | **Tests use the test host.** Functional and acceptance tests start their containers through `tests/TestAppHost` and `Aspire.Hosting.Testing`, never a hand-started database. They need Docker or Podman running; say so when it was not (ENG-TEST-10). |
| ENG-OPS-04 | **Local-only relaxations stay local.** Anonymous Grafana, the dev-mode Vault, the Development seed and `*.localhost` hosts work only in Development (SEC-CFG-01, SEC-SECRET-04). A deployed environment changes `Secrets:Provider` and configuration only, never code. |
| ENG-OPS-05 | **Secrets go through the abstraction.** Read and write secrets only through `ISecretProvider` with logical keys (`SecretKey.Platform("name")`), or `ITenantSecretProvider` for the current school's secrets (the school comes from the host, never from the caller). Never store a secret value in the database, a log, an exception message or the repo (ADR-006). |

## Telemetry (OpenTelemetry, ADR-007)

| ID | Rule |
|---|---|
| ENG-OPS-06 | **Layer rule.** Domain has no observability code. Application uses only `ILogger<T>` and `System.Diagnostics` (`ActivitySource`, `Meter` through `IMeterFactory`). OpenTelemetry packages are referenced in Infrastructure only (`Infrastructure/Observability`). Health checks, resilience and service discovery stay in `src/ServiceDefaults`. |
| ENG-OPS-07 | **Names in one place.** Every `ActivitySource` and `Meter` name lives in `Application/Common/Diagnostics/TelemetryNames.cs`, and `AddObservability` registers what is listed there. A new source is added to that file, not as a string in the code. Start spans with `activity?.` (`StartActivity` returns `null` when nothing listens). Create meters through `IMeterFactory` in a small singleton class so tests can listen to their own instance. |
| ENG-OPS-08 | **One telemetry endpoint.** The API sends logs, metrics and traces over OTLP to `Observability:OtlpEndpoint`. Locally that is the OpenTelemetry Collector (`localhost:4317`), which forwards to Prometheus, Loki, Jaeger and the Aspire dashboard, with Grafana on top (`docs/observability.md`). Grafana dashboards and data sources are edited as JSON under `src/AppHost/observability/grafana/`, not in the Grafana UI. |
| ENG-OPS-09 | **Tenant in telemetry.** `tenant.id` comes from `ICurrentTenant`, never from the caller, and is added to the request span and log scope by `TenantTelemetryMiddleware`. It is **not** a metric label unless `Observability:TenantIdAsMetricLabel` is turned on deliberately. No user id, email, request id or trace id as a metric label. |
| ENG-OPS-10 | **Never record** (SEC-LOG-02, SEC-PII-02, SEC-SECRET-01): passwords or hashes, tokens, cookies, `Authorization` headers, API keys, MFA codes, connection strings, secret values, email bodies or subjects, full request or response bodies, uploaded files, query strings that carry personal data; and no names, emails, phone numbers, addresses, dates of birth, grades or free text of students, parents or staff. Record ids only (user id, `SchoolId`, `CampusId`, entity ids). |
| ENG-OPS-11 | **Structured logging.** Use message templates (`_logger.LogInformation("Course {CourseId} created", id)`), never string concatenation of input (SEC-LOG-07), and never destructure a request object (`{@Request}`). Exceptions recorded on spans name the type only when the message could carry more than an id. `/health`, `/alive` and `/metrics` are never traced. |
| ENG-OPS-12 | **Prove a new signal arrives.** A change that adds a span, a metric or a log scope includes a test that listens to it (`IMeterFactory` listener or `ActivityListener`) and, for a new dashboard panel, a check that the signal reaches Prometheus, Loki or Jaeger with the commands in `docs/observability.md`. Update `docs/observability.md` and the README's observability rows and diagrams in the same PR (AGENTS.md section 7, rules 8). |

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [Aspire documentation (AppHost, resources, integrations)](https://aspire.dev) | Aspire 13.6 | Manual: listed for a portal check | 2026-11-22 |
| [OpenTelemetry .NET (logs, metrics, traces, OTLP)](https://opentelemetry.io/docs/languages/dotnet/) | OpenTelemetry .NET docs as read on 2026-11-22 | Manual: listed for a portal check | 2026-11-22 |
<!-- END GENERATED: sources -->
