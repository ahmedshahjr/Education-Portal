using System.Globalization;
using EducationPortal.Application.Common.Exceptions;
using EducationPortal.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EducationPortal.Application.Subscriptions.Lifecycle;

/// <summary>What one run of the lifecycle changed.</summary>
public sealed record SubscriptionLifecycleResult(int TrialsEnded, int InvoicesMarkedOverdue, int MarkedPastDue, int Suspended, int RemindersQueued);

/// <summary>
/// The daily subscription lifecycle (ADR-015), run by the scheduled job as the master tenant: only the master tenant
/// manages subscriptions (SEC-AUTHZ-07), and running as the master keeps the normal tenant query filters (SEC-AUTHZ-08).
/// In order, at one UTC instant <c>now</c>:
/// <list type="number">
/// <item>Trials past their end date become past due from the trial's end.</item>
/// <item>Open invoices past their due date are marked overdue, and an active subscription with one becomes past due from the due date.</item>
/// <item>Active subscriptions whose period ended unpaid become past due from the period's end.</item>
/// <item>Past-due subscriptions whose grace period is over are suspended: modules off, data kept.</item>
/// <item>Trial and active subscriptions ending soon get a renewal reminder queued for the nearest configured reminder day.</item>
/// </list>
/// Every rule checks the current state first, so a second run on the same day changes nothing. All changes are saved
/// together; if anything fails nothing is saved and the next run tries again (fail closed).
/// </summary>
public sealed class SubscriptionLifecycle
{
    /// <summary>The actor recorded on the audit events the job writes.</summary>
    public const string SystemActor = "system:subscription-lifecycle";

    private readonly IApplicationDbContext _context;
    private readonly ICurrentTenant _tenant;
    private readonly IModuleEntitlementStore _entitlements;
    private readonly ISecurityAuditLog _auditLog;
    private readonly SubscriptionLifecycleOptions _options;
    private readonly ILogger<SubscriptionLifecycle> _logger;

