import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexFill,
  ApexGrid,
  ApexLegend,
  ApexMarkers,
  ApexStroke,
  ApexTooltip,
  ApexXAxis,
  ApexYAxis,
  ChartComponent,
} from 'ng-apexcharts';
import { LayoutService } from '../../../../core/layout/layout.service';
import { DashboardDataService } from '../../../../core/services/dashboard-data.service';
import { AnalyticsCardComponent } from '../../../../shared/components/analytics-card/analytics-card.component';
import { FilterOption, FilterPanelComponent } from '../../../../shared/components/filter-panel/filter-panel.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';
import { OverallBalanceTab } from '../../../../shared/models/dashboard.models';

@Component({
  selector: 'app-overall-balance-card',
  standalone: true,
  imports: [
    MatCardModule,
    ChartComponent,
    FilterPanelComponent,
    StatusBadgeComponent,
    AnalyticsCardComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-card class="mm-card mm-elevated mm-balance-card">
      <mat-card-content class="mm-balance-card__content">
        <div class="mm-balance-grid">
          <div class="mm-balance-main">
            <div class="mm-balance-head">
              <div>
                <p class="mm-balance-eyebrow">Overall Balance</p>
                <h2 class="mm-balance-total">{{ data.totalBalance }}</h2>
                <div class="mm-balance-delta">
                  <app-status-badge
                    tone="success"
                    variant="soft"
                    prefixIcon="down"
                    [label]="data.deltaPercent"
                  />
                  <span class="mm-balance-period">{{ data.deltaLabel }}</span>
                </div>
              </div>

              <app-filter-panel
                [options]="tabs"
                [selected]="activeTab()"
                (selectedChange)="activeTab.set($event)"
                ariaLabel="Overall Balance series view"
              />
            </div>

            <div class="mm-balance-chart">
              <apx-chart
                [series]="chartSeries()"
                [chart]="chartConfig()"
                [colors]="chartColors()"
                [stroke]="chartStroke()"
                [fill]="chartFill()"
                [markers]="chartMarkers()"
                [dataLabels]="dataLabels"
                [legend]="legend"
                [grid]="chartGrid()"
                [xaxis]="chartXAxis()"
                [yaxis]="chartYAxis()"
                [tooltip]="chartTooltip()"
              />
            </div>
          </div>

          <div class="mm-balance-side">
            @for (stat of data.miniStats; track stat.id) {
              <app-analytics-card
                [label]="stat.label"
                [value]="stat.value"
                [icon]="stat.icon"
                [tone]="stat.tone"
              />
            }
          </div>
        </div>
      </mat-card-content>
    </mat-card>
  `,
  styles: `
    .mm-balance-card {
      height: 100%;
    }

    .mm-balance-card__content {
      padding: 24px !important;
      height: 100%;
    }

    .mm-balance-grid {
      display: grid;
      grid-template-columns: minmax(0, 2fr) minmax(200px, 1fr);
      gap: 24px;
      height: 100%;
    }

    .mm-balance-main {
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      padding-right: 24px;
      border-right: 1px solid var(--mm-border-color);
    }

    .mm-balance-head {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      flex-wrap: wrap;
      gap: 16px;
      margin-bottom: 12px;
    }

    .mm-balance-eyebrow {
      margin: 0;
      font-size: var(--mm-text-sm);
      font-weight: 400;
      color: var(--mm-text-secondary);
    }

    .mm-balance-total {
      margin: 4px 0 8px;
      font-size: var(--mm-text-3xl);
      font-weight: 500;
      letter-spacing: -0.02em;
      line-height: 1.15;
      color: var(--mm-text-primary);
    }

    .mm-balance-delta {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .mm-balance-period {
      font-size: var(--mm-text-xs);
      font-weight: 500;
      color: var(--mm-text-muted);
    }

    .mm-balance-chart {
      min-height: 225px;
      width: 100%;
    }

    .mm-balance-side {
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      gap: 14px;
    }

    @media (max-width: 1180px) {
      .mm-balance-grid {
        grid-template-columns: 1fr;
      }

      .mm-balance-main {
        padding-right: 0;
        border-right: 0;
        padding-bottom: 20px;
        border-bottom: 1px solid var(--mm-border-color);
      }

      .mm-balance-side {
        display: grid;
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }
    }

    @media (max-width: 560px) {
      .mm-balance-side {
        grid-template-columns: 1fr;
      }
    }
  `,
})
export class OverallBalanceCardComponent {
  private readonly dataService = inject(DashboardDataService);
  private readonly layout = inject(LayoutService);

  protected readonly data = this.dataService.overallBalanceData;
  protected readonly activeTab = signal<OverallBalanceTab>('orders');

  protected readonly tabs: readonly FilterOption<OverallBalanceTab>[] = [
    { id: 'orders', label: 'Orders' },
    { id: 'income', label: 'Income' },
  ];

  protected readonly dataLabels: ApexDataLabels = { enabled: false };
  protected readonly legend: ApexLegend = { show: false };

  protected readonly chartSeries = computed<ApexAxisChartSeries>(() => {
    if (this.activeTab() === 'orders') {
      return [
        { name: 'Orders', data: [...this.data.ordersPrimarySeries] },
        { name: 'Previous', data: [...this.data.ordersComparisonSeries] },
      ];
    }
    return [{ name: 'Income', data: [...this.data.incomeSeries] }];
  });

  protected readonly chartConfig = computed<ApexChart>(() => ({
    type: this.activeTab() === 'orders' ? 'line' : 'area',
    height: 220,
    fontFamily: 'Inter, sans-serif',
    toolbar: { show: false },
    sparkline: { enabled: false },
  }));

  protected readonly chartColors = computed<string[]>(() => {
    const compColor = this.layout.isDark() ? 'rgba(148, 163, 184, 0.35)' : '#dfe5ef';
    return this.activeTab() === 'orders' ? ['#00a1ff', compColor] : ['#00a1ff'];
  });

  protected readonly chartStroke = computed<ApexStroke>(() => ({
    curve: 'smooth',
    width: this.activeTab() === 'orders' ? [3, 3] : 2.5,
  }));

  protected readonly chartFill = computed<ApexFill>(() => {
    if (this.activeTab() === 'orders') {
      return { type: 'solid', opacity: 1 };
    }
    return {
      type: 'gradient',
      gradient: {
        shadeIntensity: 0.4,
        opacityFrom: 0.25,
        opacityTo: 0.02,
        stops: [0, 95, 100],
      },
    };
  });

  protected readonly chartMarkers = computed<ApexMarkers>(() => ({
    size: 0,
    hover: { size: 5 },
  }));

  protected readonly chartGrid = computed<ApexGrid>(() => ({
    show: this.activeTab() === 'income',
    borderColor: this.layout.isDark() ? 'rgba(255,255,255,0.08)' : '#e4ebf0',
    strokeDashArray: 3,
    padding: { left: 8, right: 8, top: 0, bottom: 0 },
  }));

  protected readonly chartXAxis = computed<ApexXAxis>(() => ({
    categories:
      this.activeTab() === 'orders'
        ? [...this.data.ordersCategories]
        : [...this.data.incomeCategories],
    axisBorder: { show: false },
    axisTicks: { show: false },
    labels: {
      style: {
        colors: this.layout.isDark() ? '#94a3b8' : '#7b8794',
        fontSize: '12px',
      },
    },
  }));

  protected readonly chartYAxis = computed<ApexYAxis>(() => ({
    show: this.activeTab() === 'income',
    min: this.activeTab() === 'orders' ? 5 : 10,
    max: this.activeTab() === 'orders' ? 60 : 75,
    tickAmount: 6,
    labels: {
      style: {
        colors: this.layout.isDark() ? '#94a3b8' : '#7b8794',
        fontSize: '11px',
      },
    },
  }));

  protected readonly chartTooltip = computed<ApexTooltip>(() => ({
    theme: this.layout.isDark() ? 'dark' : 'light',
  }));
}
