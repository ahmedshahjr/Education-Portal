import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatMenuModule } from '@angular/material/menu';
import { SemanticTone } from '../../models/dashboard.models';
import { AppIconName } from '../../models/navigation.models';
import { AppIconComponent } from '../app-icon/app-icon.component';
import { StatusBadgeComponent } from '../status-badge/status-badge.component';

export interface CardMenuAction {
  readonly id: string;
  readonly label: string;
}

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [MatCardModule, MatButtonModule, MatMenuModule, AppIconComponent, StatusBadgeComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-card
      class="mm-stat-card"
      [class.mm-elevated]="!tinted()"
      [class.mm-card--tint-primary]="tinted() && tone() === 'primary'"
      [class.mm-card--tint-secondary]="tinted() && tone() === 'secondary'"
      [class.mm-card--tint-success]="tinted() && tone() === 'success'"
      [class.mm-card--tint-error]="tinted() && tone() === 'error'"
      [class.mm-card--tint-warning]="tinted() && tone() === 'warning'"
    >
      <mat-card-content class="mm-stat-card__content">
        <div class="mm-stat-card__header">
          <div class="mm-stat-card__identity">
            <span class="mm-icon-pill mm-stat-card__icon" [class]="'mm-icon-pill--' + tone()">
              <app-icon [name]="icon()" [size]="20" />
            </span>
            <span class="mm-stat-card__title">{{ title() }}</span>
          </div>

          <button
            mat-icon-button
            type="button"
            class="mm-stat-card__menu-btn"
            [matMenuTriggerFor]="cardMenu"
            [attr.aria-label]="title() + ' options'"
          >
            <app-icon name="more-vertical" [size]="18" />
          </button>

          <mat-menu #cardMenu="matMenu" xPosition="before">
            @for (item of menuItems(); track item.id) {
              <button mat-menu-item type="button" (click)="menuAction.emit(item.id)">
                {{ item.label }}
              </button>
            }
          </mat-menu>
        </div>

        <div class="mm-stat-card__body">
          <div class="mm-stat-card__metric-col">
            <h2 class="mm-stat-card__value">{{ value() }}</h2>
            @if (trendBadge()) {
              <app-status-badge variant="outlined" [label]="trendBadge()" />
            }
          </div>
          <div class="mm-stat-card__chart-col">
            <ng-content />
          </div>
        </div>
      </mat-card-content>
    </mat-card>
  `,
  styles: `
    .mm-stat-card {
      height: 100%;
      box-shadow: none !important;

      &.mm-elevated {
        box-shadow: var(--mm-shadow-card) !important;
      }
    }

    .mm-stat-card__content {
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      height: 100%;
      padding: 24px !important;
    }

    .mm-stat-card__header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 8px;
      margin-bottom: 14px;
    }

    .mm-stat-card__identity {
      display: flex;
      align-items: center;
      gap: 12px;
      min-width: 0;
    }

    .mm-stat-card__icon {
      padding: 8px 14px;
      border-radius: var(--mm-radius-pill);
    }

    .mm-stat-card__title {
      font-size: var(--mm-text-base);
      font-weight: 500;
      color: var(--mm-text-secondary);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .mm-stat-card__menu-btn {
      color: var(--mm-text-primary);
      flex-shrink: 0;
    }

    .mm-stat-card__body {
      display: grid;
      grid-template-columns: minmax(0, 1fr) minmax(0, 1fr);
      align-items: flex-end;
      gap: 12px;
    }

    .mm-stat-card__metric-col {
      display: flex;
      flex-direction: column;
      align-items: flex-start;
      gap: 10px;
    }

    .mm-stat-card__value {
      margin: 0;
      font-size: var(--mm-text-3xl);
      font-weight: 500;
      line-height: 1.15;
      letter-spacing: -0.02em;
      color: var(--mm-text-primary);
    }

    .mm-stat-card__chart-col {
      display: flex;
      align-items: flex-end;
      justify-content: flex-end;
      min-height: 82px;
    }
  `,
})
export class StatCardComponent {
  readonly title = input.required<string>();
  readonly value = input.required<string>();
  readonly trendBadge = input<string>('');
  readonly icon = input.required<AppIconName>();
  readonly tone = input<SemanticTone>('primary');
  readonly tinted = input<boolean>(true);
  readonly menuItems = input<readonly CardMenuAction[]>([
    { id: 'refresh', label: 'Refresh Metric' },
    { id: 'quarter', label: 'Compare Last Quarter' },
    { id: 'export', label: 'Export CSV' },
  ]);

  readonly menuAction = output<string>();
}
