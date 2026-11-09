---
name: standard-owasp-top10
description: How each OWASP Top 10:2025 risk applies to this .NET + Angular multi-tenant app, with the concrete rules that prevent it. Use when writing or reviewing any endpoint, handler, Angular component, dependency or config change. Part of the mandatory Education Portal compliance rules.
---

# OWASP Top 10:2025 (MUST follow)

Part of the mandatory secure coding and compliance rules. This skill explains what the standard asks of this codebase and points to the topic skills that hold the actual rules. Always follow the [overview](../SKILL.md) too.

**Never claim certification.** Do not write that the product "is OWASP certified" or "OWASP compliant", or show an OWASP badge or logo in code, the UI, docs, marketing text, commits or PRs until the maintainer has the authorization, report or certificate in hand and says so. Badges and logos are controlled by the owning body and come only after an independent assessment. Use wording like "designed to mitigate the OWASP Top 10 risks".

## What it is

The OWASP Top 10 is an awareness list of the most critical web application security risks, updated every few years (current edition: 2025). It is **not a standard and there is no certification or badge**. For verifiable requirements, use the OWASP Application Security Verification Standard (ASVS); targeting ASVS Level 2 is a sensible goal for an app holding children's data (adoption is a **TODO** maintainer decision).

## The ten risks in this codebase

| Risk | How it shows up here | Prevent with |
|---|---|---|
| A01 Broken Access Control | Reading another school's or branch's students by changing an ID or host; missing module/right check; SSRF | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) (SEC-AUTHZ-01..11) |
| A02 Security Misconfiguration | `AllowAnyOrigin` CORS, Scalar UI in production, missing headers, dev seed data in prod | [web-api-hardening](../web-api-hardening/SKILL.md), [configuration-environments](../configuration-environments/SKILL.md) |
| A03 Software Supply Chain Failures | Vulnerable or malicious NuGet/npm packages, unpinned CI actions | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) |
| A04 Cryptographic Failures | Weak algorithms, no TLS to PostgreSQL, unprotected Data Protection keys | [cryptography-secrets](../cryptography-secrets/SKILL.md) |
| A05 Injection | `FromSqlRaw` with input, Angular `innerHTML`/`bypassSecurityTrust*`, CSV formula injection | [input-validation](../input-validation/SKILL.md) |
| A06 Insecure Design | Tenant taken from the request body, permissions only in Angular, no deny-by-default | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md), [security-testing](../security-testing/SKILL.md) |
| A07 Authentication Failures | No lockout or MFA, weak passwords, cookies shared across school domains | [authentication](../authentication/SKILL.md) |
| A08 Software or Data Integrity Failures | Unsafe deserialization, unverified packages or build artefacts | [input-validation](../input-validation/SKILL.md), [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) |
| A09 Security Logging and Alerting Failures | Sign-in failures or cross-tenant attempts not audited; PII in logs | [logging-audit](../logging-audit/SKILL.md) |
| A10 Mishandling of Exceptional Conditions | Stack traces to clients, failing open when tenant lookup fails, swallowed exceptions | [error-handling](../error-handling/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) |

## Review checklist for every PR

- Could a user of `school1.com` reach `school2.com` data, or a branch they are not assigned to? (A01)
- Is anything enabled outside Development that should not be? (A02)
- New or updated package: audited and justified? (A03)
- Any crypto, secret or connection string touched? (A04)
- Any user input reaching SQL, HTML, CSV, logs or redirects? (A05)
- Is the security decision made on the server, deny by default? (A06)
- Sign-in, session or cookie behaviour changed? (A07)
- Any deserialization of client data or new build step? (A08)
- Security-relevant action without an audit event? (A09)
- What happens when this code throws or a dependency is down? (A10)

## Category mapping

| Category | Title | Where the rules are |
|---|---|---|
| A01 | Broken Access Control | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) |
| A02 | Security Misconfiguration | [cryptography-secrets](../cryptography-secrets/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) |
| A03 | Software Supply Chain Failures | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md) |
| A04 | Cryptographic Failures | [cryptography-secrets](../cryptography-secrets/SKILL.md) |
| A05 | Injection | [input-validation](../input-validation/SKILL.md) |
| A06 | Insecure Design | [authorization-tenant-isolation](../authorization-tenant-isolation/SKILL.md) |
| A07 | Authentication Failures | [authentication](../authentication/SKILL.md) |
| A08 | Software or Data Integrity Failures | [dependencies-supply-chain](../dependencies-supply-chain/SKILL.md), [input-validation](../input-validation/SKILL.md) |
| A09 | Security Logging and Alerting Failures | [logging-audit](../logging-audit/SKILL.md) |
| A10 | Mishandling of Exceptional Conditions | [error-handling](../error-handling/SKILL.md), [web-api-hardening](../web-api-hardening/SKILL.md) |

## Rules for any change

1. Before coding, find which rows in the mapping table below your change touches and load those topic skills.
2. Changes must keep the evidence an assessor will ask for: the PR (with the security checklist), the review approval, the CI run, and tests. Never bypass the PR flow (SEC-CHG-01).
3. Do not invent control parameters, scope or legal applicability. If the standard needs a value the maintainer has not set (marked **TODO**), ask.
4. If the standard updates (the twice-monthly sources review opens an issue), update this skill and the affected topic skills in one PR.
