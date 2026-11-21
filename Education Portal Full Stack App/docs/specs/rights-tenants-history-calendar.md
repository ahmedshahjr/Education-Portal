# Spec: per-campus rights, tenants, users and roles, change history, calendar

Status: draft for review (mission msn_37932efe88c3d08c24485c98, task t2). Each requirement has an acceptance criterion
(AC) that a test can check. Every use case also needs the three denial tests: another school, another campus, a module
that is off.

## Decision from the maintainer (2026-11-21)

Revised the same day: old data stays untouched. `RolePermission` gets a boolean `AllCampuses` (the backend check) and
the role screen offers an "All campuses" choice. When `AllCampuses` is true the permission applies to every campus of the
school, including campuses added later; when false it applies only to the row's `CampusId`. Existing rows migrate with
`AllCampuses = true` and need no rewrite, so nobody loses access. No campus id is ever guessed or defaulted to null
in code: the domain method takes either the flag or a campus, never neither.

Final (same day): `CampusId` is **required** (non-nullable column). It always holds a real campus of the school. When
`AllCampuses` is true the campus is ignored by the check and only records where the row was made (the role screen sets
it to the school's main campus). The migration adds the column and fills old rows with `AllCampuses = true` and the
school's main campus; the rights themselves do not change.

Every school has exactly one main campus, created at registration (`School.Create` adds `Campus.CreateMain()`,
`CampusKind.Main`, unique index `IX_Campuses_SingleMainPerSchool`) and it cannot be deleted or deactivated
(`DeleteCampus`, `Campus.CanBeDeactivated`). So the backfill uses the school's main campus, no school lacks one, and
the role screen's default campus is the main campus. Nothing new is needed for this; tests only prove it stays true.

## Phase 1: rights foundation

| ID | Requirement | Acceptance criterion |
|---|---|---|
| R1.1 | `RolePermission` has a required `CampusId` and a bool `AllCampuses` (true for old rows); unique per (role, module, campus). When the flag is true the rights apply to every campus of the school. | A row without a campus cannot be created; a duplicate is rejected; old rows keep their rights on every campus after migration. |
| R1.2 | New right `ViewHistory` (`AccessRight` = 7) and `CanViewHistory` on `RolePermission`; it needs View. | Setting ViewHistory without View throws; enum number 7 is in the lookup table. |
| R1.3 | `AuthorizationBehaviour` passes the request's campus to the rights check; a user's rights are the union of their roles for that campus. | A user with Edit on campus A only: Edit on A passes, Edit on B returns 403. A role with `AllCampuses` passes on every campus, including one added later. Two roles combine. |
| R1.4 | `ICampusAccess` combines role-campus rights with `UserCampus`; both must allow. | A campus in the role but not in `UserCampus` (and the reverse) is refused. |
| R1.5 | `GET /api/Account/me/rights` returns modules x rights x campuses for the signed-in user, only for modules the school has. | A school with the module off gets no entry for it; another school's roles never appear. |
| R1.6 | Angular `RightsService` (signal), rights-aware `access` in `navigation.ts`, `rightsGuard(module, right)`. | A non-administrator with View in Students sees the Students item and can open its route; without View the item is hidden and the route redirects. |
| R1.7 | Administrator keeps all rights and the master tenant is not limited (documented, unchanged). | Existing administrator tests stay green. |

## Phase 2: Tenant level (ADR-021 first, maintainer reviews before code)

| ID | Requirement | Acceptance criterion |
|---|---|---|
| R2.1 | `Tenant` entity above `School`; `School.TenantId` required. | Every school has a tenant after migration. |
| R2.2 | Exactly one master tenant: unique filtered index on `Tenants.IsMaster`. | A second master tenant fails with a unique violation. |
| R2.3 | Only the master domain adds a school to a tenant (`MasterTenant = true`). | A school administrator calling it gets 403. |
| R2.4 | Registration creates a tenant, its first school and the default modules. | After registering, only default modules are enabled. |
| R2.7 | The tenant administrator is the administrator created at registration (public screen or master add). | Both paths create an `Administrator` account tied to the tenant; no extra role exists. |
| R2.8 | The master tenant has an edit screen per tenant with all register-screen options plus master-only ones (data region, tax override, subscription, modules, active). | Every field saves through a `MasterTenant` use case and writes a change-audit entry; a school administrator calling it gets 403. |
| R2.9 | Only the master sets or changes a school's domain; a school with a domain cannot sign in on the master domain. | A school administrator changing the domain gets 403; sign-in of that school's user on the master host is refused, and works on its own host. |
| R2.5 | Every non-public, non-master use case names a module or is on an explicit exemption list. | An architecture test fails when one has neither. |
| R2.6 | Angular routes for subscription, audit, branding and admissions carry a module or rights guard. | A direct URL to a disabled module redirects; a direct API call returns 403 `MODULE_NOT_ENABLED`. |

## Phase 3: users and roles in one screen

| ID | Requirement | Acceptance criterion |
|---|---|---|
| R3.1 | Role editor: name, description, modules the school has, rights matrix per campus. | Choosing "All campuses" saves one row with `AllCampuses = true`; choosing campuses saves one row per campus; modules the school lacks are refused. |
| R3.2 | One command saves a user with several roles and campuses in one transaction. | If one part is invalid, nothing is saved. |
| R3.3 | `SchoolRole` is soft-deleted; system roles cannot be deleted. | A deleted role is hidden from queries and kept in the table. |
| R3.4 | Create rule: small entities (roles, campuses, holidays, events, shifts) in a dialog; large ones (people, schools, admissions) on a page. | Each listed entity opens the matching container; delete uses `ConfirmDialogComponent`. |

## Phase 4: readable change history

| ID | Requirement | Acceptance criterion |
|---|---|---|
| R4.1 | `SchoolRole`, `RolePermission`, `Campus`, `UserDetails` are audited; the entry keeps who and when. | Editing a campus writes an entry with actor and UTC time. |
| R4.2 | `GET /api/ChangeAudit` pages and filters by entity, user, date and module, and needs ViewHistory per module and campus. | A user without ViewHistory gets 403; another school's entries are never returned. |
| R4.3 | Entries are returned as localised sentences (en, ar, ur): who, what, when, old and new value. | The same entry renders in all three languages; no personal data beyond names. |
| R4.4 | The Angular page shows only with the right. | The menu item and route are hidden and guarded without ViewHistory. |

## Phase 5: calendar and My children

| ID | Requirement | Acceptance criterion |
|---|---|---|
| R5.1 | Full-screen month grid (and week) from Angular Material and `--mm-*` tokens, no new package. | The grid shows events and holidays of the month, keeps the campus filter, mirrors in RTL. |
| R5.2 | `DateOnly` values use `calendarDate`, `fromApiDate`, `dateKey`. | A date near midnight does not shift with the time zone. |
| R5.3 | "My children" shows only for parents. | Staff and administrators do not see the item. |

## Cross-cutting

- i18n ids and en/ar/ur translations for every new string (`translations.spec.ts` passes).
- Unit tests for each new class; functional tests with the denial cases; README and diagram guards before each PR.
- EF Core migrations are regenerated, never merged by hand.
