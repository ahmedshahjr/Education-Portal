---
name: change-management
description: Change management rules (PRs into development, human review, green CI, ADRs, commit/PR content, releases). Use when committing, opening PRs or preparing a release. Part of the mandatory Education Portal compliance rules.
---

# Change management (how work gets merged) (MUST follow)

Part of the mandatory secure coding and compliance rules. Also follow the [overview skill](../SKILL.md): its rules for AI assistants, exceptions process and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose a compliant alternative.

Framework abbreviations: **800-53** = NIST SP 800-53 Rev 5 (FedRAMP Moderate Rev 5 and GovRAMP baselines); **SOC 2** = AICPA Trust Services Criteria; **ISO** = ISO/IEC 27001:2022 Annex A; **OWASP** = OWASP Top 10:2025.

| ID | Rule |
|---|---|
| SEC-CHG-01 | Every change lands through a pull request into `development` (then `development` → `main` for releases). No direct pushes to `main` or `development`. |
| SEC-CHG-02 | At least one human reviewer who is not the author approves. Changes to authentication, authorization, tenant resolution, query filters, crypto, logging/audit or CI need review by the maintainer. |
| SEC-CHG-03 | CI must be green (build with warnings-as-errors, tests, dependency audit, scans) before merge. Tests MUST NOT be skipped, deleted or weakened to get green. |
| SEC-CHG-04 | Security-relevant design decisions get an ADR in `Education Portal Full Stack App/docs/decisions/`. |
| SEC-CHG-05 | Commits describe what and why; PRs link the issue or request and include the security checklist ([overview](../SKILL.md) section 5). |
| SEC-CHG-06 | Releases are tagged, traceable to commits, and reversible (rollback plan for migrations). |

Supports: 800-53 CM-3, CM-4, CM-5, SA-10, SA-11; SOC 2 CC8.1; ISO 8.25, 8.32.
