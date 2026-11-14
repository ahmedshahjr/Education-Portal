---
name: standard-eu
description: EU compliance rules for the Education Portal (GDPR including children's data, ePrivacy cookies, NIS2, EU AI Act for education, Cyber Resilience Act, accessibility). Use when handling personal data of EU students, parents or staff, adding cookies, analytics, AI features, data exports, hosting regions or subprocessors. Part of the mandatory Education Portal compliance rules.
---

# EU compliance: GDPR and related EU law (MUST follow)

Part of the mandatory secure coding and compliance rules. This skill explains what EU law asks of this codebase and adds EU-specific rules (`SEC-EU-*`). The general rules live in the topic skills; always follow the [overview](../SKILL.md) too.

**Never claim compliance or certification.** Do not write that the product "is GDPR compliant", "GDPR certified" or show an EU-related badge or seal in code, the UI, docs, marketing text, commits or PRs, unless the maintainer confirms it with legal sign-off. Use wording like "designed to support GDPR requirements". GDPR certifications (Art. 42) exist only through accredited schemes.

**Applicability is a legal decision (TODO).** Whether and how each law applies (for example: are we a processor for schools or a controller; do we serve EU schools; which member states) must be confirmed by the maintainer with legal advice. Until then, build so the product *can* meet these rules: they are cheap in code and expensive to retrofit.

## What applies, in short

| Law | Why it matters here | Status for this product |
|---|---|---|
| **GDPR** (Regulation (EU) 2016/679) and UK GDPR | Student, parent and staff personal data; most students are children | Applies when serving EU/UK schools or people. Schools are usually controllers and we are their processor (Art. 28); for our own staff/admin accounts and the master portal we are a controller. **TODO** confirm roles |
| **ePrivacy Directive** (cookie rules, Art. 5(3)) | Cookies and local storage in Angular | Strictly necessary cookies (auth session, antiforgery) need no consent; anything else does |
| **NIS2** (Directive (EU) 2022/2555) | Cybersecurity risk management and incident reporting for covered entities | Education is not a listed sector, and a school SaaS is usually not "cloud computing service" in scope, but member states can extend scope and customers may pass duties down by contract. **TODO** legal check; the topic skills already cover NIS2 Art. 21 style measures |
| **EU AI Act** (Regulation (EU) 2024/1689) | AI used in education is high-risk (Annex III point 3: admission, evaluating learning outcomes, steering learning, assessing education level, monitoring cheating) | No AI features exist. Adding any is a high-risk decision (SEC-EU-12) |
| **Cyber Resilience Act** (Regulation (EU) 2024/2847) | Security requirements for products with digital elements | Pure SaaS is generally outside scope unless it is the remote data processing of a product; reporting duties start 2026-09-11, main duties 2027-12-11. **TODO** legal check |
| **Accessibility** (Web Accessibility Directive for public-sector bodies, European Accessibility Act, EN 301 549 / WCAG 2.1 AA) | Public schools are public-sector bodies and often require accessible procurement | Target WCAG 2.1 AA for the Angular app (SEC-EU-14) |

## GDPR principles and how the code meets them

| GDPR | Requirement | Where the rules are |
|---|---|---|
| Art. 5(1)(a), 12–14 | Lawful, fair, transparent; privacy notices | SEC-EU-05; notices are organisational |
| Art. 5(1)(b)–(c), 25 | Purpose limitation, data minimisation, privacy by design and by default | [student-data-privacy](../student-data-privacy/SKILL.md) SEC-PII-01, SEC-EU-01 |
| Art. 5(1)(d), 16 | Accuracy, rectification | SEC-EU-03 |
| Art. 5(1)(e), 17 | Storage limitation, erasure | SEC-PII-06, SEC-EU-03 |
| Art. 5(1)(f), 32 | Integrity and confidentiality; security of processing | All topic skills, especially [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md), [cryptography-secrets](../cryptography-secrets/SKILL.md), [logging-audit](../logging-audit/SKILL.md) |
| Art. 5(2), 30 | Accountability; records of processing | SEC-EU-06 |
| Art. 8 | Children's consent for information society services: age 16 by default, member states may lower to 13 | SEC-EU-04 |
| Art. 9 | Special category data (health, special educational needs, religion, ethnicity, biometrics) | SEC-EU-02 |
| Art. 15, 20 | Access and portability | SEC-EU-03 |
| Art. 28 | Processor duties: act only on the school's instructions, subprocessors approved | SEC-EU-07, SEC-PII-07 |
| Art. 33–34 | Breach notification within 72 hours | SEC-EU-08, [logging-audit](../logging-audit/SKILL.md) |
| Art. 35 | DPIA for high-risk processing (large-scale children's data) | SEC-EU-09 |
| Art. 44–49 | Transfers outside the EU/EEA | SEC-EU-10 |

## EU-specific rules

| ID | Rule |
|---|---|
| SEC-EU-01 | **Privacy by default.** New features ship with the least data and the narrowest visibility switched on. Optional personal fields are off/empty by default, and sharing beyond the branch is opt-in by the school. |
| SEC-EU-02 | **Special category data** (health, special educational needs, safeguarding, religion, ethnicity, biometrics, photos used for identification) MUST NOT be added without maintainer approval, a documented lawful basis and Art. 9 condition, and a DPIA. When approved: Restricted class, separate role right, encrypted at rest, every read audited. No biometrics. |
| SEC-EU-03 | **Data subject rights must be possible per person and per school:** export a person's data in a machine-readable format (JSON/CSV) for access and portability; correct it; erase or anonymise it (including linked records, files and search indexes, with backups expiring on schedule); restrict processing (freeze a record). Build these as audited, staff-only use cases scoped by tenant; the school (controller) decides, we execute. Design new entities so these operations stay possible (no personal data in immutable keys, logs or URLs). |
| SEC-EU-04 | **Children.** Never collect data directly from students for purposes the school did not instruct. Any feature that relies on consent (for example optional newsletters or third-party integrations) MUST get it from the parent/guardian when the student is under the member-state age (13–16, configurable per school), and store who consented, when, to what, and the wording version. No profiling, behavioural advertising or marketing to students, ever. |
| SEC-EU-05 | **Transparency hooks.** The app MUST be able to show the school's privacy notice and our processor notice (configurable link per school) on sign-in and in the footer. Text is provided by the school/legal, not invented by an AI. |
| SEC-EU-06 | **Record of processing.** When a PR adds a new category of personal data, a new purpose, a new subprocessor or a new transfer, it MUST update the data inventory (`docs/privacy/data-inventory.md`, create it with the first such PR): data fields, data class, purpose, lawful basis (**TODO** if unknown), retention, who can access. |
| SEC-EU-07 | **Subprocessors.** No new external service that receives personal data (email, SMS, storage, logging, AI APIs, analytics, CDNs that see user IPs) without maintainer approval, a data processing agreement, and an entry in the subprocessor list. Schools must be able to be told about changes. |
| SEC-EU-08 | **Breach readiness.** Audit events (SEC-LOG-03/04) MUST be detailed enough to tell which schools, branches and people a security incident touched, so the 72-hour notification can be scoped. Do not delete or shorten audit retention to save space. |
| SEC-EU-09 | **DPIA trigger.** Features that add large-scale or new kinds of processing of children's data, special category data, monitoring, profiling, automated decisions or AI MUST be flagged in the PR as "DPIA needed" and wait for the maintainer before merge. |
| SEC-EU-10 | **Data residency.** EU schools' data (database, backups, files, logs, telemetry) MUST be storable in an EU/EEA region; hosting region is configuration, never hard-coded. No transfer outside the EU/EEA (including support tooling and AI APIs) without an approved transfer mechanism (adequacy decision or SCCs plus a transfer assessment). Hosting is **TODO**. |
| SEC-EU-11 | **Cookies and tracking.** Only strictly necessary cookies/local storage (auth, antiforgery, language, branch selection) without consent. No analytics, tracking pixels, fingerprinting, third-party fonts or scripts that send personal data (including IPs) to third parties without a consent mechanism approved by the maintainer. Self-host fonts and scripts. |
| SEC-EU-12 | **AI features are high-risk.** MUST NOT add AI that grades, scores, ranks, admits, places, predicts performance of, profiles or proctors students without a maintainer decision covering EU AI Act high-risk obligations (risk management, data governance, logging, human oversight, transparency, accuracy). Any AI feature must keep a human in the loop, be switchable off per school (a module), and never send student data to an AI provider that is not an approved subprocessor (SEC-EU-07). Emotion recognition in education is prohibited (AI Act Art. 5). |
| SEC-EU-13 | **Automated decisions.** No decision with legal or similarly significant effect on a student or parent (for example enrolment refusal, grade changes) may be made solely by automated processing (GDPR Art. 22); a staff user must confirm. |
| SEC-EU-14 | **Accessibility.** New Angular screens SHOULD meet WCAG 2.1 AA: semantic HTML, labels for all inputs, keyboard navigation, focus visible, colour contrast, text alternatives. Do not ship screens that cannot be used by keyboard. |
| SEC-EU-15 | **Security incident and vulnerability handling** (NIS2 Art. 21/23 and CRA style): the vulnerability deadlines in SEC-DEP-07 and the incident process in the overview apply to EU customers too; never suppress or delay reporting a known vulnerability or incident to "fix it quietly". |

## Organisational work outside the code

Data processing agreements with schools, privacy notices, records of processing (Art. 30), DPIA, data protection officer if required (Art. 37), EU representative if we are outside the EU (Art. 27), breach notification procedure, transfer assessments, subprocessor list, legal review of NIS2 / CRA / AI Act applicability. See the [overview](../SKILL.md) section 3.

## Rules for any change

1. If the change touches personal data, cookies, external services, hosting or AI, check every SEC-EU rule above and say in the PR which ones apply.
2. Do not invent legal facts (ages, lawful bases, retention periods, applicability). Mark them **TODO** and ask the maintainer.
3. If EU law changes (the twice-monthly sources review lists EU sources), update this skill in a PR.

Supports: GDPR Art. 5, 8, 9, 12–22, 25, 28, 30, 32–35, 44–49; ePrivacy Art. 5(3); NIS2 Art. 21, 23; EU AI Act Art. 5, Annex III(3); CRA Annex I; Web Accessibility Directive / EN 301 549; ISO 5.34; SOC 2 P1–P8.

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [EU GDPR and EDPB guidelines (incl. children's data)](https://www.edpb.europa.eu/our-work-tools/general-guidance/guidelines-recommendations-best-practices_en) | Regulation (EU) 2016/679 and current EDPB guidelines | Manual: listed for a portal check | 2026-11-09 |
| [EU AI Act (Regulation (EU) 2024/1689), implementation timeline and guidance](https://digital-strategy.ec.europa.eu/en/policies/regulatory-framework-ai) | Regulation (EU) 2024/1689 | Manual: listed for a portal check | 2026-11-09 |
| [NIS2 Directive and national transposition](https://digital-strategy.ec.europa.eu/en/policies/nis2-directive) | Directive (EU) 2022/2555 | Manual: listed for a portal check | 2026-11-09 |
| [EU Cyber Resilience Act](https://digital-strategy.ec.europa.eu/en/policies/cyber-resilience-act) | Regulation (EU) 2024/2847 | Manual: listed for a portal check | 2026-11-09 |
<!-- END GENERATED: sources -->
