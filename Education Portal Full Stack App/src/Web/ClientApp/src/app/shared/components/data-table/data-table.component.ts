import { NgTemplateOutlet } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  input,
  model,
  output,
  TemplateRef,
  viewChild,
} from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { EmptyStateComponent } from '../empty-state/empty-state.component';
import { LoadingStateComponent } from '../loading-state/loading-state.component';
import { SearchInputComponent } from '../search-input/search-input.component';

/** The context a cell template receives: the row it is drawn for. */
export interface DataTableCellContext<T> {
  readonly $implicit: T;
}

/**
 * One column. A plain column shows `valueFn(row)` or `row[key]`. A column with `cell` draws that template for each row
 * instead, for avatars, badges and icon buttons. Search reads the plain text of every column, so give a templated column
 * a `valueFn` when its text should be searchable.
 */
export interface DataTableColumn<T> {
  readonly key: string;
  readonly header: string;
  readonly align?: 'left' | 'right' | 'center';
  readonly sortable?: boolean;
  readonly valueFn?: (row: T) => string | number | null | undefined;
  readonly cell?: TemplateRef<DataTableCellContext<T>>;
}

/**
 * The project's table, from the theme's data table (`Theme Angular/src/app/shared/components/data-table`): search, sortable
 * columns and pages. Every list of records uses it, so the lists look and behave the same. A row click reports the row.
 */
@Component({
  selector: 'app-data-table',
  imports: [NgTemplateOutlet, MatTableModule, MatPaginatorModule, MatSortModule, SearchInputComponent, EmptyStateComponent, LoadingStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './data-table.component.html',
  styleUrl: './data-table.component.css',
})
export class DataTableComponent<T extends object> {
  readonly data = input.required<readonly T[]>();
  readonly columns = input.required<readonly DataTableColumn<T>[]>();
  readonly searchable = input<boolean>(true);
  readonly searchPlaceholder = input<string>('Search records...');
  readonly paginated = input<boolean>(true);
  readonly pageSize = input<number>(10);
  readonly loading = input<boolean>(false);
  readonly emptyTitle = input<string>('No matching rows');
  readonly emptyDescription = input<string>('Adjust your search or filter to see results.');

  readonly searchTerm = model<string>('');
  readonly rowClick = output<T>();

  protected readonly paginator = viewChild(MatPaginator);
  protected readonly sort = viewChild(MatSort);

  protected readonly dataSource = new MatTableDataSource<T>([]);
  protected readonly displayedColumnKeys = computed(() => this.columns().map((c) => c.key));

  /** The rows the search lets through, in the order given. */
  protected readonly visibleRows = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    if (term === '') {
      return this.data();
    }
    return this.data().filter((row) => this.searchText(row).includes(term));
  });

  constructor() {
    effect(() => {
      this.dataSource.data = [...this.visibleRows()];
    });

    // The table and its paginator are drawn only once there are rows, so they are attached when they appear.
    effect(() => {
      this.dataSource.paginator = this.paginated() ? (this.paginator() ?? null) : null;
    });
    effect(() => {
      this.dataSource.sort = this.sort() ?? null;
    });

    this.dataSource.sortingDataAccessor = (row, id) => {
      const column = this.columns().find((c) => c.key === id);
      if (!column) {
        return '';
      }
      const value = column.valueFn ? column.valueFn(row) : (row as Record<string, unknown>)[id];
      return typeof value === 'number' ? value : String(value ?? '').toLowerCase();
    };
  }

  protected cellText(column: DataTableColumn<T>, row: T): string {
    const value = column.valueFn ? column.valueFn(row) : (row as Record<string, unknown>)[column.key];
    return value === null || value === undefined ? '' : String(value);
  }

  private searchText(row: T): string {
    return this.columns()
      .map((column) => this.cellText(column, row))
      .join(' ')
      .toLowerCase();
  }
}
