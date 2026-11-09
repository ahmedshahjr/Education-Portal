---
name: student-data-privacy
description: Personal data rules for students, minors, parents and staff (data classes, minimisation, no PII in logs or tests, parent/child access, exports, retention, third parties). Use when adding or exposing any personal data field. Part of the mandatory Education Portal compliance rules.
---

# Personal data, students and minors (MUST follow)

Part of the mandatory secure coding and compliance rules. Also follow the [overview skill](../SKILL.md): its rules for AI assistants, exceptions process and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose a compliant alternative.

Framework abbreviations: **800-53** = NIST SP 800-53 Rev 5 (FedRAMP Moderate Rev 5 and GovRAMP baselines); **SOC 2** = AICPA Trust Services Criteria; **ISO** = ISO/IEC 27001:2022 Annex A; **OWASP** = OWASP Top 10:2025.

Most students are children. Treat their data with the highest care.

**Data classes** (label new entities and fields in code comments or attributes):

| Class | Examples | Handling |
|---|---|---|
| Restricted | Student date of birth, home address, health / special-needs / safeguarding notes, government or national IDs, grades and assessments, parent contact details, photos | Need-to-know only, encrypted at rest, never in logs, masked in UI lists, every read in bulk/export audited |
| Confidential | Names, school email, enrolments, class lists, staff records | Tenant/branch scoped, never in logs beyond IDs |
| Internal | School/branch names, course catalogue, configuration | Tenant scoped |
| Public | Marketing pages | None |

| ID | Rule |
|---|---|
| SEC-PII-01 | Data minimisation: collect only fields a listed MVP feature needs. Adding a new Restricted field needs maintainer approval in the PR. |
| SEC-PII-02 | MUST NOT log Restricted or Confidential values. Log IDs (UserId, SchoolId, BranchId, entity ID), not names, emails, DOBs, grades or request bodies. |
| SEC-PII-03 | Real personal data MUST NOT be used in development, test, demo or AI prompts. Use synthetic data (e.g. Bogus) that is obviously fake. Production data is never copied to lower environments. |
| SEC-PII-04 | Parents see only their own linked children; students see only themselves; both are read-only (AGENTS.md fixed rules). Tests MUST prove a parent cannot read another child. |
| SEC-PII-05 | Bulk export, report and print features are Restricted operations: separate right, audited, rate limited. |
| SEC-PII-06 | Deletion and retention: every entity with personal data has a defined retention rule and a deletion path (soft delete + scheduled purge, including backups' expiry). Retention periods are a **TODO** maintainer/legal decision. |
| SEC-PII-07 | Third parties (email, SMS, analytics, AI APIs) MUST NOT receive student data unless the maintainer approved the vendor and a data processing agreement exists. No third-party analytics or tracking scripts in Angular pages used by students. |
| SEC-PII-08 | Which privacy laws apply (e.g. FERPA and COPPA in the US, GDPR/UK GDPR, local education data laws) is a **TODO** legal decision. Do not claim compliance with any of them in product text. |

Supports: 800-53 AC-3, AC-21, AU-3(3), SC-28, SI-12, SI-12(1), SI-12(2), SI-12(3), PM-5(1), PT family where adopted; SOC 2 C1.1, C1.2, P1–P8 (if the Privacy criteria are in scope); ISO 5.34, 8.10, 8.11, 8.12, 8.33.
