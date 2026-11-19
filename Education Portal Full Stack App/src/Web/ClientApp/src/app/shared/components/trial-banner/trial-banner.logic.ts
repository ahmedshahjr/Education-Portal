/** Whole days from `now` to `trialEnd`, rounded down, never negative (AC2.3). */
export function daysLeft(trialEnd: Date, now: Date): number {
  const milliseconds = trialEnd.getTime() - now.getTime();
  if (milliseconds <= 0) {
    return 0;
  }
  return Math.floor(milliseconds / (24 * 60 * 60 * 1000));
}

/** The banner shows for a school in its trial, never for the master tenant (AC2.1, AC2.2, AC2.4). */
export function isBannerVisible(state: { isMaster: boolean; subscription: { isTrial: boolean } | null }): boolean {
  return !state.isMaster && state.subscription?.isTrial === true;
}
