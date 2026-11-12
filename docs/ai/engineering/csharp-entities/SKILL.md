---
name: csharp-entities
description: C# entity and domain model design rules following Microsoft's official guidance (encapsulated entities, private setters, factory methods and guard clauses, value objects, domain events, nullable reference types, UTC DateTimeOffset, SchoolId/BranchId). Use when adding or changing a class in src/Domain (entity, value object, enum, domain event). Part of the mandatory Education Portal engineering rules.
---

# C# entity design (MUST follow)

Part of the mandatory engineering rules. Also follow the [overview skill](../SKILL.md): its precedence, exceptions and PR checklist apply to every change. Rule IDs are stable; cite them in reviews and PRs. If a request would break a rule here, stop, name the rule ID and propose an alternative.

Based on Microsoft's guidance for [domain model entities](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/net-core-microservice-domain-model), [value objects](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/implement-value-objects), [domain-layer validation](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-model-layer-validations), [EF Core entity types](https://learn.microsoft.com/ef/core/modeling/entity-types), [nullable reference types in EF Core](https://learn.microsoft.com/ef/core/miscellaneous/nullable-reference-types) and the [.NET design guidelines](https://learn.microsoft.com/dotnet/standard/design-guidelines/). See also the [efcore skill](../efcore/SKILL.md) for mapping.

