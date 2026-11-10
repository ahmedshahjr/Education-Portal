import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexFill,
  ApexGrid,
  ApexLegend,
  ApexNonAxisChartSeries,
  ApexPlotOptions,
  ApexStroke,
  ApexTooltip,
  ApexXAxis,
  ApexYAxis,
  ChartComponent,
} from 'ng-apexcharts';
import { LayoutService } from '../../../../core/layout/layout.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { StatCardComponent } from '../../../../shared/components/stat-card/stat-card.component';

@Component({
  selector: 'app-kpi-trio-row',
  standalone: true,
  imports: [StatCardComponent, ChartComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-grid">
      <div class="mm-col-4">
        <app-stat-card
          title="Total followers"
          [value]="followersValue()"
          [trendBadge]="quarterMode() ? '+31% last quarter' : '+23% last month'"
          icon="users"
          tone="error"
          (menuAction)="onCardAction('Total followers', $event)"
        >
          <apx-chart
            [series]="followersSeries()"
            [chart]="followersChart"
            [plotOptions]="followersPlot"
            [colors]="followersColors()"
            [dataLabels]="noDataLabels"
            [legend]="noLegend"
            [grid]="noGrid"
            [xaxis]="hiddenXAxis"
            [yaxis]="hiddenYAxis"
            [tooltip]="tooltip()"
          />
        </app-stat-card>
      </div>

      <div class="mm-col-4">
        <app-stat-card
          title="Total Income"
          [value]="incomeValue()"
          [trendBadge]="quarterMode() ? '+27% last quarter' : '+18% last month'"
          icon="wallet"
          tone="secondary"
          (menuAction)="onCardAction('Total Income', $event)"
        >
          <apx-chart
            [series]="incomeSeries()"
            [chart]="incomeChart"
            [colors]="['#8965e5']"
            [stroke]="incomeStroke"
            [fill]="incomeFill"
            [dataLabels]="noDataLabels"
            [legend]="noLegend"
            [grid]="noGrid"
            [xaxis]="hiddenXAxis"
            [yaxis]="hiddenYAxis"
            [tooltip]="tooltip()"
          />
        </app-stat-card>
      </div>

      <div class="mm-col-4">
        <app-stat-card
          title="Current Balance"
          [value]="balanceValue()"
          [trendBadge]="quarterMode() ? '+54% last quarter' : '+42% last month'"
          icon="banknote"
          tone="success"
          (menuAction)="onCardAction('Current Balance', $event)"
        >
          <apx-chart
            [series]="balanceSeries()"
            [chart]="balanceChart"
            [colors]="balanceColors()"
            [plotOptions]="balancePlot"
            [stroke]="noStroke"
            [dataLabels]="noDataLabels"
            [legend]="noLegend"
            [labels]="['Reserved', 'Available']"
            [tooltip]="tooltip()"
          />
        </app-stat-card>
      </div>
    </div>
  `,
})
export class KpiTrioRowComponent {
  private readonly layout = inject(LayoutService);
  private readonly notifications = inject(NotificationService);

  protected readonly quarterMode = signal(false);

  protected readonly followersValue = computed(() => (this.quarterMode() ? '13,480' : '4,562'));
  protected readonly incomeValue = computed(() => (this.quarterMode() ? '$18,940' : '$6,280'));
  protected readonly balanceValue = computed(() => (this.quarterMode() ? '$7,810' : '$2,529'));

  protected readonly noDataLabels: ApexDataLabels = { enabled: false };
  protected readonly noLegend: ApexLegend = { show: false };
  protected readonly noStroke: ApexStroke = { show: false, width: 0 };
  protected readonly noGrid: ApexGrid = {
    show: false,
    padding: { left: 0, right: 0, top: 0, bottom: 0 },
  };
  protected readonly hiddenXAxis: ApexXAxis = {
    labels: { show: false },
    axisBorder: { show: false },
    axisTicks: { show: false },
  };
  protected readonly hiddenYAxis: ApexYAxis = { show: false };

  protected readonly tooltip = computed<ApexTooltip>(() => ({
    theme: this.layout.isDark() ? 'dark' : 'light',
  }));

  // 1. Total Followers stacked pill bar sparkline
  protected readonly followersChart: ApexChart = {
    type: 'bar',
    height: 78,
    width: 145,
    stacked: true,
    stackType: '100%',
    sparkline: { enabled: true },
    toolbar: { show: false },
  };

  protected readonly followersPlot: ApexPlotOptions = {
    bar: {
      columnWidth: '34%',
      borderRadius: 4,
      borderRadiusApplication: 'around',
    },
  };

  protected readonly followersColors = computed<string[]>(() => [
    '#ff6692',
    this.layout.isDark() ? 'rgba(255, 102, 146, 0.22)' : '#e7d0d9',
  ]);

  protected readonly followersSeries = computed<ApexAxisChartSeries>(() => [
    { name: 'Active', data: this.quarterMode() ? [42, 68, 52, 74, 82] : [28, 52, 34, 46, 62] },
    { name: 'Target', data: this.quarterMode() ? [58, 32, 48, 26, 18] : [72, 48, 66, 54, 38] },
  ]);

  // 2. Total Income area sparkline
  protected readonly incomeChart: ApexChart = {
    type: 'area',
    height: 76,
    width: 148,
    sparkline: { enabled: true },
    toolbar: { show: false },
  };

  protected readonly incomeStroke: ApexStroke = {
    curve: 'smooth',
    width: 2,
  };

  protected readonly incomeFill: ApexFill = {
    type: 'gradient',
    gradient: {
      shadeIntensity: 0.4,
      opacityFrom: 0.25,
      opacityTo: 0.02,
      stops: [0, 100],
    },
  };

  protected readonly incomeSeries = computed<ApexAxisChartSeries>(() => [
    {
      name: 'Income',
      data: this.quarterMode() ? [28, 62, 48, 70, 52, 84] : [18, 56, 36, 48, 28, 62, 22],
    },
  ]);

  // 3. Current Balance donut ring
  protected readonly balanceChart: ApexChart = {
    type: 'donut',
    height: 112,
    width: 130,
    sparkline: { enabled: true },
  };

  protected readonly balancePlot: ApexPlotOptions = {
    pie: {
      donut: {
        size: '78%',
      },
    },
  };

  protected readonly balanceColors = computed<string[]>(() => [
    this.layout.isDark() ? 'rgba(0, 206, 182, 0.24)' : '#b2efe8',
    '#00ceb6',
  ]);

  protected readonly balanceSeries = computed<ApexNonAxisChartSeries>(() =>
    this.quarterMode() ? [32, 68] : [45, 55]
  );

  protected onCardAction(cardName: string, actionId: string): void {
    if (actionId === 'quarter') {
      this.quarterMode.update((q) => !q);
      this.notifications.notify(
        `${cardName}: switched to ${this.quarterMode() ? 'Last Quarter' : 'Last Month'}`,
        'primary'
      );
      return;
    }
    this.notifications.notify(`${cardName}: ${actionId} completed`, 'success');
  }
}
