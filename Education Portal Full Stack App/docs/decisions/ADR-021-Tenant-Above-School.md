# ADR-021: A Tenant Above the School

## Status

Accepted by the maintainer on 2026-11-21 for the model: option B, and several schools per tenant in the first release
and the tenant administrator is the administrator account created when the tenant registers (see below). Mission
msn_37932efe88c3d08c24485c98.

## Date

2026-11-21

## Context

Today `School` is the tenant. The master tenant is one school with `IsMaster` (unique index
`IX_Schools_SingleMasterTenant`), and the customer-level things hang on the school: `Subscription`, `TenantModule`,
invoices, the domain, branding and country. The maintainer wants a **Tenant table above School**: a customer that can
own one or more schools, with exactly one master tenant, where only the master domain can add a school to a tenant,
default modules are granted on registration, and every API and Angular route checks the module.

What already works and stays: host-based resolution, `ITenantOwned` with `SchoolId` query filters (84 use cases),
`IModuleAccess` with a 5-minute cache, `SchoolDefaultModules` on registration, one main campus per school.

## Options

**A. Tenant is a thin parent only.** `Tenant` (name, `IsMaster`) and `School.TenantId`. Subscription, modules, invoices
and domains stay on School. Least change, but a tenant with two schools would buy modules twice, so the new level
does not do much.

**B. Tenant owns what the customer buys (recommended).** `Tenant` (name, `IsMaster`, country, data region, tax override)
owns `Subscription`, `TenantModule` and invoices. `School` keeps domain, branding, campuses, roles and all
`ITenantOwned` data. Module checks resolve school to tenant (one extra lookup, cached with the entitlements).

**C. Tenant owns everything including the domain.** Resolution would find a tenant and then a school, which changes
tenant resolution, cookies and every test. Too large for one phase.

## Decision (proposed: B)

- `Tenant` entity: `Name`, `IsMaster`, `CountryId`, `DataRegion`, `TaxRateOverride`, soft-deletable, audited.
  Unique filtered index `IX_Tenants_SingleMasterTenant` on `IsMaster` (same pattern as today). Created by seeding or
  registration only.
- `School.TenantId` is required. A school belongs to exactly one tenant; the master tenant owns the master school.
  `School.IsMaster` becomes `Tenant.IsMaster` read through the school; the school column is dropped in a later step
  once nothing reads it.
- `Subscription` and `TenantModule` get `TenantId` and lose `SchoolId`; a campus-only grant keeps its `CampusId`.
  `IModuleAccess.HasModuleAsync(schoolId, ...)` keeps its signature: it looks up the school's tenant.
- **Only the master domain adds a school to a tenant:** use case `AddSchoolToTenant` with `MasterTenant = true`
  (SEC-AUTHZ-07). A tenant's own administrator cannot.
- **Registration** creates a tenant, its first school (with the main campus) and grants the default modules
  (`SchoolDefaultModules`, now per tenant) in the existing transaction.
- **Module gaps** (separate from the schema, same phase): every non-public, non-master use case names a module or is on
  an exemption list, enforced by an architecture test; Angular routes for subscription, audit, branding and admissions
  get a module or rights guard.
- **Existing data:** one tenant per existing school, named like the school; the master school's tenant is the master.
  The migration is generated with `dotnet ef`, never merged by hand.

## Consequences

- Easier: one subscription can cover several schools; the master sees customers, not only schools.
- Harder: invoices, the lifecycle job (ADR-015) and `SubscriptionPlanner` move from school to tenant scope; each
  needs its tests redone. This is the bulk of the work and is why phase 2 is large.
- Risk: the master tenant is "never limited by modules" today through `ICurrentTenant.IsMaster`; that must keep working
  through the tenant.
- AGENTS.md section 2 (hierarchy diagram, data isolation, modules) and the README diagrams change in the same PR.

## Tenant administrator, master editing and domains (maintainer, 2026-11-21)

- **The tenant administrator is the `Administrator` account created at registration**, either from the public Register a
  school screen (ADR-020) or when the master tenant adds the school (one-time password). No new role is added. That
  account administers its tenant's schools through the existing school administrator rights.
- **The master tenant gets one edit screen per tenant** with every option the register screen has: tenant and first
  school name, country, administrator name and email (password reset sends a new one-time password, never shows one),
  plus what only the master sets (data region, tax rate override, subscription and modules, active switch). The
  master changes data on behalf of the tenant through audited use cases with `MasterTenant = true`.
- **Domain is set only by the master tenant**, in the school edit screen. A school's own administrator cannot set or
  change it. A school with a domain signs in on that domain only and cannot sign in on the master tenant's domain to
  see its data; a school without a domain signs in on the master tenant's domain (ADR-020). Setting a domain
  therefore ends the master-domain sign-in for that school's users: the edit screen warns the master before saving, and
  a test proves the old route is refused.

## Decided

1. Option B. 2. Several schools per tenant. 3. Tenant administrator = the registration administrator.
