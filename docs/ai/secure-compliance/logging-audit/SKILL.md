---
name: logging-audit
description: Logging, audit and monitoring rules (structured ILogger/OpenTelemetry, what never to log, required security audit events and fields, append-only audit, log injection). Use when adding logs or security-relevant actions. Part of the mandatory Education Portal compliance rules.
---

# Logging, audit and monitoring (MUST follow)

Part of the mandatory secure coding and compliance rules. Also follow the [overview skill](../SKILL.md): its rules for AI assistants, exceptions process and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose a compliant alternative.

Framework abbreviations: **800-53** = NIST SP 800-53 Rev 5 (FedRAMP Moderate Rev 5 and GovRAMP baselines); **SOC 2** = AICPA Trust Services Criteria; **ISO** = ISO/IEC 27001:2022 Annex A; **OWASP** = OWASP Top 10:2025.

| ID | Rule |
|---|---|
| SEC-LOG-01 | Use `ILogger` with structured templates and OpenTelemetry (ServiceDefaults). MUST NOT use `Console.WriteLine` for diagnostics. |
| SEC-LOG-02 | MUST NOT log passwords, password hashes, tokens, cookies, auth headers, MFA codes, connection strings, full request/response bodies, or Restricted/Confidential data (SEC-PII-02). Destructured objects (`{@Request}`) MUST NOT be logged unless every property is known to be safe. |
| SEC-LOG-03 | Write a **security audit event** for: sign-in success/failure, lockout, sign-out, MFA changes, password reset, user create/disable/delete, role and permission changes, branch assignment changes, module on/off per school, tenant mismatch / cross-tenant attempts, 403s, master-tenant cross-school reads, data exports, and configuration changes. |
| SEC-LOG-04 | Each audit event records: UTC timestamp, event type, outcome, actor user ID, actor's SchoolId and BranchId, target entity type and ID, source IP, user agent, correlation/trace ID. Clocks use `TimeProvider` / UTC. |
| SEC-LOG-05 | Audit records are append-only for the application: no update or delete use cases, no tenant user can read other tenants' audit records, and only authorized roles can read their own school's. |
| SEC-LOG-06 | Logs and audit records are exported to a central store where they are retained and alerted on. Retention and alerting setup are platform decisions (**TODO**; FedRAMP points to OMB M-21-31). Code MUST NOT delete or truncate audit data. |
| SEC-LOG-07 | Log injection: do not concatenate user input into log messages; use structured parameters. |

Supports: 800-53 AU-2, AU-3, AU-6, AU-8, AU-9, AU-11, AU-12, SI-4; SOC 2 CC7.2, CC7.3; ISO 8.15, 8.16, 8.17; OWASP A09.

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [OMB M-21-31 logging requirements (referenced by FedRAMP AU-11)](https://www.whitehouse.gov/omb/information-for-agencies/memoranda/) | OMB M-21-31 (2021) | Manual: listed for a portal check | 2026-11-09 |
<!-- END GENERATED: sources -->
