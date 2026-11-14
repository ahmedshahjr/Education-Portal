import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

export type BadgeTone = 'primary' | 'success' | 'warning' | 'error';

/** A small soft-coloured label, taken from the theme's status badge (Theme Angular, ENG-UI-17). */
@Component({
  selector: 'app-status-badge',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `<span class="mm-badge" [class]="toneClass()"><ng-content /></span>`,
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

    .mm-badge--soft-primary {
      background-color: var(--mm-light-primary);
      color: var(--mm-primary);
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
  `,
})
export class StatusBadgeComponent {
  readonly tone = input<BadgeTone>('primary');

  protected readonly toneClass = computed(() => `mm-badge--soft-${this.tone()}`);
}
