import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'app-loading-state',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-loading" aria-busy="true" aria-label="Loading content">
      @for (row of rowsArray(); track $index) {
        <div class="mm-loading__bar" [style.width]="$index % 2 === 0 ? '100%' : '76%'"></div>
      }
    </div>
  `,
  styles: `
    .mm-loading {
      display: flex;
      flex-direction: column;
      gap: 12px;
      padding: 16px 0;
    }

    .mm-loading__bar {
      height: 16px;
      border-radius: var(--mm-radius-sm);
      background: linear-gradient(
        90deg,
        var(--mm-bg-subtle) 25%,
        var(--mm-border-color) 50%,
        var(--mm-bg-subtle) 75%
      );
      background-size: 200% 100%;
      animation: mm-shimmer 1.4s infinite ease-in-out;
    }

    @keyframes mm-shimmer {
      0% {
        background-position: 200% 0;
      }
      100% {
        background-position: -200% 0;
      }
    }
  `,
})
export class LoadingStateComponent {
  readonly rows = input<number>(4);

  protected rowsArray(): number[] {
    return Array.from({ length: this.rows() }, (_, idx) => idx);
  }
}
