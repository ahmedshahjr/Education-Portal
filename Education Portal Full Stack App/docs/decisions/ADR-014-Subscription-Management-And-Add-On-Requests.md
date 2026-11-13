# ADR-014: Subscription Management and Add-On Requests

## Status

Accepted.

## Date

2026-11-13

## Context

Phase 6 of eight of the module-selling work. Until now the master tenant could start a subscription
([ADR-011](ADR-011-Subscriptions-And-Tenant-Modules.md)) and bill it ([ADR-013](ADR-013-Invoices-And-Manual-Payments.md)) but
not change it, and a school could not see it. The platform owner needs to run a school's subscription from the master
portal, and a school administrator needs to see theirs and ask for more.

## Decision

- **Master actions** on `/api/Schools/{schoolId}/subscription/...` (master tenant administrators only, all audited as
  `SubscriptionChanged`, entitlements dropped afterwards): `POST suspend`, `POST reactivate`, `POST extend` (`days` 1 to 366),
  `PUT plan` (`planCode`), `POST modules` (`moduleCode`) and `DELETE modules/{moduleCode}`.
  - **Suspend** sets the subscription and every Active module to Suspended (requests are refused with 403
    `MODULE_NOT_ENABLED`; data is kept). **Reactivate** works from Suspended or PastDue only.
  - **Extend** moves the period end (and the trial end while on trial) and every module's `ValidTo` without a payment.
  - **Change plan, add module and remove module share one engine** (`SubscriptionChanges`): work out what the subscription
    holds now (its plan and the paid whole-school modules the plan does not include are its add-ons), apply the change to
    that choice, re-quote with `SubscriptionPlanner` from the subscription's own price book, and apply the result with
    `Subscription.Reconfigure`. Modules that leave are **Expired now** (rows kept, data kept); new ones start now and run to
    the period end; modules that return are restored. A removal is refused when the plan or another module still includes
    the module. A module that an add-on required counts as an add-on once it is held, so it stays until removed itself.
  - **No proration:** a change takes effect at once and the current period is neither credited nor charged; the next invoice
    uses the new price. The price book must keep the subscription's currency (`SUBSCRIPTION_CURRENCY_MISMATCH` otherwise).
- **Add-on requests** (`AddOnRequest`, table `AddOnRequests`, tenant-owned, status Pending, Approved or Rejected in a lookup
  table, unique pending request per school and module): `POST /api/AddOnRequests` (school administrator, school from the
  host) creates one and is audited as `AddOnRequested`. `GET /api/AddOnRequests?status=` (master only) lists them,
  `POST /api/AddOnRequests/{id}/approve` runs the same add-module engine in one save with the decision, and
  `POST .../reject` changes nothing else; both are audited as `AddOnRequestDecided`. A request changes nothing by itself.
- **Tenant side:** `GET /api/Subscriptions/mine` (school administrator, module SCHOOL) returns the school's own subscription,
  the paid modules it could add with their price for its billing cycle, and its requests. The master tenant needs no
  subscription and has no menu entry for it.
- **Screens (Angular):** under Schools, the selected school gets a Subscription panel (start one, see status, price and
  modules, suspend, reactivate, extend, change plan, add or remove a module, issue invoices, record a payment for the whole
  total, void). The master portal has an Add-on requests page. School administrators get My subscription with Ask for this
  module. New components use external template and style files (ENG-UI-18).

## Rationale

- One engine for plan, add and remove keeps pricing in one place (the planner) and avoids three subtly different rules.
- Expiring instead of deleting a module row keeps its data and lets it come back.
- Approving through the same engine means "the school asked" and "the master did it by hand" cannot drift apart.

## Consequences

**Easier:** phase 7 (lifecycle job) calls `Suspend`, `Reactivate` and `Extend`; phase 8 adds old and new values to the audit.
**Harder:** a change is not prorated, so a mid-period upgrade is free until the next invoice. A rule for restarting a lapsed
period whose end already passed is decided in phase 7. Branch-limited grants are not changed by these actions yet.
