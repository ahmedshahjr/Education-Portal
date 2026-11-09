---
name: security-testing
description: Security testing rules (tenant/branch/module denial tests, architecture tests, hardening tests, validator tests, DAST). Use when writing or reviewing tests for any change. Part of the mandatory Education Portal compliance rules.
---

# Security testing (MUST follow)

Part of the mandatory secure coding and compliance rules. Also follow the [overview skill](../SKILL.md): its rules for AI assistants, exceptions process and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose a compliant alternative.

Framework abbreviations: **800-53** = NIST SP 800-53 Rev 5 (FedRAMP Moderate Rev 5 and GovRAMP baselines); **SOC 2** = AICPA Trust Services Criteria; **ISO** = ISO/IEC 27001:2022 Annex A; **OWASP** = OWASP Top 10:2025.

| ID | Rule |
|---|---|
| SEC-TEST-01 | Tenant/branch/module denial tests for every use case (SEC-AUTHZ-09). |
| SEC-TEST-02 | Architecture tests: Domain/Application do not reference Infrastructure; every MediatR request has an authorization declaration; every tenant-owned entity has `SchoolId` and a query filter. |
| SEC-TEST-03 | Tests for the hardening rules: security headers present, antiforgery required on unsafe methods, Scalar/OpenAPI not reachable outside Development, lockout after failed sign-ins, cookie flags. |
| SEC-TEST-04 | Validators have unit tests for boundary and malicious input (overlong strings, script tags, SQL metacharacters, Unicode edge cases). |
| SEC-TEST-05 | Before a production release: dynamic scan (e.g. OWASP ZAP baseline) against a staging environment; findings tracked to closure (**TODO** once hosting exists). |

Supports: 800-53 SA-11, SA-11(1), RA-5, CA-2, CA-8; SOC 2 CC7.1, CC4.1; ISO 8.29.
