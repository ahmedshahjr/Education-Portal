# ADR-012: Module Enforcement and School Roles

## Status

Accepted.

## Date

2026-11-13

## Context

Phase 4 of eight of the module-selling work. [ADR-011](ADR-011-Subscriptions-And-Tenant-Modules.md) stores what a
school bought as `TenantModule` rows, but access still read the master's per-school switches (`SchoolModules`).
Enforcement must follow the subscription, answer fast, and fail closed.

## Decision

- **Entitlements** (`ModuleEntitlementLoader`): the active core modules, plus the paid modules whose `TenantModule` is
  Active, inside `ValidFrom <= now < ValidTo`, with the module itself active. A grant limited to a branch counts only
  for that branch; a check that names no branch fails closed for branch-only grants. The master tenant is never limited.
- **`IModuleAccess.HasModuleAsync(schoolId, code, branchId)`** and `GetEnabledCodesAsync(schoolId)` replace
  `IsEnabledAsync`. `AuthorizationBehaviour` refuses a request to a module that is off with `ModuleNotEnabledException`,
  a `ForbiddenAccessException` that the API answers as 403 ProblemDetails with `code = MODULE_NOT_ENABLED` and the
  module code. Data is never deleted when a module turns off.
- **Cache** (`IModuleEntitlementStore`, in memory, 5 minutes per school): dropped for one school when its subscription
  is created and for every school when a module is changed or deleted. It is per server instance, so another instance
  may serve a stale answer for up to five minutes. Expiry by time needs no invalidation: the check compares the stored
  validity with the clock.
- **`SchoolModules` is retired** (entity, table, endpoint `PUT /api/Schools/{id}/modules/{code}`). `GET
  /api/Schools/{id}/modules` is read only. The Angular page shows On or Off. Development seeds give the three demo
  schools a yearly Full-plan subscription. Audit event number 21 (`SchoolModuleChanged`) stays in the enum; numbers are
  never reused.
- **Applied to** the school branding update (module SCHOOL). There are no branch endpoints yet, so none could be guarded.
- **School roles**: `SchoolRole` (name unique per school) and `RolePermission` (View, Create, Edit, Delete per module;
  Create, Edit and Delete need View, also enforced by a check constraint), both tenant-owned with query filters. The
  school administrator manages them under `/api/SchoolRoles` (module USERS_ROLES, audited as `RoleCreated`,
  `RoleChanged`, `RoleDeleted`); the school comes from the host. Rights can only be set in modules the school has now.
  Rights in a module that is off are hidden and kept untouched, so they return with the module. Assigning roles to users
  and checking rights per request are not part of this phase.

## Rationale

- One entitlement query behind a cache keeps the per-request check cheap and gives one place to reason about access.
- Time-based expiry needs no job: it is part of the check; the lifecycle job (phase 7) only changes statuses.
- Keeping rights of switched-off modules avoids the school administrator having to redo roles after a lapse.

## Consequences

**Easier:** later modules are guarded with one attribute; billing and the lifecycle job only change `TenantModule`
status or dates and drop the cache.

**Harder:** the cache is per instance (a shared cache is a later decision). `MODULE_NOT_ENABLED` end to end is
covered by behaviour tests, because no paid module has an endpoint yet.
