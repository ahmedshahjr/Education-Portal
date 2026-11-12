---
name: design-principles
description: Strict design rules (SOLID, KISS, DRY, YAGNI) applied to this Clean Architecture .NET and Angular codebase. Use when designing any class, interface, MediatR handler, service or Angular component, or when deciding whether to add an abstraction. Part of the mandatory Education Portal engineering rules.
---

# Design principles: SOLID, KISS, DRY, YAGNI (MUST follow)

Part of the mandatory engineering rules. Also follow the [overview skill](../SKILL.md): its precedence, exceptions and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose an alternative.

These principles are applied **strictly but pragmatically**: they exist to keep the code easy to change. When two principles pull in different directions, KISS and YAGNI win over adding structure "for SOLID". The architecture decisions in `Education Portal Full Stack App/docs/decisions/` (for example ADR-001: handlers use `IApplicationDbContext` directly, no repository layer) are the agreed application of these principles and are not re-litigated in feature PRs.

## SOLID

| ID | Rule |
|---|---|
| ENG-DESIGN-01 | **Single Responsibility.** A class has one reason to change. One MediatR request = one use case = one handler; one validator per request; one Angular component per UI responsibility. Split a class when its name needs "And", "Manager" or "Helper" to describe it, or when it changes for two unrelated reasons. |
| ENG-DESIGN-02 | **Open/Closed.** Add behaviour by adding code (a new request, handler, pipeline behaviour, strategy, `IEntityTypeConfiguration`), not by growing `switch`/`if` chains over types in existing classes. Cross-cutting concerns (validation, authorization, logging, performance) go in MediatR pipeline behaviours, not in each handler. |
| ENG-DESIGN-03 | **Liskov Substitution.** A subtype must honour its base type's contract: no `NotImplementedException`/`NotSupportedException` overrides, no stronger preconditions, no weaker guarantees. If it cannot, it is not a subtype; use composition. |
| ENG-DESIGN-04 | **Interface Segregation.** Interfaces are small and owned by the layer that uses them (Application declares `IUser`, `IIdentityService`, `IApplicationDbContext`). A consumer must not depend on members it does not call. Split an interface when implementers start throwing for members they cannot support. |
| ENG-DESIGN-05 | **Dependency Inversion.** Depend on abstractions declared in the inner layer and injected through constructors. No `new` of services, no service locator (`IServiceProvider.GetService` in business code), no static mutable state, no `DateTime.Now` (inject `TimeProvider`). Composition happens only in `DependencyInjection.cs` of each layer and in `Web`. |

## KISS, DRY, YAGNI

| ID | Rule |
|---|---|
| ENG-DESIGN-06 | **KISS.** Choose the simplest design that meets today's requirement and the rules. Prefer plain methods, LINQ and framework features over custom frameworks, reflection, generic base handlers or meta-programming. If a reviewer needs the author to explain it, simplify it. |
| ENG-DESIGN-07 | **DRY (knowledge, not text).** Each business rule, constant, validation rule or mapping lives in exactly one place (domain entity, validator, `IEntityTypeConfiguration`, shared constant in `Domain/Constants` or `src/Shared`). Do not copy-paste logic between handlers or components; extract it when the **third** copy appears or when the copies must change together. Two pieces of code that look alike but change for different reasons are not duplication. |
| ENG-DESIGN-08 | **YAGNI.** Build only what a current requirement in AGENTS.md, an issue or the maintainer asks for. No speculative interfaces with one implementation "for later", no unused parameters, options, feature flags, generic type parameters, extension points or configuration. Out-of-scope items in AGENTS.md section 8 stay out. |
| ENG-DESIGN-09 | An interface with a single implementation is allowed only when it is a layer boundary (Application declares, Infrastructure implements) or is needed to test something that touches the outside world (time, identity, external HTTP). Otherwise use the concrete class. |
| ENG-DESIGN-10 | **Composition over inheritance.** Inherit only from the template base types (`BaseEntity`, `BaseAuditableEntity`, `ValueObject`, `BaseEvent`) or framework types. No new inheritance hierarchies deeper than one level without an ADR. |
| ENG-DESIGN-11 | **Tell, don't ask.** Business rules that protect an entity's invariants live in the entity (methods such as `Enrol(...)`, `Archive()`), not in handlers that read and set its properties. Handlers orchestrate; entities decide. |
| ENG-DESIGN-12 | Keep methods short and flat: one level of abstraction per method, early returns instead of deep nesting, no boolean flag parameters that switch behaviour (make two methods). |

## Review questions

Use these when reviewing a design (human or AI):

1. Can I describe each new class in one sentence without "and"? (ENG-DESIGN-01)
2. Did adding this feature modify a stable class that other features depend on? (ENG-DESIGN-02)
3. Is any new abstraction used by more than one caller or needed at a layer boundary today? (ENG-DESIGN-08/09)
4. Is any logic now in two places that must change together? (ENG-DESIGN-07)
5. Is there a simpler way that a new team member would understand at first read? (ENG-DESIGN-06)

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [Microsoft .NET Framework design guidelines (naming, type and member design)](https://learn.microsoft.com/dotnet/standard/design-guidelines/) | .NET design guidelines (current on Learn) | Automatic: new commits in `dotnet/docs/docs/standard/design-guidelines` | 2026-11-10 |
| [Microsoft architectural principles (SOLID, DRY, separation of concerns, persistence ignorance)](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/architectural-principles) | Architect Modern Web Applications with ASP.NET Core and Azure (current edition on Learn) | Automatic: new commits in `dotnet/docs/docs/architecture/modern-web-apps-azure/architectural-principles.md` | 2026-11-10 |
<!-- END GENERATED: sources -->
