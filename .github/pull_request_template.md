## What and why

<!-- What changes, and why. Link the issue or request. -->

## How it was tested

<!-- Commands run (dotnet build, dotnet test, npm test) and their results. -->

### Review checklist ([AGENTS.md section 7, rule 9](../AGENTS.md))

Ask each question of the change itself: read the code and the tests, not this description. Fix what you find, or explain it under "What and why", then tick the box. CI fails until all six are ticked (`docs/ai/review-checklist-guard.py`).

- [ ] Does it actually do what it claims? Read the logic, not the PR description. (REVIEW-01)
- [ ] Does it fit the architecture, or is it a local shortcut? (REVIEW-02)
- [ ] What does this change break somewhere else? (REVIEW-03)
- [ ] Are nulls, empty inputs, errors and retries handled? (REVIEW-04)
- [ ] Do the tests cover real failures, or only the happy path? (REVIEW-05)
- [ ] Is it over-engineered or under-built for the real constraints? (REVIEW-06)

### Security & compliance checklist ([rules](../docs/ai/secure-compliance/SKILL.md))

Tick each item or explain why it does not apply.

- [ ] No secrets, credentials or real personal data added (SEC-SECRET-01, SEC-PII-03)
- [ ] New/changed endpoints require auth; anonymous ones listed here (SEC-AUTHN-01)
- [ ] Authorization declared for each new MediatR request (module + right) (SEC-AUTHZ-01/02)
- [ ] New tenant-owned entities have SchoolId/BranchId and query filters; no IgnoreQueryFilters (SEC-AUTHZ-03)
- [ ] SchoolId never taken from the client; BranchId checked against the user (SEC-AUTHZ-04)
- [ ] Denial tests: unauthenticated, other school, other branch, disabled module, missing right (SEC-AUTHZ-09)
- [ ] Validators for all input; no raw SQL with user input (SEC-INPUT-01/02)
- [ ] No sensitive data in logs; audit events added for security-relevant actions (SEC-LOG-02/03)
- [ ] No new packages, or each one justified with licence (SEC-DEP-04/05)
- [ ] `dotnet build` and `dotnet test` pass; `npm audit` clean if the frontend changed (SEC-AI-07, SEC-DEP-03)
- [ ] Development-only behaviour guarded by `IsDevelopment()` (SEC-CFG-01)
- [ ] AI instructions/skills changed: content is AI-agnostic and `python3 docs/ai/sync-ai-wrappers.py --check` passes (SEC-AI-10)
- [ ] Payments touched: PCI scope impact stated, no card data reaches our systems (SEC-PCI-02, SEC-PCI-14) / not applicable
- [ ] README.md updated in this PR for any change to technologies, flows (diagrams), features, setup or configuration (AGENTS.md section 7, rule 8; checked by `docs/ai/readme-guard.py`), or a `README: not needed - <reason>` line added below
- [ ] Every README diagram that draws code changed here is updated with its walkthrough (Diagram index in README; checked by `docs/ai/architecture-diagram-guard.py`), or a `Diagrams: not needed - <reason>` line added below
- [ ] Exceptions to any rule: none, or listed with maintainer approval (section 15)

### Engineering checklist ([rules](../docs/ai/engineering/SKILL.md))

- [ ] One purpose, small diff; no unrelated refactoring or reformatting (ENG-PROC-01, ENG-CODE-07)
- [ ] Bug fix and refactoring in separate commits or PRs (ENG-PROC-03)
- [ ] No speculative abstractions or unused code; SOLID/KISS/DRY/YAGNI respected (ENG-DESIGN)
- [ ] Tests describe behaviour and were written first for new logic (ENG-CODE-03, ENG-AGENT-05)
- [ ] Backend logic has unit tests in the matching `*.UnitTests` or integration project: valid, rejected and boundary cases (ENG-CODE-10), or not applicable
- [ ] Errors fail fast and loud; no empty catch (ENG-CODE-04/05)
- [ ] Entities and EF Core changes follow ENG-ENTITY / ENG-EF, or not applicable
- [ ] Only `Guid` public ids in DTOs and routes, `int` keys inside; enums by name on the API, explicit numbers in the database (ENG-ID, ENG-ENUM), or not applicable
- [ ] Angular changes follow ENG-UI (standalone, signals, OnPush, Material, a11y) and reuse the project theme in `Theme Angular/` (ENG-UI-17), or not applicable
- [ ] Definition of done met: built, tested, documented, README updated (ENG-PROC-02)
