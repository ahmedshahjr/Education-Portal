---
name: authentication
description: Authentication rules (ASP.NET Core Identity, passwords, MFA, lockout, cookies, sessions, password reset). Use when touching sign-in, accounts, Identity options, cookies or sessions. Part of the mandatory Education Portal compliance rules.
---

# Authentication (ASP.NET Core Identity) (MUST follow)

Part of the mandatory secure coding and compliance rules. Also follow the [overview skill](../SKILL.md): its rules for AI assistants, exceptions process and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose a compliant alternative.

Framework abbreviations: **800-53** = NIST SP 800-53 Rev 5 (FedRAMP Moderate Rev 5 and GovRAMP baselines); **SOC 2** = AICPA Trust Services Criteria; **ISO** = ISO/IEC 27001:2022 Annex A; **OWASP** = OWASP Top 10:2025.

| ID | Rule |
|---|---|
| SEC-AUTHN-01 | Every endpoint requires an authenticated user by default. Anonymous access is allowed only for login, password reset, health probes needed by the platform and static Angular files, and each one MUST be listed in the PR. |
| SEC-AUTHN-02 | Use ASP.NET Core Identity's password hasher. MUST NOT write custom password hashing, custom token formats or custom crypto. |
| SEC-AUTHN-03 | Password policy: minimum length 12 (15 when a password is the only factor, per NIST SP 800-63B Rev 4), maximum at least 64, allow all printable characters and spaces, no forced composition rules and no forced periodic rotation; reject passwords found in a breached/common-password list. |
| SEC-AUTHN-04 | MFA MUST be required for master-tenant users, school owners and any role with Create/Edit/Delete rights or access to Restricted data (see [student-data-privacy](../student-data-privacy/SKILL.md)). Prefer phishing-resistant MFA (passkeys/WebAuthn) for master-tenant (privileged) users. MFA for students and parents is a **TODO** decision. |
| SEC-AUTHN-05 | Account lockout MUST be on: lock after no more than 3 consecutive failed sign-ins within 15 minutes, for at least 30 minutes or until an admin unlocks (FedRAMP Moderate AC-7 parameters). Do not reveal whether the account exists in error messages. |
| SEC-AUTHN-06 | Auth cookies: `Secure`, `HttpOnly`, `SameSite=Lax` or `Strict`, **host-only** (never set the `Domain` attribute) so a session from `school1.com` is never sent to `school2.com`. |
| SEC-AUTHN-07 | Sessions: idle timeout of 15 minutes for staff and master users (SHOULD be configurable per role), absolute lifetime no more than 12 hours, sign-out invalidates the session server-side (rotate the security stamp on password change, role change, MFA change and account disable). |
| SEC-AUTHN-08 | Accounts are created by staff only (no public sign-up). Disabling a user MUST take effect on the next request, not at cookie expiry. |
| SEC-AUTHN-09 | The tenant in the user's claims MUST match the tenant resolved from the request host on every request; mismatch → reject and write a security audit event. |
| SEC-AUTHN-10 | Password reset and invite links: single use, expire in 60 minutes or less, sent only to the address on file, and responses do not reveal whether the address exists. |

Supports: 800-53 IA-2, IA-2(1), IA-2(2), IA-5, IA-5(1), IA-8, AC-7, AC-11, AC-12, SC-23; SOC 2 CC6.1, CC6.2; ISO 5.17, 8.5; OWASP A07.

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [NIST SP 800-63 Digital Identity Guidelines (Rev 4)](https://pages.nist.gov/800-63-4/) | SP 800-63-4 (Rev 4) | Automatic: new commits in `usnistgov/800-63-4` | 2026-11-09 |
<!-- END GENERATED: sources -->
