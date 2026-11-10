import { CurrencyPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { RouterLink } from '@angular/router';
import { DashboardDataService } from '../../core/services/dashboard-data.service';
import { DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { FilterOption, FilterPanelComponent } from '../../shared/components/filter-panel/filter-panel.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { TransactionItem, TransactionStatus } from '../../shared/models/dashboard.models';
import { BreadcrumbItem } from '../../shared/models/navigation.models';

type TxStatusFilter = 'All' | TransactionStatus;

@Component({
  selector: 'app-transactions-page',
  standalone: true,
  imports: [
    RouterLink,
    CurrencyPipe,
    MatCardModule,
    MatButtonModule,
    PageHeaderComponent,
    FilterPanelComponent,
    DataTableComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Transactions & Payment Methods"
      subtitle="Full ledger of card settlements, payouts, and bonus rewards"
      [breadcrumbs]="breadcrumbs"
    >
      <a mat-stroked-button routerLink="/dashboards/dashboard3">Back to Dashboard 3</a>
    </app-page-header>

    <div class="mm-grid">
      @for (card of dataService.paymentCards(); track card.id) {
        <div class="mm-col-6">
          <mat-card class="mm-card mm-elevated">
            <mat-card-content class="mm-tx-card-summary">
              <div>
                <div class="mm-tx-card-eyebrow">
                  {{ card.brand === 'visa' ? 'VISA Corporate' : 'Mastercard Business' }} · **** {{ card.last4 }}
                </div>
                <div class="mm-tx-card-bal">{{ card.balance | currency }}</div>
              </div>
              <span class="mm-tx-card-exp">Exp {{ card.expiry }}</span>
            </mat-card-content>
          </mat-card>
        </div>
      }

      <div class="mm-col-12">
        <mat-card class="mm-card mm-elevated">
          <mat-card-content>
            <app-data-table
              [data]="filteredTransactions()"
              [columns]="columns"
              searchPlaceholder="Search merchant or category..."
            >
              <div table-actions>
                <app-filter-panel
                  [options]="statusTabs"
                  [selected]="statusFilter()"
                  (selectedChange)="statusFilter.set($event)"
                />
              </div>
            </app-data-table>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: `
    .mm-tx-card-summary {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 16px;
    }

    .mm-tx-card-eyebrow {
      font-size: var(--mm-text-xs);
      color: var(--mm-text-muted);
      margin-bottom: 6px;
    }

    .mm-tx-card-bal {
      font-size: var(--mm-text-2xl);
      font-weight: 600;
      color: var(--mm-text-primary);
    }

    .mm-tx-card-exp {
      font-size: var(--mm-text-xs);
      font-weight: 600;
      padding: 6px 12px;
      border-radius: var(--mm-radius-pill);
      background-color: var(--mm-light-primary);
      color: var(--mm-primary);
    }
  `,
})
export class TransactionsPageComponent {
  protected readonly dataService = inject(DashboardDataService);

  protected readonly breadcrumbs: readonly BreadcrumbItem[] = [
    { label: 'Dashboard 3', route: '/dashboards/dashboard3' },
    { label: 'Transactions' },
  ];

  protected readonly statusTabs: readonly FilterOption<TxStatusFilter>[] = [
    { id: 'All', label: 'All' },
    { id: 'Approved', label: 'Approved' },
    { id: 'Pending', label: 'Pending' },
  ];

  protected readonly statusFilter = signal<TxStatusFilter>('All');

  protected readonly filteredTransactions = computed(() => {
    const status = this.statusFilter();
    const list = this.dataService.transactions();
    return status === 'All' ? list : list.filter((tx) => tx.status === status);
  });

  protected readonly columns: readonly DataTableColumn<TransactionItem>[] = [
    { key: 'merchant', header: 'Merchant' },
    { key: 'category', header: 'Category' },
    { key: 'dateLabel', header: 'Date & Time' },
    { key: 'status', header: 'Status' },
    {
      key: 'amount',
      header: 'Amount',
      align: 'right',
      valueFn: (row) => `${row.isCredit ? '+' : '-'} $${row.amount.toFixed(2)}`,
    },
    { key: 'bonusLabel', header: 'Rewards', align: 'right' },
  ];
}
