# Education Portal solution

This folder holds the .NET solution (`EducationPortal.slnx`). Run every `dotnet` command from here.

Built on the [Clean Architecture Solution Template](https://github.com/jasontaylordev/CleanArchitecture) with ASP.NET Core 10, Angular (`src/Web/ClientApp`), PostgreSQL with EF Core and ASP.NET Core Identity, and .NET Aspire.

- **How to run it, the architecture and the development process:** the [project README](../README.md).
- **Decisions, multi-tenancy rules and conventions:** [AGENTS.md](../AGENTS.md).
- **Mandatory security and compliance rules:** [docs/ai/secure-compliance/SKILL.md](../docs/ai/secure-compliance/SKILL.md).
- **Architecture decision records:** [docs/decisions](docs/decisions/README.md).

## Quick commands

```bash
dotnet build                        # warnings fail the build
dotnet run --project src/AppHost    # or: aspire run (Docker must be running)
dotnet test                         # functional and acceptance tests need Docker
```

## Code style

The [EditorConfig](https://editorconfig.org/) file (`.editorconfig`) defines the coding style for this solution.

## Code scaffolding

The template can scaffold new commands and queries. Install it once:

```bash
dotnet new install Clean.Architecture.Solution.Template
```

Then, from the `./src/Application/` folder:

```bash
dotnet new ca-usecase --name CreateCourse --feature-name Courses --usecase-type command --return-type int
dotnet new ca-usecase -n GetCourses -fn Courses -ut query -rt CoursesVm
```
