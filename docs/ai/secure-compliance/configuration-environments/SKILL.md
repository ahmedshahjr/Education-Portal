---
name: configuration-environments
description: Configuration and environment rules (Development-only relaxations, separate environments, EF Core migrations, least-privilege DB user, infrastructure as code). Use when changing appsettings, startup, AppHost, migrations or deployment. Part of the mandatory Education Portal compliance rules.
---

# Configuration and environments (MUST follow)

Part of the mandatory secure coding and compliance rules. Also follow the [overview skill](../SKILL.md): its rules for AI assistants, exceptions process and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose a compliant alternative.

Framework abbreviations: **800-53** = NIST SP 800-53 Rev 5 (FedRAMP Moderate Rev 5 and GovRAMP baselines); **SOC 2** = AICPA Trust Services Criteria; **ISO** = ISO/IEC 27001:2022 Annex A; **OWASP** = OWASP Top 10:2025.

| ID | Rule |
|---|---|
| SEC-CFG-01 | Secure defaults in code; Development-only relaxations (dev DB reset, seeded admin, permissive CORS, detailed errors, Scalar UI) MUST be inside `IsDevelopment()` checks and covered by a test or startup assertion that they are off elsewhere. |
| SEC-CFG-02 | Environments are separate (Development, Test, Staging, Production) with separate databases, secrets and identities. No shared credentials between environments. |
| SEC-CFG-03 | Database schema changes go through EF Core migrations reviewed in a PR. No manual schema changes in shared environments. `EnsureDeleted`/`EnsureCreated` MUST NOT run outside Development. |
| SEC-CFG-04 | The database user used by the app has only the rights it needs (no superuser, no `CREATE ROLE`); migrations run with a separate identity. |
| SEC-CFG-05 | Infrastructure as code (Aspire/azd/Bicep) is reviewed like application code; no portal-only production changes. |

Supports: 800-53 CM-2, CM-3, CM-6, CM-7, AC-6, SA-10; SOC 2 CC8.1, CC6.1; ISO 8.9, 8.31, 8.32.
