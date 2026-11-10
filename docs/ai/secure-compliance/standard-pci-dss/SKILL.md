---
name: standard-pci-dss
description: PCI DSS v4.0.1 rules for taking tuition or fee payments by card through a certified processor (e.g. Stripe) so card data never reaches our servers and scope stays at SAQ A level (scope reduction, no sensitive authentication data, tokenization, TLS, access control, logging, vulnerability management, annual review). Use when adding or changing anything about payments, fees, refunds, payment pages, processor keys or webhooks. Part of the mandatory Education Portal compliance rules.
---

# PCI DSS: card payments for fees (MUST follow)

Part of the mandatory secure coding and compliance rules. This skill explains what the Payment Card Industry Data Security Standard asks of this codebase and adds payment-specific rules (`SEC-PCI-*`). The general rules live in the topic skills; always follow the [overview](../SKILL.md) too.

**Version tracked:** PCI DSS **v4.0.1** (published June 2024 by the PCI Security Standards Council; v4.0 was retired on 2024-12-31 and the v4.0 "future-dated" requirements became mandatory on 2025-03-31). The SAQs used are the v4.0.1 versions, including the **January 2025 revision of SAQ A**. If a newer version is published, see "Keeping this skill current" below.

**Never claim compliance or certification.** Do not write that the product or a school "is PCI compliant", "PCI certified" or show a PCI badge or logo in code, the UI, docs, marketing text, commits or PRs. PCI DSS compliance is validated per merchant or service provider, every year, by a Self-Assessment Questionnaire (SAQ) or a QSA Report on Compliance plus an Attestation of Compliance (AOC) given to the acquirer or card brands. There is no product certification. Use wording like "card payments are handled by a PCI DSS validated processor; card data never reaches the portal".

**Status for this product:** fees and payments are **out of scope for the MVP** (AGENTS.md section 8). No payment code exists. Before the first payment PR, the maintainer must decide the merchant model and processor (SEC-PCI-01) and add a Fees/Payments module to the module list in AGENTS.md section 2. Until then, any payment work stops at that decision.

## What it is, and the scope we aim for

PCI DSS protects **account data**: cardholder data (primary account number (PAN), cardholder name, expiry date, service code) and **sensitive authentication data** (SAD: full track data, card verification code (CVV2/CVC2/CID), PINs and PIN blocks). Every system that stores, processes or transmits account data, or can affect its security, is in scope (the cardholder data environment, CDE).

The cheapest and safest scope is to **never let account data touch our systems**:

| Approach | Who sees card numbers | Typical SAQ | Allowed here? |
|---|---|---|---|
| **Redirect** to the processor's hosted payment page (e.g. Stripe Checkout, hosted invoice page) | Only the processor | **SAQ A** | **Yes, the default** |
| Processor fields **fully inside processor iframes** on our page (e.g. Stripe Payment Element / Elements), all payment fields served by the processor | Only the processor, but our page hosts the iframe | SAQ A (if the SAQ A eligibility criteria are met) | Only with maintainer approval and an ADR |
| Our own JavaScript form that posts card data to the processor ("direct post", JS that builds the form) | Our page code can read it | SAQ A-EP | **No** |
| Card data reaches our API, database, logs or queues | Us | SAQ D / full assessment | **No, never** |

SAQ A (v4.0.1, January 2025 revision) applies to card-not-present merchants that fully outsource all account data functions to PCI DSS compliant processors and do not electronically store, process or transmit account data. The revision removed requirements 6.4.3 and 11.6.1 from SAQ A and replaced them with an eligibility criterion that the merchant confirms its site is not susceptible to attacks from scripts that could affect its e-commerce system(s). SEC-PCI-08 is how we meet that criterion. Which SAQ the merchant actually files is decided by the merchant with its acquirer (**TODO**, see SEC-PCI-01).

## Requirement areas and where the rules are

