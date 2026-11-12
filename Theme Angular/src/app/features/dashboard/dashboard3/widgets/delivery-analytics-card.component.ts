import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexFill,
  ApexLegend,
  ApexMarkers,
  ApexPlotOptions,
  ApexStroke,
  ApexTooltip,
  ApexXAxis,
  ApexYAxis,
  ChartComponent,
} from 'ng-apexcharts';
import { LayoutService } from '../../../../core/layout/layout.service';
import { DashboardDataService } from '../../../../core/services/dashboard-data.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AppIconComponent } from '../../../../shared/components/app-icon/app-icon.component';
import { ChartCardComponent } from '../../../../shared/components/chart-card/chart-card.component';
import { FilterOption } from '../../../../shared/components/filter-panel/filter-panel.component';
import { DeliveryTab } from '../../../../shared/models/dashboard.models';

@Component({
  selector: 'app-delivery-analytics-card',
  standalone: true,
  imports: [ChartComponent, ChartCardComponent, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-chart-card
      title="Delivery Analytics"
      [tabs]="tabs"
      [selectedTab]="selectedTab()"
      (selectedTabChange)="onTabChange($event)"
      (menuAction)="onMenuAction($event)"
    >
      <div class="mm-del-grid">
        <div class="mm-del-radar">
          <apx-chart
            [series]="chartSeries()"
            [chart]="chartConfig"
            [colors]="['#00a1ff', '#ffe4ec']"
            [stroke]="chartStroke"
            [fill]="chartFill"
            [markers]="chartMarkers"
            [plotOptions]="chartPlot()"
            [dataLabels]="noDataLabels"
            [legend]="noLegend"
            [xaxis]="chartXAxis()"
            [yaxis]="chartYAxis"
            [tooltip]="chartTooltip()"
          />
        </div>

        <div class="mm-del-aside">
          <div class="mm-del-banner">
            <app-icon name="rocket" [size]="24" iconClass="mm-del-banner__icon" />
            <h4 class="mm-del-banner__title">{{ dataset().headline }}</h4>
            <p class="mm-del-banner__sub">{{ dataset().subheadline }}</p>
          </div>

          <ul class="mm-del-metrics">
            @for (m of dataset().metrics; track m.id) {
              <li class="mm-del-metric">
                <span class="mm-del-dot" [style.background-color]="m.color"></span>
                <span class="mm-del-val">{{ m.value }}</span>
                <span class="mm-del-label">{{ m.label }}</span>
              </li>
            }
          </ul>
        </div>
      </div>
    </app-chart-card>
  `,
  styles: `
    .mm-del-grid {
      display: grid;
      grid-template-columns: minmax(0, 1.35fr) minmax(220px, 1fr);
      gap: 24px;
      align-items: center;
      flex: 1;
    }

    .mm-del-radar {
      min-height: 335px;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .mm-del-aside {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .mm-del-banner {
      padding: 20px 22px;
      border-radius: var(--mm-radius-lg);
      background-color: var(--mm-bg-subtle);
      display: flex;
      flex-direction: column;
      gap: 6px;
    }

    :host ::ng-deep .mm-del-banner__icon {
      color: var(--mm-primary);
      margin-bottom: 4px;
    }

    .mm-del-banner__title {
      margin: 0;
      font-size: var(--mm-text-lg);
      font-weight: 500;
      color: var(--mm-text-primary);
    }

    .mm-del-banner__sub {
      margin: 0;
      font-size: var(--mm-text-sm);
      color: var(--mm-text-muted);
    }

    .mm-del-metrics {
      list-style: none;
      margin: 0;
      padding: 0 4px;
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .mm-del-metric {
      display: flex;
      align-items: center;
      gap: 12px;
    }

    .mm-del-dot {
      width: 18px;
      height: 8px;
      border-radius: var(--mm-radius-pill);
      flex-shrink: 0;
    }

    .mm-del-val {
      font-size: var(--mm-text-base);
      font-weight: 600;
      color: var(--mm-text-primary);
      min-width: 52px;
    }

    .mm-del-label {
      font-size: var(--mm-text-sm);
      color: var(--mm-text-muted);
    }

    @media (max-width: 900px) {
      .mm-del-grid {
        grid-template-columns: 1fr;
      }
    }
  `,
})
export class DeliveryAnalyticsCardComponent {
  private readonly dataService = inject(DashboardDataService);
  private readonly layout = inject(LayoutService);
  private readonly notifications = inject(NotificationService);

  protected readonly selectedTab = signal<DeliveryTab>('email');
  protected readonly dataset = computed(() => this.dataService.deliveryDatasets[this.selectedTab()]);

  protected readonly tabs: readonly FilterOption<DeliveryTab>[] = [
    { id: 'email', label: 'Email' },
    { id: 'last_week', label: 'From last week' },
  ];

  protected readonly chartConfig: ApexChart = {
    type: 'radar',
    height: 345,
    fontFamily: 'Inter, sans-serif',
    toolbar: { show: false },
  };

  protected readonly chartStroke: ApexStroke = { width: 2 };
  protected readonly chartFill: ApexFill = { opacity: 0.92 };
  protected readonly chartMarkers: ApexMarkers = { size: 3 };
  protected readonly noDataLabels: ApexDataLabels = { enabled: false };
  protected readonly noLegend: ApexLegend = { show: false };
  protected readonly chartYAxis: ApexYAxis = { show: false, min: 0, max: 100 };

  protected readonly chartPlot = computed<ApexPlotOptions>(() => ({
    radar: {
      polygons: {
        strokeColors: this.layout.isDark() ? 'rgba(255,255,255,0.1)' : '#e4ebf0',
        connectorColors: this.layout.isDark() ? 'rgba(255,255,255,0.1)' : '#e4ebf0',
      },
    },
  }));

  protected readonly chartSeries = computed<ApexAxisChartSeries>(() => {
    const ds = this.dataset();
    return [
      { name: ds.primaryName, data: [...ds.primarySeries] },
      { name: ds.secondaryName, data: [...ds.secondarySeries] },
    ];
  });

  protected readonly chartXAxis = computed<ApexXAxis>(() => ({
    categories: [...this.dataset().categories],
    labels: {
      style: {
        colors: Array(6).fill(this.layout.isDark() ? '#94a3b8' : '#7b8794'),
        fontSize: '12px',
      },
    },
  }));

  protected readonly chartTooltip = computed<ApexTooltip>(() => ({
    theme: this.layout.isDark() ? 'dark' : 'light',
  }));

  protected onTabChange(tab: string): void {
    if (tab === 'email' || tab === 'last_week') {
      this.selectedTab.set(tab);
    }
  }

  protected onMenuAction(action: string): void {
    this.notifications.notify(
      action === 'download' ? 'Delivery analytics exported' : 'Delivery telemetry refreshed',
      'primary'
    );
  }
}
