# SOP: running and changing rights, tenants and change history

Decisions: [ADR-021](../decisions/ADR-021-Tenant-Above-School.md), [ADR-022](../decisions/ADR-022-Role-Rights-Per-Campus.md),
[ADR-023](../decisions/ADR-023-Readable-Change-History.md), [ADR-024](../decisions/ADR-024-Users-And-Roles-Screen-And-Create-Rule.md).
Requirements and design: `docs/specs/rights-tenants-history-calendar.md` and `...-design.md`.

## Add a use case on school data

1. Mark it `[Authorize(Module = ModuleCode.X, Right = AccessRight.Y)]` (the module is required with a right).
2. If it touches campus data, call `ICampusAccess.EnsureCampusAsync(campusId, module, right)` or filter with
   `GetAllowedCampusIdsAsync(module, right)`; `null` means every campus.
3. Write the three denial tests: another school, another campus, a module that is off. Add a rights test per role.

## Change a role's rights

Edit in the Users and roles screen. "All campuses" applies to every campus, including new ones. Rights in a module that is off
are kept and hidden. Administrators and the master tenant are not checked against roles.

## Add a school to a tenant, or change a domain

Only from the master tenant's domain (`MasterTenant = true`). A school with a domain signs in on that domain only; setting one
ends master-domain sign-in for its users, so warn before saving. The tenant administrator is the administrator
created at registration.

## Audit a new entity

Add its type to `ChangeAuditInterceptor.AuditedTypes`, give it a module and a sentence resource (en, ar, ur), and mark
personal-data properties as value-hidden. Never use `ExecuteUpdate` on audited tables without writing entries.

## Migrations and checks

Generate migrations with `dotnet ef`; never merge them by hand (AGENTS.md section 7, rule 10). Before a PR run
`dotnet build`, `dotnet test`, the Angular build and tests, `readme-guard.py` and `architecture-diagram-guard.py`.
