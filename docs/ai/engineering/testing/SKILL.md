---
name: testing
description: How to write and run tests in this repo (which test project holds what, NUnit with Shouldly and Moq, functional tests against a real PostgreSQL through Aspire and Respawn, the tenant, campus, module and right denial cases every use case needs, Angular component tests, and the Playwright with Reqnroll whole-flow scenario in Web.AcceptanceTests, with fake data only). Use when adding or changing a use case, a screen or a flow, or when writing or reviewing any test. Part of the mandatory Education Portal engineering rules.
---

# Testing (MUST follow)

Part of the mandatory engineering rules. Also follow the [overview skill](../SKILL.md): its precedence, exceptions and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose an alternative. The security-specific cases are in the [security-testing skill](../../secure-compliance/security-testing/SKILL.md); this skill is about how tests are built and where they live. `ENG-CODE-03` (test behaviour) and `ENG-CODE-10` (backend unit tests) still apply.

## Where a test goes

| Project (under `tests/`) | Holds | Runs against |
|---|---|---|
| `Domain.UnitTests` | Entities, value objects, guard clauses, enums | Nothing: pure code |
| `Application.UnitTests` | Validators, use case handlers, request rules, mappings | Fakes and mocks of the outside world only |
| `Application.FunctionalTests` | A use case end to end, query filters, tenancy, modules, rights | A real PostgreSQL container started by `TestAppHost`; Respawn resets it between tests |
| `Infrastructure.IntegrationTests` | EF Core configuration, interceptors, migrations, providers | A real database |
| `Web.AcceptanceTests` | A user flow in a real browser | The whole app started by Aspire; Playwright and Reqnroll |
| `src/Web/ClientApp` (`*.spec.ts`) | Angular components, services, guards, translations | Vitest with `TestBed` and component harnesses |

## Rules

| ID | Rule |
|---|---|
| ENG-TEST-00 | **Integration and functional tests for every feature (project rule, 2026-11-23).** A feature, backend or frontend, is not done until it has integration tests and functional tests and both pass (ENG-PROC-02, ENG-CODE-10). Backend: `Infrastructure.IntegrationTests` and `Application.FunctionalTests`. Frontend: ENG-TEST-06 and ENG-TEST-07. Do not add unit-test-only files or projects for new code. |
| ENG-TEST-01 | **Test first.** A new use case or a bug fix starts with a failing test that describes the behaviour; the fix is the smallest change that turns it green (ENG-PROC-03, ENG-AGENT-05). Name tests as sentences (`ShouldRejectEnrolmentWhenCourseIsFull`). |
| ENG-TEST-02 | **Every new use case has a functional test** in `Application.FunctionalTests`, in the feature folder that matches it (`Campuses`, `SchoolRoles`, `Billing`...). It covers the valid case, each rejected case and each boundary (empty, exact limit, one over). |
| ENG-TEST-03 | **The four denial cases are mandatory** for any use case on tenant-owned or module-owned data (SEC-AUTHZ-09): (1) another school's data is a 404 or empty, (2) another campus the user does not belong to is a 403, (3) a module that is off for the school is a 403 with `MODULE_NOT_ENABLED`, (4) a role without the needed right is a 403. Write each as its own test so a failure names the missing guard. |
| ENG-TEST-04 | **Real database, mock only the outside world.** Use the functional setup (`FunctionalTestSetup`, `DatabaseResetter`) instead of mocking `IApplicationDbContext`. Moq is for time, identity and external HTTP. Assertions use Shouldly and check observable outcomes: result, saved state, HTTP status, error code, rendered text. |
| ENG-TEST-05 | **Tests are independent and repeatable.** No test depends on another's data or on run order; each starts from the reset database and creates what it needs. No sleeps; wait for a condition. Time comes from `TimeProvider`, never the clock. |
| ENG-TEST-06 | **Angular integration tests (project rule, 2026-11-23).** Every Angular feature has integration specs (`*.spec.ts`) that render the real standalone component with its real services and the real `HttpClient`, with only `HttpTestingController` at the HTTP boundary. They cover the rendered result, the form's validation messages (including the client mirror of every server rule), the module guard, and the right-to-left and translation cases the change touches. A spec that tests one function or one class alone, with its collaborators mocked, is a unit test and is not used for new code. `translations.spec.ts` must pass after `ng extract-i18n`. Vitest with `TestBed` and harnesses (ENG-UI-15). |
| ENG-TEST-07 | **One whole-flow scenario per ticket** in `Web.AcceptanceTests`: a Reqnroll `.feature` file in `Features/`, step classes in `StepDefinitions/`, and a page object in `Pages/` that derives from `BasePage`. The scenario follows the ticket's flow from sign-in on a school domain (`*.localhost`) through every screen to the final visible result, plus one denial scenario (for example a user without the right or a module that is off). Locators use roles and visible text, not CSS paths or generated ids; no fixed waits (Playwright auto-waits). |
| ENG-TEST-08 | **Fake data only.** Test data is invented or generated (Bogus). No real names, emails, phone numbers, student data or production values (SEC-PII). The Development seed account used by the acceptance tests exists only in Development; never reuse its password anywhere else, and never add another real credential to a test (SEC-SECRET-04). |
| ENG-TEST-09 | **Never weaken a test to get green.** No `[Ignore]`, no deleted or loosened assertion, no raised baseline (ENG-ALL-03). A flaky test is fixed or quarantined with an issue link, not retried until it passes. |
| ENG-TEST-10 | **Report the real result.** Say which projects ran, how many tests passed, failed or were skipped, and what could not run (Docker not running means the functional and acceptance tests did not run). Commands: `dotnet test` (all, Docker must be running), `dotnet test tests/Application.UnitTests` (one project), `npx ng test --watch=false` in `src/Web/ClientApp`. |

## Quick checklist for a new use case

- [ ] Unit tests for the validator and handler logic
- [ ] Functional test: valid, rejected and boundary cases
- [ ] Denial tests: other school, other campus, module off, missing right
- [ ] Angular spec for the screen, including validation messages and the module guard
- [ ] One acceptance scenario for the whole flow, with its denial path
- [ ] Fake data only; no skipped or loosened tests

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [NUnit documentation (assertions, setup fixtures)](https://docs.nunit.org/) | NUnit 4 | Manual: listed for a portal check | 2026-11-22 |
| [Playwright for .NET (locators, auto-waiting, best practices)](https://playwright.dev/dotnet/docs/best-practices) | Playwright for .NET, docs as read on 2026-11-22 | Manual: listed for a portal check | 2026-11-22 |
| [Reqnroll documentation (feature files, bindings)](https://docs.reqnroll.net/) | Reqnroll docs as read on 2026-11-22 | Manual: listed for a portal check | 2026-11-22 |
| [Aspire integration testing with DistributedApplicationTestingBuilder](https://aspire.dev/testing/overview/) | Aspire 13.6 | Manual: listed for a portal check | 2026-11-22 |
<!-- END GENERATED: sources -->
