---
name: standard-fedramp-moderate
description: What FedRAMP Moderate Rev 5 (NIST SP 800-53 Rev 5 Moderate baseline) requires of this codebase, the FedRAMP-specific parameters we apply, and which topic skills implement each control. Use when working on anything security-related or preparing FedRAMP evidence. Part of the mandatory Education Portal compliance rules.
---

# FedRAMP Moderate (Rev 5) (MUST follow)

Part of the mandatory secure coding and compliance rules. This skill explains what the standard asks of this codebase and points to the topic skills that hold the actual rules. Always follow the [overview](../SKILL.md) too.

**Never claim certification.** Do not write that the product "is FedRAMP authorized" or show its badge or logo in code, the UI, docs, marketing text, commits or PRs until the maintainer has the authorization, report or certificate in hand and says so. Badges and logos are controlled by the owning body and come only after an independent assessment. Use wording like "designed to support FedRAMP Moderate controls".

## What it is

FedRAMP is the US federal government's security authorization program for cloud services. The Moderate baseline is the NIST SP 800-53 Rev 5 Moderate control set plus FedRAMP-specific parameters and requirements. Authorization needs a System Security Plan (SSP), an independent assessment by an accredited third-party assessment organization (3PAO), an authorization decision, and continuous monitoring afterwards. Program rules change (for example the FedRAMP 20x initiative), so check [fedramp.gov](https://www.fedramp.gov/) and the sources review issue rather than memory.

## FedRAMP parameters these rules already use

| Control | Parameter we apply | Rule |
|---|---|---|
| AC-7 | Lock after no more than 3 failed sign-ins in 15 minutes, for at least 30 minutes | SEC-AUTHN-05 |
| AC-11 / AC-12 | 15-minute idle timeout for staff and master users | SEC-AUTHN-07 |
| IA-2(1), IA-2(2) | MFA for privileged and staff accounts; phishing-resistant for master tenant | SEC-AUTHN-04 |
| IA-5(1) | Password length and breached-password checks per NIST SP 800-63B | SEC-AUTHN-03 |
| SC-8, SC-13 | TLS everywhere; FIPS 140-validated cryptographic modules in deployed environments | SEC-CRYPTO-02/03 |
| RA-5, SI-2 | Fix vulnerabilities: High/Critical 30 days, Moderate 90 days, Low 180 days | SEC-DEP-07 |
| AU-11 | Retention per OMB M-21-31 (exact period **TODO**) | SEC-LOG-06 |

## FedRAMP-specific engineering duties

- **Authorization boundary:** every component, external service and data flow must be known. Adding a new external service, region, or data store changes the boundary: it needs maintainer approval (SEC-AI-05) and, once authorized, a significant change request.
- **Hosting:** must be FedRAMP-authorized cloud services (e.g. an authorized Azure region and services). Hosting is **TODO**.
- **Inventory:** keep packages and images traceable (Central Package Management, SBOM) for CM-8 and SR controls.
- **Continuous monitoring:** monthly vulnerability scans and a POA&M for every open finding; code fixes follow SEC-DEP-07 deadlines.

## Control mapping (code-level controls)

Controls not listed (for example AT, PE, PS, PL, CP, IR, MA, MP families and most of CA and PM) are organisational; see the [overview](../SKILL.md) section 3. Some mapped controls (e.g. SC-24, PM-5(1) and SI-12 enhancements) may sit outside the FedRAMP Moderate baseline; we follow them as good practice.

