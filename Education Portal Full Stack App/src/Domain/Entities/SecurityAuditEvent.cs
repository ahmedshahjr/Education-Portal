namespace EducationPortal.Domain.Entities;

/// <summary>
/// One record in the security audit log (SEC-LOG-03, SEC-LOG-04). The log is append-only for the
/// application: records are created once and never updated or deleted (SEC-LOG-05), which is why every
/// property is init-only. Records hold ids, never names, emails or other personal data (SEC-PII-02).
/// </summary>
public class SecurityAuditEvent
{
    public long Id { get; init; }

    /// <summary>When it happened, in UTC.</summary>
    public required DateTimeOffset OccurredAt { get; init; }

    public required SecurityAuditEventType EventType { get; init; }

    public required SecurityAuditOutcome Outcome { get; init; }

    /// <summary>The user who acted, when known.</summary>
    public string? ActorUserId { get; init; }

    /// <summary>The actor's school, once schools exist (step 2b).</summary>
    public int? ActorSchoolId { get; init; }

    /// <summary>The actor's current branch, once branches exist (step 2b).</summary>
    public int? ActorBranchId { get; init; }

    /// <summary>The kind of thing acted on, for example <c>User</c> or a request name.</summary>
    public string? TargetType { get; init; }

    public string? TargetId { get; init; }

    public string? SourceIp { get; init; }

    public string? UserAgent { get; init; }

    /// <summary>The OpenTelemetry trace id, to find the matching logs and traces.</summary>
    public string? TraceId { get; init; }
}
