namespace EducationPortal.Domain.Entities;

/// <summary>
/// One change to a commercial record (subscription, its modules, invoice, payment, catalogue, plan or price), with who
/// made it, when, and every changed value before and after (ADR-016). Written in the same save as the change, so a change
/// is never stored without its entry. Append-only like <see cref="SecurityAuditEvent"/>: every property is init-only and
/// the application never updates or deletes a row (SEC-LOG-05). Values are ids, codes, amounts, dates and statuses,
/// never personal data (SEC-PII-02).
/// </summary>
public class ChangeAuditEntry
{
    public const int EntityTypeMaxLength = 64;

    /// <summary>The internal key (ENG-ID-01). The change log is not served by the API yet.</summary>
    public long Id { get; init; }

    /// <summary>The identity to use if a record is ever exposed (ENG-ID-02).</summary>
    public Guid PublicId { get; init; } = Guid.CreateVersion7();

    /// <summary>When the change was saved, in UTC.</summary>
    public required DateTimeOffset OccurredAt { get; init; }

    /// <summary>The user who made it, or <c>null</c> for the system (seed data or a scheduled job).</summary>
    public string? ActorUserId { get; init; }

    /// <summary>The school the changed row belongs to, or <c>null</c> for platform data such as the catalogue.</summary>
    public int? SchoolId { get; init; }

    /// <summary>The changed entity's type name, for example <c>Subscription</c>.</summary>
    public required string EntityType { get; init; }

    /// <summary>The changed row's public id.</summary>
    public required Guid EntityId { get; init; }

    public required ChangeAuditAction Action { get; init; }

    /// <summary>
    /// The changed properties as a JSON array of <c>{"property", "old", "new"}</c>, values as invariant text.
    /// <c>old</c> is null when the row was added, <c>new</c> is null when it was removed.
    /// </summary>
    public required string Changes { get; init; }

    /// <summary>The OpenTelemetry trace id, to find the request's logs and its security audit event.</summary>
    public string? TraceId { get; init; }
}
