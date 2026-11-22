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
3. **The create rule, by size:** a form is a dialog only when it fits a modal, and a page otherwise. The test is
   `fitsDialog` (`shared/components/entity-form/form-size.ts`): at most 5 fields and, for a grid or table inside the form,
   at most 6 rows. Anything larger gets a page (`/x/new`, `/x/:id`). Delete always uses `ConfirmDialogComponent`.
   Applied: people are pages (many fields, a login, and lists of roles, campuses and children). Roles are pages: their
   rights grid has a row per module, which is more than a modal holds. Campuses are pages: the add-campus form has well
   over 5 fields (name, address, state, city, postal code, phones, map location, school number, short name, principal and
   more). Holidays, events and shifts stay small forms and may use a dialog, judged by the same test when they change.
4. **Soft delete** for user-deletable entities that are still hard-deleted: `SchoolRole` first (`BaseSoftDeletableEntity`,
   filters combined with the tenant filter, never a second `HasQueryFilter`). System roles stay undeletable. Other entities
   only where a user can delete them today.
5. **Calendar and My children** (phase 5): the full-screen calendar is an own month grid built from Angular Material
   and the theme tokens, with no new package; `mat-calendar` stays as the date picker. "My children" shows only to
   parents.

## Consequences

- Easier: one place to manage access, one pattern to learn.
- Harder: the old pages and routes are removed; links and tests move with them.
