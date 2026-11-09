## What and why

<!-- What changes, and why. Link the issue or request. -->

## How it was tested

<!-- Commands run (dotnet build, dotnet test, npm test) and their results. -->

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
- [ ] Exceptions to any rule: none, or listed with maintainer approval (section 15)
