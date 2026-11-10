import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DashboardDataService } from '../../../../core/services/dashboard-data.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AppIconComponent } from '../../../../shared/components/app-icon/app-icon.component';
import { ChartCardComponent } from '../../../../shared/components/chart-card/chart-card.component';
import { CardMenuAction } from '../../../../shared/components/stat-card/stat-card.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-payment-methods-card',
  standalone: true,
  imports: [RouterLink, CurrencyPipe, ChartCardComponent, AppIconComponent, StatusBadgeComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-chart-card
      title="Payment Methods"
      [menuItems]="menuItems"
      (menuAction)="onMenuAction($event)"
    >
      <div class="mm-pay-cards">
        @for (card of dataService.paymentCards(); track card.id) {
          <button
            type="button"
            class="mm-bank-card"
            [class.mm-bank-card--dark]="card.theme === 'dark'"
            [class.mm-bank-card--light]="card.theme === 'light'"
            [class.is-selected]="selectedCardId() === card.id"
            (click)="toggleCardFilter(card.id)"
          >
            <!-- Subtle geometric circle motif matching MaterialM reference -->
            <svg
              class="mm-bank-card__motif"
              viewBox="0 0 120 120"
              fill="none"
              aria-hidden="true"
            >
              <circle cx="95" cy="95" r="56" fill="currentColor" opacity="0.08" />
              <circle cx="95" cy="95" r="34" fill="currentColor" opacity="0.1" />
            </svg>

            <div class="mm-bank-card__top">
              <span class="mm-bank-card__num">**** {{ card.last4 }}</span>
              @if (card.brand === 'visa') {
                <span class="mm-visa-mark">VISA</span>
              } @else {
                <span class="mm-mc-mark" aria-label="Mastercard">
                  <span class="mm-mc-mark__dot mm-mc-mark__dot--left"></span>
                  <span class="mm-mc-mark__dot mm-mc-mark__dot--right"></span>
                </span>
              }
            </div>

            <div class="mm-bank-card__bottom">
              <span class="mm-bank-card__label">Balance</span>
              <span class="mm-bank-card__balance">{{ card.balance | currency }}</span>
            </div>
          </button>
        }
      </div>

      <div class="mm-tx-header">
        <h4 class="mm-tx-title">Transactions</h4>
        <a routerLink="/apps/transactions" class="mm-tx-see-all">
          <span>See all</span>
          <app-icon name="chevron-right" [size]="16" />
        </a>
      </div>

      <div class="mm-tx-list">
        @for (tx of visibleTransactions(); track tx.id) {
          <div class="mm-tx-row">
            <div class="mm-tx-row__left">
              <span class="mm-tx-avatar">{{ tx.shortCode }}</span>
              <div class="mm-tx-info">
                <span class="mm-tx-merchant">{{ tx.merchant }}</span>
                <span class="mm-tx-date">{{ tx.dateLabel }}</span>
              </div>
            </div>

            <div class="mm-tx-row__status">
              <app-status-badge
                [label]="tx.status"
                [tone]="tx.status === 'Approved' ? 'success' : tx.status === 'Pending' ? 'warning' : 'error'"
              />
            </div>

            <div class="mm-tx-row__right">
              <span class="mm-tx-amount">
                {{ tx.isCredit ? '+' : '-' }} {{ tx.amount | currency }}
              </span>
              <span
                class="mm-tx-bonus"
                [class.mm-tx-bonus--credit]="tx.isCredit"
                [class.mm-tx-bonus--debit]="!tx.isCredit"
              >
                {{ tx.bonusLabel }}
              </span>
            </div>
          </div>
        }
      </div>
    </app-chart-card>
  `,
  styles: `
    .mm-pay-cards {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 20px;
      margin-bottom: 22px;
    }

    .mm-bank-card {
      appearance: none;
      border: 2px solid transparent;
      position: relative;
      overflow: hidden;
      border-radius: var(--mm-radius-card);
      padding: 22px 24px;
      min-height: 170px;
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      text-align: left;
      cursor: pointer;
      transition: transform var(--mm-duration) var(--mm-ease),
                  border-color var(--mm-duration) var(--mm-ease);

      &:hover {
        transform: translateY(-2px);
      }

      &.is-selected {
        border-color: var(--mm-primary);
      }

      &--dark {
        background-color: var(--mm-bg-card-navy);
        color: #ffffff;
        box-shadow: var(--mm-shadow-card);

        .mm-bank-card__label {
          color: rgba(255, 255, 255, 0.72);
        }
      }

      &--light {
        background-color: var(--mm-bg-subtle);
        color: var(--mm-text-primary);

        .mm-bank-card__label {
          color: var(--mm-text-muted);
        }
      }
    }

    .mm-bank-card__motif {
      position: absolute;
      right: -8px;
      bottom: -8px;
      width: 120px;
      height: 120px;
      pointer-events: none;
    }

    .mm-bank-card__top {
      display: flex;
      align-items: center;
      justify-content: space-between;
      position: relative;
      z-index: 1;
    }

    .mm-bank-card__num {
      font-size: var(--mm-text-lg);
      font-weight: 500;
      letter-spacing: 0.02em;
    }

    .mm-visa-mark {
      font-size: 1.05rem;
      font-weight: 800;
      font-style: italic;
      letter-spacing: 0.04em;
    }

    .mm-mc-mark {
      display: inline-flex;
      align-items: center;

      &__dot {
        width: 18px;
        height: 18px;
        border-radius: 999px;

        &--left {
          background-color: #1f2a3d;
          opacity: 0.9;
        }

        &--right {
          background-color: #52627a;
          margin-left: -7px;
          opacity: 0.85;
        }
      }
    }

    .mm-bank-card__bottom {
      display: flex;
      flex-direction: column;
      gap: 4px;
      position: relative;
      z-index: 1;
    }

    .mm-bank-card__label {
      font-size: var(--mm-text-sm);
      font-weight: 500;
    }

    .mm-bank-card__balance {
      font-size: var(--mm-text-lg);
      font-weight: 600;
    }

    .mm-tx-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding-bottom: 10px;
      border-bottom: 1px solid var(--mm-border-color);
    }

    .mm-tx-title {
      margin: 0;
      font-size: var(--mm-text-lg);
      font-weight: 500;
      color: var(--mm-text-primary);
    }

    .mm-tx-see-all {
      display: inline-flex;
      align-items: center;
      gap: 2px;
      font-size: var(--mm-text-sm);
      font-weight: 600;
      color: var(--mm-primary);
    }

    .mm-tx-list {
      display: flex;
      flex-direction: column;
    }

    .mm-tx-row {
      display: grid;
      grid-template-columns: minmax(0, 1.5fr) auto auto;
      align-items: center;
      gap: 16px;
      padding: 16px 0;

      & + & {
        border-top: 1px solid var(--mm-border-color);
      }
    }

    .mm-tx-row__left {
      display: flex;
      align-items: center;
      gap: 14px;
      min-width: 0;
    }

    .mm-tx-avatar {
      width: 44px;
      height: 44px;
      border-radius: var(--mm-radius-md);
      background-color: var(--mm-bg-subtle);
      color: var(--mm-text-muted);
      font-weight: 700;
      font-size: var(--mm-text-base);
      display: inline-flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    .mm-tx-info {
      display: flex;
      flex-direction: column;
      gap: 3px;
      min-width: 0;
    }

    .mm-tx-merchant {
      font-size: var(--mm-text-base);
      font-weight: 500;
      color: var(--mm-text-body);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .mm-tx-date {
      font-size: var(--mm-text-xs);
      color: var(--mm-text-muted);
    }

    .mm-tx-row__right {
      display: flex;
      flex-direction: column;
      align-items: flex-end;
      gap: 3px;
    }

    .mm-tx-amount {
      font-size: var(--mm-text-base);
      font-weight: 500;
      color: var(--mm-text-body);
      white-space: nowrap;
    }

    .mm-tx-bonus {
      font-size: var(--mm-text-xs);
      font-weight: 600;

      &--credit {
        color: var(--mm-success);
      }

      &--debit {
        color: var(--mm-error);
      }
    }

    @media (max-width: 640px) {
      .mm-pay-cards {
        grid-template-columns: 1fr;
      }

      .mm-tx-row {
        grid-template-columns: minmax(0, 1fr) auto;
      }

      .mm-tx-row__status {
        display: none;
      }
    }
  `,
})
export class PaymentMethodsCardComponent {
  protected readonly dataService = inject(DashboardDataService);
  private readonly notifications = inject(NotificationService);

  protected readonly selectedCardId = signal<string | null>(null);

  protected readonly menuItems: readonly CardMenuAction[] = [
    { id: 'all', label: 'Show All Cards' },
    { id: 'sync', label: 'Sync Card Balances' },
  ];

  protected readonly visibleTransactions = computed(() => {
    const filter = this.selectedCardId();
    const list = this.dataService.transactions();
    const matching = filter ? list.filter((tx) => tx.cardId === filter) : list;
    return matching.slice(0, 2);
  });

  protected toggleCardFilter(cardId: string): void {
    this.selectedCardId.update((curr) => (curr === cardId ? null : cardId));
  }

  protected onMenuAction(action: string): void {
    if (action === 'all') {
      this.selectedCardId.set(null);
      return;
    }
    this.notifications.notify('Payment card balances synchronized', 'success');
  }
}
