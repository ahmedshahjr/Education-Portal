import { Injectable } from '@angular/core';
import { MatPaginatorIntl } from '@angular/material/paginator';

/** Angular Material's paginator text in the page's language. */
@Injectable()
export class LocalizedPaginatorIntl extends MatPaginatorIntl {
  override itemsPerPageLabel = $localize`:@@paginator.itemsPerPage:Items per page:`;
  override nextPageLabel = $localize`:@@paginator.next:Next page`;
  override previousPageLabel = $localize`:@@paginator.previous:Previous page`;
  override firstPageLabel = $localize`:@@paginator.first:First page`;
  override lastPageLabel = $localize`:@@paginator.last:Last page`;

  override getRangeLabel = (page: number, pageSize: number, length: number): string => {
    if (length === 0 || pageSize === 0) {
      return $localize`:@@paginator.rangeEmpty:0 of ${length}:LENGTH:`;
    }

    const start = page * pageSize + 1;
    const end = Math.min(start + pageSize - 1, length);
    return $localize`:@@paginator.range:${start}:START: – ${end}:END: of ${length}:LENGTH:`;
  };
}
