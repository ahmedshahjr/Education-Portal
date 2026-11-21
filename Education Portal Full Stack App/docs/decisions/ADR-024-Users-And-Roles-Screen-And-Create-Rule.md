# ADR-024: One Users and Roles Screen, the Create Rule and Soft Delete

## Status

Accepted by the maintainer on 2026-11-21. Builds on [ADR-022](ADR-022-Role-Rights-Per-Campus.md).

## Context

People and roles have separate screens and menu items. Create, edit and delete look different on every screen, and
roles are hard-deleted.

## Decision

1. **One "Users and roles" screen** replaces `/people` and `/roles` in the menu: the users table with their roles and
   campuses, and the role editor beside it. A user can hold several roles. One command saves a user with roles and
   campuses in one transaction.
2. **The role editor** sets name, description, the modules the school has and the rights matrix per campus, with an
   "All campuses" choice (ADR-022).
3. **The create rule:** small entities (roles, campuses, holidays, events, shifts) are created, viewed and edited in a
   dialog; large ones (people, schools, admissions) get pages (`/x/new`, `/x/:id`). One shared dialog wrapper keeps a
   single pattern. Delete always uses `ConfirmDialogComponent`.
4. **Soft delete** for user-deletable entities that are still hard-deleted: `SchoolRole` first (`BaseSoftDeletableEntity`,
   filters combined with the tenant filter, never a second `HasQueryFilter`). System roles stay undeletable. Other entities
   only where a user can delete them today.
5. **Calendar and My children** (phase 5): the full-screen calendar is an own month grid built from Angular Material
   and the theme tokens, with no new package; `mat-calendar` stays as the date picker. "My children" shows only to
   parents.

## Consequences

- Easier: one place to manage access, one pattern to learn.
- Harder: the old pages and routes are removed; links and tests move with them.