| PCI DSS v4.0.1 | Requirement | Where the rules are |
|---|---|---|
| Scoping (intro), 12.5.2 | Define and confirm scope; keep the CDE out of our systems | SEC-PCI-01, SEC-PCI-02, SEC-PCI-14 |
| 1, 2 | Network security controls and secure configuration | SEC-PCI-08; [web-api-hardening](../web-api-hardening/SKILL.md), [configuration-environments](../configuration-environments/SKILL.md) |
| 3.2, 3.3.1 | Keep stored account data to a minimum; never store SAD after authorization | SEC-PCI-02, SEC-PCI-03 |
| 3.4.1, 3.5.1 | Mask PAN when displayed (BIN and last four at most); render stored PAN unreadable (tokenization, truncation) | SEC-PCI-04 |
| 4.2.1 | Strong cryptography for account data over open, public networks | SEC-PCI-07; [cryptography-secrets](../cryptography-secrets/SKILL.md) SEC-CRYPTO-03 |
| 5 | Protect against malicious software | Hosting (**TODO**), organisational |
| 6.2, 6.3.3, 6.4.1–6.4.2 | Secure development, patching, protecting public web apps | SEC-PCI-12; [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md), [input-validation](../input-validation/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) |
| 6.4.3, 11.6.1 | Payment page script inventory, authorization and integrity; change and tamper detection on payment pages | SEC-PCI-08 (SAQ A eligibility; required in full if we ever fall into SAQ A-EP) |
| 7, 8 | Need-to-know access, unique IDs, MFA, password rules | SEC-PCI-10; [authentication](../authentication/SKILL.md), [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) |
| 10 | Log and monitor access; retain audit logs 12 months, 3 months immediately available (10.5.1) | SEC-PCI-11; [logging-audit](../logging-audit/SKILL.md) |
| 11.3 | Internal and external vulnerability scans (external by a PCI SSC Approved Scanning Vendor (ASV) where the SAQ requires it) | SEC-PCI-12; [security-testing](../security-testing/SKILL.md) |
| 12.8 | Manage third-party service providers (processor AOC, written agreement, responsibility matrix) | SEC-PCI-13 |
| 12.3.1, 12.4, 12.5.2, annual SAQ/AOC | Targeted risk analyses, executive responsibility, annual scope confirmation and validation | SEC-PCI-14, organisational |

## PCI-specific rules

