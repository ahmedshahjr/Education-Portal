# ADR-026: Tenant Column, Row-Level Security and the Master Bypass

## Status

Accepted on 2026-11-23 for the isolation layer. It builds on [ADR-021](ADR-021-Tenant-Above-School.md) (the tenant
above the school, option B). The move of subscriptions and modules from school to tenant (ADR-021 phase 2) is not part of
this decision and stays open.

## Date

2026-11-23

## Context

ADR-021 added the `Tenant` table and `School.TenantId`. Isolation of school rows still rested on one layer: the EF Core
query filter `IsMaster || SchoolId == CurrentSchoolId`. Three gaps followed from that:

1. The rows had no tenant of their own. A query that lost its filter (a second `HasQueryFilter`, an `IgnoreQueryFilters`
   or a raw SQL statement) returned other tenants' rows, and nothing in the database refused them.
2. The master bypass was tied to the **host**, not to a signed-in user. An anonymous request on the master domain
   therefore read every school's rows that a query filter allowed.
3. The rule "one tenant cannot read or write another tenant's data" was enforced only by the application.

## Options

**A. Keep the application filters only.** Cheapest, but the database trusts every query and an ad-hoc SQL session sees all
rows. Rejected.

**B. A `TenantId` on every tenant-owned row, filled by the database, with row-level security (chosen).** The row carries
its tenant. A trigger sets the tenant from the row's school on every insert and update and refuses a row that moves to
another school, so the application cannot choose it. PostgreSQL row-level security (`FORCE`d) then refuses any row whose
tenant is not the session's `app.tenant_id`, unless `app.tenant_bypass` is `on`. The EF Core filter stays as the first
layer, so the same rule is visible in code and tests.

**C. Schema or database per tenant.** Makes migrations and cross-school overviews (ADR-005, the master overview) much harder.
Rejected for the MVP, as in AGENTS.md section 2.

## Decision (option B)

- **Column.** Every `ITenantOwned` entity (40 tables today) has `TenantId` (`integer`, not null) as a shadow EF property,
  a foreign key to `schools.Tenants` and an index `(TenantId, SchoolId)`. No entity class has the property, so the domain
  does not change. The migration `TenantRowLevelSecurity` adds it, backfills it from each school, then applies the
  trigger and the policy.
- **Trigger `TenantFromSchool`** (function `public.education_portal_set_tenant`). Before insert or update it sets
  `TenantId` to the school's tenant and raises `check_violation` when `SchoolId` changes on update. The database, not the
  application, decides the tenant.
- **Row-level security.** Each table has `ENABLE` and `FORCE ROW LEVEL SECURITY` and the policy `TenantIsolation`:
  `TenantId = NULLIF(current_setting('app.tenant_id', true), '')::integer OR coalesce(current_setting('app.tenant_bypass', true), 'off') = 'on'`
  for both `USING` and `WITH CHECK`.
- **Session.** `TenantSessionInterceptor` runs `set_config` for `app.tenant_id` and `app.tenant_bypass` every time a
  physical connection opens, so a pooled connection never keeps an earlier request's tenant.
- **EF Core filter.** A row is visible when the request is a master bypass, or when its tenant and its school are the
  request's school and tenant. With no school resolved, nothing matches.
- **Master bypass (SEC-AUTHZ-13).** `CurrentTenant.GrantMasterBypass` is called only by the tenant session check, for a
  signed-in session whose school is the master school on the master domain. Every `/api` call under it is audited as
  `MasterTenantAccess`. An anonymous request on the master domain sees its own school only. The scheduled lifecycle job
  grants the bypass on its own scope.
- **System scope (SEC-AUTHZ-14).** `ITenantSystemScope` is an explicit bypass for work with no signed-in user: the
  development seed and the self-service registration (`RegisterSchool`), which create rows of a school the visitor does
  not belong to. It is opened around that work only.
- **Campus is not renamed.** The coordinator asked whether `Campus` should replace `Branch`. The code already uses
  `Campus` (see AGENTS.md section 2), so there is nothing to rename.
- **Exemptions.** `ApplicationUser` (the login account, read across tenants by email at sign-in) and `ChangeAuditEntry`
  (the audit trail with a nullable school) keep their own rules. The model test lists them with reasons.
  `SecurityAuditEvent` has no `SchoolId` (only `ActorSchoolId`) and is not tenant-owned; it is append-only and is written
  from the sign-in path before a tenant is known.

## Consequences

- Easier: a missing filter, a raw SQL statement or an `IgnoreQueryFilters` no longer shows another tenant's rows to the
  application's database role, and a new table without the policy fails `TenantRowLevelSecurityTests`.
- Harder: a production deployment must connect as a role that is not a superuser and does not bypass row-level security
  (SEC-CFG-04). The development database is owned by a superuser, so there the database layer is proved by the tests
  (a dedicated test role), not by the running app. Every new tenant-owned table needs the same migration SQL.
- The module entitlement loader reads a named school's grants with an explicit school predicate, because a sign-in on
  the master domain loads a school's modules before the request acts as that school. Without this, the per-school cache
  held an empty list for five minutes (found by the tests of school-without-domain sign-in).
- Known limits: a tenant-level role (one user over several schools of one tenant) does not exist yet; when it is added
  it widens the filter explicitly, in a new decision. The bypass is per request, so a long job must grant it itself.

## Tests

- `TenantRowLevelSecurityTests`: a non-superuser role sees only its tenant's rows, nothing without a tenant, everything
  with the bypass; the trigger refuses another tenant's insert and a move to another school and overwrites a wrong tenant;
  every tenant-owned table has forced row-level security, the policy and the trigger.
- `TenantFilterTests`: a school sees only its own rows inside its own tenant; a master host without a session sees its own
  school only; the bypass is refused for a school tenant; the session settings are set on each connection.
- `MasterBypassAuditTests`: a signed-in master API call is audited; a school session gets no bypass and no audit entry.
- `TenantIsolationModelTests`: every `ITenantOwned` entity has the `TenantId` column and its foreign key; every entity with
  a `SchoolId` is tenant-owned or listed with its reason.
