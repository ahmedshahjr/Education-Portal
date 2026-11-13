import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { problemMessages } from '../../core/http/problem-details';
import { ModuleTextPipe } from '../../core/i18n/module-text.pipe';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { SchoolModuleDto, SchoolsClient, SchoolSummaryDto } from '../../web-api-client';

/**
 * Master portal: shows which modules one school can use now. Core modules are always on; paid modules are on while the
 * school's subscription gives them, so they change through the subscription, not here. A module the master made
 * inactive is off for every school. The school's users can use only the modules that are on.
 */
@Component({
  selector: 'app-school-modules',
  imports: [ModuleTextPipe, MatCardModule, LoadingStateComponent, EmptyStateComponent, StatusBadgeComponent],
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
                @if (module.isEnabled) {
                  <app-status-badge tone="success" i18n="@@modules.on">On</app-status-badge>
                } @else {
                  <app-status-badge tone="primary" i18n="@@modules.off">Off</app-status-badge>
                }
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

  readonly school = input.required<SchoolSummaryDto>();

  protected readonly modules = signal<readonly SchoolModuleDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);

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
}
