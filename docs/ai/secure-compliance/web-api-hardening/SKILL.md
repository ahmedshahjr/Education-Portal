---
name: web-api-hardening
description: Web and API hardening rules (TLS/HSTS, CORS, CSRF/antiforgery, security headers, OpenAPI/Scalar exposure, ProblemDetails, rate limiting, host filtering). Use when changing Program.cs, middleware, endpoints or Angular hosting. Part of the mandatory Education Portal compliance rules.
---

# Web and API hardening (MUST follow)

Part of the mandatory secure coding and compliance rules. Also follow the [overview skill](../SKILL.md): its rules for AI assistants, exceptions process and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose a compliant alternative.

Framework abbreviations: **800-53** = NIST SP 800-53 Rev 5 (FedRAMP Moderate Rev 5 and GovRAMP baselines); **SOC 2** = AICPA Trust Services Criteria; **ISO** = ISO/IEC 27001:2022 Annex A; **OWASP** = OWASP Top 10:2025.

| ID | Rule |
|---|---|
| SEC-WEB-01 | HTTPS only: TLS 1.2 minimum (prefer 1.3), HTTP → HTTPS redirect, HSTS (at least 1 year, `includeSubDomains`) outside Development. |
| SEC-WEB-02 | CORS: the Angular app is served same-origin, so outside Development no CORS policy is needed. MUST NOT use `AllowAnyOrigin` with credentials or in any non-Development environment. If cross-origin access is ever needed, use an explicit allow-list. |
| SEC-WEB-03 | CSRF: because auth uses cookies, every state-changing endpoint (POST/PUT/PATCH/DELETE) MUST be protected by antiforgery (ASP.NET Core antiforgery with Angular's `XSRF-TOKEN` / `X-XSRF-TOKEN` header). GET MUST NOT change state. |
| SEC-WEB-04 | Security headers on every response: `Content-Security-Policy` (no `unsafe-inline` scripts, no `unsafe-eval`, `frame-ancestors 'none'`), `X-Content-Type-Options: nosniff`, `Referrer-Policy: strict-origin-when-cross-origin` or stricter, `Permissions-Policy` limiting unused features. |
| SEC-WEB-05 | OpenAPI document and Scalar UI MUST be disabled or require master-tenant authentication outside Development. |
| SEC-WEB-06 | Errors return RFC 9457 ProblemDetails with a correlation ID only. MUST NOT return stack traces, exception messages from the framework, SQL, file paths or other tenants' data outside Development. |
| SEC-WEB-07 | Rate limiting (ASP.NET Core rate limiter) on sign-in, password reset, invite and other expensive or enumerable endpoints, partitioned by IP and by account/tenant. Request body size limits set explicitly. |
| SEC-WEB-08 | `AllowedHosts` / host filtering MUST match the tenant domain table resolution; never trust `X-Forwarded-Host` unless the reverse proxy is configured as a known proxy. |
| SEC-WEB-09 | Remove or disable unused endpoints, features and sample code (least functionality). |

Supports: 800-53 SC-5, SC-7, SC-8, SC-8(1), SC-23, CM-6, CM-7, SI-11; SOC 2 CC6.6, CC6.7; ISO 8.20, 8.21, 8.26; OWASP A02, A10.
