import { ChangeDetectionStrategy, Component, input, model } from '@angular/core';

export interface FilterOption<T extends string = string> {
  readonly id: T;
  readonly label: string;
}

@Component({
  selector: 'app-filter-panel',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-pill-tabs" role="tablist" [attr.aria-label]="ariaLabel()">
      @for (option of options(); track option.id) {
        <button
          type="button"
          role="tab"
          class="mm-pill-tabs__btn"
          [class.is-active]="selected() === option.id"
          [attr.aria-selected]="selected() === option.id"
          (click)="selected.set(option.id)"
        >
          {{ option.label }}
        </button>
      }
    </div>
  `,
})
export class FilterPanelComponent<T extends string = string> {
  readonly options = input.required<readonly FilterOption<T>[]>();
  readonly selected = model.required<T>();
  readonly ariaLabel = input<string>('Filter options');
}
