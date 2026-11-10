---
name: efcore
description: EF Core 10 with PostgreSQL best-practice rules from Microsoft's official EF Core docs (IApplicationDbContext in handlers, no-tracking projections, avoiding N+1 and cartesian explosion, pagination, async with cancellation, Fluent API configurations, global query filters for tenants, migrations, concurrency, bulk updates). Use when writing a query, a command handler that saves data, an entity configuration, a query filter or a migration. Part of the mandatory Education Portal engineering rules.
---

# EF Core best practices (MUST follow)

Part of the mandatory engineering rules. Also follow the [overview skill](../SKILL.md): its precedence, exceptions and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose an alternative.

Based on Microsoft's EF Core documentation: [efficient querying](https://learn.microsoft.com/ef/core/performance/efficient-querying), [efficient updating](https://learn.microsoft.com/ef/core/performance/efficient-updating), [single vs split queries](https://learn.microsoft.com/ef/core/querying/single-split-queries), [tracking vs no-tracking](https://learn.microsoft.com/ef/core/querying/tracking), [global query filters](https://learn.microsoft.com/ef/core/querying/filters), [multi-tenancy](https://learn.microsoft.com/ef/core/miscellaneous/multitenancy), [concurrency](https://learn.microsoft.com/ef/core/saving/concurrency), [migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/), [testing strategy](https://learn.microsoft.com/ef/core/testing/choosing-a-testing-strategy) and the [Npgsql EF Core provider](https://www.npgsql.org/efcore/). Security rules for queries (raw SQL, `IgnoreQueryFilters`) are in the [compliance skills](../../secure-compliance/SKILL.md) and take precedence.

## Data access shape

| ID | Rule |
|---|---|
| ENG-EF-01 | Handlers use `IApplicationDbContext` and its `DbSet<T>` properties directly (ADR-001). Do not add generic repositories or unit-of-work wrappers over EF Core. One `SaveChangesAsync` per command, so a command is one transaction (SEC-ERR-03). |
| ENG-EF-02 | `DbContext` is scoped (one per request) and never shared across threads, cached in a singleton or used in parallel `Task.WhenAll` queries. Background work creates its own scope. |

## Queries

| ID | Rule |
|---|---|
| ENG-EF-03 | **Read-only queries project.** Queries return DTOs/view models with `Select(...)` or AutoMapper `ProjectTo<T>(...)`, loading only the columns needed. When you must load entities you will not change, use `AsNoTracking()`. Track only in commands that modify the loaded entities. |
| ENG-EF-04 | **No N+1 and no lazy loading.** Lazy-loading proxies stay off. Load related data with projection or explicit `Include`/`ThenInclude` in the same query; never query inside a loop over results. Use `AsSplitQuery()` when including more than one collection navigation to avoid cartesian explosion. |
| ENG-EF-05 | **Filter, sort and page in the database.** Keep `Where`, `OrderBy`, `Skip`/`Take` on `IQueryable` before materialising (`ToListAsync`). Never call `ToList()`/`AsEnumerable()` and then filter in memory. Every list endpoint is paginated with a stable `OrderBy` and a maximum page size; prefer keyset pagination for large tables. |
| ENG-EF-06 | **Async with cancellation.** Use the async operators (`ToListAsync`, `FirstOrDefaultAsync`, `AnyAsync`, `SaveChangesAsync`, ...) and pass the request's `CancellationToken`. Never block with `.Result`, `.Wait()` or `.GetAwaiter().GetResult()`. |
| ENG-EF-07 | Check existence with `AnyAsync`, count with `CountAsync`, and fetch one row with `FirstOrDefaultAsync`/`SingleOrDefaultAsync` on a key or unique index. Do not load a collection to check `.Any()` or `.Count`. |
| ENG-EF-08 | Write queries in LINQ. When SQL is unavoidable, use `FromSql`/`SqlQuery`/`ExecuteSql` with interpolated parameters only (SEC-INPUT-02). Use `EF.Functions.ILike` and other Npgsql functions instead of client-side string work. |

## Model configuration

| ID | Rule |
|---|---|
| ENG-EF-09 | All mapping is Fluent API in one `IEntityTypeConfiguration<T>` per entity in `src/Infrastructure/Data/Configurations` (applied by `ApplyConfigurationsFromAssembly`). Configure: max length for every string, required/optional matching nullability, indexes for every foreign key and every column used in filters or sorting, unique indexes for natural keys, enums with `HasConversion<string>()`, value objects as owned or complex types, backing fields for read-only collections, explicit delete behaviour (`Restrict` by default for tenant data). |
| ENG-EF-10 | **Tenant filters.** Every school-owned entity gets a global query filter on `SchoolId` (and `BranchId` where branch-scoped) fed by the current-user/tenant service, defined once in the `DbContext`, never by hand per query (AGENTS.md section 2, SEC-AUTHZ-03). Include `SchoolId` as the leading column of composite indexes on tenant tables. |
| ENG-EF-11 | **Concurrency.** Entities that several users can edit (grades, enrolments, roles) use optimistic concurrency with PostgreSQL's `xmin` system column (Npgsql `IsRowVersion()` on a `uint Version` property). Handle `DbUpdateConcurrencyException` by returning 409 Conflict, never by retrying blindly. |

## Writes and schema

| ID | Rule |
|---|---|
| ENG-EF-12 | Change entities through their methods, then `SaveChangesAsync` once. For set-based changes that need no domain logic or events, use `ExecuteUpdateAsync`/`ExecuteDeleteAsync` and remember they bypass interceptors, auditing and domain events, so do not use them on audited or event-raising entities. |
| ENG-EF-13 | **Migrations.** Once migrations exist (AGENTS.md section 5), every model change ships with a migration generated by `dotnet ef migrations add` in the same PR. Review the generated SQL (`dotnet ef migrations script`) for data loss; never edit a migration that has reached `development`; destructive changes (drop/rename column) are split into expand and contract steps with a rollback plan (SEC-CHG-06). Production schemas are never created with `EnsureCreated`. |
| ENG-EF-14 | **Testing.** Query and command behaviour is tested against real PostgreSQL in `Application.FunctionalTests` (ADR-002), not the EF Core in-memory provider or mocked `DbSet`s. |
| ENG-EF-15 | Do not log SQL with parameter values outside Development (`EnableSensitiveDataLogging` only under `IsDevelopment()`, SEC-LOG-02). |

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [Microsoft EF Core documentation (performance, querying, modeling, saving, migrations, testing)](https://learn.microsoft.com/ef/core/) | EF Core 10 | Automatic: new commits in `dotnet/EntityFramework.Docs/entity-framework/core` | 2026-11-10 |
| [Npgsql EF Core provider documentation (PostgreSQL specifics, xmin concurrency)](https://www.npgsql.org/efcore/) | Npgsql.EntityFrameworkCore.PostgreSQL 10 | Manual: listed for a portal check | 2026-11-10 |
| [Microsoft .NET agent skills: optimizing-ef-core-queries](https://github.com/dotnet/skills) | dotnet/skills plugins/dotnet-data (main) | Automatic: new commits in `dotnet/skills/plugins/dotnet-data/skills/optimizing-ef-core-queries` | 2026-11-10 |
<!-- END GENERATED: sources -->
