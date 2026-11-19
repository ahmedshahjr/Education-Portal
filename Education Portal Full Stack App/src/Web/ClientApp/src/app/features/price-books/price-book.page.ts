import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin, Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { moduleText } from '../../core/i18n/module-text.pipe';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import {
  BillingCycle,
  ModuleDto,
  ModulesClient,
  PlanDto,
  PlansClient,
  PriceBookDto,
  PriceBooksClient,
  SetPriceCommand,
  UpdatePriceBookCommand,
} from '../../web-api-client';
import { PriceBookChanges, PriceBookDialogComponent, PriceBookDialogData } from './price-book-dialog.component';
import { SetPriceDialogComponent, SetPriceDialogData } from './set-price-dialog.component';
import { PriceRow, priceFor, unpricedModuleCount } from './price-matrix';

/**
 * One country's price book: its currency, tax rate and switch, and the price of every plan and paid module per billing
 * cycle. A paid module without a monthly price cannot be added to a school's subscription, so the page says so first.
 */
@Component({
  selector: 'app-price-book-page',
  imports: [
    DecimalPipe,
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
  templateUrl: './price-book.page.html',
  styleUrl: './price-book.page.css',
})
export class PriceBookPage {
  private readonly route = inject(ActivatedRoute);
  private readonly booksClient = inject(PriceBooksClient);
  private readonly plansClient = inject(PlansClient);
  private readonly modulesClient = inject(ModulesClient);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly bookId = this.route.snapshot.paramMap.get('bookId') ?? '';
  protected readonly columns = ['item', 'monthly', 'yearly'];
  protected readonly monthly = BillingCycle.Monthly;
  protected readonly yearly = BillingCycle.Yearly;
  protected readonly priceBooksLabel = $localize`:@@priceBooks.breadcrumb:Price books`;

  protected readonly book = signal<PriceBookDto | null>(null);
  protected readonly plans = signal<readonly PlanDto[]>([]);
  protected readonly modules = signal<readonly ModuleDto[]>([]);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);

  protected readonly rows = computed<readonly PriceRow[]>(() => [
    ...this.plans().map((plan) => ({ kind: 'plan' as const, code: plan.code, name: plan.name, inactive: !plan.isActive })),
    ...this.modules()
      .filter((module) => !module.isCore)
      .map((module) => ({ kind: 'module' as const, code: module.code, name: moduleText(module), inactive: !module.isActive })),
  ]);

  /** The heading of the page: the country, and the book's currency and switch. */
  protected readonly header = computed(() => {
    const current = this.book();
    if (!current) {
      return { title: '', subtitle: '' };
    }

    const state = current.isActive ? $localize`:@@priceBooks.stateActive:active` : $localize`:@@priceBooks.stateInactive:inactive`;
    return { title: current.countryCode, subtitle: `${current.currencyCode} · ${state}` };
  });

  /** Paid modules with no monthly price: a school cannot be given them until the price is set. */
  protected readonly unpricedModules = computed(() => unpricedModuleCount(this.rows(), this.book()?.prices ?? []));

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    forkJoin({
      book: this.booksClient.getPriceBook(this.bookId),
      plans: this.plansClient.getPlans(),
      modules: this.modulesClient.getModules(),
    }).subscribe({
      next: ({ book, plans, modules }) => {
        this.book.set(book);
        this.plans.set(plans);
        this.modules.set(modules);
      },
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  /** The price of one row for one billing cycle, or null when none is set. */
  protected priceOf(row: PriceRow, cycle: BillingCycle): number | null {
    return priceFor(this.book()?.prices ?? [], row, cycle);
  }

  protected cycleName(cycle: BillingCycle): string {
    return cycle === BillingCycle.Monthly ? $localize`:@@priceBooks.monthly:Monthly` : $localize`:@@priceBooks.yearly:Yearly`;
  }

  protected editBook(): void {
    const current = this.book();
    if (!current) {
      return;
    }

    const data: PriceBookDialogData = { book: current };
    this.dialog
      .open<PriceBookDialogComponent, PriceBookDialogData, PriceBookChanges>(PriceBookDialogComponent, { data, autoFocus: 'first-tabbable' })
      .afterClosed()
      .subscribe((changes) => {
        if (changes) {
          this.run(
            this.booksClient.updatePriceBook(
              this.bookId,
              new UpdatePriceBookCommand({ currencyCode: changes.currencyCode, isActive: changes.isActive, taxRate: changes.taxRate })
            ),
            $localize`:@@priceBooks.saved:Price book saved.`
          );
        }
      });
  }

  protected setPrice(row: PriceRow, cycle: BillingCycle): void {
    const current = this.book();
    if (!current) {
      return;
    }

    const data: SetPriceDialogData = {
      title: `${row.name} · ${this.cycleName(cycle)}`,
      currencyCode: current.currencyCode,
      current: this.priceOf(row, cycle),
    };
    this.dialog
      .open<SetPriceDialogComponent, SetPriceDialogData, number>(SetPriceDialogComponent, { data, autoFocus: 'first-tabbable' })
      .afterClosed()
      .subscribe((price: number | undefined) => {
        if (price === undefined) {
          return;
        }

        const command = new SetPriceCommand({
          planCode: row.kind === 'plan' ? row.code : undefined,
          moduleCode: row.kind === 'module' ? row.code : undefined,
          billingCycle: cycle,
          price,
        });
        this.run(this.booksClient.setPrice(this.bookId, command), $localize`:@@priceBooks.priceSaved:Price saved.`);
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
      },
    });
  }
}
