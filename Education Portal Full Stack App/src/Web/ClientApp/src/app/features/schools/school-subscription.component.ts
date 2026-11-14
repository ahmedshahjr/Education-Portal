import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggle, MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { LocalDatePipe } from '../../core/i18n/local-date.pipe';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { BadgeTone, StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import {
  BillingCycle,
  ChangeSubscriptionPlanCommand,
  CreateSubscriptionCommand,
  ExtendSubscriptionCommand,
  PlanDto,
  PlansClient,
  SchoolsClient,
  SchoolSummaryDto,
  SubscriptionDto,
  SubscriptionStatus,
} from '../../web-api-client';

/** Limits shared with the server (Subscription.MaxTrialDays, Subscription.MaxExtensionDays); the server checks again. */
export const MAX_TRIAL_DAYS = 90;
export const MAX_EXTENSION_DAYS = 366;

/**
 * Master portal: one school's plan. Without a subscription the master starts one (plan, billing cycle, trial). With one,
 * the master turns the plan on or off with the switch (off suspends it, on reactivates it), extends the period and
 * changes the plan. Modules and invoices have their own tabs. Changes take effect at once with no credit or charge for the
 * current period (ADR-014). Master tenant administrators only; the server enforces it.
 */
@Component({
  selector: 'app-school-subscription',
  imports: [
    DecimalPipe,
    LocalDatePipe,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
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
  private readonly plansClient = inject(PlansClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialog = inject(MatDialog);

  readonly school = input.required<SchoolSummaryDto>();

  protected readonly subscription = signal<SubscriptionDto | null>(null);
  protected readonly hasNone = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly plans = signal<readonly PlanDto[]>([]);
  protected readonly busy = signal(false);

  protected readonly cycles = [BillingCycle.Monthly, BillingCycle.Yearly];
  protected readonly maxTrialDays = MAX_TRIAL_DAYS;
  protected readonly maxExtensionDays = MAX_EXTENSION_DAYS;
  protected readonly statuses = SubscriptionStatus;

  protected readonly startForm = this.formBuilder.nonNullable.group({
    planCode: [''],
    billingCycle: [BillingCycle.Monthly, Validators.required],
    trialDays: [0, [Validators.required, Validators.min(0), Validators.max(MAX_TRIAL_DAYS)]],
  });
  protected readonly extendForm = this.formBuilder.nonNullable.group({
    days: [30, [Validators.required, Validators.min(1), Validators.max(MAX_EXTENSION_DAYS)]],
  });
  protected readonly planForm = this.formBuilder.nonNullable.group({ planCode: ['', Validators.required] });

  constructor() {
    this.plansClient.getPlans().subscribe({ next: (plans) => this.plans.set(plans.filter((p) => p.isActive)) });
    effect(() => {
      this.school();
      this.load();
    });
  }

  /** Suspended and past-due plans are not running; the switch shows whether the plan is on. */
  protected isOn(sub: SubscriptionDto): boolean {
    return sub.status !== SubscriptionStatus.Suspended && sub.status !== SubscriptionStatus.Cancelled;
  }

  protected load(): void {
    this.loadError.set(null);
    this.subscription.set(null);
    this.hasNone.set(false);

    this.schools.getSchoolSubscription(this.school().id).subscribe({
      next: (subscription) => this.subscription.set(subscription),
      error: (error: unknown) => {
        if (isNotFound(error)) {
          this.hasNone.set(true);
        } else {
          this.loadError.set(problemMessages(error)[0]);
        }
      },
    });
  }

  protected tone(status: SubscriptionStatus): BadgeTone {
    switch (status) {
      case SubscriptionStatus.Active:
        return 'success';
      case SubscriptionStatus.Trial:
        return 'primary';
      case SubscriptionStatus.PastDue:
      case SubscriptionStatus.Suspended:
        return 'warning';
      default:
        return 'error';
    }
  }

  /**
   * The plan switch. Turning it off suspends the plan (modules off, data kept) after a confirm step; turning it on
   * reactivates it. If the master says no, the switch goes back to where it was.
   */
  protected togglePlan(toggle: MatSlideToggle, on: boolean): void {
    if (on) {
      this.run(this.schools.reactivateSubscription(this.school().id), $localize`:@@subscription.reactivated:Plan activated.`);
      return;
    }
    const data: ConfirmDialogData = {
      title: $localize`:@@subscription.deactivateTitle:Deactivate this plan?`,
      message: $localize`:@@subscription.deactivateMessage:The school loses access to its modules until the plan is activated again. Its data is kept.`,
      confirmLabel: $localize`:@@subscription.deactivate:Deactivate`,
    };
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed === true) {
          this.run(this.schools.suspendSubscription(this.school().id), $localize`:@@subscription.suspended:Plan deactivated.`);
        } else {
          toggle.checked = true;
        }
      });
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
        this.load();
      },
    });
  }
}

function isNotFound(error: unknown): boolean {
  return typeof error === 'object' && error !== null && 'status' in error && Number(error.status) === 404;
}
