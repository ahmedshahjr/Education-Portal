namespace EducationPortal.Application.Audit.Common;

/// <summary>One recorded change of a commercial or access record, as the school's administrator reads it (read only).</summary>
/// <param name="ActorUserId">The account that made the change, or null for the system (for example the lifecycle job).</param>
/// <param name="Changes">The changed values as recorded: a JSON list of property, old value and new value.</param>
public sealed record ChangeAuditDto(
    DateTimeOffset OccurredAt,
    string? ActorUserId,
    string EntityType,
    Guid EntityId,
    ChangeAuditAction Action,
    string Changes);