| ID | Rule |
|---|---|
| SEC-PCI-01 | **Decide the merchant model first (TODO, maintainer).** Before any payment code, record in an ADR: who the merchant of record is (each school with its own processor account, e.g. Stripe Connect connected accounts, or the platform), which processor, which integration (redirect by default) and the expected SAQ. If the platform accepts payments **on behalf of** schools or could affect the security of their card payments, it may be a **service provider** with a much larger scope (SAQ D for service providers or a QSA assessment); that needs the maintainer and the acquirer/processor to confirm. Processor account IDs are tenant-owned (`SchoolId`) and covered by the query filters. |
| SEC-PCI-02 | **No account data in our systems, ever.** Card numbers, CVV/CVC, expiry dates, track data and PINs MUST NOT be accepted, stored, logged, cached, queued, sent in emails, put in URLs or held in Angular state. No DTO, entity, validator, form control or column may be named or used for them (`CardNumber`, `Cvv`, `Cvc`, `Expiry` for a card, `TrackData`, ...); `docs/ai/compliance-guard.py` blocks the common names. Payment UI is the processor's hosted page or iframe only (SAQ A rows in the table above). Building our own card form is forbidden. |
| SEC-PCI-03 | **Sensitive authentication data is never stored (3.3.1).** We never receive it (SEC-PCI-02). If card data is found anywhere we control (a support ticket, a free-text field, a log, a screenshot), treat it as a security incident: purge it, record what happened, and tell the maintainer. Free-text fields on payment-related screens SHOULD warn "do not enter card details". |
| SEC-PCI-04 | **Tokens, not cards.** Store only processor references (customer, payment method, checkout session, payment intent, charge, refund and event IDs) and display-only data the processor returns: card brand and last four digits. Never store a full or partial PAN beyond the last four, and never the expiry with it. Processor references are tied to `SchoolId` (and `BranchId` when fees are per branch) and are never accepted from the client for another tenant (SEC-AUTHZ-04). |
| SEC-PCI-05 | **The server decides what is charged.** Amount, currency, description and the fee/invoice being paid come from our database for the current tenant, never from the browser. Create checkout sessions/payment intents on the server with an idempotency key per fee payment attempt. Payment status changes only from a verified processor event (SEC-PCI-06) or a server-side call to the processor API, never from a browser redirect parameter. |
| SEC-PCI-06 | **Webhooks are verified.** The processor webhook endpoint is the only anonymous payment endpoint (list it in the PR, `compliance-exception` comment, SEC-AUTHN-01). It MUST verify the processor signature with the webhook signing secret (Key Vault), reject missing/invalid signatures and stale timestamps (Stripe's library default tolerance is 5 minutes), process each event ID once (idempotent), resolve the tenant from our stored processor reference rather than the payload, return quickly, and be rate limited. Never log the raw payload (SEC-LOG-02). |
| SEC-PCI-07 | **Transport and keys.** All calls to the processor use TLS 1.2 or higher with certificate validation (4.2.1, SEC-CRYPTO-03). Processor secret keys, restricted keys and webhook secrets live in Key Vault / user-secrets only (SEC-SECRET-01); only the publishable key may reach Angular. Use restricted keys with the minimum permissions the server needs. Rotate keys when someone with access leaves, on suspicion of compromise, and at least yearly (**TODO** confirm interval). |
| SEC-PCI-08 | **Protect the pages that lead to payment** (SAQ A eligibility; 6.4.3 / 11.6.1 if scope ever grows). Pages that redirect to or embed the processor MUST: load no third-party scripts except the processor's own (no analytics, chat widgets, tag managers, A/B tools); have a Content Security Policy whose `script-src`, `frame-src` and `connect-src` allow only our origin and the processor's documented domains; load the processor script from the processor's domain as documented (do not self-host or bundle Stripe.js); keep the redirect target fixed on the server (no open redirect, SEC-INPUT). Keep a list of every script on those pages, with the reason, in the payment ADR; a PR that adds or changes a script there updates the list and is reviewed for payment-page impact. |
| SEC-PCI-09 | **Payment UX does not leak.** Receipts, emails, exports and audit views show only brand, last four, amount, date and processor reference. Refund and payment screens are tenant- and branch-scoped like all other data. |
| SEC-PCI-10 | **Access control (7, 8).** Payment features sit behind their own module and role rights (View payments, Refund, Manage payment settings). Refunds and payment settings require MFA (SEC-AUTHN-04) and are limited to named roles. Accounts on the processor dashboard are individual (no shared logins), MFA-protected and reviewed at least every six months (**TODO** organisational owner). A refund can never exceed the captured amount and is done through the processor API from the server. |
| SEC-PCI-11 | **Audit events (10).** Add security audit events (SEC-LOG-03/04 fields) for: payment initiated, payment succeeded/failed (from webhook), refund requested/completed, webhook signature failures, payment settings or processor account changes, processor key rotation. Audit and security logs for payment events are retained at least **12 months with the last 3 months immediately available** (10.5.1); this is a floor, longer platform retention (SEC-LOG-06) wins. |
| SEC-PCI-12 | **Vulnerability management (6, 11).** Critical and high vulnerabilities in anything serving payment pages or the webhook are fixed within one month (6.3.3) and never later than SEC-DEP-07. External vulnerability scans by an ASV are run quarterly and after significant changes when the merchant's SAQ requires them (**TODO** confirm with acquirer). Payment flows get tests: server-side amount, webhook signature rejection, idempotency, tenant/branch/module denial (SEC-AUTHZ-09). Tests use the processor's test mode and published test card numbers only, never real cards. |
| SEC-PCI-13 | **Processor management (12.8).** The processor is an approved subprocessor (SEC-EU-07, SEC-PII-07). Keep its current PCI DSS AOC on file and check it at least yearly (12.8.4), keep the written agreement (12.8.2) and a responsibility matrix saying which requirements the processor meets and which stay with the merchant (12.8.5). Prefer processors that are PCI DSS Level 1 service providers. |
| SEC-PCI-14 | **Annual review and change control.** Every PR that touches payments states its **PCI scope impact** (none / changes the payment page / changes data flow) in the description. Any change that could move us off SAQ A (new integration type, new scripts on payment pages, any path where card data could reach us) needs maintainer approval and an updated ADR before merge. Once a year, and after any significant change, the maintainer confirms scope (12.5.2), re-files the SAQ and AOC with the acquirer, and re-checks this skill against the current PCI DSS version. |

## Organisational work outside the code

Choosing the processor and merchant model, the merchant agreement with the acquirer, the annual SAQ and AOC, ASV scans if required, the processor's AOC and responsibility matrix, incident response for suspected card data compromise (card brand and acquirer notification), security awareness for staff who handle payments, and a named owner for PCI DSS (12.4). See the [overview](../SKILL.md) section 3.

## Keeping this skill current

PCI SSC publishes no machine-readable feed, so the twice-monthly sources review lists PCI DSS for a manual check (see "Sources and update checks" at the end of this skill). When checking, look in the [document library](https://www.pcisecuritystandards.org/document_library/) for a new PCI DSS version or errata, a new Summary of Changes, revised SAQs (especially SAQ A and its eligibility criteria) and new FAQs, and in the [PCI SSC blog](https://blog.pcisecuritystandards.org/) for announced dates. Apply a change by updating the version line at the top, the requirement table and the SEC-PCI rules in the same PR as `sources.json`.

## Rules for any change

1. If the change touches fees, payments, refunds, payment pages, processor keys or webhooks, check every SEC-PCI rule above and state the PCI scope impact in the PR.
2. Do not invent PCI facts (SAQ type, merchant level, scan duties, retention beyond the floor above, merchant model). Mark them **TODO** and ask the maintainer.
3. If PCI DSS or the SAQs change (the twice-monthly sources review lists PCI SSC), update this skill in a PR.

Supports: PCI DSS v4.0.1 Requirements 3.2, 3.3.1, 3.4.1, 3.5.1, 4.2.1, 6.3.3, 6.4.1–6.4.3, 7, 8.4, 10.2, 10.5.1, 11.3, 11.6.1, 12.5.2, 12.8; SAQ A (v4.0.1, Jan 2025); 800-53 SC-8, SC-28, AC-6, AU-11, SA-9; SOC 2 CC6.1, CC6.7, CC9.2; ISO 5.19–5.22, 8.24; OWASP A01, A02, A05, A08.

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [PCI DSS and SAQs (PCI Security Standards Council)](https://www.pcisecuritystandards.org/document_library/) | PCI DSS v4.0.1 (June 2024); SAQ A v4.0.1 (January 2025 revision) | Manual: listed for a portal check | 2026-11-10 |
<!-- END GENERATED: sources -->
