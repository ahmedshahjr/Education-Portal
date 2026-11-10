import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppIconComponent } from '../app-icon/app-icon.component';

@Component({
  selector: 'app-search-input',
  standalone: true,
  imports: [FormsModule, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-search">
      <app-icon name="search" [size]="16" iconClass="mm-search__icon" />
      <input
        type="search"
        class="mm-search__input"
        [placeholder]="placeholder()"
        [attr.aria-label]="ariaLabel()"
        [ngModel]="value()"
        (ngModelChange)="value.set($event)"
      />
      @if (value()) {
        <button
          type="button"
          class="mm-search__clear"
          aria-label="Clear search"
          (click)="value.set('')"
        >
          <app-icon name="x" [size]="14" />
        </button>
      }
    </div>
  `,
  styles: `
    .mm-search {
      position: relative;
      display: flex;
      align-items: center;
      width: 100%;
      min-width: 220px;
    }

    :host ::ng-deep .mm-search__icon {
      position: absolute;
      left: 12px;
      color: var(--mm-text-muted);
      pointer-events: none;
    }

    .mm-search__input {
      width: 100%;
      height: 40px;
      padding: 0 36px;
      border-radius: var(--mm-radius-md);
      border: 1px solid var(--mm-border-color);
      background-color: var(--mm-bg-subtle);
      color: var(--mm-text-primary);
      font-size: var(--mm-text-sm);
      font-family: inherit;
      transition: border-color var(--mm-duration) var(--mm-ease),
                  background-color var(--mm-duration) var(--mm-ease);

      &::placeholder {
        color: var(--mm-text-muted);
      }

      &:focus {
        outline: none;
        border-color: var(--mm-primary);
        background-color: var(--mm-bg-surface);
      }
    }

    .mm-search__clear {
      position: absolute;
      right: 8px;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      width: 24px;
      height: 24px;
      border: 0;
      border-radius: var(--mm-radius-pill);
      background: transparent;
      color: var(--mm-text-muted);
      cursor: pointer;

      &:hover {
        background-color: var(--mm-bg-hover);
        color: var(--mm-text-primary);
      }
    }
  `,
})
export class SearchInputComponent {
  readonly value = model<string>('');
  readonly placeholder = input<string>('Search...');
  readonly ariaLabel = input<string>('Search');
}
