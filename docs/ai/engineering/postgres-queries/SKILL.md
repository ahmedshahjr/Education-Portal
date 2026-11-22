---
name: postgres-queries
description: Rules for writing and running PostgreSQL queries against the Education Portal database, both ad-hoc SQL for investigation (psql, a database tool or an MCP server) and raw SQL inside code (FromSql, ExecuteSql). Covers read-only by default, local development database only, quoted PascalCase identifiers in feature schemas, doing the tenant and soft-delete filtering by hand because raw SQL bypasses EF Core query filters, Guid public ids and numeric enums, parameters, no personal data in output, and EXPLAIN. Use when a task needs a SQL query, a database investigation, raw SQL, an index or a performance question. Part of the mandatory Education Portal engineering rules.
---

# PostgreSQL queries (MUST follow)

Part of the mandatory engineering rules. Also follow the [overview skill](../SKILL.md): its precedence, exceptions and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose an alternative. Normal data access in code is LINQ through EF Core and follows the [efcore skill](../efcore/SKILL.md); this skill covers SQL you write yourself.

## What the database looks like

- **PostgreSQL only**, one shared database for all schools (AGENTS.md section 1). Locally it is an Aspire container on the fixed port `15432` (`src/AppHost/LocalPorts.cs`).
- **One PostgreSQL schema per feature**, set in `Infrastructure/Data/DatabaseSchemas.cs`: `identity`, `access`, `schools`, `modules`, `billing`, `audit`, `lookups`, `admissions`, `academics`, `communication`. A table is `schema."Table"`, for example `schools."Campuses"`.
- **Names are PascalCase and case-sensitive**, so tables and columns must be double-quoted (`"SchoolId"`, not `schoolid`). There is no snake_case convention.
- Every entity has an internal `int Id` and a `Guid "PublicId"`. Enums are stored as numbers; each has a lookup table in the `lookups` schema seeded from the enum (ADR-008).
- Tenant-owned rows have `"SchoolId"`, and campus-scoped ones `"CampusId"`. Deletable entities (`School`, `Country`, and others that inherit `BaseSoftDeletableEntity`) have `"IsDeleted"`, `"DeletedAt"`, `"DeletedBy"`. Every entity has `"Created"`, `"CreatedBy"`, `"LastModified"`, `"LastModifiedBy"`.

## Rules

| ID | Rule |
|---|---|
| ENG-SQL-01 | **Read-only by default.** Ad-hoc SQL is `SELECT`, `EXPLAIN` or `SHOW` against the **local development database only**. Never `INSERT`, `UPDATE`, `DELETE`, `TRUNCATE` or DDL by hand: data comes from the app or the Development seed, and schema changes come from an EF Core migration (SEC-CFG migrations rule). Never connect to a staging or production database from an AI session. |
| ENG-SQL-02 | **Look before you query.** Confirm a table or column name in `information_schema.columns` (or the entity configuration) instead of guessing, and quote every identifier. Start with `SELECT count(*)`, then select only the columns you need with a `LIMIT`. |
| ENG-SQL-03 | **Raw SQL has no query filters.** `FromSql` and hand-written SQL bypass the EF Core global filters, so the query must add the filter itself: `"SchoolId" = @schoolId` (taken from `ICurrentTenant`, never from the caller), `"IsDeleted" = false` for soft-deletable tables, and campus access through `ICampusAccess`. Raw SQL in code needs a reason in the PR, and the four denial tests from the [testing skill](../testing/SKILL.md) (ENG-TEST-03). Master-only cross-school reads say so in a comment. |
| ENG-SQL-04 | **Parameters, never concatenation.** In code use `FromSql($"...")` or `ExecuteSql($"...")` with interpolation (which parameterises), never `FromSqlRaw` or `ExecuteSqlRaw` with built strings (SEC-INPUT). Ad-hoc values go in as literals you typed yourself, never copied from user input or logs. |
| ENG-SQL-05 | **Ids and enums.** Filter and join public data by `"PublicId"` (Guid); the `int "Id"` is internal and never leaves the query result or an API. Read an enum column by joining its lookup table in `lookups` to get the name, not by guessing the number. |
| ENG-SQL-06 | **No personal data leaves the query.** Do not select names, emails, phone numbers, dates of birth, addresses or grades unless the task needs them, and never paste such rows into a chat, ticket, PR, log, Ruflo memory or test (SEC-PII, student-data-privacy skill). Prefer counts, ids and aggregates. Never print a password hash, token, secret value or connection string. |
| ENG-SQL-07 | **Credentials stay out of the repo and the chat.** The local connection comes from the AppHost parameters and user-secrets (SEC-SECRET-04); a database tool or MCP server is configured in the user's own settings. If it fails to connect, report that instead of searching for or guessing a password. |
| ENG-SQL-08 | **Performance work uses `EXPLAIN`.** Use `EXPLAIN (ANALYZE, BUFFERS)` only on a `SELECT` against the development database (`ANALYZE` runs the statement). Read the plan before adding an index. An index is added through a migration, with the tenant column first in a composite index when queries filter by school, and with a test or measurement that shows the need (KISS, YAGNI). |
| ENG-SQL-09 | **Page large results.** Use keyset pagination (`WHERE "Id" > @last ORDER BY "Id" LIMIT n`) for big tables instead of deep `OFFSET`. Never `SELECT *` into code. |
| ENG-SQL-10 | **Bulk statements bypass the audit.** `ExecuteUpdate`, `ExecuteDelete` and raw `UPDATE` or `DELETE` skip `ChangeAuditInterceptor` and `AuditableEntityInterceptor`. They must not touch the audited commercial tables (ADR-016) unless the code writes the audit entries itself, and they are never used to hard-delete rows that soft delete protects. |
| ENG-SQL-11 | **Say what you ran.** In a report or PR, name the query, the database (local development) and the row counts, and say what was not checked. Do not present a guess about a table as a fact. |

## Starter queries (local development database)

List tables and columns before writing anything else:

```sql
SELECT table_schema, table_name, column_name, data_type
FROM information_schema.columns
WHERE table_schema IN ('schools', 'access', 'modules', 'billing')
ORDER BY table_schema, table_name, ordinal_position;
```

Count a school's rows, never reading personal fields:

```sql
SELECT count(*)
FROM schools."Campuses"
WHERE "SchoolId" = 1 AND "IsDeleted" = false;
```

Check that a table has the tenant column the isolation tests expect:

```sql
SELECT table_schema, table_name
FROM information_schema.columns
WHERE column_name = 'SchoolId' AND table_schema <> 'lookups'
ORDER BY 1, 2;
```

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [PostgreSQL documentation (SQL, EXPLAIN, information_schema, indexes)](https://www.postgresql.org/docs/current/) | PostgreSQL current (18) | Manual: listed for a portal check | 2026-11-22 |
| [Npgsql Entity Framework Core provider (raw SQL, naming, schemas)](https://www.npgsql.org/efcore/) | Npgsql EF Core 10 | Manual: listed for a portal check | 2026-11-22 |
<!-- END GENERATED: sources -->
