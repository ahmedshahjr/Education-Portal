namespace EducationPortal.Domain.Entities;

/// <summary>
/// A queued reminder that a school's subscription period ends soon, written by the daily lifecycle job. It holds no text:
/// whatever sends it renders the localized message (<c>NotificationMessages</c>) in the school's language. One row per
/// subscription, period end and reminder day, so the job can run again the same day without queueing it twice. Owned by
/// the school (<c>SchoolId</c>) and covered by the tenant query filter (SEC-AUTHZ-03).
/// </summary>
public class RenewalReminder : BaseAuditableEntity
{
    /// <summary>The longest reminder lead time the configuration may ask for.</summary>
    public const int MaxDaysBefore = 365;

    private RenewalReminder()
    {
        // For EF Core.
    }

    private RenewalReminder(Subscription subscription, int daysBefore, DateTimeOffset queuedAt)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(subscription.Id);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(daysBefore);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(daysBefore, MaxDaysBefore);

        SchoolId = subscription.SchoolId;
        SubscriptionId = subscription.Id;
        PeriodEnd = subscription.EndDate;
        DaysBefore = daysBefore;
        QueuedAt = queuedAt;
    }

    /// <summary>Queues a reminder, <paramref name="daysBefore"/> days ahead, for the end of the subscription's current period.</summary>
    public static RenewalReminder Queue(Subscription subscription, int daysBefore, DateTimeOffset queuedAt)
    {
        ArgumentNullException.ThrowIfNull(subscription);

        return new RenewalReminder(subscription, daysBefore, queuedAt);
    }

    public int SchoolId { get; private set; }

    public int SubscriptionId { get; private set; }

    /// <summary>The end of the period the reminder is about (the subscription's end date when it was queued).</summary>
    public DateTimeOffset PeriodEnd { get; private set; }

    /// <summary>The configured reminder day it answers, for example 7 for "a week before".</summary>
    public int DaysBefore { get; private set; }

    public DateTimeOffset QueuedAt { get; private set; }

    /// <summary>When it was sent, or <c>null</c> while it waits in the queue.</summary>
    public DateTimeOffset? SentAt { get; private set; }

    /// <summary>Records that the reminder was sent. Sending it again changes nothing.</summary>
    public void MarkSent(DateTimeOffset sentAt) => SentAt ??= sentAt;
}