| ID | Rule |
|---|---|
| ENG-ENTITY-01 | Entities live in `src/Domain/Entities`, one public class per file, named with a singular noun (`Student`, `Enrolment`). They inherit `BaseAuditableEntity` (or `BaseEntity` when audit fields make no sense) and reference nothing outside `Domain` except the framework and `Ardalis.GuardClauses`. No EF Core, ASP.NET Core or Application types in entities. |
| ENG-ENTITY-02 | **Encapsulate state.** Properties that carry business meaning have `private set` (or `init` for values fixed at creation). Change state only through intention-revealing methods (`Enrol`, `RecordScore`, `Deactivate`) that check invariants. New entities MUST NOT expose public setters for business data. |
| ENG-ENTITY-03 | **Valid on creation.** Create entities through a constructor or static factory method that takes every required value and checks it with guard clauses (`Guard.Against.NullOrWhiteSpace`, `Guard.Against.NegativeOrZero`, ...). Give EF Core a `private` parameterless constructor if it needs one. An entity is never in an invalid state after construction or after any public method returns. |
| ENG-ENTITY-04 | **Collections are read-only to the outside.** Expose `IReadOnlyCollection<T>` backed by a private `List<T>` field (`private readonly List<Assessment> _assessments = [];`) and change them through methods. EF Core maps the backing field. |
| ENG-ENTITY-05 | **Value objects** for concepts defined by their values, not identity (email address, score, school domain name, date range). Inherit the template's `ValueObject` or use a `record`/`readonly record struct`, make them immutable and validate in the constructor. Map them as owned or complex types (ENG-EF-09). |
| ENG-ENTITY-06 | **Nullable reference types reflect the model.** Required strings are non-nullable and initialised; optional ones are `string?`. Use `required` members or constructor parameters, not `= null!`, except for EF Core navigation properties that are always loaded with the entity. |
| ENG-ENTITY-07 | **Tenant ownership.** Every school-owned entity has `int SchoolId`; branch-scoped entities also have `int BranchId` (AGENTS.md section 2, SEC-AUTHZ-03). Both are set once at creation (no public setter) and are never taken from client input. |
| ENG-ENTITY-08 | **Time** is `DateTimeOffset` in UTC, obtained from an injected `TimeProvider` in the caller and passed into the entity method. Entities and services never call `DateTime.Now`, `DateTime.UtcNow` or `DateTimeOffset.Now`. Dates without time (birth date) use `DateOnly`. |
| ENG-ENTITY-09 | **Domain events** for side effects other parts of the system react to (`StudentEnrolledEvent`). Name them in the past tense, inherit `BaseEvent`, raise them with `AddDomainEvent` inside the entity method, and keep them small (ids and the values that changed). Entities never call services directly. |
| ENG-ENTITY-10 | **Enums** are singular (`EnrolmentStatus`) and follow the [identifiers-enums skill](../identifiers-enums/SKILL.md): an explicit number for every member (ENG-ENUM-02), stored as that number (ENG-ENUM-03), sent over the API by name (ENG-ENUM-01). `[Flags]` enums are plural and powers of two. |
| ENG-ENTITY-11 | **Identity and equality.** Entities are equal by `Id`, value objects by value. Do not override `Equals` on entities to compare properties. Every entity has the internal `int Id` and the public `Guid PublicId` from `BaseEntity`; only the `Guid` leaves the server ([identifiers-enums](../identifiers-enums/SKILL.md), ENG-ID-01..03). |
| ENG-ENTITY-12 | Keep entities persistence-ignorant but EF-friendly: no data annotations for mapping (`[Table]`, `[Column]`, `[MaxLength]`); put all mapping in `IEntityTypeConfiguration<T>` classes in `src/Infrastructure/Data/Configurations` (the template's `[NotMapped]` on `DomainEvents` is the only exception). |
| ENG-ENTITY-13 | XML doc comments on public entity members that carry a business rule or a compliance reason (as in `SecurityAuditEvent`). Do not document the obvious. |

## Example

```csharp
public class Enrolment : BaseAuditableEntity
{
    private readonly List<Assessment> _assessments = [];

    private Enrolment() { } // EF Core

    public Enrolment(int schoolId, int branchId, int courseId, int studentId, DateTimeOffset enrolledAt)
    {
        SchoolId = Guard.Against.NegativeOrZero(schoolId);
        BranchId = Guard.Against.NegativeOrZero(branchId);
        CourseId = Guard.Against.NegativeOrZero(courseId);
        StudentId = Guard.Against.NegativeOrZero(studentId);
        EnrolledAt = enrolledAt;
        Status = EnrolmentStatus.Active;
        AddDomainEvent(new StudentEnrolledEvent(this));
    }

    public int SchoolId { get; private set; }
    public int BranchId { get; private set; }
    public int CourseId { get; private set; }
    public int StudentId { get; private set; }
    public DateTimeOffset EnrolledAt { get; private set; }
    public EnrolmentStatus Status { get; private set; }
    public IReadOnlyCollection<Assessment> Assessments => _assessments.AsReadOnly();

    public void RecordAssessment(string title, Score score, DateTimeOffset assessedAt)
    {
        if (Status != EnrolmentStatus.Active)
            throw new EnrolmentNotActiveException(Id);

        _assessments.Add(new Assessment(title, score, assessedAt));
    }
}
```

<!-- BEGIN GENERATED: sources (docs/ai/sync-ai-wrappers.py) -->
## Sources and update checks

This skill is written against the versions below, recorded in [sources.json](../sources.json). `.github/workflows/compliance-sources-review.yml` checks every source on the 1st and 16th of each month and opens or updates a `compliance-review` issue. To apply a change: update this skill and any affected topic skills, then `tracked_version`, `last_seen_sha` and `last_reviewed` in sources.json, in one PR into `development`, and run `python3 docs/ai/sync-ai-wrappers.py`. The workflow never edits rules itself.

| Source | Version tracked | How it is checked | Last reviewed |
|---|---|---|---|
| [Microsoft EF Core documentation (performance, querying, modeling, saving, migrations, testing)](https://learn.microsoft.com/ef/core/) | EF Core 10 | Automatic: new commits in `dotnet/EntityFramework.Docs/entity-framework/core` | 2026-11-10 |
| [Microsoft .NET architecture guide: DDD domain model, entities, value objects, domain events](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/) | .NET Microservices: Architecture for Containerized .NET Applications (current edition on Learn) | Automatic: new commits in `dotnet/docs/docs/architecture/microservices/microservice-ddd-cqrs-patterns` | 2026-11-10 |
| [Microsoft .NET Framework design guidelines (naming, type and member design)](https://learn.microsoft.com/dotnet/standard/design-guidelines/) | .NET design guidelines (current on Learn) | Automatic: new commits in `dotnet/docs/docs/standard/design-guidelines` | 2026-11-10 |
| [Microsoft C# coding conventions and identifier naming](https://learn.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions) | C# 14 / .NET 10 coding conventions | Automatic: new commits in `dotnet/docs/docs/csharp/fundamentals/coding-style` | 2026-11-10 |
<!-- END GENERATED: sources -->
