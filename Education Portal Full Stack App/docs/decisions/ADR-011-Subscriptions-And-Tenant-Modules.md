# ADR-011: Subscriptions and Tenant Modules

## Status

Accepted.

## Date

2026-11-13

## Context

Phase 3 of eight of the module-selling work. [ADR-009](ADR-009-Sellable-Module-Catalogue.md) made the module
catalogue and [ADR-010](ADR-010-Plans-And-Regional-Price-Books.md) added plans and price books. A school now needs a
record of what it bought, at what price, and which modules that gives it.

## Decision

- **`Subscription`** (table `Subscriptions`, tenant-owned: `SchoolId`, tenant query filter): an optional `PlanId`, the
  `PriceBookId`, the **currency and the price for one billing cycle copied from the price book** (`CurrencyCode`,
  `Amount`, `numeric(18,2)`), `BillingCycle`, `Status` (Trial, Active, PastDue, Suspended, Cancelled, with a lookup table),
  `StartDate`, `EndDate` (the current period's end; the trial's end while on trial) and `TrialEndDate`. A school has at
  most one subscription that is not cancelled (a unique partial index). The master tenant needs none.
- **`TenantModule`** (table `TenantModules`, tenant-owned): `SchoolId`, `ModuleId`, `SubscriptionId`, `Status` (Active,
  Suspended, Expired, with a lookup table), `ValidFrom`, `ValidTo` and a nullable `BranchId` (empty means the whole
  school). Its data is kept when it expires or is suspended; nothing is deleted.
- **Entitlement rules** (`SubscriptionPlanner`): the school gets the plan's paid modules, the add-ons, everything those
  require (`ModuleDependencyGraph.RequiredBy`) and every core module. Inactive modules, unknown or core add-ons and
  unknown plans are refused. The price is the plan's price plus the price of each paid module the plan does not already
  include (including paid modules pulled in as dependencies), all from the price book of the school's country, active,
  in the chosen cycle; a missing price refuses the subscription. The subscription and its modules are saved with one
  `SaveChanges`, so they are stored together or not at all.
- **`Branch`** (table `Branches`): a minimal tenant-owned row (`SchoolId`, `Name`) so a `TenantModule` can point to a
  branch. It has no endpoints yet.
- **`School.DataRegion`** (SouthAsia, MiddleEast, NorthAmerica; nullable, with a lookup table) is the only new column on
  `School`. Currency and languages already come from the school's `Country`.
- **API** (master tenant administrators only, audited as `SubscriptionCreated` and `DataRegionChanged`):
  `GET` and `POST /api/Schools/{id}/subscription`, `PUT /api/Schools/{id}/data-region`.
- **Not in this phase (done in phase 4, see [ADR-012](ADR-012-Module-Enforcement-And-School-Roles.md)):** `ModuleAccess` and `[Authorize(Module = ...)]` still read the master's per-school switches
  (`SchoolModules`). Phase 4 (enforcement) moves them to `TenantModule` and retires `SchoolModules`, so schools keep
  working in between. Changing a plan, extending, suspending and cancelling come with the master screens (phase 6) and
  the lifecycle job (phase 7).

## Rationale

- Copying price and currency onto the subscription means a later price-book edit never changes what a school was sold.
- One save is the transaction: it needs no extra transaction scope and cannot leave a subscription without its modules.
- Keeping `SchoolModules` until enforcement avoids a half-migrated access check.

## Consequences

**Easier:** enforcement (phase 4) is a query over active, in-period `TenantModule` rows; invoices (phase 5) read the
subscription's amount and currency.

**Harder:** until phase 4, a school's subscription and its master switches can disagree. The data region's values are
a first guess at the markets (South Asia, Middle East, North America) and are easy to extend.
