---
name: error-handling
description: Error and exception handling rules (fail closed, no swallowed exceptions, transactions, timeouts and cancellation). Use when writing handlers, try/catch, or external calls. Part of the mandatory Education Portal compliance rules.
---

# Error and exception handling (MUST follow)

Part of the mandatory secure coding and compliance rules. Also follow the [overview skill](../SKILL.md): its rules for AI assistants, exceptions process and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose a compliant alternative.

Framework abbreviations: **800-53** = NIST SP 800-53 Rev 5 (FedRAMP Moderate Rev 5 and GovRAMP baselines); **SOC 2** = AICPA Trust Services Criteria; **ISO** = ISO/IEC 27001:2022 Annex A; **OWASP** = OWASP Top 10:2025.

| ID | Rule |
|---|---|
| SEC-ERR-01 | Fail closed: if a tenant, branch, module or permission cannot be resolved, deny. Never default to "allow" or to "no filter". |
| SEC-ERR-02 | Do not swallow exceptions. Catch only what you can handle, log with a correlation ID (without sensitive data), and return a generic ProblemDetails. |
| SEC-ERR-03 | Transactions: multi-step writes use a single `SaveChangesAsync` or an explicit transaction so failures don't leave partial state. |
| SEC-ERR-04 | Timeouts and cancellation tokens are passed through to EF Core and HTTP calls; external calls use the resilience handlers from ServiceDefaults. |

Supports: 800-53 SI-11, SC-24, SI-10; SOC 2 CC7.1; ISO 8.28; OWASP A10.
