import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { LocalDatePipe } from '../../core/i18n/local-date.pipe';
import { ModuleTextPipe } from '../../core/i18n/module-text.pipe';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { StatusBadgeComponent, BadgeTone } from '../../shared/components/status-badge/status-badge.component';
import {
  AddSubscriptionModuleCommand,
  BillingCycle,
  ChangeSubscriptionPlanCommand,
  CreateInvoiceCommand,
  CreateSubscriptionCommand,
  ExtendSubscriptionCommand,
  InvoiceDto,
  InvoicesClient,
  InvoiceStatus,
  ModuleDto,
  ModulesClient,
  PaymentMethod,
  PlanDto,
  PlansClient,
  RecordPaymentCommand,
  SchoolsClient,
  SchoolSummaryDto,
  SubscriptionDto,
  SubscriptionStatus,
  TenantModuleStatus,
} from '../../web-api-client';

/** Limits shared with the server (Subscription.MaxTrialDays, Subscription.MaxExtensionDays); the server checks again. */
export const MAX_TRIAL_DAYS = 90;
export const MAX_EXTENSION_DAYS = 366;

/**
 * Master portal: one school's subscription. Without one, the master starts it (plan, billing cycle, trial). With one, the
 * master sees what the school gets and pays, suspends or reactivates it, extends the period, changes the plan, adds or
 * removes add-on modules, issues invoices and records manual payments. Changes take effect at once with no credit or
 * charge for the current period (ADR-014). Master tenant administrators only; the server enforces it.
 */
