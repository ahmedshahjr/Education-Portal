import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleChange, MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { problemMessages } from '../../core/http/problem-details';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { SchoolModuleDto, SchoolsClient, SchoolSummaryDto, SetSchoolModuleCommand } from '../../web-api-client';

/**
 * Master portal: switches modules on or off for one school. The school's users can use only the modules that are on.
 * A choice made here wins over the module's default from then on.
 */
@Component({
  selector: 'app-school-modules',
  imports: [MatCardModule, MatSlideToggleModule, LoadingStateComponent, EmptyStateComponent, StatusBadgeComponent],
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
            @for (module of modules; track module.key) {
              <li class="mm-school-modules__row">
                <div class="mm-school-modules__text">
                  <div class="mm-school-modules__name">
                    {{ module.name }}
                    @if (module.isDefault) {
                      <app-status-badge tone="success" i18n="@@modules.default">Default</app-status-badge>
                    } @else {
                      <app-status-badge tone="warning" i18n="@@modules.custom">Custom</app-status-badge>
                    }
                  </div>
                  <div class="mm-school-modules__description">{{ module.description }}</div>
                </div>
                <mat-slide-toggle
                  [checked]="module.isEnabled"
                  [disabled]="school().isMaster || saving() === module.key"
                  (change)="setEnabled(module, $event)"
                  aria-label="{{ module.name }} for {{ school().name }}"
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
    this.saving.set(module.key);
    this.client.setSchoolModule(school.id, module.key, new SetSchoolModuleCommand({ isEnabled: event.checked })).subscribe({
      next: () => {
        this.saving.set(null);
        this.modules.update(
          (list) =>
            list?.map((m) => (m.key === module.key ? new SchoolModuleDto({ ...m, isEnabled: event.checked, followsDefault: false }) : m)) ??
            null
        );
        this.snackBar.open($localize`:@@modules.savedFor:${module.name}:MODULE: saved for ${school.name}:SCHOOL:`, undefined, {
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
