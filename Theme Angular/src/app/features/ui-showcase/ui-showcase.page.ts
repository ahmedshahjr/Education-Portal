import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { NotificationService } from '../../core/services/notification.service';
import { AnalyticsCardComponent } from '../../shared/components/analytics-card/analytics-card.component';
import { ChartCardComponent } from '../../shared/components/chart-card/chart-card.component';
import {
  ConfirmationDialogComponent,
  ConfirmationDialogData,
} from '../../shared/components/confirmation-dialog/confirmation-dialog.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { FilterOption, FilterPanelComponent } from '../../shared/components/filter-panel/filter-panel.component';
import { FormInputComponent } from '../../shared/components/form-input/form-input.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { SearchInputComponent } from '../../shared/components/search-input/search-input.component';
import { SelectInputComponent, SelectOption } from '../../shared/components/select-input/select-input.component';
import { StatCardComponent } from '../../shared/components/stat-card/stat-card.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { BreadcrumbItem } from '../../shared/models/navigation.models';

@Component({
  selector: 'app-ui-showcase-page',
  standalone: true,
  imports: [
    MatCardModule,
    MatButtonModule,
    PageHeaderComponent,
    StatCardComponent,
    ChartCardComponent,
    AnalyticsCardComponent,
    StatusBadgeComponent,
    FilterPanelComponent,
    SearchInputComponent,
    FormInputComponent,
    SelectInputComponent,
    LoadingStateComponent,
    EmptyStateComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="MaterialM Reusable UI Component Kit"
      subtitle="Centralized design tokens, cards, inputs, badges, and state patterns built for Angular 22"
      [breadcrumbs]="breadcrumbs"
    >
      <button mat-flat-button color="primary" type="button" (click)="openSampleDialog()">
        Test Confirmation Dialog
      </button>
    </app-page-header>

    <div class="mm-grid">
      <div class="mm-col-4">
        <app-stat-card
          title="Conversion Rate"
          value="18.4%"
          trendBadge="+4.2% vs target"
          icon="trending-up"
          tone="primary"
        />
      </div>
      <div class="mm-col-4">
        <app-stat-card
          title="Active Subscriptions"
          value="2,840"
          trendBadge="+11% this month"
          icon="credit-card"
          tone="secondary"
        />
      </div>
      <div class="mm-col-4">
        <app-stat-card
          title="Net Settlement"
          value="$84,120"
          trendBadge="+29% last month"
          icon="banknote"
          tone="success"
        />
      </div>

      <div class="mm-col-6">
        <app-chart-card title="Badges, Pills & Filters" [showMenu]="false">
          <div class="mm-kit-stack">
            <div class="mm-kit-row">
              <app-status-badge tone="primary" label="Primary" />
              <app-status-badge tone="secondary" label="Secondary" />
              <app-status-badge tone="success" prefixIcon="up" label="16.3% Approved" />
              <app-status-badge tone="warning" label="Pending" />
              <app-status-badge tone="error" prefixIcon="down" label="Risk" />
              <app-status-badge variant="outlined" label="+23% outlined" />
            </div>

            <app-filter-panel
              [options]="filterOptions"
              [selected]="activeFilter()"
              (selectedChange)="activeFilter.set($event)"
            />

            <app-search-input
              [value]="searchSample()"
              (valueChange)="searchSample.set($event)"
              placeholder="Reusable search input component..."
            />

            <div class="mm-kit-tiles">
              <app-analytics-card label="Total Sales" value="$14,673" icon="trending-up" tone="warning" />
              <app-analytics-card label="Total Profit" value="$9,281" icon="dollar-sign" tone="error" />
            </div>
          </div>
        </app-chart-card>
      </div>

      <div class="mm-col-6">
        <app-chart-card title="Form Controls & Async States" [showMenu]="false">
          <div class="mm-kit-stack">
            <div class="mm-kit-form-row">
              <app-form-input label="Workspace Email" [control]="emailCtrl" />
              <app-select-input label="Region" [control]="regionCtrl" [options]="regions" />
            </div>
            <app-loading-state [rows]="3" />
            <app-empty-state
              title="Empty State Preview"
              description="Reusable empty state with CTA callback."
              actionLabel="Trigger Notification"
              (action)="notifyDemo()"
            />
          </div>
        </app-chart-card>
      </div>
    </div>
  `,
  styles: `
    .mm-kit-stack {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .mm-kit-row {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }

    .mm-kit-tiles,
    .mm-kit-form-row {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 12px;
    }

    @media (max-width: 560px) {
      .mm-kit-tiles,
      .mm-kit-form-row {
        grid-template-columns: 1fr;
      }
    }
  `,
})
export class UiShowcasePageComponent {
  private readonly dialog = inject(MatDialog);
  private readonly notifications = inject(NotificationService);

  protected readonly breadcrumbs: readonly BreadcrumbItem[] = [
    { label: 'Dashboard 3', route: '/dashboards/dashboard3' },
    { label: 'UI Components' },
  ];

  protected readonly filterOptions: readonly FilterOption[] = [
    { id: 'all', label: 'All Items' },
    { id: 'active', label: 'Active' },
    { id: 'archived', label: 'Archived' },
  ];

  protected readonly activeFilter = signal('all');
  protected readonly searchSample = signal('');

  protected readonly emailCtrl = new FormControl('info@materialm.io', {
    nonNullable: true,
    validators: [Validators.required, Validators.email],
  });

  protected readonly regionCtrl = new FormControl('us-east', { nonNullable: true });
  protected readonly regions: readonly SelectOption[] = [
    { value: 'us-east', label: 'North America (US-East)' },
    { value: 'eu-west', label: 'Europe (EU-West)' },
    { value: 'ap-south', label: 'Asia Pacific' },
  ];

  protected openSampleDialog(): void {
    const data: ConfirmationDialogData = {
      title: 'Confirm Action',
      message: 'This reusable confirmation dialog follows MaterialM border radius and typography tokens.',
      confirmText: 'Proceed',
    };
    this.dialog
      .open(ConfirmationDialogComponent, { data, width: '420px', maxWidth: '92vw' })
      .afterClosed()
      .subscribe((ok) => {
        if (ok) this.notifyDemo();
      });
  }

  protected notifyDemo(): void {
    this.notifications.notify('Reusable MaterialM snackbar triggered', 'success');
  }
}
