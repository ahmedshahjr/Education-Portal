import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { RouterLink } from '@angular/router';
import { Observable, of, switchMap } from 'rxjs';
import { map } from 'rxjs/operators';
import { problemMessages } from '../../core/http/problem-details';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { CreatePriceBookCommand, PriceBookDto, PriceBooksClient, UpdatePriceBookCommand } from '../../web-api-client';
import { PriceBookChanges, PriceBookDialogComponent, PriceBookDialogData } from './price-book-dialog.component';

/**
 * Master portal: one price book per country, in one currency, with the tax rate and the price of each plan and paid
 * module per billing cycle. Open a book to set its prices. Master tenant administrators only; the server enforces it.
 */
@Component({
  selector: 'app-price-books-page',
  imports: [
    RouterLink,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    AppIconComponent,
    PageHeaderComponent,
    LoadingStateComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './price-books.page.html',
  styleUrl: './price-books.page.css',
})
export class PriceBooksPage {
  private readonly booksClient = inject(PriceBooksClient);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly columns = ['country', 'currency', 'tax', 'status', 'prices', 'actions'];
  protected readonly books = signal<readonly PriceBookDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    this.booksClient.getPriceBooks().subscribe({
      next: (books) => this.books.set(books),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected add(): void {
    const data: PriceBookDialogData = { book: null };
    this.dialog
      .open<PriceBookDialogComponent, PriceBookDialogData, PriceBookChanges>(PriceBookDialogComponent, { data, autoFocus: 'first-tabbable' })
      .afterClosed()
      .subscribe((changes) => {
        if (!changes) {
          return;
        }

        // The tax rate and the switch are set on creation only when they differ from the defaults; the add command takes just the country and currency.
        const needsUpdate = changes.taxRate !== 0 || !changes.isActive;
        const request = this.booksClient.createPriceBook(new CreatePriceBookCommand({ countryCode: changes.countryCode, currencyCode: changes.currencyCode })).pipe(
          switchMap((id) =>
            needsUpdate
              ? this.booksClient
                  .updatePriceBook(id, new UpdatePriceBookCommand({ currencyCode: changes.currencyCode, isActive: changes.isActive, taxRate: changes.taxRate }))
                  .pipe(map(() => id))
              : of(id)
          )
        );
        this.run(request, $localize`:@@priceBooks.added:Price book added.`);
      });
  }

  private run(request: Observable<unknown>, done: string): void {
    this.busy.set(true);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.snackBar.open(done, undefined, { duration: 3000 });
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.snackBar.open(problemMessages(error)[0], undefined, { duration: 5000 });
        this.load();
      },
    });
  }
}
