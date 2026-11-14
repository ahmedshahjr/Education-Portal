# ADR-009: Sellable Module Catalogue (Core and Paid Modules)

## Status

Accepted. Replaces the "default or custom" module types decided on 2026-11-11 (AGENTS.md section 2).

## Date

2026-11-13

## Context

The platform is sold per school (tenant) in Pakistan, the GCC and the USA, against OpenEduCat and local vendors. The
maintainer wants to sell single modules and bundles from the master tenant, and a school may use only what it pays for.
The existing module switches had two types: **default** (on for every school unless switched off) and **custom** (off
until switched on). That model has no notion of "always included", no codes that billing and price books can refer to,
and no dependencies (a school cannot sensibly buy FEES without STUDENTS).

This is phase 1 of eight (catalogue, plans and price books, subscriptions, enforcement, billing, onboarding screens,
lifecycle job, audit). Later phases add `Plan`, `PriceBook`, `Subscription` and `TenantModule` on top of this catalogue.

## Decision

- The existing `PortalModule` (table `Modules`) becomes the catalogue. Each module has a stable upper-case **code**
  (`FEES`, `HR_PAYROLL`; `Domain/Constants/ModuleCodes.cs`), a name, a description, **IsCore**, **SortOrder** and the
  existing **IsActive** switch and soft delete. A code is unique among live modules; a deleted module keeps its row and the rows that point to it by id, and its code can be used again. The `ModuleType` enum and its lookup table are removed.
- **Core** modules (SCHOOL, BRANCH, ACADEMIC_SETUP, USERS_ROLES, STUDENTS, STAFF) are part of every subscription and
  never sold on their own. A core module is always on: it cannot be made inactive (also a database check constraint),
  deleted, or switched off for a school.
- **Paid** modules (ADMISSIONS, ATTENDANCE, FEES, PARENT_COMMUNICATION, EXAMS, TIMETABLE, HR_PAYROLL, TRANSPORT,
  LIBRARY, LMS, HOSTEL) are on for a school only when it gets them. Until subscriptions exist (phase 3) the master's
  per-school switch (`SchoolModules`) is how a school gets one; phase 3 replaces it with `TenantModule` rows.
- `ModuleDependency` (table `ModuleDependencies`: `ModuleId`, `RequiresModuleId`) records that one module requires
  another. A module cannot require itself (check constraint), a pair is stored once (unique index), a new dependency
  that would close a loop is refused (`ModuleDependencyGraph.WouldCreateCycle`), and a core module may require only core
  modules, because everything a core module needs must be in every subscription.
- The old module keys map to the new codes: Teachers to STAFF, Courses and Enrolments to ACADEMIC_SETUP, Grades to
  EXAMS, Parents to PARENT_COMMUNICATION, Dashboard to SCHOOL (core), Branches to BRANCH, Users & Roles to USERS_ROLES.
- Only the master tenant's administrators manage the catalogue (`/api/Modules`), and every change is audited.

## Rationale

- Evolving the existing table keeps one module concept, the existing `[Authorize(Module = ...)]` check, the master's
  screens and the tests, instead of a second, parallel "sellable module" table that would drift from the access check.
- Codes instead of display names let price books, invoices and the API refer to a module without depending on its
  (translated, editable) name.
- Loops are refused when they are added rather than tolerated at resolution time, so the catalogue the master edits is
  always one that can be sold. The resolution code still visits each module once, so a loop created by two concurrent
  edits cannot hang it.

## Consequences

**Easier:**
- Plans, price books and subscriptions (phases 2 and 3) can link to modules by id and show them by code.
- Dependency resolution for a chosen plan plus add-ons is a pure function over the stored pairs (`ModuleDependencyGraph.RequiredBy`).

**Harder:**
- The "default" behaviour is gone: a paid module is never on for a new school by itself. Existing development data is
  recreated on start, so there is nothing to migrate yet; once migrations exist, renaming a code needs a data migration.
- Module names and descriptions are translated in Angular by code (`moduleText` pipe). A module the master adds in the
  catalogue shows its English name until a translation is added.
