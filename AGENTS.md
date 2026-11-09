# AGENTS.md: Education Portal

This file tells AI coding assistants (Claude, ChatGPT/Codex, Gemini, GitHub Copilot, Cursor, and others) what this project is and how to work in it. It follows the open [AGENTS.md](https://agents.md) convention. `CLAUDE.md`, `GEMINI.md` and `.github/copilot-instructions.md` only point here, so **this file is the single source of truth**. When a decision changes, update this file in the same PR.

> **Status (2026-11-09):** planning is done, no feature code yet. The source is still the **raw** Jason Taylor *Clean Architecture Solution Template* (.NET 10 + Aspire): it still has template switches such as `<!--#if (UsePostgreSQL)-->`, `CleanArchitecture` names, and the Todo/Weather samples. Step 1 of the plan (section 8) turns it into a real `EducationPortal` solution. Items marked **TODO** are undecided: do not invent answers, ask the maintainer or leave them as TODO.

---

## 1. Product

A multi-tenant **Education Portal** for schools, built as an **MVP**. Prefer the simplest thing that works and defer anything not listed here.

- **Core entities:** School (tenant), Branch, Student, Teacher, Parent, Admin/staff user, Course, Enrolment, Grade.
- **Architecture:** one Clean Architecture solution (modular monolith hosted by Aspire), same shape as the template. **Not microservices** for the MVP.
- **Frontend:** Angular (the maintainer already has an Angular app).
- **Database:** **PostgreSQL only** (decided 2026-11-09), one shared database for all schools. SQL Server and SQLite code from the template is removed; do not add it back.

## 2. Multi-tenancy and permissions

### Tenant levels

```
Master tenant (the platform owner, i.e. us)
  └── School (school-owner tenant)
        └── Branch (one or more per school)
```

- **Master tenant** sees and oversees every school and branch, and runs the master portal.
- **School owner** manages only their own school, its branches, roles and staff.
- **Branch** is where day-to-day data lives: students, teachers, courses, enrolments, grades.

### Data isolation

- All schools share one database. Every school-owned row has a `SchoolId`; branch-scoped rows also have a `BranchId`.
- Scoping is applied by **EF Core global query filters** fed by the current-user service, never by hand in each query or endpoint.
- **Master-tenant users bypass the filters** and can read across all schools.
- The current school comes from the logged-in user's claims, not from a subdomain or header.
- Database-per-school is out of scope (it makes cross-school overview and migrations harder).

### Modules and permissions

Effective access for a school user = **modules enabled for their school** ∩ **rights their role gives per module** ∩ **branches assigned to them**.

1. **Modules per school (master controls):** from the master portal, the master tenant switches modules (Students, Teachers, Parents, Courses, Enrolments, Grades, ...) on or off for each school. A disabled module is unavailable to everyone in that school.
2. **Roles per school (owner controls):** each school owner creates roles with **View / Create / Edit / Delete** rights per module.
3. **Branches per user:** each staff user is assigned to one or more branches and can only act on data in those branches.
4. **Fixed rules:** teachers grade only their own courses; parents see only their own children; students and parents have read-only access.

### Rules for code

- Check module switches and role rights **on the server**, in the Application layer (e.g. a MediatR authorization behaviour / attributes). Hiding things in Angular is not enough: a direct API call to a disabled module or a branch the user doesn't have must be rejected.
- Every new tenant-owned entity gets `SchoolId` (and `BranchId` if branch-scoped) and is covered by the query filters.
- Every new use case gets a test proving that another school, another branch, and a disabled module are denied.

## 3. Tech stack (from the template files)

| Area | Choice |
|---|---|
| Runtime | .NET 10 (`net10.0`), C#, `Nullable` and `ImplicitUsings` on |
| Build | `TreatWarningsAsErrors=true`; output in `./artifacts` (`Directory.Build.props`) |
| Packages | Central Package Management with transitive pinning (`Directory.Packages.props`) |
| Solution | `.slnx` format (needs a recent .NET SDK / VS 2022 17.13+) |
| Orchestration | .NET Aspire 13.6; `src/AppHost` is the startup project |
| API | ASP.NET Core 10 minimal APIs, OpenAPI + Scalar UI |
| Application | MediatR 14 (CQRS), FluentValidation 12, AutoMapper 16, Ardalis.GuardClauses |
| Data | EF Core 10 + ASP.NET Core Identity, PostgreSQL (Npgsql) |
| Frontend | Angular |
| Observability | OpenTelemetry, health checks, resilience, service discovery (`src/ServiceDefaults`) |
| Cloud | Aspire Azure Container Apps + Key Vault packages are referenced. Actual hosting: **TODO** |
| Tests | NUnit, Shouldly, Moq, Respawn, `Aspire.Hosting.Testing`; Playwright + Reqnroll for acceptance tests |

Package versions live **only** in `Directory.Packages.props`. In a `.csproj`, write `<PackageReference Include="X" />` with no `Version`.

## 4. Solution layout

Expected after step 1 (names change from `CleanArchitecture` to `EducationPortal`):

```
src/
  AppHost/            Aspire orchestrator (startup): PostgreSQL, API, Angular
  ServiceDefaults/    OpenTelemetry, health checks, resilience, service discovery
  Domain/             Entities, value objects, domain events, enums. No dependencies.
  Application/        Use cases (MediatR commands/queries), validators, DTOs, interfaces
  Infrastructure/     EF Core DbContext, query filters, migrations, Identity
  Shared/             Constants shared across projects (e.g. Aspire resource names)
  Web/                ASP.NET Core host: endpoints, DI composition, Angular client
tests/
  Domain.UnitTests/
  Application.UnitTests/
  Application.FunctionalTests/      Use cases against a real DB (Respawn resets it)
  Infrastructure.IntegrationTests/
  Web.AcceptanceTests/              Playwright + Reqnroll
  TestAppHost/                      Aspire host used by tests
```

**Dependency rule:** `Domain` <- `Application` <- `Infrastructure` and `Web`.

- `Domain` references nothing else in the solution.
- `Application` references only `Domain` and declares interfaces (e.g. `IApplicationDbContext`, `IUser`) that `Infrastructure` implements.
- `Web` composes everything; business logic does not go in endpoints.
- Never reference `Infrastructure` from `Application` or `Domain`.

## 5. Build, run, test

Prerequisites: .NET 10 SDK, Docker or Podman (for PostgreSQL), Node.js LTS (for Angular), Aspire CLI (optional).

```bash
dotnet build                               # warnings fail the build
aspire run                                 # or: dotnet run --project src/AppHost
dotnet test                                # all tests
dotnet test tests/Application.UnitTests    # one project
```

The Aspire dashboard shows service URLs and logs; API docs are served by Scalar.

EF Core migration:

```bash
dotnet ef migrations add <Name> --project src/Infrastructure --startup-project src/Web --output-dir Data/Migrations
```

Scaffold a use case (run inside `src/Application`):

```bash
dotnet new ca-usecase -n CreateCourse -fn Courses -ut command -rt int
dotnet new ca-usecase -n GetCourses  -fn Courses -ut query   -rt CoursesVm
```

## 6. Conventions

- **Feature folders:** `Application/<Feature>/Commands/<UseCase>/` and `Application/<Feature>/Queries/<UseCase>/`, each with its handler and FluentValidation validator.
- **Endpoints:** one file per feature in `Web/Endpoints/<Feature>.cs`, sending MediatR requests. Keep them thin.
- **Validation** lives in FluentValidation validators run by the MediatR pipeline, not in endpoints.
- **Entities** live in `Domain/Entities` and inherit the template's base/auditable entity. Raise domain events instead of calling services from entities.
- **Grades** are several scored assessments per enrolment, not one final grade.
- **Accounts** are created by staff; there is no public sign-up.
- Nullable is on and warnings are errors: fix warnings rather than suppressing them; if you must suppress, add a comment explaining why.
- Follow the template's `.editorconfig`.
- New NuGet package: add its version to `Directory.Packages.props` and say why in the PR.
- Tests use NUnit and Shouldly. New use cases get an `Application.FunctionalTests` test (including the tenant/branch/module denial cases above).
- No secrets in the repo: user-secrets locally, Key Vault when deployed.
- Never commit `artifacts/`, `bin/`, `obj/`, `node_modules/` or `*.db`.

## 7. How AI assistants should work here

1. Read this file, then the code. If they disagree, trust the code and fix this file in the same change.
2. Do not invent requirements or choices marked TODO. Ask, or propose them in the PR description.
3. Work in small layered steps: Domain, then Application use case, then Infrastructure, then Web endpoint, then Angular, then tests.
4. Run `dotnet build` and `dotnet test` before saying a change works.
5. Never weaken tenant isolation (removing query filters, skipping permission checks) to make something work.

## 8. MVP plan

Each step is one reviewable PR.

1. **Baseline:** turn the template into an `EducationPortal` solution with Angular and PostgreSQL only (remove SQL Server/SQLite code and the `#if` switches), remove the Todo/Weather samples, get build and tests green.
2. **Schools, branches and permissions:** API and Angular screens for creating schools, switching modules per school (master portal), adding branches, roles and staff. Tests prove schools and branches can't see each other's data.
3. **Academics:** students, teachers, parents, courses, enrolments and grades, all respecting modules and branches.
4. **Master overview and polish:** master dashboard across all schools and branches, demo data, smoke tests per role.

**Out of scope for the MVP:** fees, attendance, timetables, billing schools, microservices, database-per-school, subdomain-per-school.

## 9. Open decisions (TODO)

- [ ] Default branch: the repo has no `main` branch yet
- [ ] Authentication: template ASP.NET Core Identity vs. an external IdP (Entra ID, Keycloak, ...)
- [ ] Hosting and CI/CD (Azure Container Apps via `azd`? GitHub Actions?)
- [ ] Whether a student or parent can belong to more than one branch
- [ ] Final module list beyond the core entities

## 10. References

- Template docs: https://cleanarchitecture.jasontaylor.dev
- Template source: https://github.com/jasontaylordev/CleanArchitecture
- Aspire: https://aspire.dev
