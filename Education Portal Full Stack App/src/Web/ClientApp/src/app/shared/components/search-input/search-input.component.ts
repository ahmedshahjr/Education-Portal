import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';
import { AppIconComponent } from '../app-icon/app-icon.component';

/**
 * A search box with a clear button, from the project theme (`Theme Angular/src/app/shared/components/search-input`).
 * Adapted to the engineering rules: no `::ng-deep` (the icon sits in a positioned wrapper) and localised labels.
 */
@Component({
  selector: 'app-search-input',
  imports: [AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-search">
      <span class="mm-search__icon" aria-hidden="true"><app-icon name="search" [size]="16" /></span>
      <input
        type="search"
        class="mm-search__input"
        [placeholder]="placeholder()"
        [attr.aria-label]="ariaLabel()"
        [value]="value()"
        (input)="value.set($any($event.target).value)"
      />
      @if (value()) {
        <button type="button" class="mm-search__clear" aria-label="Clear search" i18n-aria-label="@@search.clear" (click)="value.set('')">
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

    .mm-search__icon {
      position: absolute;
      inset-inline-start: 12px;
      display: inline-flex;
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
      transition:
        border-color var(--mm-duration) var(--mm-ease),
        background-color var(--mm-duration) var(--mm-ease);

      &::placeholder {
        color: var(--mm-text-muted);
      }

      &:focus-visible {
        outline: 2px solid var(--mm-primary);
        outline-offset: 1px;
        border-color: var(--mm-primary);
        background-color: var(--mm-bg-surface);
      }
    }

    .mm-search__clear {
      position: absolute;
      inset-inline-end: 8px;
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
  readonly placeholder = input<string>($localize`:@@search.placeholder:Search...`);
  readonly ariaLabel = input<string>($localize`:@@search.label:Search`);
}
