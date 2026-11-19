import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';

/** The campus shape the filter needs. */
export interface CampusOption {
  readonly id: string;
  readonly name: string;
}

/**
 * A dropdown above a list that filters it by campus. It is shown only when the school has more than one campus (AC4.1).
 * The selected value is a campus id, or `null` for "All campuses" (AC4.2).
 */
@Component({
  selector: 'app-campus-filter',
  imports: [MatFormFieldModule, MatSelectModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './campus-filter.component.html',
  styleUrl: './campus-filter.component.css',
})
export class CampusFilterComponent {
  readonly campuses = input.required<readonly CampusOption[]>();

  /** The campus id the list is filtered to, or `null` for all campuses. */
  readonly selected = output<string | null>();

  protected readonly visible = computed(() => this.campuses().length > 1);

  protected choose(value: string | null): void {
    this.selected.emit(value);
  }
}
