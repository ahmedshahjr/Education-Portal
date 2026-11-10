import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import {
  ApexChart,
  ApexDataLabels,
  ApexLegend,
  ApexNonAxisChartSeries,
  ApexPlotOptions,
  ApexStroke,
  ApexTooltip,
  ChartComponent,
} from 'ng-apexcharts';
import { LayoutService } from '../../../../core/layout/layout.service';
import { DashboardDataService } from '../../../../core/services/dashboard-data.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AppIconComponent } from '../../../../shared/components/app-icon/app-icon.component';
import { ChartCardComponent } from '../../../../shared/components/chart-card/chart-card.component';

@Component({
  selector: 'app-marketing-report-card',
  standalone: true,
  imports: [ChartComponent, ChartCardComponent, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-chart-card
      title="Marketing Report"
      (menuAction)="onMenuAction($event)"
    >
      <div class="mm-mkt-grid">
        <ul class="mm-mkt-list">
          @for (item of report.metrics; track item.id) {
            <li class="mm-mkt-item">
              <span class="mm-icon-pill mm-icon-pill--surface mm-mkt-icon">
                <app-icon [name]="item.icon" [size]="18" />
              </span>
              <div class="mm-mkt-meta">
                <span class="mm-mkt-label">{{ item.label }}</span>
                <span class="mm-mkt-value">{{ item.value }}</span>
              </div>
            </li>
          }
        </ul>

        <div class="mm-mkt-gauge-col">
          <div class="mm-mkt-gauge-wrap">
            <apx-chart
              [series]="gaugeSeries"
              [chart]="gaugeChart"
              [plotOptions]="gaugePlot()"
              [colors]="gaugeColors"
              [stroke]="gaugeStroke()"
              [dataLabels]="noDataLabels"
              [legend]="noLegend"
              [labels]="gaugeLabels"
              [tooltip]="tooltip()"
            />
          </div>
          <div class="mm-mkt-score-block">
            <h2 class="mm-mkt-score">{{ report.score }}</h2>
            <p class="mm-mkt-caption">{{ report.caption }}</p>
          </div>
        </div>
      </div>

      <div class="mm-mkt-banner">
        <app-icon name="send" [size]="20" iconClass="mm-mkt-banner__send" />
        <p class="mm-mkt-banner__text">{{ report.ctaText }}</p>
        <button
          type="button"
          class="mm-mkt-banner__play"
          aria-label="Open marketing report details"
          (click)="openReportPage()"
        >
          <app-icon name="play" [size]="16" />
        </button>
      </div>
    </app-chart-card>
  `,
  styles: `
    .mm-mkt-grid {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 20px;
      align-items: center;
      padding-bottom: 20px;
    }

    .mm-mkt-list {
      list-style: none;
      margin: 0;
      padding: 0;
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .mm-mkt-item {
      display: flex;
      align-items: center;
      gap: 14px;
    }

    .mm-mkt-icon {
      padding: 10px 14px;
      color: var(--mm-text-muted);
    }

    .mm-mkt-meta {
      display: flex;
      flex-direction: column;
      gap: 2px;
    }

    .mm-mkt-label {
      font-size: var(--mm-text-sm);
      font-weight: 500;
      color: var(--mm-text-muted);
    }

    .mm-mkt-value {
      font-size: var(--mm-text-xl);
      font-weight: 500;
      color: var(--mm-text-primary);
      line-height: 1.25;
    }

    .mm-mkt-gauge-col {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
    }

    .mm-mkt-gauge-wrap {
      width: 100%;
      max-width: 230px;
      min-height: 130px;
      overflow: hidden;
    }

    .mm-mkt-score-block {
      margin-top: -22px;
    }

    .mm-mkt-score {
      margin: 0 0 6px;
      font-size: var(--mm-text-3xl);
      font-weight: 500;
      color: var(--mm-text-primary);
    }

    .mm-mkt-caption {
      margin: 0;
      font-size: var(--mm-text-xs);
      color: var(--mm-text-muted);
      line-height: 1.45;
      max-width: 195px;
    }

    .mm-mkt-banner {
      margin-top: auto;
      display: flex;
      align-items: center;
      gap: 14px;
      padding: 14px 18px;
      border-radius: var(--mm-radius-lg);
      background-color: var(--mm-bg-subtle);
    }

    :host ::ng-deep .mm-mkt-banner__send {
      color: var(--mm-text-primary);
    }

    .mm-mkt-banner__text {
      flex: 1;
      margin: 0;
      font-size: var(--mm-text-xs);
      color: var(--mm-text-muted);
      line-height: 1.45;
    }

    .mm-mkt-banner__play {
      appearance: none;
      border: 0;
      width: 38px;
      height: 38px;
      border-radius: var(--mm-radius-pill);
      background-color: var(--mm-primary);
      color: var(--mm-text-inverse);
      display: inline-flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
      cursor: pointer;
      transition: background-color var(--mm-duration) var(--mm-ease),
                  transform var(--mm-duration) var(--mm-ease);

      &:hover {
        background-color: var(--mm-primary-hover);
        transform: scale(1.04);
      }
    }

    @media (max-width: 560px) {
      .mm-mkt-grid {
        grid-template-columns: 1fr;
      }
    }
  `,
})
export class MarketingReportCardComponent {
  private readonly dataService = inject(DashboardDataService);
  private readonly layout = inject(LayoutService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly report = this.dataService.marketingReportData;
  protected readonly gaugeSeries: ApexNonAxisChartSeries = [...this.report.gaugeSeries];
  protected readonly gaugeLabels = ['Bounce', 'CTR', 'Warm', 'Engaged', 'Converted'];
  protected readonly gaugeColors = ['#ff6692', '#ffae1f', '#fff1cc', '#d2f9f4', '#00ceb6'];
  protected readonly noDataLabels: ApexDataLabels = { enabled: false };
  protected readonly noLegend: ApexLegend = { show: false };

  protected readonly gaugeChart: ApexChart = {
    type: 'donut',
    height: 205,
    sparkline: { enabled: true },
  };

  protected readonly gaugePlot = computed<ApexPlotOptions>(() => ({
    pie: {
      startAngle: -90,
      endAngle: 90,
      offsetY: 16,
      donut: {
        size: '80%',
      },
    },
  }));

  protected readonly gaugeStroke = computed<ApexStroke>(() => ({
    width: 2,
    colors: [this.layout.isDark() ? '#1a2537' : '#ffffff'],
  }));

  protected readonly tooltip = computed<ApexTooltip>(() => ({
    theme: this.layout.isDark() ? 'dark' : 'light',
  }));

  protected openReportPage(): void {
    void this.router.navigateByUrl('/pages/reports');
  }

  protected onMenuAction(action: string): void {
    if (action === 'download') {
      this.notifications.notify('Marketing report exported as CSV', 'success');
      return;
    }
    this.notifications.notify('Marketing metrics synchronized', 'primary');
  }
}
