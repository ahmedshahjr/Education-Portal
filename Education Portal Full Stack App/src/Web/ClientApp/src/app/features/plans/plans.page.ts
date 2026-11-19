import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { forkJoin, Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { moduleText } from '../../core/i18n/module-text.pipe';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import {
  CreatePlanCommand,
  ModuleDto,
  ModulesClient,
  PlanDto,
  PlansClient,
  UpdatePlanCommand,
} from '../../web-api-client';
import { PlanChanges, PlanDialogComponent, PlanDialogData } from './plan-dialog.component';

/**
 * Master portal: the plans the platform sells, each a named bundle of paid modules (core modules are in every plan).
 * Prices are set per country in the price books. Master tenant administrators only; the server enforces it.
 */
@Component({
  selector: 'app-plans-page',
  imports: [
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
  templateUrl: './plans.page.html',
  styleUrl: './plans.page.css',
})
export class PlansPage {
  private readonly plansClient = inject(PlansClient);
  private readonly modulesClient = inject(ModulesClient);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly columns = ['name', 'modules', 'status', 'actions'];
  protected readonly plans = signal<readonly PlanDto[] | null>(null);
  protected readonly modules = signal<readonly ModuleDto[]>([]);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);

  /** The paid modules a plan can include. */
  protected readonly paidModules = computed(() => this.modules().filter((m) => !m.isCore));

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    forkJoin({ plans: this.plansClient.getPlans(), modules: this.modulesClient.getModules() }).subscribe({
      next: ({ plans, modules }) => {
        this.plans.set(plans);
        this.modules.set(modules);
      },
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  /** The name of a module by its code, in the page's language. */
  protected moduleName(code: string): string {
    return moduleText({ code });
  }

  protected add(): void {
    this.openDialog({ plan: null, modules: this.paidModules() }, (changes) =>
      this.plansClient.createPlan(
        new CreatePlanCommand({
          code: changes.code,
          name: changes.name,
          description: changes.description,
          modules: [...changes.modules],
        })
      ),
      $localize`:@@plans.added:Plan added.`
    );
  }

  protected edit(plan: PlanDto): void {
    this.openDialog({ plan, modules: this.paidModules() }, (changes) =>
      this.plansClient.updatePlan(
        plan.id,
        new UpdatePlanCommand({
          name: changes.name,
          description: changes.description,
          isActive: changes.isActive,
          modules: [...changes.modules],
        })
      ),
      $localize`:@@plans.saved:Plan saved.`
    );
  }

  protected remove(plan: PlanDto): void {
    const data: ConfirmDialogData = {
      title: $localize`:@@plans.removeTitle:Delete this plan?`,
      message: $localize`:@@plans.removeMessage:Its prices are deleted too. Schools already on the plan keep their modules until their subscription changes.`,
      confirmLabel: $localize`:@@plans.remove:Delete plan`,
    };
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed === true) {
          this.run(this.plansClient.deletePlan(plan.id), $localize`:@@plans.removed:Plan deleted.`);
        }
      });
  }

  private openDialog(data: PlanDialogData, request: (changes: PlanChanges) => Observable<unknown>, done: string): void {
    this.dialog
      .open<PlanDialogComponent, PlanDialogData, PlanChanges>(PlanDialogComponent, { data, width: '560px', autoFocus: 'first-tabbable' })
      .afterClosed()
      .subscribe((changes) => {
        if (changes) {
          this.run(request(changes), done);
        }
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
