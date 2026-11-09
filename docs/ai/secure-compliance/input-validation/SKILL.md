---
name: input-validation
description: Input validation and injection rules (FluentValidation, EF Core/SQL, Angular XSS, uploads, CSV injection, deserialization, redirects). Use when code accepts or renders any user input. Part of the mandatory Education Portal compliance rules.
---

# Input validation, output encoding and injection (MUST follow)

Part of the mandatory secure coding and compliance rules. Also follow the [overview skill](../SKILL.md): its rules for AI assistants, exceptions process and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose a compliant alternative.

Framework abbreviations: **800-53** = NIST SP 800-53 Rev 5 (FedRAMP Moderate Rev 5 and GovRAMP baselines); **SOC 2** = AICPA Trust Services Criteria; **ISO** = ISO/IEC 27001:2022 Annex A; **OWASP** = OWASP Top 10:2025.

| ID | Rule |
|---|---|
| SEC-INPUT-01 | Every command and query with input has a FluentValidation validator run by the MediatR pipeline: type, length, range, format and allowed values. Validate on the server even if Angular validates too. |
| SEC-INPUT-02 | Data access only through EF Core LINQ or parameterized SQL (`FromSql` / `ExecuteSql` with interpolation, which parameterizes). MUST NOT build SQL with string concatenation or `FromSqlRaw`/`ExecuteSqlRaw` with user input. |
| SEC-INPUT-03 | Angular: rely on Angular's built-in escaping. MUST NOT use `bypassSecurityTrust*`, `innerHTML` with untrusted data, `eval`, `new Function` or dynamic script injection. |
| SEC-INPUT-04 | File uploads (if added): size limit, allow-listed content types checked by content not just extension, random server-side file names, stored outside the web root, malware scan before use, served with `Content-Disposition: attachment` unless an image is re-encoded. |
| SEC-INPUT-05 | CSV/Excel exports MUST neutralize formula injection (prefix cells starting with `=`, `+`, `-`, `@` with `'`). |
| SEC-INPUT-06 | Deserialization: System.Text.Json only; MUST NOT enable polymorphic type handling from client input (no `TypeNameHandling`-style features). |
| SEC-INPUT-07 | Redirects (e.g. `returnUrl`) MUST be local-only (`Url.IsLocalUrl` / equivalent). |

Supports: 800-53 SI-10, SI-15, SC-18; SOC 2 CC6.1, CC7.1; ISO 8.26, 8.28; OWASP A05, A08.
