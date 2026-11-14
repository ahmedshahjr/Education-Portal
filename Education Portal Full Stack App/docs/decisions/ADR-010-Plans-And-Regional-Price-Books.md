# ADR-010: Plans and Regional Price Books

## Status

Accepted.

## Date

2026-11-13

## Context

Phase 2 of eight of the module-selling work ([ADR-009](ADR-009-Sellable-Module-Catalogue.md) is phase 1). The master
tenant sells bundles of paid modules (plans) and single add-on modules to schools in Pakistan, Saudi Arabia, the UAE and
the USA, each in its own currency. Subscriptions (phase 3) and invoices (phase 5) need prices to refer to.

## Decision

- **`Plan`** (table `Plans`): a stable upper-case `Code` (`STARTER`), a name, a description and the soft-delete and
  `IsActive` switches of `BaseSoftDeletableEntity`. **`PlanModule`** (`PlanModules`) lists the **paid** modules in a plan;
  core modules are in every subscription and are never listed (the domain refuses them). A plan code is unique among live
  plans, so a deleted plan's code can be reused.
- **`PriceBook`** (`PriceBooks`): one per country (`CountryCode`, ISO 3166-1 alpha-2, set once) in one `CurrencyCode`
  (ISO 4217). Unique per country among live books. It is a plain code, not a foreign key to `Countries`, so a book can
  exist before its country is imported and billing never depends on location data.
- **`PriceBookItem`** (`PriceBookItems`): a price for either a plan or a paid module (a check constraint demands exactly
  one), for one `BillingCycle` (`Monthly = 1`, `Yearly = 2`, with a seeded lookup table), as `numeric(18,2)` that is zero
  or more. Unique per book, target and cycle. Core modules cannot be priced.
- A price book prices both plans and add-on modules, so a school can buy a plan plus extra modules in one currency.
- **Seed (Development only):** plans STARTER, STANDARD and FULL, and price books PK/PKR, SA/SAR, AE/AED and US/USD with
  placeholder prices (`Domain/Entities/PricingCatalog.cs`; yearly is ten months). The maintainer edits them in the app;
  they are not real prices. Books are seeded only for countries that exist.
- **API** (master tenant administrators only, antiforgery on unsafe verbs, every change audited as `PlanCreated`,
  `PlanChanged`, `PlanDeleted`, `PriceBookCreated`, `PriceBookChanged`, `PriceBookDeleted`, `PriceChanged`):
  `/api/Plans`, `/api/Plans/{id}`, `/api/PriceBooks`, `/api/PriceBooks/{id}`, `PUT` and `DELETE`
  `/api/PriceBooks/{id}/prices`.
- Deleting a plan removes its prices. A module that a live plan includes or a price book prices cannot be deleted from
  the catalogue until it is removed from them.

## Rationale

- Prices are separate rows, not columns on the plan, so adding a country or a billing cycle is data, not a schema change.
- Money is `decimal` with its currency stored on the book, so an amount is never read without its currency.
- Reference data has no `SchoolId`: only the master tenant changes it, and schools read their price through
  subscriptions (phase 3), never the books.

## Consequences

**Easier:** subscription pricing (phase 3) is a lookup of book by the school's country and item by plan or module and
cycle.

**Harder:** changing a book's currency does not convert its prices; the master re-enters them. A price change does not
touch existing subscriptions: phase 3 copies the price and currency onto each subscription and invoice.
