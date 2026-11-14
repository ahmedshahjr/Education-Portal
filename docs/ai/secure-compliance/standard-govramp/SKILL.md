---
name: standard-govramp
description: What GovRAMP (formerly StateRAMP, for US state and local government) requires of this codebase and how it reuses the FedRAMP Moderate / NIST SP 800-53 Rev 5 rules. Use when preparing for state or local government customers or GovRAMP evidence. Part of the mandatory Education Portal compliance rules.
---

# GovRAMP (MUST follow)

Part of the mandatory secure coding and compliance rules. This skill explains what the standard asks of this codebase and points to the topic skills that hold the actual rules. Always follow the [overview](../SKILL.md) too.

**Never claim certification.** Do not write that the product "is GovRAMP authorized or verified" or show its badge or logo in code, the UI, docs, marketing text, commits or PRs until the maintainer has the authorization, report or certificate in hand and says so. Badges and logos are controlled by the owning body and come only after an independent assessment. Use wording like "designed to support GovRAMP requirements".

## What it is

GovRAMP (formerly StateRAMP) is a non-profit program that verifies the security of cloud products sold to US state and local governments, including education bodies. Its security baselines are built on NIST SP 800-53 Rev 5, aligned with FedRAMP, and it uses independent assessors and continuous monitoring. Products with a FedRAMP authorization can often reuse that work. Check [govramp.org](https://govramp.org/) for current levels, the security snapshot process and reciprocity rules; they change.

## What this means for the code

- The code-level controls are the same NIST SP 800-53 Rev 5 controls as FedRAMP Moderate. Follow the [standard-fedramp-moderate](../standard-fedramp-moderate/SKILL.md) skill and its mapping table; it applies to GovRAMP as written.
- Target impact level (Low, Moderate, High) is a **TODO** maintainer decision. These rules are written for Moderate.
- State and local customers (schools, districts) often add their own data privacy agreements for student data. Those can add requirements on top of [student-data-privacy](../student-data-privacy/SKILL.md); record them there when the maintainer shares them.

## Organisational work outside the code

Registration with GovRAMP, an independent assessment, a security plan, continuous monitoring reports and a named security contact. See the [overview](../SKILL.md) section 3.

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
| [GovRAMP (formerly StateRAMP) program and baselines](https://govramp.org/) | GovRAMP program and security baselines (NIST SP 800-53 Rev 5 based) | Manual: listed for a portal check | 2026-11-09 |
<!-- END GENERATED: sources -->
