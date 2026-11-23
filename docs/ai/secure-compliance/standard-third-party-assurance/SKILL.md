---
name: standard-third-party-assurance
description: Checklist of third-party audit reports, certificates and published standards to request or track for vendors and suppliers (SOC 1 and SOC 2 reports, PCI DSS AoC, penetration tests, iBeta biometric presentation attack reports, ETSI eIDAS conformity, and ISO certificates such as 27001, 22301, 27017, 27018, 31000, 9001, 29115 and 29003). Use when choosing or onboarding a supplier, handling identity proofing or biometrics, or collecting assurance evidence.
---

# Third-party audits and security standards (MUST follow)

Part of the mandatory secure coding and compliance rules. This skill is the checklist of third-party audit reports, certificates and standards that the Education Portal tracks. It says what each item evidences, what we must ask for, and what the update check watches. Always follow the [overview](../SKILL.md) too.

**Never claim certification.** These items are evidence about a supplier, or a published standard we design against. They do not make the Education Portal certified. Do not write that the product "is SOC 2", "ISO 27001" or "PCI DSS certified", and do not show a vendor's badge or logo as ours, until the maintainer has the report or certificate in hand and says so. Use wording like "designed to support ISO/IEC 27001:2022 controls".

## Kinds of item

- **Report to request**: an attestation that is issued for a period or a test run (SOC 1, SOC 2, penetration test, iBeta test report, PCI DSS AoC). Ask the supplier for the current report, its scope and its period, and file the date it covers.
- **Certificate to request**: a certificate issued by an accredited certification body after an audit (ISO certificates, ETSI conformity). Check the certification body, the scope, the standard edition and the expiry date.
- **Published standard**: a document we design against. The rules live in the topic skills; the standard is only tracked for updates or retirement.

Where a standard is a certification rather than a published standard, the update check tracks the report or certificate to request (its `valid_until` date), not a standard edition.

## Checklist

Each row is tracked in [sources.json](../sources.json) with its `kind`, `status`, `tracked_version` and `valid_until` (where it applies).

| # | Item | Kind | What it gives us | What we do |
|---|---|---|---|---|
| 1 | SOC 1 Type 1 report (financial system controls of a supplier, point in time) | Report to request | Evidence that a supplier's controls over financial reporting are designed as described | Request the report for any supplier that handles payments or fee data; check the date and scope |
| 2 | SOC 2 Type 2 report (independent security assessment over a period) | Report to request | Evidence that a supplier's security controls operated over a period (Trust Services Criteria) | Request the latest period; check it covers our data flows and the controls we rely on |
| 3 | ETSI EN 319 and 119 series (conformity assessment under eIDAS) | Certificate to request | Evidence that a trust service provider meets the eIDAS trust-service standards | Needed only if we use qualified electronic signatures or seals; check the certification body and scope |
| 4 | PCI DSS v4.0.1 and Attestation of Compliance (AoC) | Report to request (annual) | Evidence that a card processor is PCI DSS compliant for its scope | Request the AoC and the responsibility matrix; see [standard-pci-dss](../standard-pci-dss/SKILL.md) |
| 5 | Penetration testing report | Report to request | Evidence of regular independent security testing | Request the latest report for suppliers that host our data; track findings to closure |
| 6 | iBeta Level 1 report (biometric presentation attack detection) | Report to request | Evidence that a biometric liveness check resists basic presentation attacks (ISO/IEC 30107-3) | Needed only if biometrics are used; request it with the supplier's test scope |
| 7 | iBeta Level 2 report (biometric presentation attack detection) | Report to request | Same as level 1 at a higher attack level | As for level 1 |
| 8 | iBeta Level 3 report (biometric presentation attack detection) | Report to request | Same as level 1 at the highest attack level tracked here | As for level 1 |
| 9 | ISO/IEC 27001:2022 certificate (information security management system, supplier) | Certificate to request | Evidence that a supplier runs an ISMS audited by an accredited body | Request the certificate and its scope; our own ISMS is in [standard-iso27001](../standard-iso27001/SKILL.md) |
| 10 | ISO 22301:2019 certificate (business continuity management) | Certificate to request | Evidence of tested continuity arrangements for a supplier | Request the certificate; check it covers the services we use |
| 11 | ISO/IEC 27017 certificate (cloud security controls) | Certificate to request | Evidence of cloud-specific security controls for a cloud supplier | Request for cloud hosting and SaaS suppliers; check the edition |
| 12 | ISO/IEC 27018 certificate (protection of personal data in the cloud) | Certificate to request | Evidence of PII protection controls for a cloud processor | Request for suppliers that process personal data of students, parents or staff; see [student-data-privacy](../student-data-privacy/SKILL.md) |
| 13 | ISO 31000:2018 (risk management framework) | Certificate or report to request | Evidence of a supplier's risk management practice | Request when a supplier is assessed for risk; not a replacement for our own risk register |
| 14 | ISO 9001:2015 certificate (quality management system) | Certificate to request | Evidence of a supplier's quality management system | Request for suppliers of services we depend on; check the edition, since ISO 9001 is being revised |
| 15 | ISO/IEC 29115:2013 (entity authentication assurance framework) | Published standard | Vocabulary for the assurance levels of an authentication process | Used when we describe authentication assurance; see [authentication](../authentication/SKILL.md) and NIST SP 800-63-4 |
| 16 | ISO/IEC 29003:2018 (identity proofing) | Published standard | Requirements for identity proofing and verification | Used for any identity verification of students, parents or staff; personal data rules apply (see [standard-eu](../standard-eu/SKILL.md)) |

