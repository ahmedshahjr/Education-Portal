---
name: identifiers-enums
description: Rules for entity keys, public ids and enums across the API, Application and database layers (internal long Id plus Guid PublicId alternate key, never exposing the long id, resolving Guid to long in the handler, enums as names on the API and numbers in the database with explicit values and seeded lookup tables). Use when adding or changing an entity, a DTO, a command or query with an id, an endpoint route, an enum, or an enum column.
---

# Identifiers and enums (MUST follow)

Part of the mandatory engineering rules. Also follow the [overview skill](../SKILL.md): its precedence, exceptions and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose an alternative.

Decided by the maintainer on 2026-11-11 for .NET 10 and EF Core 10 with PostgreSQL. These rules replace the earlier "persist enums as strings" and "ids stay `int`" rules (old ENG-ENTITY-10, ENG-ENTITY-11 and ENG-EF-09), which is how the gaps below got into the code.

## Identifiers

| ID | Rule |
|---|---|
| ENG-ID-01 | **Internal key.** Every entity has a `long Id` primary key (PostgreSQL `bigint`, identity), from `BaseEntity`. Foreign keys (`SchoolId`, `BranchId`, `CountryId`, ...), joins, query filters, claims, secret paths and the security audit log use it. |
| ENG-ID-02 | **Public id.** Every entity also has a `Guid PublicId` (version 7, set once when the object is created, `Guid.CreateVersion7()`), mapped as an **alternate key** (unique constraint) for every `BaseEntity` in `ApplicationDbContext`. It is the only identity the API exposes: DTOs, routes (`{id:guid}`), query strings, request bodies and `Created` locations carry the `Guid`, named for what it identifies (`Id`, `SchoolId`, `CountryId`). The `long` never appears in a request or response, the OpenAPI document or the Angular client. |
| ENG-ID-03 | **Translate in the handler.** Endpoints take the `Guid` and pass it to the MediatR request unchanged. The handler (the service layer) resolves it to the `long` before any business logic, with `ResolveIdAsync` (`Application/Common/Models/PublicIdExtensions.cs`) or a query on `PublicId`. Resolution goes through the context's query filters, so a public id from another school is simply not found (404, SEC-AUTHZ-06). Validators check `NotEmpty()` on a `Guid`, never `GreaterThan(0)`. |
| ENG-ID-04 | **A public id is not an access check.** An unguessable `Guid` makes enumeration harder; it does not replace authorization. Every handler still applies the tenant, branch, module and role checks (SEC-AUTHZ-01..06). |
| ENG-ID-05 | **Tests use both ids.** Functional tests set up tenants and users with the `long` (`EntityIds.Id`) and call the API with the `Guid` (`EntityIds.PublicId`). A use case with an id in its route gets a test that the internal `long` is refused. |

ASP.NET Core Identity's own user key (`string`, a GUID) is left as the framework defines it; it is already not sequential.

## Enums

