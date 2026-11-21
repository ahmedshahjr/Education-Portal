# ADR-018: Checking School Role Rights on Each Request

- **Status:** Accepted. Campus-aware rights and the ViewHistory right: [ADR-022](ADR-022-Role-Rights-Per-Campus.md).
- **Date:** 2026-11-19
- **Builds on:** ADR-012 (module enforcement and school roles), ADR-014 (people and account roles)

## Context

A school's roles (`SchoolRole` with `RolePermission` per module) and the roles given to each account (`AccountRole`) were
stored and shown, but no request was checked against them. A teacher could reach any endpoint of a module the school had.

## Decision

1. **A use case names the right it needs.** `[Authorize(Module = ModuleCode.X, Right = AccessRight.View | Create | Edit | Delete | Approve | Export)]`. Approve and Export were added on 2026-11-20 (phase B of the entity spec); every right needs View.
   A request that names a right must also name its module; otherwise it is refused with an error in the code.
2. **Roles decide, per school.** The right holds when one of the user's `AccountRole`s of the request's school has a
   `RolePermission` for that module that gives the right. The check is `IRoleRights.HasRightAsync`, which fails closed.
3. **Who is not checked.** Users in the `Administrator` role (the school's owners) and the master tenant. The module check
   still applies first: a disabled module refuses everyone.
4. **Scope.** Only use cases that declare a right are checked. Existing endpoints keep their checks until they are moved;
   moving them is a follow-up, endpoint by endpoint, each with its denial tests.

## Consequences

- A role with no right in a module refuses every request that names that right in that module.
- Parents and students get their rights from the built-in roles (`BuiltInRoles`), which are a starting proposal the school
  may change.
- The check adds one query per named right per request. It can be cached per request later if it matters.
