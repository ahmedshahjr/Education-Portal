import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexLegend,
  ApexPlotOptions,
  ApexStroke,
  ApexXAxis,
  ChartComponent,
} from 'ng-apexcharts';
import { DashboardDataService } from '../../../core/services/dashboard-data.service';
import { AnalyticsCardComponent } from '../../../shared/components/analytics-card/analytics-card.component';
import { ChartCardComponent } from '../../../shared/components/chart-card/chart-card.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { StatCardComponent } from '../../../shared/components/stat-card/stat-card.component';
import { BreadcrumbItem } from '../../../shared/models/navigation.models';
import { CompactNumberPipe } from '../../../shared/pipes/compact-number.pipe';

@Component({
  selector: 'app-dashboard-overview-page',
  standalone: true,
  imports: [
    RouterLink,
    MatButtonModule,
    ChartComponent,
    PageHeaderComponent,
    StatCardComponent,
    ChartCardComponent,
    AnalyticsCardComponent,
    CompactNumberPipe,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      [title]="pageTitle()"
      [subtitle]="pageSubtitle()"
      [breadcrumbs]="breadcrumbs()"
    >
      <a mat-flat-button color="primary" routerLink="/dashboards/dashboard3">
        Open Dashboard 3
      </a>
    </app-page-header>

    <div class="mm-grid">
      <div class="mm-col-4">
        <app-stat-card
          title="Active Projects"
          [value]="'' + dataService.projects().length"
          trendBadge="+14% this sprint"
          icon="kanban"
          tone="primary"
        />
      </div>
      <div class="mm-col-4">
        <app-stat-card
          title="Tracked Budget"
          [value]="dataService.totalProjectBudget() | compactNumber : '$'"
          trendBadge="+21% quarter"
          icon="dollar-sign"
          tone="success"
        />
      </div>
      <div class="mm-col-4">
        <app-stat-card
          title="Transactions Logged"
          [value]="'' + dataService.transactions().length"
          trendBadge="100% reconciled"
          icon="wallet"
          tone="secondary"
        />
      </div>

      <div class="mm-col-8">
        <app-chart-card [title]="chartTitle()">
          <apx-chart
            [series]="series()"
            [chart]="barChart"
            [colors]="['#00a1ff', '#00ceb6']"
            [plotOptions]="barPlot"
            [dataLabels]="noLabels"
            [stroke]="stroke"
            [legend]="legend"
            [xaxis]="xaxis"
          />
        </app-chart-card>
      </div>

      <div class="mm-col-4">
        <app-chart-card title="Channel Summary" [showMenu]="false">
          <div class="mm-ov-tiles">
            @for (tile of dataService.overallBalanceData.miniStats; track tile.id) {
              <app-analytics-card
                [label]="tile.label"
                [value]="tile.value"
                [icon]="tile.icon"
                [tone]="tile.tone"
              />
            }
          </div>
        </app-chart-card>
      </div>
    </div>
  `,
  styles: `
    .mm-ov-tiles {
      display: flex;
      flex-direction: column;
      gap: 14px;
    }
  `,
})
export class DashboardOverviewPageComponent {
  protected readonly dataService = inject(DashboardDataService);
  readonly variant = input<'1' | '2'>('1');

  protected readonly pageTitle = computed(() =>
    this.variant() === '2' ? 'Dashboard 2 — Revenue Velocity' : 'Dashboard 1 — Executive Summary'
  );

  protected readonly pageSubtitle = computed(() =>
    this.variant() === '2'
      ? 'Monthly revenue breakdown, margin velocity, and channel attribution'
      : 'High-level platform telemetry built with reusable MaterialM components'
  );

  protected readonly chartTitle = computed(() =>
    this.variant() === '2' ? 'Monthly Revenue vs Target' : 'Weekly Order Throughput'
  );

  protected readonly breadcrumbs = computed<readonly BreadcrumbItem[]>(() => [
    { label: 'Home', route: '/dashboards/dashboard3' },
    { label: `Dashboard ${this.variant()}` },
  ]);

  protected readonly barChart: ApexChart = {
    type: 'bar',
    height: 290,
    fontFamily: 'Inter, sans-serif',
    toolbar: { show: false },
  };

  protected readonly barPlot: ApexPlotOptions = {
    bar: {
      columnWidth: '42%',
      borderRadius: 8,
    },
  };

  protected readonly noLabels: ApexDataLabels = { enabled: false };
  protected readonly stroke: ApexStroke = { show: true, width: 4, colors: ['transparent'] };
  protected readonly legend: ApexLegend = { position: 'top' };
  protected readonly xaxis: ApexXAxis = {
    categories: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
  };

  protected readonly series = computed<ApexAxisChartSeries>(() =>
    this.variant() === '2'
      ? [
          { name: 'Revenue ($k)', data: [44, 55, 57, 56, 61, 58, 68] },
          { name: 'Target ($k)', data: [35, 41, 46, 48, 52, 50, 58] },
        ]
      : [
          { name: 'Completed Orders', data: [62, 74, 68, 86, 92, 79, 96] },
          { name: 'Active Users', data: [48, 54, 59, 64, 71, 66, 78] },
        ]
  );
}
