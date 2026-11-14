# ADR-016: Change Audit for Commercial Records

## Status

Accepted.

## Date

2026-11-13

## Context

Phase 8, the last phase of the module-selling work. Subscriptions, their modules, invoices and payments change money and
access, so the maintainer asked for an audit entry for every subscription, module and payment change with who, when,
the old value and the new value. The security audit log (`SecurityAuditEvents`, [ADR-004](ADR-004-Security-Baseline.md))
records that an action happened (for example `SubscriptionChanged`), but holds no values. Writing old and new values by
hand in each use case would be easy to forget in the next one.

## Decision

- **A separate, append-only table** `ChangeAuditEntries` (`ChangeAuditEntry`): when (`OccurredAt`, UTC), who
  (`ActorUserId`, `null` for the system: the lifecycle job of [ADR-015](ADR-015-Subscription-Lifecycle-Job.md) or seed
  data), which school (`SchoolId`, `null` for platform data), which row (`EntityType` and its `PublicId`), what happened
  (`Action`: Added, Modified, Removed, a numeric enum with its `ChangeAuditActions` lookup table) and `Changes`, a jsonb
  list of `{ property, old, new }`. Every property is init-only, and the application never updates or deletes an entry
  (SEC-LOG-05). No foreign keys, so an entry outlives its row.
- **Written by an EF Core `SaveChangesInterceptor`** (`ChangeAuditInterceptor`, Infrastructure), registered after the
  auditable-entity and domain-event interceptors. It adds the entries to the same `SaveChanges`, so a change and its
  entry are committed together or not at all, and a refused or failed change leaves none. Use cases need no code.
- **Audited types:** `Subscription`, `TenantModule`, `Invoice`, `InvoiceLine`, `Payment`, `AddOnRequest`, `PortalModule`,
  `ModuleDependency`, `Plan`, `PlanModule`, `PriceBook`, `PriceBookItem`. A new commercial entity is audited by adding
  its type to `ChangeAuditInterceptor.AuditedTypes`.
- **Values:** an added row records every value, a removed row every old value, a modified row only the values that really
  changed. The key and the four audit fields (`Created`, `CreatedBy`, `LastModified`, `LastModifiedBy`) are left out,
  because the row already has them. Values are culture-free text: dates in ISO 8601 UTC, decimals without trailing
  zeros, enums by name. These records hold ids, codes, amounts, dates and statuses, never personal data (SEC-PII-02).
- **Tenant filter:** `IsMasterTenant || SchoolId == CurrentSchoolId`, so a school could only ever read entries about its
  own rows, and never platform rows. No endpoint serves the entries yet; the master tenant reads them in the database.
- The security audit log is unchanged and still records each action and denial.

## Rationale

- An interceptor sees every tracked change, including ones a future use case adds, so the rule cannot be forgotten.
- One save keeps the record and the change consistent without a second transaction or an outbox.
- A separate table keeps the security log small and its schema stable, and jsonb holds any entity's values.

## Consequences

- Bulk updates (`ExecuteUpdate`, raw SQL) bypass the change tracker and are not audited. None touch the audited tables
  today; any that do must write their own entries.
- Every save of an audited row writes one more row per changed entity. The volumes are small (a few changes per school
  per month).
- Retention and an API or screen to read the entries are not decided (TODO); entries are kept for now.
- The table is created by `EnsureCreated` like the rest of the schema; there are still no migrations.