| ID | Rule |
|---|---|
| ENG-ENUM-01 | **Names on the API.** Enums travel by member name in JSON, routes and query strings. `JsonStringEnumConverter(allowIntegerValues: false)` is registered once with `ConfigureHttpJsonOptions` in `Web/DependencyInjection.cs`, so a number in a body is a 400. Route segments that bind an enum use the `alpha` constraint (`{kind:alpha}`), because route and query binding use `Enum.TryParse`, which accepts numbers. Code that parses a name itself uses `EnumNames.TryParse` (`Domain/Common/EnumNames.cs`), never `Enum.TryParse`. DTO properties keep the enum type (strongly typed in C#, a string union in the generated Angular client); validators check `IsInEnum()`. |
| ENG-ENUM-02 | **Explicit numbers.** Every member of every enum (Domain, Application, Infrastructure options) has an explicit value: `Pending = 1, Approved = 2`. Never renumber, reuse or remove a value that has been stored; append new members with the next number. Mark a retired member obsolete in its doc comment instead of deleting it. Use a meaningful zero (`None = 0`) only when "nothing" is a real value; otherwise start at 1 so an unset value (`default`) is invalid and fails `IsInEnum()`. `[Flags]` enums are plural, powers of two, with `None = 0`. |
| ENG-ENUM-03 | **Numbers in the database.** EF Core stores enums as their number (`integer`; use a `byte`-backed enum only when the column size matters). `HasConversion<string>()` and `EnumToStringConverter` are not allowed. Never rename a member either: the name is the API contract (ENG-ENUM-01). |
| ENG-ENUM-04 | **Lookup tables for reports.** Every enum column gets a lookup table seeded from the enum (`EnumLookupTables.Configure` in `Infrastructure/Data/Lookups`): one row per member with its number and name, and a foreign key from the column, so SQL reports and audit reviews can read names and the database refuses a number the enum does not define. Add a line there when an entity gets an enum property. |

## Why (sources)

- EF Core maps enums to a numeric column by default and only stores names when a string conversion is configured ([Value conversions](https://learn.microsoft.com/ef/core/modeling/value-conversions); [EF Core 8 breaking changes](https://learn.microsoft.com/ef/core/what-is-new/ef-core-8.0/breaking-changes): "EF has always, by default, mapped enums to a numeric column in relational databases").
- An alternate key is a second unique identifier backed by a unique constraint ([Keys: alternate keys](https://learn.microsoft.com/ef/core/modeling/keys#alternate-keys)). `Guid.CreateVersion7()` gives time-ordered values, so the unique index grows at its end ([What's new in .NET 9 libraries: System.Guid](https://learn.microsoft.com/dotnet/core/whats-new/dotnet-9/libraries#systemguid)).
- Minimal APIs read and write JSON with the options from `ConfigureHttpJsonOptions` ([Configure JSON serialization options](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-10.0#configure-json-serialization-options)); with a `JsonStringEnumConverter` the OpenAPI schema lists the names ([OpenAPI metadata for enums](https://learn.microsoft.com/aspnet/core/fundamentals/openapi/include-metadata?view=aspnetcore-10.0#include-openapi-metadata-for-data-types)). The converter accepts numbers unless `allowIntegerValues` is `false` ([JsonStringEnumConverter](https://learn.microsoft.com/dotnet/api/system.text.json.serialization.jsonstringenumconverter)). Route, query, header and form binding use `Enum.TryParse`, not JSON ([What's new in ASP.NET Core in .NET 11: OpenAPI](https://learn.microsoft.com/aspnet/core/release-notes/aspnetcore-11#openapi)).
- Validate enum values; the CLR lets any number be cast to an enum ([Parameter design](https://learn.microsoft.com/dotnet/standard/design-guidelines/parameter-design); [Enumeration types](https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/enum#implicit-conversions-from-zero)). Enum shape: [Enum design](https://learn.microsoft.com/dotnet/standard/design-guidelines/enum), [CA1008](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1008), [CA1069](https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1069).
- APIs should not expose internal implementation details or mirror the database ([RESTful web API design](https://learn.microsoft.com/azure/architecture/best-practices/api-design#define-restful-web-api-resource-uris); [Microservices API design](https://learn.microsoft.com/azure/architecture/microservices/design/api-design#restful-api-design)).

PostgreSQL has no clustered indexes (tables are heaps), so the `long` key's benefit here is compact primary and foreign key indexes and cheap joins, not clustering as on SQL Server.

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [Microsoft EF Core documentation (performance, querying, modeling, saving, migrations, testing)](https://learn.microsoft.com/ef/core/) | EF Core 10 | Automatic: new commits in `dotnet/EntityFramework.Docs/entity-framework/core` | 2026-11-10 |
| [Microsoft .NET Framework design guidelines (naming, type and member design)](https://learn.microsoft.com/dotnet/standard/design-guidelines/) | .NET design guidelines (current on Learn) | Automatic: new commits in `dotnet/docs/docs/standard/design-guidelines` | 2026-11-10 |
| [ASP.NET Core minimal APIs: JSON options, parameter binding and OpenAPI metadata (enums as strings)](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis/responses?view=aspnetcore-10.0#configure-json-serialization-options) | ASP.NET Core 10 | Automatic: new commits in `dotnet/AspNetCore.Docs/aspnetcore/fundamentals/minimal-apis` | 2026-11-11 |
| [Azure Architecture Center: best practices for RESTful web API design (do not expose internal implementation or mirror the database)](https://learn.microsoft.com/azure/architecture/best-practices/api-design) | Azure Architecture Center API design guidance (current on Learn) | Automatic: new commits in `MicrosoftDocs/architecture-center/docs/best-practices/api-design.md` | 2026-11-11 |
<!-- END GENERATED: sources -->
