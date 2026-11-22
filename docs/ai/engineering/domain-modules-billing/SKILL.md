---
name: domain-modules-billing
description: Loader for the sellable module catalogue, plans and regional price books, subscriptions and tenant modules, module enforcement, school roles and rights, invoices and manual payments, add-on requests, the daily subscription lifecycle job, and the change audit of commercial records. Says where the code lives, which ADR decides each part, and which rules and tests a change must keep. Use when a ticket touches modules, ModuleCode, plans, prices, subscriptions, trials, invoices, payments, add-on requests, role rights per module, the lifecycle job, or the change audit. Part of the mandatory Education Portal engineering rules.
---

# Modules, subscriptions and billing (MUST follow)

Part of the mandatory engineering rules. Also follow the [overview skill](../SKILL.md): its precedence, exceptions and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose an alternative. This is a map, not a copy: the decisions and the long text live in `AGENTS.md` section 2 and the ADRs below, which win if this skill disagrees. Read the ADR for the part you change before editing it. Security rules win over this skill, in particular the [authorization-tenant-isolation skill](../../secure-compliance/authorization-tenant-isolation/SKILL.md) and, for card payments, the [PCI DSS skill](../../secure-compliance/standard-pci-dss/SKILL.md).

## Where each part lives

| Part | Decision | Code (under `Education Portal Full Stack App/src/`) |
|---|---|---|
| Module catalogue: fixed `ModuleCode` enum, core or paid, `IsDefaultEnabled`, dependencies | ADR-009, ADR-017 | `Domain/Enums/ModuleCode.cs`, `Domain/Entities/ModuleCatalog.cs`, `ModuleDependencyGraph.cs`; `/api/Modules` |
| Plans and price books per country | ADR-010 | `Plan`, `PlanModule`, `PriceBook`, `PriceBookItem`, seed in `PricingCatalog.cs`; `/api/Plans`, `/api/PriceBooks` |
| Subscriptions and tenant modules | ADR-011 | `Subscription`, `TenantModule`, `SubscriptionPlanner`; `/api/Schools/{id}/subscription` |
| Module enforcement and school roles | ADR-012, ADR-018, ADR-022 | `IModuleAccess`, `IModuleEntitlementStore`, `SchoolRole`, `RolePermission`; `/api/SchoolRoles` |
| Invoices and manual payments | ADR-013 | `Invoice`, `InvoiceLine`, `Payment`, `IPaymentProvider`, `ManualPaymentProvider`; `/api/Invoices` |
| Subscription changes and add-on requests | ADR-014 | `SubscriptionChanges`, `Subscription.Reconfigure`, `AddOnRequest`; `/api/AddOnRequests`, `/api/Subscriptions/mine` |
| Lifecycle job (trials, overdue, grace, reminders) | ADR-015 | `SubscriptionLifecycleJob` (Quartz.NET, UTC) |
| Change audit of commercial records | ADR-016 | `ChangeAuditInterceptor`, `ChangeAuditEntry` |
| School registration and approval | ADR-017, ADR-020 | school request and registration use cases |
| Tenant above school | ADR-021 | `Tenant` |

## Rules

