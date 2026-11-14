using EducationPortal.Domain.Enums;

namespace EducationPortal.Application.Common.Interfaces;

/// <summary>
/// Writes the security audit log (SEC-LOG-03). Records are append-only (SEC-LOG-05) and hold ids only,
/// never names, emails or other personal data (SEC-PII-02).
/// </summary>
public interface ISecurityAuditLog
{
    /// <summary>
    /// Writes one record straight away, outside the caller's unit of work, so it is kept even when the
    /// request fails afterwards. Time, source IP, user agent and trace id are filled in automatically;
    /// the actor defaults to the signed-in user.
    /// </summary>
    Task WriteAsync(
        SecurityAuditEventType eventType,
        SecurityAuditOutcome outcome,
        string? actorUserId = null,
        string? targetType = null,
        string? targetId = null);
}
