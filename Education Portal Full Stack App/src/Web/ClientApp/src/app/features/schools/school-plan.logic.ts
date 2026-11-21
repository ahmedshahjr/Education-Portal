import { BadgeTone } from '../../shared/components/status-badge/status-badge.component';
import { SubscriptionStatus } from '../../web-api-client';

const DAY_MS = 24 * 60 * 60 * 1000;

/** How a school's plan shows in the master portal's table: its label (already translated) and badge colour. */
export interface SchoolPlan {
  readonly label: string;
  readonly tone: BadgeTone;
}

/** The whole days left of a trial, rounded up, never below 0 (the same count the server uses). */
export function trialDaysLeft(trialEnd: Date, now: Date): number {
  return Math.max(0, Math.ceil((trialEnd.getTime() - now.getTime()) / DAY_MS));
}

/**
 * Whether a school is on its free trial or is a paid school. The master tenant has no plan (null). A school on a trial shows
 * the days left; an active subscription is a paid school; a lapsed one says so. A school with no subscription has no plan yet.
 */
export function schoolPlan(
  // NSwag types a nullable enum as an empty class (SubscriptionStatus2); on the wire it is the enum's name, so it is compared as that.
  school: { isMaster: boolean; subscriptionStatus?: unknown; trialEndDate?: Date },
  now: Date,
): SchoolPlan | null {
  if (school.isMaster) {
    return null;
  }

  switch (school.subscriptionStatus) {
    case SubscriptionStatus.Trial: {
      const days = school.trialEndDate ? trialDaysLeft(school.trialEndDate, now) : 0;
      if (days === 0) {
        return { label: $localize`:@@schools.planTrialEndsToday:Trial ends today`, tone: 'primary' };
      }
      return days === 1
        ? { label: $localize`:@@schools.planTrialOneDay:Trial (1 day left)`, tone: 'primary' }
        : { label: $localize`:@@schools.planTrialDays:Trial (${days}:days: days left)`, tone: 'primary' };
    }
    case SubscriptionStatus.Active:
      return { label: $localize`:@@schools.planPaid:Paid`, tone: 'success' };
    case SubscriptionStatus.PastDue:
      return { label: $localize`:@@schools.planPastDue:Payment overdue`, tone: 'warning' };
    case SubscriptionStatus.Suspended:
      return { label: $localize`:@@schools.planSuspended:Suspended`, tone: 'warning' };
    default:
      return { label: $localize`:@@schools.planNone:No plan`, tone: 'warning' };
  }
}
