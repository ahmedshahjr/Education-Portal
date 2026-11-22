---
name: standard-fedramp-moderate
description: What FedRAMP 20x Class C (Moderate) requires of this codebase (its Key Security Indicators, mapped to NIST SP 800-53 Rev 5 controls), the values we set for it, and which topic skills implement each control. Use when working on anything security-related or preparing FedRAMP evidence. Part of the mandatory Education Portal compliance rules.
---

# FedRAMP 20x Moderate (Class C) (MUST follow)

Part of the mandatory secure coding and compliance rules. This skill explains what the standard asks of this codebase and points to the topic skills that hold the actual rules. Always follow the [overview](../SKILL.md) too.

**Never claim certification.** Do not write that the product "is FedRAMP authorized" or show its badge or logo in code, the UI, docs, marketing text, commits or PRs until the maintainer has the authorization, report or certificate in hand and says so. Badges and logos are controlled by the owning body and come only after an independent assessment. Use wording like "designed to support FedRAMP 20x Class C (Moderate) key security indicators".

## What it is

FedRAMP is the US federal government's security authorization program for cloud services. FedRAMP 20x is its current model: providers set their own security goals, show that their security decisions work, and give agencies continuous evidence. Class C is the Moderate level. The Class C rules are 46 Key Security Indicators (KSIs) in 10 groups. Each KSI lists the NIST SP 800-53 Rev 5 controls it covers, so the control table below keeps the links to the topic skills. Rev 5 is being retired: FedRAMP's target date for the last new Rev 5 certifications is 11 June 2027, and it expects existing Rev 5 certifications to end no later than about 2029 (NTC-0013). A provider does not pursue Rev 5 and 20x for the same offering. Authorization needs a Certification Package Overview and a Security Decision Record, which replace most of the System Security Plan, an independent assessment by a FedRAMP Recognized service (3PAO is the older term), an authorization decision, and continuous monitoring afterwards. The 20x page is in a phased rollout and some of its dates are estimates, so check [fedramp.gov/20x](https://www.fedramp.gov/20x/) for the current phase rather than relying on this text.

## FedRAMP 20x values we set

Under 20x the provider sets these values and records them in its Security Decision Record. FedRAMP removes most of the assigned Rev 5 parameter values (NTC-0013), so the values below are our own.

| Control | Parameter we apply | Rule |
|---|---|---|
| AC-7 | Lock after no more than 3 failed sign-ins in 15 minutes, for at least 30 minutes | SEC-AUTHN-05 |
| AC-11 / AC-12 | 15-minute idle timeout for staff and master users | SEC-AUTHN-07 |
| IA-2(1), IA-2(2) | MFA for privileged and staff accounts; phishing-resistant for master tenant | SEC-AUTHN-04 |
| IA-5(1) | Authenticators per NIST SP 800-63, with phishing-resistant MFA where passwordless is not feasible (20x KSI-IAM-APM; maintainer to review the scope) | SEC-AUTHN-03 |
| SC-8, SC-13 | TLS everywhere; FIPS 140-validated cryptographic modules in deployed environments (20x Class C: SHOULD use validated modules and MUST document them; we keep the stricter rule) | SEC-CRYPTO-02/03 |
| RA-5, SI-2 | Fix vulnerabilities within the 20x window for each finding's Potential Agency Impact rating (PAIN 2 to 5) | SEC-DEP-07 |
| AU-11 | 20x sets no retention period; OMB M-26-14 rescinds M-21-31 (exact period **TODO**) | SEC-LOG-06 |

## FedRAMP-specific engineering duties

- **Authorization boundary:** every component, external service and data flow must be known. Adding a new external service, region, or data store changes the boundary: it needs maintainer approval (SEC-AI-05) and, once authorized, a significant change request.
- **Hosting:** must be FedRAMP-authorized cloud services (e.g. an authorized Azure region and services). Hosting is **TODO**.
- **Inventory:** keep packages and images traceable (Central Package Management, SBOM) for CM-8 and SR controls.
- **Continuous monitoring:** FedRAMP 20x Class C asks for an Ongoing Certification Report every 3 months and a quarterly review (CCM-OCR-AVL, CCM-QTR-MTG). Our monthly vulnerability scans are team practice, and every open finding has a plan of action. Code fixes follow SEC-DEP-07 windows.

## Control mapping (code-level controls)

Controls not listed (for example AT, PE, PS, PL, CP, IR, MA, MP families and most of CA and PM) are organisational; see the [overview](../SKILL.md) section 3. The 20x column lists the Key Security Indicators whose rules name each control (FedRAMP Consolidated Rules 2026, version 2026.11.02.01). A dash means no 20x indicator covers the control (for example PM-5(1), SI-12 enhancements and CA-8); we follow those as good practice.

| Control | Title | Where the rules are | 20x KSI |
|---|---|---|---|
| AC-2 | Account Management | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) | KSI-IAM-APM, KSI-IAM-JIT, KSI-IAM-SNU, KSI-IAM-SUS |
| AC-3 | Access Enforcement | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md), [student-data-privacy](../student-data-privacy/SKILL.md) | KSI-IAM-APM, KSI-IAM-ELP, KSI-IAM-JIT |
| AC-4 | Information Flow Enforcement | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) | KSI-IAM-ELP, KSI-IAM-JIT, KSI-IAM-SNU |
| AC-5 | Separation of Duties | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) | KSI-IAM-JIT, KSI-PIY-RIS, KSI-PIY-RSD |
| AC-6 | Least Privilege | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md), [configuration-environments](../configuration-environments/SKILL.md) | KSI-IAM-ELP, KSI-IAM-JIT |
| AC-6(9) | Log Use of Privileged Functions | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) | KSI-IAM-JIT, KSI-MLA-LET, KSI-MLA-RVL |
| AC-6(10) | Prohibit Non-privileged Users from Executing Privileged Functions | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) | KSI-IAM-JIT |
| AC-7 | Unsuccessful Logon Attempts | [authentication](../authentication/SKILL.md) | KSI-IAM-JIT, KSI-IAM-SUS |
| AC-11 | Device Lock | [authentication](../authentication/SKILL.md) | KSI-SVC-SIN |
| AC-12 | Session Termination | [authentication](../authentication/SKILL.md) | KSI-CNA-ULN, KSI-IAM-ELP |
| AC-21 | Information Sharing | [student-data-privacy](../student-data-privacy/SKILL.md) | KSI-IAM-ELP, KSI-IAM-JIT, KSI-SVC-SIN |
| AU-2 | Event Logging | [logging-audit](../logging-audit/SKILL.md) | KSI-CMT-LMC, KSI-MLA-LET, KSI-MLA-OSM, KSI-MLA-RVL |
| AU-3 | Content of Audit Records | [logging-audit](../logging-audit/SKILL.md) | KSI-MLA-OSM |
| AU-3(3) | Limit Personally Identifiable Information Elements | [student-data-privacy](../student-data-privacy/SKILL.md) | KSI-PIY-RSD |
| AU-6 | Audit Record Review, Analysis, and Reporting | [logging-audit](../logging-audit/SKILL.md) | KSI-MLA-RVL |
| AU-8 | Time Stamps | [logging-audit](../logging-audit/SKILL.md) | KSI-MLA-OSM |
| AU-9 | Protection of Audit Information | [logging-audit](../logging-audit/SKILL.md) | KSI-MLA-OSM |
| AU-11 | Audit Record Retention | [logging-audit](../logging-audit/SKILL.md) | KSI-MLA-OSM |
| AU-12 | Audit Record Generation | [logging-audit](../logging-audit/SKILL.md) | KSI-MLA-LET |
| CA-2 | Control Assessments | [security-testing](../security-testing/SKILL.md) | KSI-PIY-RIS |
| CA-8 | Penetration Testing | [security-testing](../security-testing/SKILL.md) | — |
| CM-2 | Baseline Configuration | [configuration-environments](../configuration-environments/SKILL.md) | KSI-CMT-RMV, KSI-CNA-DFP, KSI-CNA-IBP, KSI-MLA-EVC, KSI-SVC-ACM |
| CM-3 | Configuration Change Control | [change-management](../change-management/SKILL.md), [configuration-environments](../configuration-environments/SKILL.md), [overview](../SKILL.md) | KSI-CMT-LMC, KSI-CMT-RMV, KSI-CMT-RVP, KSI-CMT-VTD |
| CM-4 | Impact Analyses | [change-management](../change-management/SKILL.md) | — |
| CM-5 | Access Restrictions for Change | [change-management](../change-management/SKILL.md) | KSI-CMT-RMV, KSI-CMT-RVP, KSI-IAM-JIT |
| CM-6 | Configuration Settings | [configuration-environments](../configuration-environments/SKILL.md), [cryptography-secrets](../cryptography-secrets/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) | KSI-CMT-LMC, KSI-CMT-RMV, KSI-MLA-EVC, KSI-SVC-ACM |
| CM-7 | Least Functionality | [configuration-environments](../configuration-environments/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) | KSI-CMT-RMV, KSI-IAM-JIT |
| CM-8 | System Component Inventory | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) | KSI-PIY-GIV |
| IA-2 | Identification and Authentication (Organizational Users) | [authentication](../authentication/SKILL.md) | KSI-IAM-APM, KSI-IAM-ELP |
| IA-2(1) | Multi-factor Authentication to Privileged Accounts | [authentication](../authentication/SKILL.md) | KSI-IAM-APM |
| IA-2(2) | Multi-factor Authentication to Non-privileged Accounts | [authentication](../authentication/SKILL.md) | KSI-IAM-APM |
| IA-5 | Authenticator Management | [authentication](../authentication/SKILL.md) | KSI-IAM-APM |
| IA-5(1) | Password-based Authentication | [authentication](../authentication/SKILL.md) | KSI-IAM-APM |
| IA-5(7) | No Embedded Unencrypted Static Authenticators | [cryptography-secrets](../cryptography-secrets/SKILL.md) | KSI-PIY-RSD |
| IA-8 | Identification and Authentication (Non-organizational Users) | [authentication](../authentication/SKILL.md) | KSI-IAM-APM |
| PM-5(1) | Inventory of Personally Identifiable Information | [student-data-privacy](../student-data-privacy/SKILL.md) | — |
| RA-5 | Vulnerability Monitoring and Scanning | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md), [security-testing](../security-testing/SKILL.md) | KSI-SCR-MON |
| SA-8 | Security and Privacy Engineering Principles | [overview](../SKILL.md) | KSI-PIY-RSD |
| SA-10 | Developer Configuration Management | [change-management](../change-management/SKILL.md), [configuration-environments](../configuration-environments/SKILL.md), [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md), [overview](../SKILL.md) | KSI-SCR-MIT |
| SA-11 | Developer Testing and Evaluation | [change-management](../change-management/SKILL.md), [overview](../SKILL.md), [security-testing](../security-testing/SKILL.md) | KSI-SCR-MIT |
| SA-11(1) | Static Code Analysis | [security-testing](../security-testing/SKILL.md) | KSI-SCR-MIT |
| SA-22 | Unsupported System Components | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) | KSI-SCR-MIT |
| SC-4 | Information in Shared System Resources | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) | KSI-CNA-ULN, KSI-IAM-ELP, KSI-PIY-RSD, KSI-SVC-PRR, KSI-SVC-SIN |
| SC-5 | Denial-of-service Protection | [web-api-hardening](../web-api-hardening/SKILL.md) | KSI-CNA-RVP |
| SC-7 | Boundary Protection | [web-api-hardening](../web-api-hardening/SKILL.md) | KSI-CNA-ULN, KSI-SVC-EIS |
| SC-8 | Transmission Confidentiality and Integrity | [cryptography-secrets](../cryptography-secrets/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) | KSI-CNA-MAT, KSI-CNA-ULN, KSI-SVC-SIN |
| SC-8(1) | Cryptographic Protection (in transit) | [web-api-hardening](../web-api-hardening/SKILL.md) | KSI-SVC-SIN |
| SC-12 | Cryptographic Key Establishment and Management | [cryptography-secrets](../cryptography-secrets/SKILL.md) | KSI-SVC-ASM |
| SC-13 | Cryptographic Protection | [cryptography-secrets](../cryptography-secrets/SKILL.md) | KSI-SVC-SIN, KSI-SVC-VRI |
| SC-18 | Mobile Code | [input-validation](../input-validation/SKILL.md) | KSI-PIY-RSD, KSI-SCR-MIT |
| SC-23 | Session Authenticity | [authentication](../authentication/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) | KSI-IAM-APM, KSI-IAM-ELP, KSI-IAM-JIT, KSI-SVC-SIN, KSI-SVC-VCM, KSI-SVC-VRI |
| SC-24 | Fail in Known State | [error-handling](../error-handling/SKILL.md) | KSI-CNA-EIS |
| SC-28 | Protection of Information at Rest | [cryptography-secrets](../cryptography-secrets/SKILL.md), [student-data-privacy](../student-data-privacy/SKILL.md) | KSI-SVC-SIN |
| SI-2 | Flaw Remediation | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) | KSI-CMT-VTD |
| SI-4 | System Monitoring | [logging-audit](../logging-audit/SKILL.md) | KSI-IAM-SUS, KSI-MLA-RVL, KSI-SVC-EIS |
| SI-7 | Software, Firmware, and Information Integrity | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) | KSI-CNA-EIS, KSI-SVC-VRI |
| SI-10 | Information Input Validation | [error-handling](../error-handling/SKILL.md), [input-validation](../input-validation/SKILL.md) | KSI-CNA-MAT, KSI-CNA-RNT, KSI-PIY-RSD |
| SI-11 | Error Handling | [error-handling](../error-handling/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) | KSI-CNA-MAT, KSI-MLA-ALA, KSI-PIY-RSD |
| SI-12 | Information Management and Retention | [student-data-privacy](../student-data-privacy/SKILL.md) | — |
| SI-12(1) | Limit Personally Identifiable Information Elements | [student-data-privacy](../student-data-privacy/SKILL.md) | — |
| SI-12(2) | Minimize PII in Testing, Training, and Research | [student-data-privacy](../student-data-privacy/SKILL.md) | — |
| SI-12(3) | Information Disposal | [student-data-privacy](../student-data-privacy/SKILL.md) | KSI-SVC-RUD |
| SI-15 | Information Output Filtering | [input-validation](../input-validation/SKILL.md) | — |
| SR-3 | Supply Chain Controls and Processes | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) | KSI-SCR-MIT, KSI-SCR-MON |
| SR-4 | Provenance | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) | — |
| SR-11 | Component Authenticity | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) | KSI-SVC-VRI |
| PT family | Personally Identifiable Information Processing and Transparency | [student-data-privacy](../student-data-privacy/SKILL.md) | — |

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
| [NIST SP 800-53 Rev 5 catalog (OSCAL)](https://csrc.nist.gov/projects/risk-management/sp800-53-controls/release-search) | SP 800-53 Rev 5.2.0 (released 27 Aug 2025; OSCAL catalog 5.2.0, last changed 2026-10-22) | Automatic: new commits in `usnistgov/oscal-content/nist.gov/SP800-53/rev5` | 2026-11-22 |
| [FedRAMP 20x Class C (Moderate) rules: Key Security Indicators and Consolidated Rules](https://www.fedramp.gov/20x/) | FedRAMP Consolidated Rules 2026, version 2026.11.02.01 (20x Class C) | Automatic: new commits in `FedRAMP/rules/fedramp-consolidated-rules.json` | 2026-11-22 |
| [NIST SP 800-63 Digital Identity Guidelines (Rev 4)](https://pages.nist.gov/800-63-4/) | SP 800-63-4 (Rev 4) | Automatic: new commits in `usnistgov/800-63-4` | 2026-11-22 |
| [OMB M-26-14 logging requirements (rescinds OMB M-21-31; referenced by FedRAMP AU-11)](https://www.whitehouse.gov/omb/information-for-agencies/memoranda/) | OMB M-26-14 (22 May 2026), rescinds OMB M-21-31 (2021) | Manual: listed for a portal check | 2026-11-22 |
<!-- END GENERATED: sources -->
