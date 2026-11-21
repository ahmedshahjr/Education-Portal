import { ChangeDetectionStrategy, Component, computed, inject, input, OnInit, signal } from '@angular/core';
import { SubscriptionsClient } from '../../../web-api-client';

/**
 * The banner across the top of a school's pages while its free trial runs: the days left. It shows nothing when the
 * school is not on a trial, and for the master tenant (which has no subscription).
 *
 * Inside the app it loads the school's own subscription. The sign-in page has no session, so it passes the days from the
 * public trial notice instead (`[daysRemaining]`, `null` for no trial) and nothing is loaded here.
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
export class TrialBannerComponent implements OnInit {
  private readonly subscriptions = inject(SubscriptionsClient);

  /** The days from the public trial notice (`null`: no trial). Left out, the banner loads the signed-in school's subscription. */
  readonly daysRemaining = input<number | null | undefined>(undefined);

  private readonly loaded = signal<number | null>(null);

  /** The whole days left of the trial, or null when there is no trial. */
  protected readonly daysLeft = computed(() => this.daysRemaining() ?? this.loaded());

  ngOnInit(): void {
    if (this.daysRemaining() !== undefined) {
      return;
    }

    this.subscriptions.getMySubscription().subscribe({
      next: (mine) => this.loaded.set(mine.trialDaysRemaining ?? null),
      error: () => this.loaded.set(null), // the master tenant has no subscription, and a failed request hides the banner
    });
  }
}
