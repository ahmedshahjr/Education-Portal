import { ChangeDetectionStrategy, Component, inject, signal, WritableSignal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ToastService } from '../../shared/services/toast.service';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { BillingCycle, CountriesClient, CountryDto, CreatePlanCommand, CreatePriceBookCommand, ModuleCode, ModuleDto, ModulesClient, PlanDto, PlansClient, PriceBookDto, PriceBooksClient, PriceDto, SetPriceCommand } from '../../web-api-client';

/**
 * The master tenant's pricing: the plans (named bundles of paid modules) and one price book per country, with the price of
 * each plan and each paid module for a monthly and a yearly billing cycle. A school's trial and subscriptions are priced from
 * its country's book.
 */
@Component({
  selector: 'app-pricing-page',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    PageHeaderComponent,
    EmptyStateComponent,
    LoadingStateComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './pricing.page.html',
  styleUrl: './pricing.page.css',
})
export class PricingPage {
  private readonly plansClient = inject(PlansClient);
  private readonly bookClient = inject(PriceBooksClient);
  private readonly modulesClient = inject(ModulesClient);
  private readonly countriesClient = inject(CountriesClient);
  private readonly formBuilder = inject(FormBuilder);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);

  protected readonly plans = signal<readonly PlanDto[] | null>(null);
  protected readonly books = signal<readonly PriceBookDto[] | null>(null);
  protected readonly modules = signal<readonly ModuleDto[]>([]);
  protected readonly countries = signal<readonly CountryDto[]>([]);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  /** Each section shows its own errors, next to its own form. */
  protected readonly planErrors = signal<readonly string[]>([]);
  protected readonly bookErrors = signal<readonly string[]>([]);
  protected readonly priceErrors = signal<readonly string[]>([]);

  protected readonly cycles = [BillingCycle.Monthly, BillingCycle.Yearly];

  protected readonly planForm = this.formBuilder.nonNullable.group({
    code: ['', [Validators.required, Validators.maxLength(50), Validators.pattern(/^[A-Z][A-Z0-9_]*$/)]],
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(300)]],
    modules: [[] as ModuleCode[]],
  });

  protected readonly bookForm = this.formBuilder.nonNullable.group({
    countryCode: ['', [Validators.required]],
    currencyCode: ['', [Validators.required, Validators.pattern(/^[A-Z]{3}$/)]],
  });

  protected readonly priceForm = this.formBuilder.nonNullable.group({
    bookId: ['', [Validators.required]],
    target: ['plan', [Validators.required]],
    planCode: [''],
    moduleCode: [null as ModuleCode | null],
    billingCycle: [BillingCycle.Monthly as BillingCycle, [Validators.required]],
    price: [0, [Validators.required, Validators.min(0)]],
  });

  constructor() {
    this.loadAll();
  }

  protected loadAll(): void {
    this.loadError.set(null);
    this.plansClient.getPlans().subscribe({
      next: (plans) => this.plans.set(plans),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
    this.bookClient.getPriceBooks().subscribe({
      next: (books) => this.books.set(books),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
    this.modulesClient.getModules().subscribe({ next: (modules) => this.modules.set(modules) });
    this.countriesClient.getCountries(1, 100).subscribe({ next: (page) => this.countries.set(page.items) });
  }

  protected toggleModule(code: ModuleCode): void {
    const current = this.planForm.controls.modules.value;
    this.planForm.controls.modules.setValue(current.includes(code) ? current.filter((c) => c !== code) : [...current, code]);
  }

  protected addPlan(): void {
    if (this.planForm.invalid) {
      this.planForm.markAllAsTouched();
      return;
    }
    const value = this.planForm.getRawValue();
    this.run(
      this.plansClient.createPlan(new CreatePlanCommand({ code: value.code.trim(), name: value.name.trim(), description: value.description.trim(), modules: value.modules })),
      $localize`:@@pricing.planAdded:Plan added.`,
      this.planErrors,
      () => this.planForm.reset({ code: '', name: '', description: '', modules: [] })
    );
  }

  protected deletePlan(plan: PlanDto): void {
    this.confirm(
      $localize`:@@pricing.deletePlanTitle:Delete this plan?`,
      $localize`:@@pricing.deletePlanMessage:Schools already on it keep their modules. The plan can no longer be chosen.`,
      () => this.run(this.plansClient.deletePlan(plan.id), $localize`:@@pricing.planDeleted:Plan deleted.`, this.planErrors)
    );
  }

  protected addBook(): void {
    if (this.bookForm.invalid) {
      this.bookForm.markAllAsTouched();
      return;
    }
    const value = this.bookForm.getRawValue();
    this.run(
      this.bookClient.createPriceBook(new CreatePriceBookCommand({ countryCode: value.countryCode, currencyCode: value.currencyCode.trim().toUpperCase() })),
      $localize`:@@pricing.bookAdded:Price book added.`,
      this.bookErrors,
      () => this.bookForm.reset({ countryCode: '', currencyCode: '' })
    );
  }

  protected deleteBook(book: PriceBookDto): void {
    this.confirm(
      $localize`:@@pricing.deleteBookTitle:Delete this price book?`,
      $localize`:@@pricing.deleteBookMessage:Schools in this country cannot be registered until a price book exists again.`,
      () => this.run(this.bookClient.deletePriceBook(book.id), $localize`:@@pricing.bookDeleted:Price book deleted.`, this.bookErrors)
    );
  }

  protected setPrice(): void {
    if (this.priceForm.invalid) {
      this.priceForm.markAllAsTouched();
      return;
    }
    const value = this.priceForm.getRawValue();
    const isPlan = value.target === 'plan';
    if (isPlan ? !value.planCode : !value.moduleCode) {
      this.priceErrors.set([$localize`:@@pricing.chooseItem:Choose a plan or a module.`]);
      return;
    }
    this.run(
      this.bookClient.setPrice(
        value.bookId,
        new SetPriceCommand({
          priceBookId: value.bookId,
          planCode: isPlan ? value.planCode : undefined,
          moduleCode: isPlan ? undefined : (value.moduleCode ?? undefined),
          billingCycle: value.billingCycle,
          price: value.price,
        })
      ),
      $localize`:@@pricing.priceSaved:Price saved.`,
      this.priceErrors,
      () => this.priceForm.patchValue({ planCode: '', moduleCode: null, price: 0 })
    );
  }

  /** The paid modules, the ones a price can be set for. */
  protected paidModules(): readonly ModuleDto[] {
    return this.modules().filter((m) => !m.isCore);
  }

  private confirm(title: string, message: string, onConfirm: () => void): void {
    const data: ConfirmDialogData = {
      title,
      message,
      confirmLabel: $localize`:@@pricing.delete:Delete`,
    };
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed === true) {
          onConfirm();
        }
      });
  }

  /** The names of a plan's paid modules, for display. */
  protected planModuleNames(plan: PlanDto): string {
    return plan.modules.map((code) => this.moduleName(code)).join(', ');
  }

  /** The name of a module by its code, for display. */
  protected moduleName(code: ModuleCode | string): string {
    return this.modules().find((m) => m.code === code)?.name ?? code;
  }

  /** The name of a price's plan or module, for display. */
  protected priceTargetName(price: PriceDto): string {
    if (price.planCode) {
      return this.plans()?.find((p) => p.code === price.planCode)?.name ?? price.planCode;
    }
    return this.moduleName(price.moduleCode ?? '');
  }

  private run(request: Observable<unknown>, done: string, errors: WritableSignal<readonly string[]>, onSuccess?: () => void): void {
    this.busy.set(true);
    errors.set([]);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        onSuccess?.();
        this.toast.success(done);
        this.loadAll();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        errors.set(problemMessages(error));
      },
    });
  }
}
