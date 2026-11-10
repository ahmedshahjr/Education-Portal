---
name: code-quality-process
description: Day-to-day code quality and process rules (readability over cleverness, test behaviour not implementation, fail fast and loud, surgical changes, small focused PRs, definition of done, no refactoring while fixing bugs, dependencies as code you don't control). Use when writing or reviewing code or tests, committing, opening or reviewing a PR, or adding or upgrading a dependency. Part of the mandatory Education Portal engineering rules.
---

# Code quality and process (MUST follow)

Part of the mandatory engineering rules. Also follow the [overview skill](../SKILL.md): its precedence, exceptions and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose an alternative.

## Code quality

| ID | Rule |
|---|---|
| ENG-CODE-01 | **Readability over cleverness.** Code is read far more often than it is written. Use clear, intention-revealing names (`activeEnrolments`, not `list2`; `IsEnrolledIn(course)`, not `Check(c)`), straightforward control flow and small methods. No dense one-liners, nested ternaries, or LINQ chains that need a comment to decode. |
| ENG-CODE-02 | Follow the naming and style in the [Microsoft C# coding conventions](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions) and the repo `.editorconfig` (PascalCase types and members, `_camelCase` private fields, `I` prefix for interfaces, `Async` suffix on async methods, file-scoped namespaces). Angular code follows the [Angular style guide](https://angular.dev/style-guide). Comments explain **why**, not what. |
| ENG-CODE-03 | **Test behaviour, not implementation.** Tests assert observable outcomes (returned result, saved state, raised domain event, HTTP status, rendered text), not private calls or call order. Name tests as sentences describing the behaviour, e.g. `ShouldRejectEnrolmentWhenCourseIsFull`. Prefer functional tests against the real PostgreSQL database (ADR-001, ADR-002) over mocks; mock only the outside world (time, identity, external HTTP). A refactor that keeps behaviour must not break tests. |
| ENG-CODE-04 | **Fail fast.** Validate input at the boundary (FluentValidation in the MediatR pipeline, Guard clauses in entity methods and constructors) and throw as early as the problem is known. Never continue with a half-valid state, `null` where a value is required, or a default that hides a missing value. |
| ENG-CODE-05 | **Fail loud.** Do not catch exceptions you cannot handle. No empty `catch`, no `catch (Exception)` that only logs and continues, no returning `null`/`false` to hide an error. When you do catch, add context and rethrow (`throw;` or a domain exception with the original as `InnerException`), or turn it into a specific ProblemDetails response. Security-sensitive cases also follow SEC-ERR-01/02. |
| ENG-CODE-06 | No dead code: no commented-out code, unused members, unused `using`s, or `TODO` without an issue link or an AGENTS.md TODO. |
| ENG-CODE-07 | **Surgical changes.** Touch only what the task needs. Do not "improve", rename, reformat or reorder adjacent code or unrelated files in the same change. Match the existing style of the file even when you would do it differently. |
| ENG-CODE-08 | Async all the way: async methods take and pass a `CancellationToken`, return `Task`/`ValueTask`, and are awaited. No `async void` (except event handlers), no blocking on async code. |
| ENG-CODE-09 | Nullable reference types and warnings-as-errors stay on. Fix the warning; do not suppress it. A suppression needs a comment explaining why and an engineering or compliance exception. |
| ENG-CODE-10 | **Backend unit tests (maintainer rule, 2026-11-10).** Every new or changed C# class with logic gets unit tests in the matching project in the same PR, as the frontend does with Vitest (ENG-UI-15): entities and value objects in `tests/Domain.UnitTests`, validators, handlers, pipeline behaviours and mappings in `tests/Application.UnitTests`, and anything needing a real database in `tests/Infrastructure.IntegrationTests` or `tests/Application.FunctionalTests`. Cover the valid case, each rejected case and each boundary (empty, exact limit, one over). Tests run in CI (`dotnet test`), and a PR that adds logic without tests is not done (ENG-PROC-02). |

## Process and collaboration

| ID | Rule |
|---|---|
| ENG-PROC-01 | **Small, focused pull requests.** One purpose per PR, into `development`. Aim for under about 400 changed lines of hand-written code (generated files, lockfiles and migrations excluded); split larger work into a sequence of PRs that each build and pass tests (AGENTS.md section 8 steps). |
| ENG-PROC-02 | **Definition of done.** A change is done only when all of these hold: (1) the requirement is met and agreed edge cases are handled; (2) `dotnet build` and `dotnet test` pass, and the Angular build and tests pass if the frontend changed; (3) new behaviour has tests, including the tenant/branch/module denial tests required by SEC-AUTHZ-09; (4) AGENTS.md, README.md, ADRs and skills are updated where the change affects them; (5) the PR checklists are filled in; (6) a human reviewer approved it and CI is green. |
| ENG-PROC-03 | **Don't refactor while fixing bugs.** A bug fix contains the failing test that reproduces the bug and the smallest fix. Refactoring goes in a separate commit, preferably a separate PR, before or after the fix. |
| ENG-PROC-04 | **Commits** are small and self-contained, with a message that says what changed and why (SEC-CHG-05). |
| ENG-PROC-05 | **Treat dependencies as code you don't control.** Before adding a package: check it is actively maintained (recent releases, open issues handled), widely used, licence-compatible, and that the standard library or an existing dependency cannot do the job. Versions are pinned centrally (`Directory.Packages.props`, `package-lock.json`). Before upgrading: read the changelog and breaking changes. Justify every new package in the PR (SEC-DEP-04/05). |
| ENG-PROC-06 | **Reviews** check behaviour, design (ENG-DESIGN), tests and the rules, not personal style the `.editorconfig` already covers. Review comments cite rule IDs. The author resolves every comment by a change or a reply. |
| ENG-PROC-07 | Decisions that change architecture, a convention or a rule get an ADR or an update to AGENTS.md or the relevant skill in the same PR. |

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [Microsoft C# coding conventions and identifier naming](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions) | C# 14 / .NET 10 coding conventions | Automatic: new commits in `dotnet/docs/docs/csharp/fundamentals/coding-style` | 2026-11-10 |
| [Microsoft .NET agent skills: test-anti-patterns](https://github.com/dotnet/skills) | dotnet/skills plugins/dotnet-test (main) | Automatic: new commits in `dotnet/skills/plugins/dotnet-test/skills/test-anti-patterns` | 2026-11-10 |
| [Google engineering practices (code review, small changes)](https://google.github.io/eng-practices/) | google/eng-practices (main) | Automatic: new commits in `google/eng-practices/review` | 2026-11-10 |
<!-- END GENERATED: sources -->
