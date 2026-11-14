import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatSlideToggle, MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { problemMessages } from '../../core/http/problem-details';
import { ModuleTextPipe } from '../../core/i18n/module-text.pipe';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import {
  AddSubscriptionModuleCommand,
  PlanDto,
  PlansClient,
  SchoolModuleDto,
  SchoolsClient,
  SchoolSummaryDto,
  SubscriptionDto,
} from '../../web-api-client';

/**
 * Master portal: which modules one school can use, as switches. Core modules are always on. A paid module in the school's
 * plan is on while the plan is, so its switch is locked on. An add-on is switched on (added to the subscription) or off
 * (removed after a confirm step). A module the master made inactive is off for every school. Each change is checked on the
 * server (ADR-012, ADR-014).
 */
@Component({
  selector: 'app-school-modules',
  imports: [ModuleTextPipe, MatCardModule, MatSlideToggleModule, LoadingStateComponent, EmptyStateComponent, StatusBadgeComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-card class="mm-card mm-elevated">
      <mat-card-content>
        @if (loadError(); as message) {
          <app-empty-state
            icon="alert-triangle"
            title="Modules could not be loaded"
            i18n-title="@@modules.loadError"
            [description]="message"
            actionLabel="Try again"
            i18n-actionLabel="@@common.tryAgain"
            (action)="load()"
          />
        } @else if (modules(); as modules) {
          @if (!subscription()) {
            <p class="mm-school-modules__hint" i18n="@@modules.needSubscription">Start the school's subscription to add paid modules.</p>
          }
          <ul class="mm-school-modules">
            @for (module of modules; track module.code) {
              <li class="mm-school-modules__row">
                <div class="mm-school-modules__text">
                  <div class="mm-school-modules__name">
                    {{ module | moduleText }}
                    @if (module.isCore) {
                      <app-status-badge tone="success" i18n="@@modules.core">Core</app-status-badge>
                    } @else {
                      <app-status-badge tone="warning" i18n="@@modules.paid">Paid</app-status-badge>
                    }
                    @if (!module.isActive) {
                      <app-status-badge tone="error" i18n="@@modules.inactive">Inactive</app-status-badge>
                    }
                  </div>
                  <div class="mm-school-modules__description">{{ module | moduleText: 'description' }}</div>
                  @if (lockedByPlan(module)) {
                    <div class="mm-school-modules__hint" i18n="@@modules.inPlan">Included in the school's plan.</div>
                  }
                </div>
                <mat-slide-toggle
                  #toggle
                  [checked]="module.isEnabled"
                  [disabled]="isLocked(module) || busy()"
                  (change)="change(module, $event.checked, toggle)"
                  [aria-label]="(module | moduleText) + ' switch'"
                />
              </li>
            }
          </ul>
        } @else {
          <app-loading-state />
        }
      </mat-card-content>
    </mat-card>
  `,
  styles: `
    .mm-school-modules {
      list-style: none;
      margin: 0;
      padding: 0;
    }

    .mm-school-modules__row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--mm-space-4);
      padding-block: var(--mm-space-3);
      border-bottom: 1px solid var(--mm-border-color);
    }

    .mm-school-modules__row:last-child {
      border-bottom: none;
    }

    .mm-school-modules__name {
      display: flex;
      align-items: center;
      gap: var(--mm-space-2);
      font-weight: 600;
    }

    .mm-school-modules__description,
    .mm-school-modules__hint {
      color: var(--mm-text-muted);
      font-size: var(--mm-text-sm);
    }

    .mm-school-modules__hint {
      margin-bottom: var(--mm-space-3);
    }
  `,
})
export class SchoolModulesComponent {
  private readonly schools = inject(SchoolsClient);
  private readonly plansClient = inject(PlansClient);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  readonly school = input.required<SchoolSummaryDto>();

  protected readonly modules = signal<readonly SchoolModuleDto[] | null>(null);
  protected readonly subscription = signal<SubscriptionDto | null>(null);
  protected readonly plans = signal<readonly PlanDto[]>([]);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);

  constructor() {
    this.plansClient.getPlans().subscribe({ next: (plans) => this.plans.set(plans) });
    effect(() => {
      this.school();
      this.load();
    });
  }

  /** Core modules and modules the master made inactive cannot be switched here. */
  protected isLocked(module: SchoolModuleDto): boolean {
    return module.isCore || !module.isActive || this.lockedByPlan(module) || (this.subscription() === null && !module.isEnabled);
  }

  /** A paid module the school's plan includes: it stays on while the plan does. */
  protected lockedByPlan(module: SchoolModuleDto): boolean {
    const planCode = this.subscription()?.planCode;
    const plan = this.plans().find((p) => p.code === planCode);
    return !module.isCore && (plan?.modules ?? []).includes(module.code);
  }

  protected load(): void {
    this.loadError.set(null);
    this.modules.set(null);
    this.schools.getSchoolSubscription(this.school().id).subscribe({
      next: (subscription) => this.subscription.set(subscription),
      error: () => this.subscription.set(null), // no subscription yet: nothing to add to, but the list still shows
    });
    this.schools.getSchoolModules(this.school().id).subscribe({
      next: (modules) => this.modules.set(modules),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  /** Switches a module on (added to the subscription) or off (removed after a confirm step). A refusal puts the switch back. */
  protected change(module: SchoolModuleDto, on: boolean, toggle: MatSlideToggle): void {
    if (on) {
      this.run(
        this.schools.addSubscriptionModule(this.school().id, new AddSubscriptionModuleCommand({ moduleCode: module.code })),
        $localize`:@@modules.added:Module added.`,
        module,
        toggle
      );
      return;
    }

    const data: ConfirmDialogData = {
      title: $localize`:@@modules.removeTitle:Turn this module off?`,
      message: $localize`:@@modules.removeMessage:The school's users lose access to this module at once. Its data is kept.`,
      confirmLabel: $localize`:@@modules.remove:Turn off`,
    };
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed === true) {
          this.run(
            this.schools.removeSubscriptionModule(this.school().id, module.code),
            $localize`:@@modules.removed:Module turned off.`,
            module,
            toggle
          );
        } else {
          toggle.checked = true;
        }
      });
  }

  private run(request: ReturnType<SchoolsClient['addSubscriptionModule']>, done: string, module: SchoolModuleDto, toggle: MatSlideToggle): void {
    this.busy.set(true);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.snackBar.open(done, undefined, { duration: 3000 });
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        toggle.checked = module.isEnabled;
        this.snackBar.open(problemMessages(error)[0], undefined, { duration: 5000 });
        this.load();
      },
    });
  }
}
