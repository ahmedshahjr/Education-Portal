using EducationPortal.Domain.Common;

namespace EducationPortal.Domain.Entities;

/// <summary>
/// A notification for one account of the school about a school event (the table <c>Notifications</c>). It belongs to its
/// recipient only; the title and text are copied from the event when it is created, so the user keeps what was sent (ADR-019).
/// </summary>
public class Notification : BaseAuditableEntity, ITenantOwned
{
    /// <summary>The length of an identity user id.</summary>
    public const int RecipientMaxLength = 450;

    private Notification()
    {
        // For EF Core.
    }

    private Notification(int schoolId, string recipientAccountId, SchoolEvent schoolEvent)
    {
        SchoolId = schoolId;
        RecipientAccountId = recipientAccountId;
        SchoolEvent = schoolEvent;
        Title = schoolEvent.Title;
        Body = schoolEvent.Description;
    }

    public static Notification For(SchoolEvent schoolEvent, string recipientAccountId)
    {
        ArgumentNullException.ThrowIfNull(schoolEvent);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientAccountId);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(recipientAccountId.Length, RecipientMaxLength, nameof(recipientAccountId));

        return new Notification(schoolEvent.SchoolId, recipientAccountId, schoolEvent);
    }

    /// <summary>Marks the notification as read. The first time it is read is kept.</summary>
    public void MarkRead(DateTimeOffset at)
    {
        ReadAt ??= at.ToUniversalTime();
    }

    public int SchoolId { get; private set; }

    public string RecipientAccountId { get; private set; } = string.Empty;

    public int SchoolEventId { get; private set; }

    public SchoolEvent SchoolEvent { get; private set; } = null!;

    public string Title { get; private set; } = string.Empty;

    public string? Body { get; private set; }

    public DateTimeOffset? ReadAt { get; private set; }
}
