---
name: cryptography-secrets
description: Cryptography and secrets rules (approved algorithms, FIPS, TLS to PostgreSQL, encryption at rest, Data Protection keys, user-secrets/Key Vault, secret scanning, dev credentials). Use when handling keys, secrets, connection strings or crypto. Part of the mandatory Education Portal compliance rules.
---

# Cryptography and secrets (MUST follow)

Part of the mandatory secure coding and compliance rules. Also follow the [overview skill](../SKILL.md): its rules for AI assistants, exceptions process and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose a compliant alternative.

Framework abbreviations: **800-53** = NIST SP 800-53 Rev 5 (FedRAMP Moderate Rev 5 and GovRAMP baselines); **SOC 2** = AICPA Trust Services Criteria; **ISO** = ISO/IEC 27001:2022 Annex A; **OWASP** = OWASP Top 10:2025.

| ID | Rule |
|---|---|
| SEC-CRYPTO-01 | Use only platform crypto (.NET `System.Security.Cryptography`, ASP.NET Core Data Protection, Identity). Approved algorithms only: AES-GCM (or AES-CBC + HMAC via Data Protection), SHA-256 or stronger, RSA ≥ 2048 / ECDSA P-256 or stronger. MUST NOT use MD5, SHA-1, DES/3DES, RC4, ECB mode or home-made crypto for security purposes. |
| SEC-CRYPTO-02 | For FedRAMP/GovRAMP deployments, crypto MUST run on FIPS 140-validated modules (e.g. OS/OpenSSL in FIPS mode on the hosting platform). This is a hosting decision (**TODO**); code MUST NOT block it (no algorithms that are unavailable in FIPS mode). |
| SEC-CRYPTO-03 | Data in transit: TLS everywhere, including app → PostgreSQL (`SSL Mode=VerifyFull` or `Require` with certificate validation outside Development) and service-to-service calls. |
| SEC-CRYPTO-04 | Data at rest: database, backups and file storage encrypted (platform encryption with customer- or platform-managed keys in Key Vault). Restricted fields (see [student-data-privacy](../student-data-privacy/SKILL.md)) SHOULD also get column-level protection when the maintainer approves the design. |
| SEC-CRYPTO-05 | ASP.NET Core Data Protection keys MUST be persisted to shared storage and protected with Key Vault outside Development, so cookies survive restarts and keys are not left on container disks. |
| SEC-SECRET-01 | No secrets in the repo, ever: not in `appsettings*.json`, `launchSettings.json`, Angular `environment*.ts`, Dockerfiles, tests or docs. Local: `dotnet user-secrets` / Aspire parameters (including the local HashiCorp Vault). Deployed: the environment's secret store chosen by `Secrets:Provider` (Azure Key Vault, AWS Secrets Manager or Google Secret Manager, ADR-006) with the platform's workload identity (managed identity, IAM role, service account). |
| SEC-SECRET-02 | Anything shipped to Angular is public. MUST NOT put API keys, secrets or internal URLs into the frontend bundle. |
| SEC-SECRET-03 | Secret scanning with push protection MUST be on for the repo. A leaked secret is rotated immediately, then removed from history, and reported as a security incident. |
| SEC-SECRET-04 | Default/dev credentials (e.g. the seeded `administrator@localhost` user and the local Postgres password) are Development-only and MUST NOT exist in any other environment. Seeding of default users MUST be guarded by `IsDevelopment()`. |

Supports: 800-53 SC-8, SC-12, SC-13, SC-28, IA-5(7), CM-6; SOC 2 CC6.1, CC6.7, C1.1; ISO 5.17, 8.24; OWASP A04, A02.
