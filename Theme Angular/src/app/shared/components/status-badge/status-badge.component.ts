import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { SemanticTone } from '../../models/dashboard.models';

export type BadgeVariant = 'soft' | 'outlined' | 'solid';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <span class="mm-badge" [class]="badgeClasses()">
      @if (prefixIcon() === 'down') {
        <span class="mm-badge__arrow" aria-hidden="true">&#9662;</span>
      } @else if (prefixIcon() === 'up') {
        <span class="mm-badge__arrow" aria-hidden="true">&#9652;</span>
      }
      <ng-content />
      @if (label()) {
        <span>{{ label() }}</span>
      }
    </span>
  `,
  styles: `
    .mm-badge {
      display: inline-flex;
      align-items: center;
      gap: 4px;
      padding: 4px 10px;
      border-radius: var(--mm-radius-pill);
      font-size: var(--mm-text-xs);
      font-weight: 600;
      line-height: 1.25;
      white-space: nowrap;
    }

    .mm-badge__arrow {
      font-size: 10px;
      line-height: 1;
    }

    .mm-badge--soft-primary {
      background-color: var(--mm-light-primary);
      color: var(--mm-primary);
    }

    .mm-badge--soft-secondary {
      background-color: var(--mm-light-secondary);
      color: var(--mm-secondary);
    }

    .mm-badge--soft-success {
      background-color: var(--mm-light-success);
      color: var(--mm-success);
    }

    .mm-badge--soft-warning {
      background-color: var(--mm-light-warning);
      color: var(--mm-warning);
    }

    .mm-badge--soft-error {
      background-color: var(--mm-light-error);
      color: var(--mm-error);
    }

    .mm-badge--soft-info {
      background-color: var(--mm-light-info);
      color: var(--mm-info);
    }

    .mm-badge--outlined {
      background-color: transparent;
      color: var(--mm-text-secondary);
      border: 1px solid var(--mm-border-pill);
    }

    .mm-badge--solid-primary {
      background-color: var(--mm-primary);
      color: var(--mm-text-inverse);
    }

    .mm-badge--solid-success {
      background-color: var(--mm-success);
      color: var(--mm-text-inverse);
    }

    .mm-badge--solid-warning {
      background-color: var(--mm-warning);
      color: var(--mm-text-inverse);
    }

    .mm-badge--solid-error {
      background-color: var(--mm-error);
      color: var(--mm-text-inverse);
    }
  `,
})
export class StatusBadgeComponent {
  readonly label = input<string>('');
  readonly tone = input<SemanticTone>('primary');
  readonly variant = input<BadgeVariant>('soft');
  readonly prefixIcon = input<'none' | 'up' | 'down'>('none');

  protected readonly badgeClasses = computed(() => {
    if (this.variant() === 'outlined') {
      return 'mm-badge--outlined';
    }
    return `mm-badge--${this.variant()}-${this.tone()}`;
  });
}
