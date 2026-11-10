import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexPlotOptions,
  ApexXAxis,
  ChartComponent,
} from 'ng-apexcharts';
import { DashboardDataService } from '../../core/services/dashboard-data.service';
import { ChartCardComponent } from '../../shared/components/chart-card/chart-card.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { BreadcrumbItem } from '../../shared/models/navigation.models';

@Component({
  selector: 'app-reports-page',
  standalone: true,
  imports: [RouterLink, MatButtonModule, ChartComponent, PageHeaderComponent, ChartCardComponent, StatusBadgeComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Marketing & Earning Reports"
      subtitle="Regional settlements, startup funnel efficiency, and campaign channel breakdown"
      [breadcrumbs]="breadcrumbs"
    >
      <a mat-stroked-button routerLink="/dashboards/dashboard3">Back to Dashboard 3</a>
    </app-page-header>

    <div class="mm-grid">
      <div class="mm-col-7">
        <app-chart-card title="Market Revenue Attribution">
          <apx-chart
            [series]="marketSeries"
            [chart]="marketChart"
            [colors]="['#00a1ff', '#8965e5', '#00ceb6', '#ffae1f']"
            [plotOptions]="marketPlot"
            [dataLabels]="noLabels"
            [xaxis]="marketXAxis"
          />
        </app-chart-card>
      </div>

      <div class="mm-col-5">
        <app-chart-card title="Market Channel Growth" [showMenu]="false">
          <div class="mm-rep-list">
            @for (item of dataService.earningReports; track item.id) {
              <div class="mm-rep-row">
                <div>
                  <div class="mm-rep-title">{{ item.title }} ({{ item.revenue }})</div>
                  <div class="mm-rep-desc">{{ item.details }}</div>
                </div>
                <app-status-badge tone="success" prefixIcon="up" [label]="item.growthPercent" />
              </div>
            }
          </div>
        </app-chart-card>
      </div>
    </div>
  `,
  styles: `
    .mm-rep-list {
      display: flex;
      flex-direction: column;
      gap: 14px;
    }

    .mm-rep-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
      padding: 14px 16px;
      border-radius: var(--mm-radius-md);
      background-color: var(--mm-bg-subtle);
    }

    .mm-rep-title {
      font-size: var(--mm-text-sm);
      font-weight: 600;
      color: var(--mm-text-primary);
    }

    .mm-rep-desc {
      font-size: var(--mm-text-xs);
      color: var(--mm-text-muted);
      margin-top: 2px;
    }
  `,
})
export class ReportsPageComponent {
  protected readonly dataService = inject(DashboardDataService);

  protected readonly breadcrumbs: readonly BreadcrumbItem[] = [
    { label: 'Dashboard 3', route: '/dashboards/dashboard3' },
    { label: 'Earning Reports' },
  ];

  protected readonly marketSeries: ApexAxisChartSeries = [
    { name: 'Settlement ($k)', data: [48.9, 31.4, 29.8, 14.2] },
  ];

  protected readonly marketChart: ApexChart = {
    type: 'bar',
    height: 310,
    fontFamily: 'Inter, sans-serif',
    toolbar: { show: false },
  };

  protected readonly marketPlot: ApexPlotOptions = {
    bar: {
      distributed: true,
      borderRadius: 10,
      columnWidth: '45%',
    },
  };

  protected readonly noLabels: ApexDataLabels = { enabled: false };
  protected readonly marketXAxis: ApexXAxis = {
    categories: ['ACH / Wire', 'Net Profit', 'SEPA / UK', 'Expenses'],
  };
}
