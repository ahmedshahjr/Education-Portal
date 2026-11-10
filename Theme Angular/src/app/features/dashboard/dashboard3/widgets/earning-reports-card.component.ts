import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router, RouterLink } from '@angular/router';
import { DashboardDataService } from '../../../../core/services/dashboard-data.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { AppIconComponent } from '../../../../shared/components/app-icon/app-icon.component';
import { ChartCardComponent } from '../../../../shared/components/chart-card/chart-card.component';
import { StatusBadgeComponent } from '../../../../shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-earning-reports-card',
  standalone: true,
  imports: [RouterLink, MatTooltipModule, ChartCardComponent, AppIconComponent, StatusBadgeComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-chart-card title="Earning Reports" (menuAction)="onMenuAction($event)">
      <div class="mm-earn-list">
        @for (item of reports; track item.id) {
          <div class="mm-earn-row">
            <div class="mm-earn-row__left">
              <span class="mm-icon-pill mm-earn-icon" [class]="'mm-icon-pill--soft-' + item.tone">
                <app-icon [name]="item.icon" [size]="18" />
              </span>
              <div class="mm-earn-meta">
                <span class="mm-earn-title">{{ item.title }}</span>
                <span class="mm-earn-sub">
                  <span>{{ item.subtitle }}</span>
                  <button
                    type="button"
                    class="mm-earn-info-btn"
                    [matTooltip]="item.details + ' (' + item.revenue + ')'"
                    [attr.aria-label]="'More info about ' + item.title"
                  >
                    <app-icon name="info" [size]="13" />
                  </button>
                </span>
              </div>
            </div>

            <app-status-badge
              tone="success"
              variant="soft"
              prefixIcon="up"
              [label]="item.growthPercent"
            />
          </div>
        }
      </div>

      <div class="mm-earn-footer">
        <a routerLink="/pages/reports" class="mm-earn-footer__link">View more markets</a>
      </div>
    </app-chart-card>
  `,
  styles: `
    .mm-earn-list {
      display: flex;
      flex-direction: column;
      flex: 1;
    }

    .mm-earn-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
      padding: 18px 0;

      & + & {
        border-top: 1px solid var(--mm-border-color);
      }
    }

    .mm-earn-row__left {
      display: flex;
      align-items: center;
      gap: 14px;
      min-width: 0;
    }

    .mm-earn-icon {
      padding: 10px 15px;
      border-radius: var(--mm-radius-pill);
    }

    .mm-earn-meta {
      display: flex;
      flex-direction: column;
      gap: 3px;
      min-width: 0;
    }

    .mm-earn-title {
      font-size: var(--mm-text-sm);
      font-weight: 500;
      color: var(--mm-text-primary);
    }

    .mm-earn-sub {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      font-size: var(--mm-text-xs);
      color: var(--mm-text-muted);
    }

    .mm-earn-info-btn {
      appearance: none;
      border: 0;
      background: transparent;
      padding: 0;
      color: var(--mm-text-muted);
      display: inline-flex;
      align-items: center;
      cursor: help;
    }

    .mm-earn-footer {
      margin-top: 8px;
      padding-top: 16px;
      border-top: 1px solid var(--mm-border-color);
      text-align: center;
    }

    .mm-earn-footer__link {
      font-size: var(--mm-text-sm);
      font-weight: 500;
      color: var(--mm-text-primary);
      transition: color var(--mm-duration) var(--mm-ease);

      &:hover {
        color: var(--mm-primary);
      }
    }
  `,
})
export class EarningReportsCardComponent {
  private readonly dataService = inject(DashboardDataService);
  private readonly notifications = inject(NotificationService);
  private readonly router = inject(Router);

  protected readonly reports = this.dataService.earningReports;

  protected onMenuAction(action: string): void {
    if (action === 'download') {
      void this.router.navigateByUrl('/pages/reports');
      return;
    }
    this.notifications.notify('Earning reports refreshed', 'success');
  }
}