| ID | Rule |
|---|---|
| ENG-DOMAIN-01 | **The catalogue is fixed in code.** A new module is added to the `ModuleCode` enum (explicit number, ADR-008), to `ModuleCatalog` and to the module table in `AGENTS.md`, in that order, with its dependencies and a lookup-table seed. The master tenant cannot invent a module through the API; there is no create or delete endpoint. |
| ENG-DOMAIN-02 | **Check the module on the server.** Declare it on the use case with `[Authorize(Module = ModuleCode.X)]` so the request rules (`IRequestRules.CheckAsync`, called by the controller action) refuse with 403 and `MODULE_NOT_ENABLED` when it is off. In Angular, give menu items `module:` and routes `moduleGuard(...)`. Hiding things in Angular is never enough. The master tenant itself is never limited by modules. |
| ENG-DOMAIN-03 | **Entitlement is computed, not stored per request.** Use `IModuleAccess.HasModuleAsync(schoolId, code, campusId)`; no campus asked means fail closed. Core modules are on for every school unless the master switched them off globally; paid modules are on only through Active `TenantModule` rows inside `ValidFrom` and `ValidTo`. Cached entitlements are dropped through `IModuleEntitlementStore` when a subscription or the catalogue changes: a change that edits those tables must do the same. Data of a module that is off is hidden and kept, never deleted. |
| ENG-DOMAIN-04 | **Dependencies and loops.** A module may require others (`ModuleDependencies`); a loop or a module requiring itself is refused by `ModuleDependencyGraph`; a core module may require only core modules. Subscription changes expire modules that leave and add what a module requires, in one transaction through `SubscriptionPlanner` or `SubscriptionChanges`. |
| ENG-DOMAIN-05 | **Money is exact and copied.** Prices, currency and tax rate are copied onto the subscription and invoice when issued (the school's `TaxRateOverride`, else the country's `PriceBook.TaxRate`), so a later price change never rewrites history. Amounts use `decimal` with the country's currency decimals, never `double`. One open invoice per subscription and a period is invoiced once. |
| ENG-DOMAIN-06 | **Payments go through `IPaymentProvider`.** Only manual payments exist (bank transfer, cash, cheque, wallet). Adding a card or gateway needs a merchant-model decision from the maintainer first and follows the PCI DSS skill (card data never reaches our servers). Never add a gateway on your own. |
| ENG-DOMAIN-07 | **Registration and approval are transactional.** Creating a school with its administrator, its default modules and its trial runs in one transaction (`ExecuteInTransactionAsync`). Registration requires a country that has a price book. A school request becomes a school only when the master approves it. |
| ENG-DOMAIN-08 | **Scheduled jobs follow the lifecycle pattern**: own scope, master tenant set on it, idempotent rules (safe to run twice or on several instances), UTC, one save, cron in configuration. Tests set `SubscriptionLifecycle:Enabled` false and call `SubscriptionLifecycleJob.RunAsMasterAsync` with a chosen time. A job never deletes customer data; suspension keeps it (ADR-015). |
| ENG-DOMAIN-09 | **Audit every commercial change.** A new commercial entity is added to `ChangeAuditInterceptor.AuditedTypes`. `ExecuteUpdate` and raw SQL bypass the interceptor, so they must not touch those tables without writing the entries (ENG-SQL-10). Master-only actions raise the matching audit event (`SubscriptionChanged` and the like). |
| ENG-DOMAIN-10 | **Roles are the school's, system roles are protected.** A school's roles carry View, Create, Edit, Delete, Approve and Export per module (every right needs View) and, since ADR-022, per campus where the design says so. Rights can be set only in modules the school has now; rights in a module that is off are hidden and kept. The built-in roles (Principal, Teacher, Student, Parent) keep their names and cannot be deleted. Campus access uses `ICampusAccess` (`EnsureCampusAsync`, `GetAllowedCampusIdsAsync`), never a hand-written check. |
| ENG-DOMAIN-11 | **Tests for every change here**: the four denial cases (ENG-TEST-03), plus the boundary cases of dates (trial end, grace period, `ValidTo`), amounts (rounding, tax override, zero price) and state changes (suspend, reactivate, extend, plan change). Master-only endpoints get a test that a school administrator is refused. |
| ENG-DOMAIN-12 | **Docs move with the change.** Update the module table, the section of `AGENTS.md` and the README (tables and diagrams) in the same PR, and add or update an ADR when a decision changes (ENG-PROC-07). Add the user-guide page after the merge (ENG-DOCS-01). |

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [Project decisions behind modules, subscriptions and billing (ADR-009 to ADR-020)](https://github.com/ahmedshahjr/Education-Portal/tree/development/Education%20Portal%20Full%20Stack%20App/docs/decisions) | ADR-009 to ADR-021 as of 2026-11-22 | Automatic: new commits in `ahmedshahjr/Education-Portal/Education Portal Full Stack App/docs/decisions` | 2026-11-22 |
<!-- END GENERATED: sources -->
