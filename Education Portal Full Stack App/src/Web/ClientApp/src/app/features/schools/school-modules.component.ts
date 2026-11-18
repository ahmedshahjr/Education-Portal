import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatSlideToggle, MatSlideToggleModule } from '@angular/material/slide-toggle';
import { ToastService } from '../../shared/services/toast.service';
import { problemMessages } from '../../core/http/problem-details';
import { ModuleTextPipe } from '../../core/i18n/module-text.pipe';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { SchoolModuleDto, SchoolsClient, SchoolSummaryDto, SetSchoolModuleCommand } from '../../web-api-client';

/**
 * Master portal: which modules one school can use, as switches. The master tenant can switch every module on or off for the
 * school, core and default modules included. A module the master switched off for every school cannot be turned on here.
 * Each change is checked on the server, which also keeps the school's audit trail (ADR-017).
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
                    @if (module.isDefaultEnabled) {
                      <app-status-badge tone="primary" i18n="@@modules.defaultOn">On by default</app-status-badge>
                    }
                    @if (!module.isActive) {
                      <app-status-badge tone="error" i18n="@@modules.inactive">Switched off for every school</app-status-badge>
                    }
                  </div>
                  <div class="mm-school-modules__description">{{ module | moduleText: 'description' }}</div>
                </div>
                <mat-slide-toggle
                  #toggle
                  [checked]="module.isEnabled"
                  [disabled]="!module.isActive || busy()"
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
      flex-wrap: wrap;
      align-items: center;
      gap: var(--mm-space-2);
      font-weight: 600;
    }

    .mm-school-modules__description {
      color: var(--mm-text-muted);
      font-size: var(--mm-text-sm);
    }
  `,
})
export class SchoolModulesComponent {
  private readonly schools = inject(SchoolsClient);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);

  readonly school = input.required<SchoolSummaryDto>();

  protected readonly modules = signal<readonly SchoolModuleDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);

  constructor() {
    effect(() => {
      this.school();
      this.load();
    });
  }

  protected load(): void {
    this.loadError.set(null);
    this.modules.set(null);
    this.schools.getSchoolModules(this.school().id).subscribe({
      next: (modules) => this.modules.set(modules),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  /** Turns a module on, or off after a confirm step. A refusal puts the switch back. */
  protected change(module: SchoolModuleDto, on: boolean, toggle: MatSlideToggle): void {
    if (on) {
      this.run(
        this.schools.setSchoolModule(this.school().id, module.code, new SetSchoolModuleCommand({ enabled: true })),
        $localize`:@@modules.added:Module turned on.`,
        module,
        toggle
      );
      return;
    }

    const data: ConfirmDialogData = {
      title: $localize`:@@modules.removeTitle:Turn this module off for the school?`,
      message: $localize`:@@modules.removeMessage:The school's users lose access to this module at once. Its data is kept.`,
      confirmLabel: $localize`:@@modules.remove:Turn off`,
    };
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed === true) {
          this.run(
            this.schools.setSchoolModule(this.school().id, module.code, new SetSchoolModuleCommand({ enabled: false })),
            $localize`:@@modules.removed:Module turned off.`,
            module,
            toggle
          );
        } else {
          toggle.checked = true;
        }
      });
  }

  private run(request: ReturnType<SchoolsClient['setSchoolModule']>, done: string, module: SchoolModuleDto, toggle: MatSlideToggle): void {
    this.busy.set(true);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success(done);
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        toggle.checked = module.isEnabled;
        this.toast.error(error);
        this.load();
      },
    });
  }
}
