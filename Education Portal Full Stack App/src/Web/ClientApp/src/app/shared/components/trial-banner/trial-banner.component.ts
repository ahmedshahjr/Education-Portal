import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { SubscriptionsClient } from '../../../web-api-client';

/**
 * The banner across the top of a school's pages while its free trial runs: the days left. It shows nothing when the
 * school is not on a trial, and for the master tenant (which has no subscription).
 */
@Component({
  selector: 'app-trial-banner',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (daysLeft() !== null) {
      <div class="mm-trial-banner" role="status">
        @if (daysLeft() === 0) {
          <span i18n="@@trial.endsToday">The free trial ends today.</span>
        } @else {
          <span i18n="@@trial.active">Trial period active: {{ daysLeft() }} days remaining.</span>
        }
      </div>
    }
  `,
  styles: `
    .mm-trial-banner {
      padding: 8px 16px;
      background-color: var(--mm-primary-container);
      color: var(--mm-on-primary-container);
      font-size: var(--mm-text-sm);
      text-align: center;
    }
  `,
})
export class TrialBannerComponent {
  private readonly subscriptions = inject(SubscriptionsClient);

  /** The whole days left of the trial, or null when there is no trial. */
  protected readonly daysLeft = signal<number | null>(null);

  constructor() {
    this.subscriptions.getMySubscription().subscribe({
      next: (mine) => this.daysLeft.set(mine.trialDaysRemaining ?? null),
      error: () => this.daysLeft.set(null), // the master tenant has no subscription, and a failed request hides the banner
    });
  }
}
