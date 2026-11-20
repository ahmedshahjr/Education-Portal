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
4. **Free trial.** A school that registers gets a 3-month (90-day) trial of the Full plan (`SchoolRegistration.TrialDays`; amended 2026-11-19 from 30 days). Its data region follows from its country (`DataRegionForCountry`), and approval asks no domain. The trial is
   a `Subscription` in status Trial, priced from the school's country's price book, so a school needs a country that has a
   price book. Default grants stay on when the trial ends; the subscription's modules are suspended by the existing billing
   rules. The banner's remaining days come from `GET /api/Subscriptions/mine` (`trialDaysRemaining`).
5. **Approval.** A school request stays pending until the master tenant approves it
   (`POST /api/SchoolRequests/{id}/approve`). Approval registers the school exactly as the master adding a school does
   (`CreateSchool`), then marks the request approved, in one request. Rejection (`POST .../reject`) creates nothing. The
   queue is `GET /api/SchoolRequests`, master only.
6. **Per-school switch.** The master switches a module for one school with `PUT /api/Schools/{id}/modules/{moduleCode}` (`SetSchoolModuleCommand`, body `{enabled}`). It refuses the master tenant and a module that is off for every school. Enabling reopens the school's ended default grant or creates one; disabling expires every active grant of the module. Each change is audited as `SchoolModuleChanged`.
7. **Campus details.** A campus holds its contact and registration details (`CampusDetails`): address, state and city (linked to the location data; the city must belong to the state), postal code, one to five phone numbers, school number and short name, plus optional website and principal. The principal is a `UserDetails` row of the same school whose type is Principal. Address and phone rules sit in the domain, not in endpoints. Each campus also has its place on the map: latitude and longitude, required, within -90 to 90 and -180 to 180, rounded to six decimals (`CampusDetails.Create`). The campus form places it with a Google Maps picker; the fields take typed coordinates when no map is set up or it cannot load. The school itself has a country only, never a state or city.
8. **Map key.** The Google Maps JavaScript key is a platform secret, `google-maps-api-key` (ADR-006), read by `GetCampusMapSettingsQuery` and served to the Campuses module only (`GET /api/Campuses/map-settings`). It is never in the repository. The Content-Security-Policy allows scripts, connections, images and fonts from Google's Maps hosts and nothing else outside the app (`SecurityHeaders`). Without the key the picker still works with typed coordinates.

## Consequences

- `TenantModule.SubscriptionId` is nullable, and a unique index covers default grants (nulls are distinct in a unique index).
- The database check constraint that kept core modules active is not created (the initial migration has no such constraint).
- Existing schools keep their modules: their core modules come from their subscriptions. A migration sets core modules to
  default-on.
- A country is required to register a school, and it must have a price book (the trial is priced from it). The master cannot register a school without one.
- The master adding a school directly also starts the trial and grants the defaults, so there is one registration path.
- Billing does not suspend default grants. A school that does not pay keeps the modules that are on by default. The master
  switches them off if that is not wanted.
- Registration and approval are one transaction: the school, its grants, its trial, its administrator and the request's approval are saved together or not at all. Audit records are written after the commit, so they never describe a rolled-back change. Plan and role creation, which save twice, are one transaction too.
- The approval screens and the trial banner are built in Angular (school requests, plans and prices, invoices, the trial banner).

**Amended 2026-11-19:** master approval is removed. The public registration form creates the school, its trial and its administrator account at once, with the password the visitor chose; the master tenant still adds schools directly and extends trials.
