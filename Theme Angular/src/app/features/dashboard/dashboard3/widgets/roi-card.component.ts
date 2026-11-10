import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexGrid,
  ApexLegend,
  ApexPlotOptions,
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
import { CardMenuAction } from '../../../../shared/components/stat-card/stat-card.component';
import { RoiPeriod } from '../../../../shared/models/dashboard.models';

@Component({
  selector: 'app-roi-card',
  standalone: true,
  imports: [ChartComponent, ChartCardComponent, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-chart-card
      title="Return On Investment"
      [tinted]="true"
      [menuItems]="menuItems"
      (menuAction)="onMenuAction($event)"
    >
      <div class="mm-roi-stats">
        <span class="mm-roi-icon-pill">
          <app-icon name="trending-up" [size]="20" />
        </span>
        <div>
          <div class="mm-roi-val">{{ dataset().percentage }}</div>
          <div class="mm-roi-sub">ROI</div>
        </div>
        <div>
          <div class="mm-roi-delta">{{ dataset().delta }}</div>
          <div class="mm-roi-sub">{{ dataset().monthLabel }}</div>
        </div>
      </div>

      <div class="mm-roi-chart">
        <apx-chart
          [series]="chartSeries()"
          [chart]="chartConfig"
          [plotOptions]="plotOptions"
          [colors]="chartColors()"
          [dataLabels]="dataLabels"
          [legend]="legend"
          [grid]="grid"
          [xaxis]="chartXAxis()"
          [yaxis]="yaxis"
          [tooltip]="chartTooltip()"
        />
      </div>
    </app-chart-card>
  `,
  styles: `
    .mm-roi-stats {
      display: flex;
      align-items: center;
      gap: 20px;
      margin-bottom: 6px;
    }

    .mm-roi-icon-pill {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      padding: 10px 16px;
      border-radius: var(--mm-radius-pill);
      background-color: var(--mm-bg-surface);
      color: var(--mm-text-primary);
    }

    .mm-roi-val {
      font-size: var(--mm-text-xl);
      font-weight: 500;
      color: var(--mm-text-primary);
      line-height: 1.2;
    }

    .mm-roi-delta {
      font-size: var(--mm-text-sm);
      font-weight: 500;
      color: var(--mm-primary);
      line-height: 1.3;
    }

    .mm-roi-sub {
      font-size: var(--mm-text-xs);
      font-weight: 500;
      color: var(--mm-text-muted);
    }

    .mm-roi-chart {
      margin-top: auto;
      min-height: 230px;
    }
  `,
})
export class RoiCardComponent {
  private readonly dataService = inject(DashboardDataService);
  private readonly layout = inject(LayoutService);
  private readonly notifications = inject(NotificationService);

  protected readonly period = signal<RoiPeriod>('h1');
  protected readonly dataset = computed(() => this.dataService.roiDatasets[this.period()]);

  protected readonly menuItems: readonly CardMenuAction[] = [
    { id: 'h1', label: 'Jan - Jun (H1)' },
    { id: 'h2', label: 'Jul - Dec (H2)' },
    { id: 'export', label: 'Download ROI Summary' },
  ];

  protected readonly chartConfig: ApexChart = {
    type: 'bar',
    height: 230,
    fontFamily: 'Inter, sans-serif',
    toolbar: { show: false },
  };

  protected readonly plotOptions: ApexPlotOptions = {
    bar: {
      columnWidth: '42%',
      distributed: true,
      borderRadius: 8,
      borderRadiusApplication: 'around',
    },
  };

  protected readonly dataLabels: ApexDataLabels = { enabled: false };
  protected readonly legend: ApexLegend = { show: false };
  protected readonly yaxis: ApexYAxis = { show: false, max: 100 };
  protected readonly grid: ApexGrid = {
    show: false,
    padding: { left: 0, right: 0, top: 0, bottom: 0 },
  };

  protected readonly chartSeries = computed<ApexAxisChartSeries>(() => [
    { name: 'ROI', data: [...this.dataset().values] },
  ]);

  protected readonly chartColors = computed<string[]>(() => {
    const muted = this.layout.isDark() ? 'rgba(255,255,255,0.09)' : 'rgba(0,0,0,0.07)';
    const hi = this.dataset().highlightIndex;
    return this.dataset().values.map((_, idx) => (idx === hi ? '#00a1ff' : muted));
  });

  protected readonly chartXAxis = computed<ApexXAxis>(() => ({
    categories: [...this.dataset().categories],
    axisBorder: { show: false },
    axisTicks: { show: false },
    labels: {
      style: {
        colors: this.layout.isDark() ? '#94a3b8' : '#7b8794',
        fontSize: '11px',
        fontWeight: 500,
      },
    },
  }));

  protected readonly chartTooltip = computed<ApexTooltip>(() => ({
    theme: this.layout.isDark() ? 'dark' : 'light',
    y: {
      formatter: (val: number) => `${val}%`,
    },
  }));

  protected onMenuAction(actionId: string): void {
    if (actionId === 'h1' || actionId === 'h2') {
      this.period.set(actionId);
      return;
    }
    this.notifications.notify('ROI report snapshot downloaded', 'success');
  }
}
