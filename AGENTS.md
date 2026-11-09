# AGENTS.md: Education Portal

This file tells AI coding assistants (Claude, ChatGPT/Codex, Gemini, GitHub Copilot, Cursor, and others) what this project is and how to work in it. It follows the open [AGENTS.md](https://agents.md) convention. Tool-specific files (`CLAUDE.md`, `GEMINI.md`, `.github/copilot-instructions.md`) only point back here, so keep this file as the single source of truth.

> **Status (2026-11-09):** the project is at the very start. The repository holds no application code yet. The working copy is an unmodified copy of Jason Taylor's **Clean Architecture Solution Template** (.NET 10 + Aspire) with only the top-level files present; `src/` and `tests/` have not been added. Anything marked **TODO** below is undecided. Do not invent answers for TODO items; ask the maintainer or leave them as TODO.

---

## 1. What the product is

An **Education Portal**: a full-stack web application for education use cases.

- Domain, users, and features: **TODO** (being defined separately; update this section once the plan is agreed).
- Target users (students, teachers, admins, parents?): **TODO**.
- Monolith vs. microservices: **TODO**. The template is a modular monolith hosted by Aspire. The maintainer works mainly with .NET Core microservices, so a later split into services is possible; do not split services without an explicit decision.

## 2. Tech stack (from the template files that exist)

| Area | Choice | Source |
|---|---|---|
| Runtime | .NET 10 (`net10.0`), C# with `Nullable` and `ImplicitUsings` enabled | `Directory.Build.props` |
| Build rules | `TreatWarningsAsErrors=true`; build output goes to `./artifacts` (`ArtifactsPath`) | `Directory.Build.props` |
| Packages | Central Package Management with transitive pinning | `Directory.Packages.props` |
| Solution format | `CleanArchitecture.slnx` (XML solution, needs a recent SDK / VS 2022 17.13+) | |
| Orchestration | .NET Aspire 13.6 (`src/AppHost` is the startup project) | `.slnx` |
| Web API | ASP.NET Core 10, OpenAPI + Scalar UI | packages |
| App layer | MediatR 14 (CQRS), FluentValidation 12, AutoMapper 16, Ardalis.GuardClauses | packages |
| Data | EF Core 10 + ASP.NET Core Identity | packages |
| Database | **TODO**: template supports SQLite (default), PostgreSQL, SQL Server | |
| Frontend | **TODO**: template supports Angular 21, React 19, or API only | |
| Observability | OpenTelemetry (OTLP exporter), health checks, via `src/ServiceDefaults` | packages |
| Resilience / discovery | `Microsoft.Extensions.Http.Resilience`, `Microsoft.Extensions.ServiceDiscovery` | packages |
| Cloud target | Azure Container Apps via Aspire (`Aspire.Hosting.Azure.AppContainers`), Key Vault config | packages. Actual hosting: **TODO** |
| Tests | NUnit 5, Shouldly, Moq, Respawn, `Aspire.Hosting.Testing`; Playwright + Reqnroll for acceptance tests (non-API-only) | packages |

Package versions live **only** in `Directory.Packages.props`. In a `.csproj`, write `<PackageReference Include="X" />` with no `Version`.

The template files still contain template conditionals such as `<!--#if (UsePostgreSQL)-->`. Once the solution is generated with `dotnet new ca-sln ...`, those are resolved. If you see them in a file you edit, the solution has not been generated yet.

## 3. Solution layout (expected, per `CleanArchitecture.slnx`)

```
src/
  AppHost/            Aspire orchestrator (startup project): wires DB, API, frontend
  ServiceDefaults/    Shared OpenTelemetry, health checks, resilience, service discovery
  Domain/             Entities, value objects, domain events, enums. No dependencies.
  Application/        Use cases (MediatR commands/queries), validators, DTOs, interfaces
  Infrastructure/     EF Core DbContext, migrations, Identity, external services
  Shared/             Constants shared across projects (e.g. resource names)
  Web/                ASP.NET Core host: minimal-API endpoints, DI composition, (SPA)
tests/
  Domain.UnitTests/
  Application.UnitTests/
  Application.FunctionalTests/      Run use cases against a real DB (Respawn resets it)
  Infrastructure.IntegrationTests/
  Web.AcceptanceTests/              Playwright + Reqnroll (only if a frontend is used)
  TestAppHost/                      Aspire host used by tests
```

### Dependency rule (Clean Architecture)

`Domain` <- `Application` <- `Infrastructure` and `Web`.

- `Domain` references nothing else in the solution.
- `Application` references only `Domain`. It defines interfaces (e.g. `IApplicationDbContext`, `IUser`) that `Infrastructure` implements.
- `Web` composes everything. Business logic does not go in endpoints.
- Never reference `Infrastructure` from `Application` or `Domain`.

## 4. Build, run, test

Prerequisites: .NET 10 SDK, Aspire CLI (optional), Docker/Podman if the DB is PostgreSQL or SQL Server, Node.js LTS if a SPA frontend is chosen.

```bash
dotnet build                          # warnings fail the build
aspire run                            # or: dotnet run --project src/AppHost
dotnet test                           # all tests
dotnet test tests/Application.UnitTests   # one project
```

The Aspire dashboard opens with service URLs and logs. API docs are served by Scalar.

EF Core migrations (once a provider is chosen):

```bash
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Web --output-dir Data/Migrations
```

Scaffold a new use case (run inside `src/Application`):

```bash
dotnet new ca-usecase -n CreateCourse -fn Courses -ut command -rt int
dotnet new ca-usecase -n GetCourses  -fn Courses -ut query   -rt CoursesVm
```

## 5. Conventions for contributors (human or AI)

- **Feature folders** in `Application`: `Application/<Feature>/Commands/<UseCase>/` and `Application/<Feature>/Queries/<UseCase>/`. Each command/query sits with its handler and its FluentValidation validator.
- **Endpoints**: group endpoints per feature in `Web/Endpoints/<Feature>.cs`, sending MediatR requests. Keep them thin.
- **Validation** belongs in FluentValidation validators, run by the MediatR pipeline. Do not hand-validate in endpoints.
- **Entities** go in `Domain/Entities`, inheriting the template's base entity / auditable base. Raise domain events instead of calling services from entities.
- **Nullable reference types are on** and warnings are errors: fix warnings, do not suppress them without a comment explaining why.
- Follow `.editorconfig` (part of the template; not in the working copy yet).
- New NuGet packages: add the version to `Directory.Packages.props` and explain why in the PR.
- Tests: new use cases get an `Application.FunctionalTests` test; pure logic gets unit tests. Use Shouldly assertions and NUnit.
- Do not commit secrets. Use user-secrets locally and Azure Key Vault in deployed environments.
- Do not commit `artifacts/`, `bin/`, `obj/`, `node_modules/`, or `*.db`.

## 6. How AI assistants should work here

1. Read this file first, then the code. If they disagree, trust the code and update this file in the same change.
2. Do not invent product requirements, entities, or architecture choices that are marked TODO. Ask, or propose them in a PR description.
3. Keep changes small and layered: Domain change, then Application use case, then Infrastructure, then Web endpoint, then tests.
4. Run `dotnet build` and `dotnet test` before saying a change works.
5. When a TODO in this file gets decided, replace it with the decision and the date.

## 7. Open decisions (TODO)

- [ ] Product scope: core features and user roles
- [ ] Frontend: Angular, React, or API only
- [ ] Database: SQLite, PostgreSQL, or SQL Server
- [ ] Monolith vs. microservices (and if microservices: service boundaries, messaging, API gateway)
- [ ] Authentication: template ASP.NET Core Identity vs. an external IdP (Entra ID, Keycloak, etc.)
- [ ] Hosting and CI/CD (Azure Container Apps via `azd`? GitHub Actions?)
- [ ] Generate the solution: `dotnet new ca-sln -cf <angular|react|none> -db <sqlite|postgresql|sqlserver> -o EducationPortal`, then rename namespaces from `CleanArchitecture` to `EducationPortal`

## 8. References

- Template docs: https://cleanarchitecture.jasontaylor.dev
- Template source: https://github.com/jasontaylordev/CleanArchitecture
- Aspire: https://aspire.dev
