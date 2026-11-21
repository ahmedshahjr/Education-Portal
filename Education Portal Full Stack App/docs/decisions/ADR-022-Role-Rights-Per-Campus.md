# ADR-022: Role Rights Per Campus and the ViewHistory Right

## Status

Accepted by the maintainer on 2026-11-21. Amends [ADR-012](ADR-012-Module-Enforcement-And-School-Roles.md) and
[ADR-018](ADR-018-Role-Rights-Checking.md).

## Context

A role's rights (`RolePermission`) hold for the whole school. A school with several campuses cannot give a teacher Edit
in one campus only. `AuthorizationBehaviour` also never knows the campus, because it sees only the request type; handlers
learn it from request fields.

## Decision

1. **`RolePermission.CampusId` is required** (never null). A bool **`AllCampuses`** says the rights apply to every campus of
   the school, including campuses added later; then `CampusId` only records where the row was made (the school's main
   campus). Unique per (role, module, campus). Old rows migrate with `AllCampuses = true` and the school's main campus
   (every school has exactly one, created at registration and not deletable), so no one loses access.
2. **New right `ViewHistory`** (`AccessRight` = 7, `CanViewHistory`, needs View) lets a role read the change history
   of a module (ADR-023).
3. **Two-step check.** The behaviour asks `IRoleRights.HasRightAsync`: does any campus give the right (fail closed, 403 before the
   handler). The handler then narrows to the request's campus with `ICampusAccess.EnsureCampusAsync(campusId, module, right)`
   and `GetAllowedCampusIdsAsync(module, right)`. A user's allowed campuses are the **intersection** of their `UserCampus`
   assignments and the campuses their roles grant; roles combine by union per campus.
4. **Administrators** keep every right on every campus and the master tenant is not limited (unchanged from ADR-018).
5. **Own rights API:** `GET /api/Account/me/rights` returns modules x rights x campuses for the signed-in user, only for
   modules the school has. Angular reads it in `RightsService` for the menu and `rightsGuard`; the server stays the
   authority.
6. Use cases move to the campus-aware overload one at a time, each with its denial test, as ADR-018 section 4 already
   allows.

7. **One shape per module, written by the role screen.** A module has either one all-campuses row (the screen always
   writes this, on the school's main campus) or per-campus rows (the data model allows them, the screen does not create
   them). Saving a role's rights replaces every row of the module, so no stale per-campus row survives. Per-campus
   editing in the screen is a later step.

## Consequences

- Easier: per-campus staff, one rights matrix per role, a menu that follows rights instead of "administrator only".
- Harder: one more query per request (cached per request like `CampusAccess`); a new campus gives rights only to roles with
  `AllCampuses`.
- The migration is generated with `dotnet ef`, never merged by hand.
