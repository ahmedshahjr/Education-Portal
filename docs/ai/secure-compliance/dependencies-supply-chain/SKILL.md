---
name: dependencies-supply-chain
description: Dependency and supply-chain rules (Central Package Management, NuGet audit, npm audit, new package justification, licences, end-of-life, vulnerability fix deadlines, CI hardening, SBOM). Use when adding or updating packages, containers or CI. Part of the mandatory Education Portal compliance rules.
---

# Dependencies and supply chain (MUST follow)

Part of the mandatory secure coding and compliance rules. Also follow the [overview skill](../SKILL.md): its rules for AI assistants, exceptions process and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose a compliant alternative.

Framework abbreviations: **800-53** = NIST SP 800-53 Rev 5 (FedRAMP Moderate Rev 5 and GovRAMP baselines); **SOC 2** = AICPA Trust Services Criteria; **ISO** = ISO/IEC 27001:2022 Annex A; **OWASP** = OWASP Top 10:2025.

| ID | Rule |
|---|---|
| SEC-DEP-01 | NuGet versions only in `Directory.Packages.props` (Central Package Management, transitive pinning on). npm versions pinned via the committed `package-lock.json`; install with `npm ci` in CI. |
| SEC-DEP-02 | NuGet audit stays on and warnings stay errors: a package with a known vulnerability fails the build. MUST NOT add `NoWarn` for `NU1901`–`NU1904`. Check with `dotnet list package --vulnerable --include-transitive`. |
| SEC-DEP-03 | Angular: `npm audit --audit-level=high` must be clean before merge, or the PR documents the exception ([overview](../SKILL.md) section 4). |
| SEC-DEP-04 | New package: justify it in the PR (why, maintainer/popularity, licence, last release date). Prefer packages already in the solution or in the .NET base library. No packages from unknown feeds; only nuget.org and npmjs.org unless approved. |
| SEC-DEP-05 | Licences: only permissive licences (MIT, Apache-2.0, BSD, similar) without approval. No GPL/AGPL in shipped code without maintainer approval. |
| SEC-DEP-06 | No unsupported/end-of-life runtimes, frameworks or packages. Track .NET, Angular, Node and PostgreSQL support dates. |
| SEC-DEP-07 | Remediate vulnerabilities in production within: Critical/High 30 days, Moderate 90 days, Low 180 days (FedRAMP ConMon timelines). Faster when a fix is available and the risk is exploited. |
| SEC-DEP-08 | CI (when it exists, **TODO**) MUST: pin GitHub Actions to commit SHAs, use least-privilege `GITHUB_TOKEN` permissions, run build + tests + dependency audit + SAST (e.g. CodeQL) + secret scanning, and produce an SBOM (CycloneDX or SPDX) for releases. Container images are built from minimal, pinned base images and scanned. |

Supports: 800-53 RA-5, SI-2, SA-22, CM-8, SR-3, SR-4, SR-11, SA-10, SI-7; SOC 2 CC7.1, CC8.1, CC9.2; ISO 5.19–5.22, 8.8, 8.19, 8.30; OWASP A03, A08.
