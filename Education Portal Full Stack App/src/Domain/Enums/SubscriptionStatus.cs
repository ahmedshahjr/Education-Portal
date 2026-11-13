namespace EducationPortal.Domain.Enums;

/// <summary>
/// Where a school's subscription is in its life. Stored as its number and sent over the API by name (ENG-ENUM-01..03):
/// never change or reuse a number, and never rename a member.
/// </summary>
public enum SubscriptionStatus
{
    /// <summary>Free trial: the modules work until the trial ends.</summary>
    Trial = 1,

    /// <summary>Paid up (or started by the master) and running.</summary>
    Active = 2,

    /// <summary>The period ended and payment is late; still within the grace period.</summary>
    PastDue = 3,

    /// <summary>The paid modules are off until the master reactivates the subscription. Data is kept.</summary>
    Suspended = 4,

    /// <summary>Ended for good. A school may then start a new subscription.</summary>
    Cancelled = 5,
}