    public SubscriptionLifecycle(
        IApplicationDbContext context,
        ICurrentTenant tenant,
        IModuleEntitlementStore entitlements,
        ISecurityAuditLog auditLog,
        IOptions<SubscriptionLifecycleOptions> options,
        ILogger<SubscriptionLifecycle> logger)
    {
        _context = context;
        _tenant = tenant;
        _entitlements = entitlements;
        _auditLog = auditLog;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SubscriptionLifecycleResult> RunAsync(DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (!_tenant.IsMaster)
        {
            throw new ForbiddenAccessException();
        }

        now = now.ToUniversalTime();

        var subscriptions = await _context.Subscriptions
            .Include(s => s.Modules)
            .Where(s => s.Status == SubscriptionStatus.Trial || s.Status == SubscriptionStatus.Active || s.Status == SubscriptionStatus.PastDue)
            // A deleted school is hidden by its soft-delete filter, so its subscription is left alone.
            .Where(s => _context.Schools.Any(school => school.Id == s.SchoolId))
            .ToListAsync(cancellationToken);
        var byId = subscriptions.ToDictionary(s => s.Id);

        var changed = new HashSet<Subscription>();

        // 1. Trials that ended unpaid.
        var trialsEnded = 0;
        foreach (var subscription in subscriptions.Where(s => s.EndTrial(now)))
        {
            trialsEnded++;
            changed.Add(subscription);
        }

        // 2. Invoices that fell due unpaid, and the subscriptions they belong to.
        var dueInvoices = await _context.Invoices
            .Where(i => i.Status == InvoiceStatus.Open && i.OverdueSince == null && i.DueDate <= now)
            .ToListAsync(cancellationToken);
        var overdue = dueInvoices.Where(i => i.MarkOverdue(now)).ToList();

        var markedPastDue = 0;
        foreach (var invoice in overdue)
        {
            if (byId.TryGetValue(invoice.SubscriptionId, out var subscription) && subscription.MarkPastDue(invoice.DueDate))
            {
                markedPastDue++;
                changed.Add(subscription);
            }
        }

        // 3. Periods that ended without a paid renewal.
        foreach (var subscription in subscriptions.Where(s => s.Status == SubscriptionStatus.Active && s.EndDate <= now))
        {
            subscription.MarkPastDue(subscription.EndDate);
            markedPastDue++;
            changed.Add(subscription);
        }

        // 4. Suspension once the grace period is over.
        var suspended = 0;
        foreach (var subscription in subscriptions.Where(s => s.SuspendAfterGrace(now, _options.GracePeriodDays)))
        {
            suspended++;
            changed.Add(subscription);
        }

        // 5. Renewal reminders.
        var reminders = await QueueRemindersAsync(subscriptions, now, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var subscription in changed)
        {
            // Module access is cached per school; a changed subscription takes effect at once.
            _entitlements.Invalidate(subscription.SchoolId);
            await _auditLog.WriteAsync(SecurityAuditEventType.SubscriptionChanged, SecurityAuditOutcome.Success, SystemActor, nameof(Subscription), subscription.Id.ToString(CultureInfo.InvariantCulture));
        }

        foreach (var invoice in overdue)
        {
            await _auditLog.WriteAsync(SecurityAuditEventType.InvoiceMarkedOverdue, SecurityAuditOutcome.Success, SystemActor, nameof(Invoice), invoice.Id.ToString(CultureInfo.InvariantCulture));
        }

        var result = new SubscriptionLifecycleResult(trialsEnded, overdue.Count, markedPastDue, suspended, reminders);
        _logger.LogInformation(
            "Subscription lifecycle run: {TrialsEnded} trials ended, {InvoicesMarkedOverdue} invoices overdue, {MarkedPastDue} past due, {Suspended} suspended, {RemindersQueued} reminders queued",
            result.TrialsEnded,
            result.InvoicesMarkedOverdue,
            result.MarkedPastDue,
            result.Suspended,
            result.RemindersQueued);

        return result;
    }

    /// <summary>
    /// The reminder day a period ending in <paramref name="daysLeft"/> whole UTC days is due for: the smallest configured
    /// day that is not less than the days left, or <c>null</c> when the end is further away than every reminder day. A
    /// job that missed days therefore queues only the nearest reminder, never a burst of old ones.
    /// </summary>
    public static int? ReminderDayFor(int daysLeft, IReadOnlyList<int> reminderDays)
    {
        int? due = null;
        foreach (var day in reminderDays)
        {
            if (day >= daysLeft && (due is null || day < due))
            {
                due = day;
            }
        }

        return due;
    }

    /// <summary>Whole UTC calendar days from <paramref name="now"/> to <paramref name="end"/>.</summary>
    public static int DaysLeft(DateTimeOffset now, DateTimeOffset end) =>
        (end.UtcDateTime.Date - now.UtcDateTime.Date).Days;

    private async Task<int> QueueRemindersAsync(List<Subscription> subscriptions, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var reminderDays = _options.EffectiveReminderDays;
        var due = subscriptions
            .Where(s => s.Status is SubscriptionStatus.Trial or SubscriptionStatus.Active && s.EndDate > now)
            .Select(s => (Subscription: s, Day: ReminderDayFor(DaysLeft(now, s.EndDate), reminderDays)))
            .Where(x => x.Day is not null)
            .ToList();

        if (due.Count == 0)
        {
            return 0;
        }

        var ids = due.Select(x => x.Subscription.Id).ToList();
        var queued = (await _context.RenewalReminders
                .Where(r => ids.Contains(r.SubscriptionId))
                .Select(r => new { r.SubscriptionId, r.PeriodEnd, r.DaysBefore })
                .ToListAsync(cancellationToken))
            .Select(r => (r.SubscriptionId, r.PeriodEnd, r.DaysBefore))
            .ToHashSet();

        var count = 0;
        foreach (var (subscription, day) in due)
        {
            if (queued.Contains((subscription.Id, subscription.EndDate, day!.Value)))
            {
                continue;
            }

            _context.RenewalReminders.Add(RenewalReminder.Queue(subscription, day.Value, now));
            count++;
        }

        return count;
    }
}
