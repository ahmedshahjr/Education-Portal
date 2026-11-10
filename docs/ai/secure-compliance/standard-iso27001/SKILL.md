---
name: standard-iso27001
description: What ISO/IEC 27001:2022 certification needs from this codebase, which Annex A controls the rules implement, and what belongs to the ISMS. Use when working on security controls or preparing ISO 27001 evidence or the Statement of Applicability. Part of the mandatory Education Portal compliance rules.
---

# ISO/IEC 27001:2022 (MUST follow)

Part of the mandatory secure coding and compliance rules. This skill explains what the standard asks of this codebase and points to the topic skills that hold the actual rules. Always follow the [overview](../SKILL.md) too.

**Never claim certification.** Do not write that the product "is ISO 27001 certified" or show its badge or logo in code, the UI, docs, marketing text, commits or PRs until the maintainer has the authorization, report or certificate in hand and says so. Badges and logos are controlled by the owning body and come only after an independent assessment. Use wording like "designed to support ISO/IEC 27001 Annex A controls".

## What it is

ISO/IEC 27001:2022 certifies an organisation's **information security management system (ISMS)**, not a product. Clauses 4–10 set management-system requirements (scope, risk assessment and treatment, objectives, internal audit, management review, continual improvement). Annex A lists 93 controls in four themes: organisational (5.x), people (6.x), physical (7.x) and technological (8.x). The organisation picks applicable controls in its Statement of Applicability (SoA). An accredited certification body audits it (stage 1 and 2), then runs surveillance audits and recertification every three years. Amendment 1:2024 added climate-change considerations to clause 4. ISO/IEC 27002:2022 gives implementation guidance for each Annex A control.

## What this means for the code

- The code implements most of the technological (8.x) controls and parts of 5.15–5.18, 5.19–5.22 and 5.34. Record in the SoA which topic skill implements each control (table below).
- **8.25–8.29, 8.32 secure development:** the topic skills plus PR review are the documented secure development policy evidence. Keep them current.
- **8.31 / 8.33:** separate environments and no real personal data in test ([configuration-environments](../configuration-environments/SKILL.md), [student-data-privacy](../student-data-privacy/SKILL.md)).
- People (6.x) and physical (7.x) controls and clauses 4–10 are organisational; see the [overview](../SKILL.md) section 3.

## Annex A mapping (code-level controls)

| Annex A control | Title | Where the rules are |
|---|---|---|
| 5.15 | Access control | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) |
| 5.17 | Authentication information | [authentication](../authentication/SKILL.md), [cryptography-secrets](../cryptography-secrets/SKILL.md) |
| 5.18 | Access rights | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) |
| 5.19–5.22 | Supplier relationships and ICT supply chain | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) |
| 5.34 | Privacy and protection of PII | [student-data-privacy](../student-data-privacy/SKILL.md) |
| 8.2 | Privileged access rights | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) |
| 8.3 | Information access restriction | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) |
| 8.5 | Secure authentication | [authentication](../authentication/SKILL.md) |
| 8.8 | Management of technical vulnerabilities | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) |
| 8.9 | Configuration management | [configuration-environments](../configuration-environments/SKILL.md) |
| 8.10 | Information deletion | [student-data-privacy](../student-data-privacy/SKILL.md) |
| 8.11 | Data masking | [student-data-privacy](../student-data-privacy/SKILL.md) |
| 8.12 | Data leakage prevention | [student-data-privacy](../student-data-privacy/SKILL.md) |
| 8.15 | Logging | [logging-audit](../logging-audit/SKILL.md) |
| 8.16 | Monitoring activities | [logging-audit](../logging-audit/SKILL.md) |
| 8.17 | Clock synchronization | [logging-audit](../logging-audit/SKILL.md) |
| 8.19 | Installation of software on operational systems | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) |
| 8.20 | Networks security | [web-api-hardening](../web-api-hardening/SKILL.md) |
| 8.21 | Security of network services | [web-api-hardening](../web-api-hardening/SKILL.md) |
| 8.24 | Use of cryptography | [cryptography-secrets](../cryptography-secrets/SKILL.md) |
| 8.25 | Secure development life cycle | [change-management](../change-management/SKILL.md), [overview](../SKILL.md) |
| 8.26 | Application security requirements | [input-validation](../input-validation/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) |
| 8.28 | Secure coding | [error-handling](../error-handling/SKILL.md), [input-validation](../input-validation/SKILL.md), [overview](../SKILL.md) |
| 8.29 | Security testing in development and acceptance | [security-testing](../security-testing/SKILL.md) |
| 8.30 | Outsourced development | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) |
| 8.31 | Separation of development, test and production environments | [configuration-environments](../configuration-environments/SKILL.md) |
| 8.32 | Change management | [change-management](../change-management/SKILL.md), [configuration-environments](../configuration-environments/SKILL.md), [overview](../SKILL.md) |
| 8.33 | Test information | [student-data-privacy](../student-data-privacy/SKILL.md) |

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
| [ISO/IEC 27001:2022 (and amendments)](https://www.iso.org/standard/27001) | ISO/IEC 27001:2022 (with Amd 1:2024) | Manual: listed for a portal check | 2026-11-09 |
<!-- END GENERATED: sources -->
