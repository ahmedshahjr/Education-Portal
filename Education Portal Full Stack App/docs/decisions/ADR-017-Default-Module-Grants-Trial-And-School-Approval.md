# ADR-017: Default Module Grants, Free Trial and Approval of School Requests

- **Status:** Accepted
- **Date:** 2026-11-15
- **Supersedes in part:** ADR-009 (core modules always on), ADR-011 (the subscription is the only way a school gets a module)

## Context

The master tenant must be able to switch off any module from its settings, core modules included. A school that registers
must get the modules that are on by default without buying anything, and a registering school gets a free trial. A school
request must be approved by the master tenant before the school exists.

Until now a core module was on for every school whenever it was active, and it could not be switched off or made inactive.
A school got paid modules only through its subscription.

## Decision

1. **Every module can be switched off.** The master's `IsActive` switch turns a module off for every school, core modules
   included. Reactivating it brings the module back as it was. A core module still means "part of every subscription and
   never sold on its own"; that is a billing rule, not an access rule.
2. **Access is by grant only.** A module is on for a school when it has an active grant in its period
   (`ModuleEntitlements`). Core modules have no special case.
3. **Default modules.** `PortalModule.IsDefaultEnabled` (set by the master) marks the modules a registering school gets.
   Core modules are on by default; paid modules are off by default. A registering school gets each active default module as
   a **default grant** (`TenantModule` with no subscription, the whole school, no end). Changing the flag affects only schools
   that register afterwards.
4. **Free trial.** A school that registers gets a 30-day trial of the Full plan (`SchoolRegistration.TrialDays`). The trial is
   a `Subscription` in status Trial, priced from the school's country's price book, so a school needs a country that has a
   price book. Default grants stay on when the trial ends; the subscription's modules are suspended by the existing billing
   rules. The banner's remaining days come from `GET /api/Subscriptions/mine` (`trialDaysRemaining`).
5. **Approval.** A school request stays pending until the master tenant approves it
   (`POST /api/SchoolRequests/{id}/approve`). Approval registers the school exactly as the master adding a school does
   (`CreateSchool`), then marks the request approved, in one request. Rejection (`POST .../reject`) creates nothing. The
   queue is `GET /api/SchoolRequests`, master only.

## Consequences

- `TenantModule.SubscriptionId` is nullable, and a unique index covers default grants (nulls are distinct in a unique index).
- The database check constraint that kept core modules active is removed (migration `AllowSwitchingOffCoreModules`).
- Existing schools keep their modules: their core modules come from their subscriptions. A migration sets core modules to
  default-on.
- A country is required to register a school, and it must have a price book (the trial is priced from it). The master cannot register a school without one.
- The master adding a school directly also starts the trial and grants the defaults, so there is one registration path.
- Billing does not suspend default grants. A school that does not pay keeps the modules that are on by default. The master
  switches them off if that is not wanted.
- Registration and approval are one transaction: the school, its grants, its trial, its administrator and the request's approval are saved together or not at all. Audit records are written after the commit, so they never describe a rolled-back change. Plan and role creation, which save twice, are one transaction too.
- The approval screens and the trial banner are not built yet (Angular); the API is.
