---
name: standard-soc2
description: What a SOC 2 report (AICPA Trust Services Criteria) needs from this codebase, which criteria the rules support, and the evidence engineering must keep. Use when working on access, change management, monitoring or confidentiality, or preparing SOC 2 evidence. Part of the mandatory Education Portal compliance rules.
---

# SOC 2 (MUST follow)

Part of the mandatory secure coding and compliance rules. This skill explains what the standard asks of this codebase and points to the topic skills that hold the actual rules. Always follow the [overview](../SKILL.md) too.

**Never claim certification.** Do not write that the product "is SOC 2 compliant" or show its badge or logo in code, the UI, docs, marketing text, commits or PRs until the maintainer has the authorization, report or certificate in hand and says so. Badges and logos are controlled by the owning body and come only after an independent assessment. Use wording like "designed to support SOC 2 Trust Services Criteria".

## What it is

SOC 2 is an attestation report by an independent CPA firm against the AICPA Trust Services Criteria (TSC). **Security** (the Common Criteria, CC1–CC9) is always in scope; **Availability**, **Processing Integrity**, **Confidentiality** and **Privacy** are optional. A Type I report covers design at a point in time; a Type II report tests that controls operated over a period (usually 3–12 months), so evidence must exist for the whole period. There is no "SOC 2 certification": it is a report.

Which criteria are in scope is a **TODO** maintainer decision. Because the product holds student data, Security + Confidentiality (and possibly Privacy) is the likely scope.

## Evidence engineering must produce continuously

- **CC8.1 change management:** every change through a PR with review, green CI and the security checklist ([change-management](../change-management/SKILL.md)). An auditor will sample merged PRs: none may skip review.
- **CC6.x logical access:** tenant isolation, role rights, MFA, lockout, joiner/mover/leaver handled in code ([authentication](../authentication/SKILL.md), [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md)).
- **CC7.x monitoring:** audit events and alerts ([logging-audit](../logging-audit/SKILL.md)), vulnerability scanning and fix deadlines ([dependencies-supply-chain](../dependencies-supply-chain/SKILL.md)).
- **C1.x confidentiality:** classification, minimisation and disposal of student data ([student-data-privacy](../student-data-privacy/SKILL.md)).

## Criteria mapping

CC1 (control environment), CC2 (communication), CC3 (risk assessment), CC5 (control activities), most of CC9 and the people/vendor processes are organisational; see the [overview](../SKILL.md) section 3.

| Criterion | Title | Where the rules are |
|---|---|---|
| C1.1 | Identifying and maintaining confidential information | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md), [cryptography-secrets](../cryptography-secrets/SKILL.md), [student-data-privacy](../student-data-privacy/SKILL.md) |
| C1.2 | Disposing of confidential information | [student-data-privacy](../student-data-privacy/SKILL.md) |
| CC4.1 | Ongoing and/or separate evaluations of controls | [security-testing](../security-testing/SKILL.md) |
| CC6.1 | Logical access security software, infrastructure and architectures | [authentication](../authentication/SKILL.md), [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md), [configuration-environments](../configuration-environments/SKILL.md), [cryptography-secrets](../cryptography-secrets/SKILL.md), [input-validation](../input-validation/SKILL.md) |
| CC6.2 | User registration and authorization before access | [authentication](../authentication/SKILL.md) |
| CC6.3 | Role-based access; granting, changing and removing access | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) |
| CC6.6 | Protection against threats from outside system boundaries | [web-api-hardening](../web-api-hardening/SKILL.md) |
| CC6.7 | Restricting transmission, movement and removal of information | [cryptography-secrets](../cryptography-secrets/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) |
| CC7.1 | Detecting configuration changes and new vulnerabilities | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md), [error-handling](../error-handling/SKILL.md), [input-validation](../input-validation/SKILL.md), [security-testing](../security-testing/SKILL.md) |
| CC7.2 | Monitoring system components for anomalies | [logging-audit](../logging-audit/SKILL.md) |
| CC7.3 | Evaluating security events | [logging-audit](../logging-audit/SKILL.md) |
| CC8.1 | Change management (authorize, design, test, approve, implement) | [change-management](../change-management/SKILL.md), [configuration-environments](../configuration-environments/SKILL.md), [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md), [overview](../SKILL.md) |
| CC9.2 | Vendor and business partner risk | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) |
| P1–P8 (if the Privacy criteria are in scope) | Privacy criteria (notice, choice, collection, use, access, disclosure, quality, monitoring) | [student-data-privacy](../student-data-privacy/SKILL.md) |

## Rules for any change

1. Before coding, find which rows in the mapping table below your change touches and load those topic skills.
2. Changes must keep the evidence an assessor will ask for: the PR (with the security checklist), the review approval, the CI run, and tests. Never bypass the PR flow (SEC-CHG-01).
3. Do not invent control parameters, scope or legal applicability. If the standard needs a value the maintainer has not set (marked **TODO**), ask.
4. If the standard updates (the twice-monthly sources review opens an issue), update this skill and the affected topic skills in one PR.

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [AICPA SOC 2 Trust Services Criteria](https://www.aicpa-cima.com/topic/audit-assurance/audit-and-assurance-greater-than-soc-2) | 2017 Trust Services Criteria with revised points of focus (2022) | Manual: listed for a portal check | 2026-11-09 |
<!-- END GENERATED: sources -->
