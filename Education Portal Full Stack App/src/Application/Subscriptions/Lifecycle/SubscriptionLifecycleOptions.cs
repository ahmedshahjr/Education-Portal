using System.ComponentModel.DataAnnotations;

namespace EducationPortal.Application.Subscriptions.Lifecycle;

/// <summary>
/// The <c>SubscriptionLifecycle</c> configuration section for the daily lifecycle job (ADR-015). The defaults are safe:
/// the job runs once a day at 01:00 UTC, gives 14 days of grace before suspending, and reminds 30, 7 and 1 day ahead.
/// </summary>
public sealed class SubscriptionLifecycleOptions : IValidatableObject
{
    public const string SectionName = "SubscriptionLifecycle";

    /// <summary>When false, the job is not scheduled (for example in tests that run it by hand).</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>A Quartz cron expression, always read in UTC. The default is every day at 01:00 UTC.</summary>
    [Required]
    public string Schedule { get; set; } = "0 0 1 * * ?";

    /// <summary>Days a past-due subscription keeps its modules before it is suspended.</summary>
    [Range(0, 90)]
    public int GracePeriodDays { get; set; } = 30;

    /// <summary>The reminder days when the configuration lists none.</summary>
    public static IReadOnlyList<int> DefaultReminderDays { get; } = [30, 7, 1];

    /// <summary>
    /// How many days before a period ends a renewal reminder is queued; one reminder for each value. Empty means
    /// <see cref="DefaultReminderDays"/>. (No default list here: the configuration binder would add to it, not replace it.)
    /// </summary>
    public int[] ReminderDays { get; set; } = [];

    /// <summary>The reminder days in force, largest first.</summary>
    public IReadOnlyList<int> EffectiveReminderDays =>
        (ReminderDays.Length == 0 ? DefaultReminderDays : ReminderDays).OrderDescending().ToArray();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ReminderDays.Any(d => d is < 1 or > Domain.Entities.RenewalReminder.MaxDaysBefore))
        {
            yield return new ValidationResult(
                $"Every reminder day is from 1 to {Domain.Entities.RenewalReminder.MaxDaysBefore}.",
                [nameof(ReminderDays)]);
        }

        if (ReminderDays.Distinct().Count() != ReminderDays.Length)
        {
            yield return new ValidationResult("Reminder days are listed once each.", [nameof(ReminderDays)]);
        }
    }
}
