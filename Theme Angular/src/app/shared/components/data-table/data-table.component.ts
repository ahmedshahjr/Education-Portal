import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  input,
  model,
  output,
  viewChild,
} from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { EmptyStateComponent } from '../empty-state/empty-state.component';
import { LoadingStateComponent } from '../loading-state/loading-state.component';
import { SearchInputComponent } from '../search-input/search-input.component';

export interface DataTableColumn<T> {
  readonly key: string;
  readonly header: string;
  readonly align?: 'left' | 'right' | 'center';
  readonly sortable?: boolean;
  readonly valueFn?: (row: T) => string | number;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    SearchInputComponent,
    EmptyStateComponent,
    LoadingStateComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-table-wrap">
      @if (searchable()) {
        <div class="mm-table-toolbar">
          <div class="mm-table-toolbar__search">
            <app-search-input
              [value]="searchTerm()"
              (valueChange)="searchTerm.set($event)"
              [placeholder]="searchPlaceholder()"
            />
          </div>
          <div class="mm-table-toolbar__extra">
            <ng-content select="[table-actions]" />
          </div>
        </div>
      }

      @if (loading()) {
        <app-loading-state [rows]="5" />
      } @else if (dataSource.filteredData.length === 0) {
        <app-empty-state
          [title]="emptyTitle()"
          [description]="emptyDescription()"
          [actionLabel]="searchTerm() ? 'Clear Search' : ''"
          (action)="searchTerm.set('')"
        />
      } @else {
        <div class="mm-table-scroll">
          <table mat-table [dataSource]="dataSource" matSort class="mm-data-table">
            @for (col of columns(); track col.key) {
              <ng-container [matColumnDef]="col.key">
                <th
                  mat-header-cell
                  *matHeaderCellDef
                  [mat-sort-header]="col.sortable !== false ? col.key : ''"
                  [disabled]="col.sortable === false"
                  [class.text-right]="col.align === 'right'"
                >
                  {{ col.header }}
                </th>
                <td
                  mat-cell
                  *matCellDef="let row"
                  [class.text-right]="col.align === 'right'"
                  (click)="rowClick.emit(row)"
                >
                  {{ col.valueFn ? col.valueFn(row) : row[col.key] }}
                </td>
              </ng-container>
            }

            <tr mat-header-row *matHeaderRowDef="displayedColumnKeys()"></tr>
            <tr
              mat-row
              *matRowDef="let row; columns: displayedColumnKeys()"
              class="mm-data-table__row"
            ></tr>
          </table>
        </div>

        @if (paginated()) {
          <mat-paginator [pageSize]="pageSize()" [pageSizeOptions]="[5, 10, 25]" showFirstLastButtons />
        }
      }
    </div>
  `,
  styles: `
    .mm-table-wrap {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .mm-table-toolbar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      flex-wrap: wrap;
      gap: 12px;
    }

    .mm-table-toolbar__search {
      width: 280px;
      max-width: 100%;
    }

    .mm-table-toolbar__extra {
      display: flex;
      align-items: center;
      gap: 10px;
      flex-wrap: wrap;
    }

    .mm-table-scroll {
      width: 100%;
      overflow-x: auto;
    }

    .mm-data-table {
      width: 100%;
      background: transparent;

      th.mat-mdc-header-cell {
        font-size: var(--mm-text-sm);
        font-weight: 600;
        color: var(--mm-text-primary);
        border-bottom-color: var(--mm-border-color);
      }

      td.mat-mdc-cell {
        font-size: var(--mm-text-sm);
        color: var(--mm-text-body);
        border-bottom-color: var(--mm-border-color);
        padding-top: 14px;
        padding-bottom: 14px;
      }

      .text-right {
        text-align: right;
      }
    }

    .mm-data-table__row {
      cursor: pointer;
      transition: background-color var(--mm-duration) var(--mm-ease);

      &:hover {
        background-color: var(--mm-bg-hover);
      }
    }
  `,
})
export class DataTableComponent<T extends Record<string, any>> implements AfterViewInit {
  readonly data = input.required<readonly T[]>();
  readonly columns = input.required<readonly DataTableColumn<T>[]>();
  readonly searchable = input<boolean>(true);
  readonly searchPlaceholder = input<string>('Search records...');
  readonly paginated = input<boolean>(true);
  readonly pageSize = input<number>(5);
  readonly loading = input<boolean>(false);
  readonly emptyTitle = input<string>('No matching rows');
  readonly emptyDescription = input<string>('Adjust your search or filter to see results.');

  readonly searchTerm = model<string>('');
  readonly rowClick = output<T>();

  protected readonly paginator = viewChild(MatPaginator);
  protected readonly sort = viewChild(MatSort);

  protected readonly dataSource = new MatTableDataSource<T>([]);
  protected readonly displayedColumnKeys = computed(() => this.columns().map((c) => c.key));

  constructor() {
    effect(() => {
      this.dataSource.data = [...this.data()];
    });

    effect(() => {
      this.dataSource.filter = this.searchTerm().trim().toLowerCase();
      this.paginator()?.firstPage();
    });
  }

  ngAfterViewInit(): void {
    const p = this.paginator();
    const s = this.sort();
    if (p) this.dataSource.paginator = p;
    if (s) this.dataSource.sort = s;
  }
}