| Control | Title | Where the rules are |
|---|---|---|
| AC-2 | Account Management | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) |
| AC-3 | Access Enforcement | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md), [student-data-privacy](../student-data-privacy/SKILL.md) |
| AC-4 | Information Flow Enforcement | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) |
| AC-5 | Separation of Duties | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) |
| AC-6 | Least Privilege | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md), [configuration-environments](../configuration-environments/SKILL.md) |
| AC-6(9) | Log Use of Privileged Functions | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) |
| AC-6(10) | Prohibit Non-privileged Users from Executing Privileged Functions | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) |
| AC-7 | Unsuccessful Logon Attempts | [authentication](../authentication/SKILL.md) |
| AC-11 | Device Lock | [authentication](../authentication/SKILL.md) |
| AC-12 | Session Termination | [authentication](../authentication/SKILL.md) |
| AC-21 | Information Sharing | [student-data-privacy](../student-data-privacy/SKILL.md) |
| AU-2 | Event Logging | [logging-audit](../logging-audit/SKILL.md) |
| AU-3 | Content of Audit Records | [logging-audit](../logging-audit/SKILL.md) |
| AU-3(3) | Limit Personally Identifiable Information Elements | [student-data-privacy](../student-data-privacy/SKILL.md) |
| AU-6 | Audit Record Review, Analysis, and Reporting | [logging-audit](../logging-audit/SKILL.md) |
| AU-8 | Time Stamps | [logging-audit](../logging-audit/SKILL.md) |
| AU-9 | Protection of Audit Information | [logging-audit](../logging-audit/SKILL.md) |
| AU-11 | Audit Record Retention | [logging-audit](../logging-audit/SKILL.md) |
| AU-12 | Audit Record Generation | [logging-audit](../logging-audit/SKILL.md) |
| CA-2 | Control Assessments | [security-testing](../security-testing/SKILL.md) |
| CA-8 | Penetration Testing | [security-testing](../security-testing/SKILL.md) |
| CM-2 | Baseline Configuration | [configuration-environments](../configuration-environments/SKILL.md) |
| CM-3 | Configuration Change Control | [change-management](../change-management/SKILL.md), [configuration-environments](../configuration-environments/SKILL.md), [overview](../SKILL.md) |
| CM-4 | Impact Analyses | [change-management](../change-management/SKILL.md) |
| CM-5 | Access Restrictions for Change | [change-management](../change-management/SKILL.md) |
| CM-6 | Configuration Settings | [configuration-environments](../configuration-environments/SKILL.md), [cryptography-secrets](../cryptography-secrets/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) |
| CM-7 | Least Functionality | [configuration-environments](../configuration-environments/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) |
| CM-8 | System Component Inventory | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) |
| IA-2 | Identification and Authentication (Organizational Users) | [authentication](../authentication/SKILL.md) |
| IA-2(1) | Multi-factor Authentication to Privileged Accounts | [authentication](../authentication/SKILL.md) |
| IA-2(2) | Multi-factor Authentication to Non-privileged Accounts | [authentication](../authentication/SKILL.md) |
| IA-5 | Authenticator Management | [authentication](../authentication/SKILL.md) |
| IA-5(1) | Password-based Authentication | [authentication](../authentication/SKILL.md) |
| IA-5(7) | No Embedded Unencrypted Static Authenticators | [cryptography-secrets](../cryptography-secrets/SKILL.md) |
| IA-8 | Identification and Authentication (Non-organizational Users) | [authentication](../authentication/SKILL.md) |
| PM-5(1) | Inventory of Personally Identifiable Information | [student-data-privacy](../student-data-privacy/SKILL.md) |
| RA-5 | Vulnerability Monitoring and Scanning | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md), [security-testing](../security-testing/SKILL.md) |
| SA-8 | Security and Privacy Engineering Principles | [overview](../SKILL.md) |
| SA-10 | Developer Configuration Management | [change-management](../change-management/SKILL.md), [configuration-environments](../configuration-environments/SKILL.md), [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md), [overview](../SKILL.md) |
| SA-11 | Developer Testing and Evaluation | [change-management](../change-management/SKILL.md), [overview](../SKILL.md), [security-testing](../security-testing/SKILL.md) |
| SA-11(1) | Static Code Analysis | [security-testing](../security-testing/SKILL.md) |
| SA-22 | Unsupported System Components | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) |
| SC-4 | Information in Shared System Resources | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) |
| SC-5 | Denial-of-service Protection | [web-api-hardening](../web-api-hardening/SKILL.md) |
| SC-7 | Boundary Protection | [web-api-hardening](../web-api-hardening/SKILL.md) |
| SC-8 | Transmission Confidentiality and Integrity | [cryptography-secrets](../cryptography-secrets/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) |
| SC-8(1) | Cryptographic Protection (in transit) | [web-api-hardening](../web-api-hardening/SKILL.md) |
| SC-12 | Cryptographic Key Establishment and Management | [cryptography-secrets](../cryptography-secrets/SKILL.md) |
| SC-13 | Cryptographic Protection | [cryptography-secrets](../cryptography-secrets/SKILL.md) |
| SC-18 | Mobile Code | [input-validation](../input-validation/SKILL.md) |
| SC-23 | Session Authenticity | [authentication](../authentication/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) |
| SC-24 | Fail in Known State | [error-handling](../error-handling/SKILL.md) |
| SC-28 | Protection of Information at Rest | [cryptography-secrets](../cryptography-secrets/SKILL.md), [student-data-privacy](../student-data-privacy/SKILL.md) |
| SI-2 | Flaw Remediation | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) |
| SI-4 | System Monitoring | [logging-audit](../logging-audit/SKILL.md) |
| SI-7 | Software, Firmware, and Information Integrity | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) |
| SI-10 | Information Input Validation | [error-handling](../error-handling/SKILL.md), [input-validation](../input-validation/SKILL.md) |
| SI-11 | Error Handling | [error-handling](../error-handling/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) |
| SI-12 | Information Management and Retention | [student-data-privacy](../student-data-privacy/SKILL.md) |
| SI-12(1) | Limit Personally Identifiable Information Elements | [student-data-privacy](../student-data-privacy/SKILL.md) |
| SI-12(2) | Minimize PII in Testing, Training, and Research | [student-data-privacy](../student-data-privacy/SKILL.md) |
| SI-12(3) | Information Disposal | [student-data-privacy](../student-data-privacy/SKILL.md) |
| SI-15 | Information Output Filtering | [input-validation](../input-validation/SKILL.md) |
| SR-3 | Supply Chain Controls and Processes | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) |
| SR-4 | Provenance | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) |
| SR-11 | Component Authenticity | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) |
| PT family | Personally Identifiable Information Processing and Transparency | [student-data-privacy](../student-data-privacy/SKILL.md) |

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
| [NIST SP 800-53 Rev 5 catalog (OSCAL)](https://csrc.nist.gov/projects/risk-management/sp800-53-controls/release-search) | SP 800-53 Rev 5 (latest release in the OSCAL catalog) | Automatic: new commits in `usnistgov/oscal-content/nist.gov/SP800-53/rev5` | 2026-11-09 |
| [FedRAMP Rev 5 baselines (Moderate)](https://www.fedramp.gov/) | FedRAMP Rev 5 Moderate baseline | Automatic: new commits in `GSA/fedramp-automation/dist/content/rev5/baselines` | 2026-11-09 |
| [NIST SP 800-63 Digital Identity Guidelines (Rev 4)](https://pages.nist.gov/800-63-4/) | SP 800-63-4 (Rev 4) | Automatic: new commits in `usnistgov/800-63-4` | 2026-11-09 |
| [OMB M-21-31 logging requirements (referenced by FedRAMP AU-11)](https://www.whitehouse.gov/omb/information-for-agencies/memoranda/) | OMB M-21-31 (2021) | Manual: listed for a portal check | 2026-11-09 |
<!-- END GENERATED: sources -->
