import { describe, expect, it } from 'vitest';
import { daysLeft, isBannerVisible } from './trial-banner.logic';

// AC2.3: days left, rounded down, never negative.
describe('daysLeft (AC2.3)', () => {
  const now = new Date('2026-11-17T12:00:00Z');

  it('is zero at the exact end of the trial', () => {
    expect(daysLeft(now, now)).toBe(0);
  });

  it('is zero after the trial has ended', () => {
    expect(daysLeft(new Date('2026-11-16T12:00:00Z'), now)).toBe(0);
  });

  it('rounds down a partial day', () => {
    expect(daysLeft(new Date('2026-11-18T11:59:00Z'), now)).toBe(0);
  });

  it('counts whole days left', () => {
    expect(daysLeft(new Date('2026-11-24T12:00:00Z'), now)).toBe(7);
  });
});

// AC2.1, AC2.2, AC2.4
describe('isBannerVisible (AC2.1, AC2.2, AC2.4)', () => {
  it('is shown for a school in trial', () => {
    expect(isBannerVisible({ isMaster: false, subscription: { isTrial: true } })).toBe(true);
  });

  it('is hidden when the subscription is not a trial', () => {
    expect(isBannerVisible({ isMaster: false, subscription: { isTrial: false } })).toBe(false);
  });

  it('is hidden when there is no subscription', () => {
    expect(isBannerVisible({ isMaster: false, subscription: null })).toBe(false);
  });

  it('is hidden for the master tenant even during a trial', () => {
    expect(isBannerVisible({ isMaster: true, subscription: { isTrial: true } })).toBe(false);
  });
});
