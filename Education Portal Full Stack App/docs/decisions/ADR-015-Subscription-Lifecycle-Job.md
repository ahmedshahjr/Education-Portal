# ADR-015: Daily Subscription Lifecycle Job

## Status

Accepted.

## Date

2026-11-13

## Context

Phase 7 of eight of the module-selling work. Subscriptions ([ADR-011](ADR-011-Subscriptions-And-Tenant-Modules.md)) and
invoices ([ADR-013](ADR-013-Invoices-And-Manual-Payments.md)) have end and due dates, but nothing acted on them: a trial
never ended, an unpaid invoice never became overdue and a school that stopped paying kept its modules. Phase 6 added the
master's manual Suspend, Reactivate and Extend actions. The maintainer chose Quartz.NET (not a plain `BackgroundService`)
for the scheduler.

## Decision

- **Quartz.NET 4** (package `Quartz`; since 4.0 the hosting extensions live in it) runs `SubscriptionLifecycleJob` in the
  API (`src/Web/Jobs`), on a cron schedule read in **UTC**. `[DisallowConcurrentExecution]` keeps it to one run at a time
  per instance. The schedule is in memory (no Quartz tables): every API instance runs the job, which is safe because a run
  is idempotent. A persistent, clustered job store is a later choice if the API scales out a lot.
- **Configuration** (section `SubscriptionLifecycle`, validated at start-up): `Enabled` (true), `Schedule` (`0 0 1 * * ?`,
  01:00 UTC daily), `GracePeriodDays` (14, 0 to 90) and `ReminderDays` (30, 7, 1; each 1 to 365, no duplicates; empty
  means the defaults). Tests switch the job off and run it by hand at a chosen time.
- **Tenant:** the job has no request, so it opens its own scope and sets the **master tenant** on it. The rules refuse to
  run as anything else (`ForbiddenAccessException`): only the master tenant manages subscriptions (SEC-AUTHZ-07), and the
  normal tenant query filters stay on (SEC-AUTHZ-08). With no master tenant in the database it does nothing.
- **Rules** (`SubscriptionLifecycle` in Application), in this order, at one UTC instant:
  1. A **trial** past its `TrialEndDate` becomes **PastDue** from the trial's end.
  2. An **open invoice** past its `DueDate` is marked **overdue** (`Invoices.OverdueSince`; the status stays Open, so it
     can still be paid or voided), and its subscription, if Active, becomes **PastDue** from the due date.
  3. An **Active** subscription whose `EndDate` has passed without a paid renewal becomes **PastDue** from the end.
  4. A **PastDue** subscription whose grace period (`PastDueSince` + `GracePeriodDays`) is over is **Suspended** with
     phase 6's `Subscription.Suspend()`: every active module is suspended, **no data is deleted**.
  5. A **Trial or Active** subscription ending within the largest reminder day gets a **renewal reminder** queued
     (`RenewalReminders`) for the nearest configured day that is not less than the whole UTC days left. A job that missed
     days queues only that nearest reminder, never a burst of old ones.
- **Idempotent:** every rule checks the current state first (`EndTrial`, `MarkOverdue`, `MarkPastDue` and
  `SuspendAfterGrace` return false and change nothing otherwise), a past-due date is never moved, and a reminder is
  unique per subscription, period end and reminder day (unique index). Running twice on the same day changes nothing.
- **One save:** all changes of a run are saved together; if anything fails nothing is saved, Quartz records the failure
  and the next run tries again. Afterwards the job drops the cached entitlements of each changed school, so suspension
  blocks access at once, and writes audit events as actor `system:subscription-lifecycle`: `SubscriptionChanged` per
  changed subscription and the new `InvoiceMarkedOverdue`.
- **`Subscription.PastDueSince`** records when the subscription lapsed. Paying (`ApplyPayment`) or reactivating clears it.
- **Renewal reminders** hold no text. Whatever sends them renders `NotificationMessages` (`RenewalReminderSubject`,
  `RenewalReminderBody` with the period end date) in the school's language; English, Arabic and Urdu exist. Nothing sends
  them yet (there is no email layer); `SentAt` marks a sent one.
- **Restarting a lapsed period:** a renewal (`nextPeriod`) invoice starts at the current `EndDate`, so the billing date
  never moves, **unless that end has already passed**. Then the new period starts on the invoice's issue date
  (`Subscription.RenewalStart`). The school is not billed for days it was past due or suspended, and a renewal paid
  late is not already over when it is paid. An invoice for the current period still reactivates a suspended
  subscription without moving its dates.

## Rationale

- Quartz gives cron schedules, misfire handling and a path to a clustered store without writing a timer loop.
- Running as the master tenant reuses the existing authorization model instead of a special bypass of the filters.
- Idempotent rules make the job safe to retry, to run on several instances and to run by hand.
- Keeping the invoice Open and adding a date avoids a new status that every billing rule would have to learn.
- Queuing reminders without text keeps localization at the edge, where the recipient's language is known.

## Consequences

**Easier:** the master no longer chases trials and unpaid schools by hand; a school that pays is back at once
(phase 5); a sender (email, in-app) can read the reminder queue later.

**Harder / watch:** each instance runs the job, so the run's audit and log lines can appear once per instance on a busy
day (the data changes once). The grace period counts from the lapse even when a trial's first invoice is due later.
Changing `ReminderDays` affects only reminders not yet queued. Module access also ends at each module's `ValidTo`
(phase 4), which is the period or trial end, so after a period ends unpaid the grace period delays only the Suspended
status, not the loss of access; inside a period (an overdue invoice) access continues through the grace period. Whether
access should continue through the grace period after a period ends is an open question for the maintainer.
