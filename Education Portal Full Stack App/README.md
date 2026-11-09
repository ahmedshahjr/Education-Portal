# Education Portal

A multi-tenant education portal: the platform owner (master tenant) oversees every school, each school owner manages their own school and its branches, and students, teachers and parents use the portal within their school.

Built on the [Clean Architecture Solution Template](https://github.com/jasontaylordev/CleanArchitecture) with:

- ASP.NET Core 10 Web API (minimal APIs, MediatR, FluentValidation, AutoMapper)
- Angular frontend (`src/Web/ClientApp`)
- PostgreSQL with EF Core and ASP.NET Core Identity
- .NET Aspire for local orchestration, telemetry and tests

See [`AGENTS.md`](../AGENTS.md) at the repository root for the product decisions, the multi-tenancy rules and the coding conventions.

## Prerequisites

- .NET 10 SDK (see `global.json`)
- Docker Desktop or Podman, because Aspire runs PostgreSQL in a container
- Node.js LTS, for the Angular app

## Build

Run `dotnet build` to build the solution. Warnings are treated as errors.

## Run

Using the [Aspire CLI](https://aspire.dev/get-started/install-cli/) (recommended):

```bash
aspire run
```

Or using the .NET CLI:

```bash
dotnet run --project ./src/AppHost
```

The Aspire dashboard opens automatically and shows the API, the Angular app, PostgreSQL and their logs. The API reference is served by Scalar at `/scalar`.

In Development the database is recreated on every start and seeded with an administrator account (`administrator@localhost` / `Administrator1!`).

## Test

The solution contains unit, integration, functional and acceptance tests. Functional tests start PostgreSQL through Aspire, so Docker must be running.

```bash
dotnet test
```

## Code Styles & Formatting

The [EditorConfig](https://editorconfig.org/) file (`.editorconfig`) defines the coding styles for this solution.

## Code Scaffolding

The template can scaffold new commands and queries. Install it once:

```bash
dotnet new install Clean.Architecture.Solution.Template
```

Then, from the `./src/Application/` folder:

```bash
dotnet new ca-usecase --name CreateCourse --feature-name Courses --usecase-type command --return-type int
dotnet new ca-usecase -n GetCourses -fn Courses -ut query -rt CoursesVm
```

## Architectural Decisions

Key design decisions are documented as [Architecture Decision Records](docs/decisions/).
