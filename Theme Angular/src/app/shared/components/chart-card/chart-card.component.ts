import { ChangeDetectionStrategy, Component, input, model, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatMenuModule } from '@angular/material/menu';
import { AppIconComponent } from '../app-icon/app-icon.component';
import { EmptyStateComponent } from '../empty-state/empty-state.component';
import { FilterOption, FilterPanelComponent } from '../filter-panel/filter-panel.component';
import { LoadingStateComponent } from '../loading-state/loading-state.component';
import { CardMenuAction } from '../stat-card/stat-card.component';

@Component({
  selector: 'app-chart-card',
  standalone: true,
  imports: [
    MatCardModule,
    MatButtonModule,
    MatMenuModule,
    AppIconComponent,
    FilterPanelComponent,
    LoadingStateComponent,
    EmptyStateComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-card
      class="mm-chart-card"
      [class.mm-elevated]="!tinted()"
      [class.mm-card--tint-primary]="tinted()"
    >
      <mat-card-content class="mm-chart-card__content">
        <div class="mm-chart-card__header">
          <div class="mm-chart-card__title-wrap">
            <h3 class="mm-chart-card__title">{{ title() }}</h3>
            @if (subtitle()) {
              <p class="mm-chart-card__subtitle">{{ subtitle() }}</p>
            }
          </div>

          <div class="mm-chart-card__controls">
            @if (tabs().length) {
              <app-filter-panel
                [options]="tabs()"
                [selected]="selectedTab()"
                (selectedChange)="selectedTab.set($event)"
                [ariaLabel]="title() + ' filter'"
              />
            }
            @if (showMenu()) {
              <button
                mat-icon-button
                type="button"
                [matMenuTriggerFor]="chartMenu"
                [attr.aria-label]="title() + ' options'"
              >
                <app-icon name="more-vertical" [size]="18" />
              </button>
              <mat-menu #chartMenu="matMenu" xPosition="before">
                @for (item of menuItems(); track item.id) {
                  <button mat-menu-item type="button" (click)="menuAction.emit(item.id)">
                    {{ item.label }}
                  </button>
                }
              </mat-menu>
            }
          </div>
        </div>

        @if (loading()) {
          <app-loading-state [rows]="5" />
        } @else if (empty()) {
          <app-empty-state [title]="emptyTitle()" [description]="emptyDescription()" />
        } @else {
          <ng-content />
        }
      </mat-card-content>
    </mat-card>
  `,
  styles: `
    .mm-chart-card {
      height: 100%;
      box-shadow: var(--mm-shadow-card);

      &.mm-card--tint-primary {
        box-shadow: none !important;
      }
    }

    .mm-chart-card__content {
      display: flex;
      flex-direction: column;
      height: 100%;
      padding: 24px !important;
    }

    .mm-chart-card__header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      flex-wrap: wrap;
      gap: 12px;
      margin-bottom: 18px;
    }

    .mm-chart-card__title {
      margin: 0;
      font-size: var(--mm-text-lg);
      font-weight: 600;
      color: var(--mm-text-primary);
    }

    .mm-chart-card__subtitle {
      margin: 4px 0 0;
      font-size: var(--mm-text-xs);
      color: var(--mm-text-muted);
    }

    .mm-chart-card__controls {
      display: flex;
      align-items: center;
      gap: 8px;
      flex-wrap: wrap;
    }
  `,
})
export class ChartCardComponent {
  readonly title = input.required<string>();
  readonly subtitle = input<string>('');
  readonly tinted = input<boolean>(false);
  readonly showMenu = input<boolean>(true);
  readonly loading = input<boolean>(false);
  readonly empty = input<boolean>(false);
  readonly emptyTitle = input<string>('No chart data');
  readonly emptyDescription = input<string>('There is no data available for this period.');
  readonly tabs = input<readonly FilterOption<string>[]>([]);
  readonly selectedTab = model<string>('');
  readonly menuItems = input<readonly CardMenuAction[]>([
    { id: 'refresh', label: 'Refresh Data' },
    { id: 'download', label: 'Export Report' },
  ]);

  readonly menuAction = output<string>();
}
