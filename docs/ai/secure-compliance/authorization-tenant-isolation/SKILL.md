---
name: authorization-tenant-isolation
description: Authorization and multi-tenant isolation rules (modules, role rights, branches, SchoolId/BranchId query filters, IDOR, SSRF). Use for any new use case, endpoint, entity, query, cache, export or background job. Part of the mandatory Education Portal compliance rules.
---

# Authorization and tenant isolation (MUST follow)

Part of the mandatory secure coding and compliance rules. Also follow the [overview skill](../SKILL.md): its rules for AI assistants, exceptions process and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose a compliant alternative.

Framework abbreviations: **800-53** = NIST SP 800-53 Rev 5 (FedRAMP Moderate Rev 5 and GovRAMP baselines); **SOC 2** = AICPA Trust Services Criteria; **ISO** = ISO/IEC 27001:2022 Annex A; **OWASP** = OWASP Top 10:2025.

| ID | Rule |
|---|---|
| SEC-AUTHZ-01 | Access = modules enabled for the school ∩ role rights per module (View/Create/Edit/Delete) ∩ branches assigned to the user, plus the fixed rules in AGENTS.md section 2. Enforce it **on the server in the Application layer** (MediatR authorization behaviour / attributes). Angular hiding is only UX. |
| SEC-AUTHZ-02 | Deny by default: every MediatR request MUST declare its module and required right. A request without an authorization declaration MUST fail a test (add an architecture test that scans for it). |
| SEC-AUTHZ-03 | Every tenant-owned entity has `SchoolId` (and `BranchId` if branch-scoped) and is covered by EF Core global query filters fed by the current-user/tenant service. MUST NOT use `IgnoreQueryFilters()` except in master-tenant code paths that are explicitly authorized and audited. |
| SEC-AUTHZ-04 | `SchoolId` comes only from the host-resolved tenant, never from the request body, query string or a client header. `BranchId` may come from the route but MUST be checked against the user's assigned branches on every request. |
| SEC-AUTHZ-05 | No mass assignment: commands are explicit DTOs; never bind request data straight onto entities. Fields like `SchoolId`, `BranchId`, `CreatedBy`, role and permission fields are never client-settable unless the use case is exactly about changing them and is authorized for it. |
| SEC-AUTHZ-06 | A record that belongs to another school or branch MUST return 404 (not 403), so IDs cannot be used to probe other tenants. A disabled module or missing right returns 403. |
| SEC-AUTHZ-07 | Least privilege: new roles start with no rights. Master-tenant (cross-school) operations are separate use cases with their own authorization and audit, never a flag on a tenant use case. |
| SEC-AUTHZ-08 | Raw SQL, Dapper, background jobs, exports, reports and caches MUST apply the same tenant and branch scoping as the query filters. Cache keys MUST include `SchoolId` (and `BranchId` where relevant). |
| SEC-AUTHZ-09 | Every new use case gets functional tests proving: unauthenticated → 401; another school → 404; another branch → 404; disabled module → 403; missing role right → 403. (Also required by AGENTS.md.) |
| SEC-AUTHZ-10 | Unknown host → 404 and nothing served. Never fall back to a default tenant. |
| SEC-AUTHZ-11 | Server-side requests to URLs supplied by users (webhooks, imports, avatars) MUST use an allow-list of hosts and block private/link-local/metadata addresses (SSRF). |

Supports: 800-53 AC-2, AC-3, AC-4, AC-5, AC-6, AC-6(9), AC-6(10), SC-4; SOC 2 CC6.1, CC6.3, C1.1; ISO 5.15, 5.18, 8.2, 8.3; OWASP A01, A06.