@Component({
  selector: 'app-school-subscription',
  imports: [
    DecimalPipe,
    LocalDatePipe,
    ModuleTextPipe,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    LoadingStateComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './school-subscription.component.html',
  styleUrl: './school-subscription.component.css',
})
export class SchoolSubscriptionComponent {
  private readonly schools = inject(SchoolsClient);
  private readonly invoicesClient = inject(InvoicesClient);
  private readonly plansClient = inject(PlansClient);
  private readonly modulesClient = inject(ModulesClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly formBuilder = inject(FormBuilder);

  readonly school = input.required<SchoolSummaryDto>();

  protected readonly subscription = signal<SubscriptionDto | null>(null);
  protected readonly hasNone = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly invoices = signal<readonly InvoiceDto[]>([]);
  protected readonly plans = signal<readonly PlanDto[]>([]);
  protected readonly modules = signal<readonly ModuleDto[]>([]);
  protected readonly busy = signal(false);
  protected readonly payingInvoice = signal<InvoiceDto | null>(null);

  protected readonly cycles = [BillingCycle.Monthly, BillingCycle.Yearly];
  protected readonly methods = Object.values(PaymentMethod);
  protected readonly maxTrialDays = MAX_TRIAL_DAYS;
  protected readonly maxExtensionDays = MAX_EXTENSION_DAYS;
  protected readonly statuses = SubscriptionStatus;
  protected readonly invoiceStatuses = InvoiceStatus;

  protected readonly startForm = this.formBuilder.nonNullable.group({
    planCode: [''],
    billingCycle: [BillingCycle.Monthly, Validators.required],
    trialDays: [0, [Validators.required, Validators.min(0), Validators.max(MAX_TRIAL_DAYS)]],
  });
  protected readonly extendForm = this.formBuilder.nonNullable.group({
    days: [30, [Validators.required, Validators.min(1), Validators.max(MAX_EXTENSION_DAYS)]],
  });
  protected readonly planForm = this.formBuilder.nonNullable.group({ planCode: ['', Validators.required] });
  protected readonly moduleForm = this.formBuilder.nonNullable.group({ moduleCode: ['', Validators.required] });
  protected readonly paymentForm = this.formBuilder.nonNullable.group({
    method: [PaymentMethod.BankTransfer, Validators.required],
    reference: ['', [Validators.required, Validators.maxLength(100)]],
  });

  /** Paid modules the school does not have now, the choices for "add a module". */
  protected readonly addableModules = computed(() => {
    const held = new Set((this.subscription()?.modules ?? []).filter((m) => m.status !== TenantModuleStatus.Expired).map((m) => m.code));
    return this.modules().filter((m) => !m.isCore && m.isActive && !held.has(m.code));
  });

  /** Add-on modules the master may remove: paid, not expired, and not part of the current plan. */
  protected readonly removableCodes = computed(() => {
    const subscription = this.subscription();
    const plan = this.plans().find((p) => p.code === subscription?.planCode);
    const core = new Set(this.modules().filter((m) => m.isCore).map((m) => m.code));
    return new Set(
      (subscription?.modules ?? [])
        .filter((m) => m.status !== TenantModuleStatus.Expired && !core.has(m.code) && !(plan?.modules ?? []).includes(m.code))
        .map((m) => m.code)
    );
  });

  constructor() {
    this.plansClient.getPlans().subscribe({ next: (plans) => this.plans.set(plans.filter((p) => p.isActive)) });
    this.modulesClient.getModules().subscribe({ next: (modules) => this.modules.set(modules) });
    effect(() => {
      this.school();
      this.load();
    });
  }

  protected load(): void {
    const schoolId = this.school().id;
    this.loadError.set(null);
    this.subscription.set(null);
    this.hasNone.set(false);
    this.payingInvoice.set(null);

    this.schools.getSchoolSubscription(schoolId).subscribe({
      next: (subscription) => {
        this.subscription.set(subscription);
        this.loadInvoices();
      },
      error: (error: unknown) => {
        if (isNotFound(error)) {
          this.hasNone.set(true);
        } else {
          this.loadError.set(problemMessages(error)[0]);
        }
      },
    });
  }

  private loadInvoices(): void {
    this.invoicesClient.getInvoices(this.school().id).subscribe({
      next: (invoices) => this.invoices.set(invoices),
      error: (error: unknown) => this.snackBar.open(problemMessages(error)[0], undefined, { duration: 5000 }),
    });
  }

  protected tone(status: SubscriptionStatus | InvoiceStatus | TenantModuleStatus): BadgeTone {
    switch (status) {
      case 'Active':
      case 'Paid':
        return 'success';
      case 'Trial':
      case 'Open':
        return 'primary';
      case 'PastDue':
      case 'Suspended':
        return 'warning';
      default:
        return 'error';
    }
  }

  protected start(): void {
    if (this.startForm.invalid) {
      return;
    }
    const value = this.startForm.getRawValue();
    this.run(
      this.schools.createSubscription(
        this.school().id,
        new CreateSubscriptionCommand({ planCode: value.planCode || undefined, modules: [], billingCycle: value.billingCycle, trialDays: value.trialDays })
      ),
      $localize`:@@subscription.started:Subscription started.`
    );
  }

  protected suspend(): void {
    this.run(this.schools.suspendSubscription(this.school().id), $localize`:@@subscription.suspended:Subscription suspended.`);
  }

  protected reactivate(): void {
    this.run(this.schools.reactivateSubscription(this.school().id), $localize`:@@subscription.reactivated:Subscription reactivated.`);
  }

  protected extend(): void {
    if (this.extendForm.invalid) {
      return;
    }
    this.run(
      this.schools.extendSubscription(this.school().id, new ExtendSubscriptionCommand({ days: this.extendForm.getRawValue().days })),
      $localize`:@@subscription.extended:Period extended.`
    );
  }

  protected changePlan(): void {
    if (this.planForm.invalid) {
      return;
    }
    this.run(
      this.schools.changeSubscriptionPlan(this.school().id, new ChangeSubscriptionPlanCommand({ planCode: this.planForm.getRawValue().planCode })),
      $localize`:@@subscription.planChanged:Plan changed.`
    );
  }

  protected addModule(): void {
    if (this.moduleForm.invalid) {
      return;
    }
    this.run(
      this.schools.addSubscriptionModule(this.school().id, new AddSubscriptionModuleCommand({ moduleCode: this.moduleForm.getRawValue().moduleCode })),
      $localize`:@@subscription.moduleAdded:Module added.`
    );
  }

  protected removeModule(code: string): void {
    this.run(this.schools.removeSubscriptionModule(this.school().id, code), $localize`:@@subscription.moduleRemoved:Module removed.`);
  }

  protected issueInvoice(nextPeriod: boolean): void {
    this.run(
      this.invoicesClient.createInvoice(new CreateInvoiceCommand({ schoolId: this.school().id, nextPeriod })),
      $localize`:@@subscription.invoiceIssued:Invoice issued.`
    );
  }

  protected startPayment(invoice: InvoiceDto): void {
    this.paymentForm.reset({ method: PaymentMethod.BankTransfer, reference: '' });
    this.payingInvoice.set(invoice);
  }

  /** Records a payment for the invoice's whole total, received now; the server refuses anything else. */
  protected recordPayment(): void {
    const invoice = this.payingInvoice();
    if (invoice === null || this.paymentForm.invalid) {
      return;
    }
    const value = this.paymentForm.getRawValue();
    this.run(
      this.invoicesClient.recordPayment(
        invoice.id,
        new RecordPaymentCommand({ invoiceId: invoice.id, amount: invoice.total, method: value.method, reference: value.reference.trim(), receivedDate: new Date() })
      ),
      $localize`:@@subscription.paymentRecorded:Payment recorded.`
    );
  }

  protected voidInvoice(invoice: InvoiceDto): void {
    this.run(this.invoicesClient.voidInvoice(invoice.id), $localize`:@@subscription.invoiceVoided:Invoice voided.`);
  }

  /** Runs one change, tells the master what happened and reloads, so the screen always shows what the server holds. */
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

function isNotFound(error: unknown): boolean {
  return typeof error === 'object' && error !== null && 'status' in error && Number(error.status) === 404;
}
