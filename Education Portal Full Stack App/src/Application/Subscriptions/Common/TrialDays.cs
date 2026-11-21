namespace EducationPortal.Application.Subscriptions.Common;

/// <summary>How a trial's remaining time is counted, in one place for every screen that shows it.</summary>
public static class TrialDays
{
    /// <summary>The whole days left of a trial ending at <paramref name="trialEnd"/>, rounded up, never below 0.</summary>
    public static int Remaining(DateTimeOffset trialEnd, DateTimeOffset now) =>
        Math.Max(0, (int)Math.Ceiling((trialEnd - now).TotalDays));
}
