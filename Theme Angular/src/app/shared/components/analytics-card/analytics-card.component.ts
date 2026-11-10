import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { SemanticTone } from '../../models/dashboard.models';
import { AppIconName } from '../../models/navigation.models';
import { AppIconComponent } from '../app-icon/app-icon.component';

@Component({
  selector: 'app-analytics-card',
  standalone: true,
  imports: [AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-mini-tile">
      <span class="mm-icon-pill mm-mini-tile__icon" [class]="'mm-icon-pill--soft-' + tone()">
        <app-icon [name]="icon()" [size]="18" />
      </span>
      <div class="mm-mini-tile__meta">
        <span class="mm-mini-tile__label">{{ label() }}</span>
        <span class="mm-mini-tile__value">{{ value() }}</span>
      </div>
    </div>
  `,
  styles: `
    .mm-mini-tile {
      display: flex;
      align-items: center;
      gap: 14px;
      padding: 14px 16px;
      border-radius: var(--mm-radius-md);
      background-color: var(--mm-bg-subtle);
      transition: transform var(--mm-duration) var(--mm-ease);

      &:hover {
        transform: translateY(-1px);
      }
    }

    .mm-mini-tile__icon {
      padding: 10px 14px;
      border-radius: var(--mm-radius-pill);
    }

    .mm-mini-tile__meta {
      display: flex;
      flex-direction: column;
      gap: 2px;
      min-width: 0;
    }

    .mm-mini-tile__label {
      font-size: var(--mm-text-xs);
      font-weight: 500;
      color: var(--mm-text-muted);
    }

    .mm-mini-tile__value {
      font-size: var(--mm-text-sm);
      font-weight: 600;
      color: var(--mm-text-primary);
    }
  `,
})
export class AnalyticsCardComponent {
  readonly label = input.required<string>();
  readonly value = input.required<string>();
  readonly icon = input.required<AppIconName>();
  readonly tone = input<SemanticTone>('primary');
}