## Rules that apply to every item

- **Scope and period.** A report or certificate counts only for the scope and period it states. Record the scope, the dates and the certification body in the supplier file, not in the repository.
- **Expiry.** An expired report or certificate is retired evidence. Request the successor before the date passes; the update check flags expiry and renewals due within 60 days.
- **Supplier data.** Do not store reports, certificates or supplier credentials in the repository. Keep the documents in the compliance evidence store that the maintainer names.
- **Biometric and identity data.** Biometric data is a special category under GDPR Article 9. Do not collect it unless the maintainer has decided the purpose, the lawful basis and the supplier (items 6 to 8 and 16 apply then).
- **Retired standards.** If a standard is withdrawn or superseded, update this skill, its `status` and `superseded_by` in sources.json, and the topic skills that cite it, in one PR into `development`.

## Sources and update checks

Items 1 to 16 are checked by `.github/workflows/compliance-sources-review.yml` on the 1st and 16th of each month. The workflow reads `status`, `superseded_by` and `valid_until` from sources.json and reports:

- Retired, superseded, withdrawn or expired items (for `status` other than `current`, and `valid_until` in the past).
- Reports and certificates that expire within 60 days (renewal due).
- Every other item for a manual portal check, with its kind and tracked version.

The workflow cannot read most ISO, vendor or iBeta portals automatically, so those stay manual checks. The workflow never edits this skill.

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [SOC 1 Type 1 report (supplier financial system controls, point in time)](https://www.aicpa-cima.com/topic/audit-assurance/audit-and-assurance-greater-than-soc-1) | Report to request; date and scope of the supplier's current report not recorded | Manual: listed for a portal check | 2026-11-23 |
| [SOC 2 Type 2 report (supplier independent security assessment)](https://www.aicpa-cima.com/topic/audit-assurance/audit-and-assurance-greater-than-soc-2) | Report to request; period of the supplier's current report not recorded | Manual: listed for a portal check | 2026-11-23 |
| [ETSI EN 319 and 119 series, conformity assessment under eIDAS](https://www.etsi.org/standards) | Certificate to request (only if qualified trust services are used) | Manual: listed for a portal check | 2026-11-23 |
| [PCI DSS v4.0.1 and Attestation of Compliance (AoC)](https://www.pcisecuritystandards.org/) | PCI DSS v4.0.1 (June 2024); AoC to request, annual | Manual: listed for a portal check | 2026-11-23 |
| [Penetration testing report (supplier)](https://owasp.org/www-project-web-security-testing-guide/) | Report to request; date of the supplier's latest test not recorded | Manual: listed for a portal check | 2026-11-23 |
| [iBeta Level 1 report (biometric presentation attack detection)](https://www.ibeta.com/) | Report to request; tested against ISO/IEC 30107-3:2023 | Manual: listed for a portal check | 2026-11-23 |
| [iBeta Level 2 report (biometric presentation attack detection)](https://www.ibeta.com/) | Report to request; tested against ISO/IEC 30107-3:2023 | Manual: listed for a portal check | 2026-11-23 |
| [iBeta Level 3 report (biometric presentation attack detection)](https://www.ibeta.com/) | Report to request; tested against ISO/IEC 30107-3:2023 | Manual: listed for a portal check | 2026-11-23 |
| [ISO/IEC 27001:2022 certificate (supplier ISMS)](https://www.iso.org/standard/27001) | ISO/IEC 27001:2022 (with Amd 1:2024); supplier certificate to request | Manual: listed for a portal check | 2026-11-23 |
| [ISO 22301:2019 certificate (business continuity)](https://www.iso.org/standard/75106.html) | ISO 22301:2019; a revision is at committee draft stage (not yet published) | Manual: listed for a portal check | 2026-11-23 |
| [ISO/IEC 27017 certificate (cloud security controls)](https://www.iso.org/standard/43757.html) | ISO/IEC 27017:2015; a 2026 edition is reported in search results, not confirmed | Manual: listed for a portal check | 2026-11-23 |
| [ISO/IEC 27018 certificate (PII protection in public clouds)](https://www.iso.org/standard/76559.html) | ISO/IEC 27018:2019; a 2025 edition is reported in search results, not confirmed | Manual: listed for a portal check | 2026-11-23 |
| [ISO 31000:2018 certificate or report (risk management)](https://www.iso.org/standard/65694.html) | ISO 31000:2018; not verified online | Manual: listed for a portal check | 2026-11-23 |
| [ISO 9001:2015 certificate (quality management)](https://www.iso.org/standard/62085.html) | ISO 9001:2015; revision ISO 9001:2026 reported as published, not confirmed | Manual: listed for a portal check | 2026-11-23 |
| [ISO/IEC 29115:2013 (entity authentication assurance framework)](https://www.iso.org/standard/45138.html) | ISO/IEC 29115:2013; amendment ISO/IEC 29115:2013/DAmd 1 at draft stage | Manual: listed for a portal check | 2026-11-23 |
| [ISO/IEC 29003:2018 (identity proofing and verification)](https://www.iso.org/standard/62290.html) | ISO/IEC 29003:2018; not verified online | Manual: listed for a portal check | 2026-11-23 |
<!-- END GENERATED: sources -->
