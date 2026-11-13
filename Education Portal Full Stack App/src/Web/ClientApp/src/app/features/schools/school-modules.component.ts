import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleChange, MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { problemMessages } from '../../core/http/problem-details';
import { moduleText, ModuleTextPipe } from '../../core/i18n/module-text.pipe';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { SchoolModuleDto, SchoolsClient, SchoolSummaryDto, SetSchoolModuleCommand } from '../../web-api-client';

/**
 * Master portal: gives paid modules to one school or takes them away. Core modules are always on, so their switch is
 * locked. A module the master made inactive is off for every school, so its switch is locked here until it is active
 * again. The school's users can use only the modules that are on.
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
                    @if (!module.isActive) {
                      <app-status-badge tone="error" i18n="@@modules.inactive">Inactive</app-status-badge>
                    }
                  </div>
                  <div class="mm-school-modules__description">{{ module | moduleText: 'description' }}</div>
                </div>
                <mat-slide-toggle
                  [checked]="module.isEnabled"
                  [disabled]="school().isMaster || module.isCore || !module.isActive || saving() === module.code"
                  (change)="setEnabled(module, $event)"
                  aria-label="{{ module | moduleText }} for {{ school().name }}"
                  i18n-aria-label="@@modules.enabledFor"
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

    .mm-school-modules__description {
      color: var(--mm-text-muted);
      font-size: var(--mm-text-sm);
    }
  `,
})
export class SchoolModulesComponent {
  private readonly client = inject(SchoolsClient);
  private readonly snackBar = inject(MatSnackBar);

  readonly school = input.required<SchoolSummaryDto>();

  protected readonly modules = signal<readonly SchoolModuleDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal<string | null>(null);

  constructor() {
    effect(() => {
      this.school();
      this.load();
    });
  }

  protected load(): void {
    this.loadError.set(null);
    this.modules.set(null);
    this.client.getSchoolModules(this.school().id).subscribe({
      next: (modules) => this.modules.set(modules),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected setEnabled(module: SchoolModuleDto, event: MatSlideToggleChange): void {
    const school = this.school();
    this.saving.set(module.code);
    this.client.setSchoolModule(school.id, module.code, new SetSchoolModuleCommand({ isEnabled: event.checked })).subscribe({
      next: () => {
        this.saving.set(null);
        this.modules.update(
          (list) =>
            list?.map((m) => (m.code === module.code ? new SchoolModuleDto({ ...m, isEnabled: event.checked }) : m)) ??
            null
        );
        this.snackBar.open($localize`:@@modules.savedFor:${moduleText(module)}:MODULE: saved for ${school.name}:SCHOOL:`, undefined, {
          duration: 3000,
        });
      },
      error: (error: unknown) => {
        this.saving.set(null);
        event.source.checked = module.isEnabled;
        this.snackBar.open(problemMessages(error)[0], undefined, { duration: 5000 });
      },
    });
  }
}
