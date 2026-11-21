import { describe, expect, it } from 'vitest';
import { SubscriptionStatus } from '../../web-api-client';
import { schoolPlan, trialDaysLeft } from './school-plan.logic';

const now = new Date('2026-11-21T12:00:00Z');
const daysFromNow = (days: number) => new Date(now.getTime() + days * 24 * 60 * 60 * 1000);

describe('trialDaysLeft', () => {
  it('rounds a part of a day up', () => {
    expect(trialDaysLeft(new Date(now.getTime() + 60 * 60 * 1000), now)).toBe(1);
  });

  it('is never below zero', () => {
    expect(trialDaysLeft(daysFromNow(-3), now)).toBe(0);
  });
});

describe('schoolPlan', () => {
  it('has no plan for the master tenant', () => {
    expect(schoolPlan({ isMaster: true, subscriptionStatus: undefined, trialEndDate: undefined }, now)).toBeNull();
  });

  it('shows a school on its trial with the days left', () => {
    const plan = schoolPlan({ isMaster: false, subscriptionStatus: SubscriptionStatus.Trial, trialEndDate: daysFromNow(88) }, now);

    expect(plan?.label).toContain('Trial');
    expect(plan?.label).toContain('88');
    expect(plan?.tone).toBe('primary');
  });

  it('says when the trial ends today or has one day left', () => {
    expect(schoolPlan({ isMaster: false, subscriptionStatus: SubscriptionStatus.Trial, trialEndDate: daysFromNow(-1) }, now)?.label).toBe('Trial ends today');
    expect(schoolPlan({ isMaster: false, subscriptionStatus: SubscriptionStatus.Trial, trialEndDate: daysFromNow(1) }, now)?.label).toBe('Trial (1 day left)');
  });

  it('shows an active subscription as a paid school', () => {
    expect(schoolPlan({ isMaster: false, subscriptionStatus: SubscriptionStatus.Active, trialEndDate: undefined }, now)).toEqual({ label: 'Paid', tone: 'success' });
  });

  it('warns about a lapsed or suspended school and a school with no subscription', () => {
    expect(schoolPlan({ isMaster: false, subscriptionStatus: SubscriptionStatus.PastDue, trialEndDate: undefined }, now)?.tone).toBe('warning');
    expect(schoolPlan({ isMaster: false, subscriptionStatus: SubscriptionStatus.Suspended, trialEndDate: undefined }, now)?.label).toBe('Suspended');
    expect(schoolPlan({ isMaster: false, subscriptionStatus: undefined, trialEndDate: undefined }, now)?.label).toBe('No plan');
  });
});
