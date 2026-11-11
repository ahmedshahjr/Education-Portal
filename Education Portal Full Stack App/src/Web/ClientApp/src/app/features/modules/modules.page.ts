import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleChange, MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { problemMessages } from '../../core/http/problem-details';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { ModuleDto, ModulesClient, UpdateModuleDefaultCommand } from '../../web-api-client';

/**
 * Master portal: the portal's modules and which are default (on for every school unless switched off for one) or custom
 * (off unless switched on for a school). Switch modules per school on the Schools page.
 * Master tenant administrators only; the server enforces it.
 */
@Component({
  selector: 'app-modules-page',
  imports: [
    MatCardModule,
    MatTableModule,
    MatSlideToggleModule,
    PageHeaderComponent,
    LoadingStateComponent,
    EmptyStateComponent,
    StatusBadgeComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Modules"
      i18n-title="@@modules.title"
      subtitle="Master portal: default modules are on for every school unless you switch them off for one; custom modules are off until you switch them on for a school on the Schools page."
      i18n-subtitle="@@modules.subtitle"
    />

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
          <table mat-table [dataSource]="modules" class="mm-modules__table">
            <ng-container matColumnDef="name">
              <th mat-header-cell *matHeaderCellDef i18n="@@modules.module">Module</th>
              <td mat-cell *matCellDef="let module">
                <div class="mm-modules__name">{{ module.name }}</div>
                <div class="mm-modules__description">{{ module.description }}</div>
              </td>
            </ng-container>

            <ng-container matColumnDef="kind">
              <th mat-header-cell *matHeaderCellDef i18n="@@modules.kind">Type</th>
              <td mat-cell *matCellDef="let module">
                @if (module.isDefault) {
                  <app-status-badge tone="success" i18n="@@modules.default">Default</app-status-badge>
                } @else {
                  <app-status-badge tone="warning" i18n="@@modules.custom">Custom</app-status-badge>
                }
              </td>
            </ng-container>

            <ng-container matColumnDef="isDefault">
              <th mat-header-cell *matHeaderCellDef i18n="@@modules.onForNewSchools">On for every school</th>
              <td mat-cell *matCellDef="let module">
                <mat-slide-toggle
                  [checked]="module.isDefault"
                  [disabled]="saving() === module.key"
                  (change)="setDefault(module, $event)"
                  aria-label="{{ module.name }} is on for every school"
                  i18n-aria-label="@@modules.defaultOf"
                />
              </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="columns"></tr>
            <tr mat-row *matRowDef="let row; columns: columns"></tr>
          </table>
        } @else {
          <app-loading-state />
        }
      </mat-card-content>
    </mat-card>
  `,
  styles: `
    .mm-modules__table {
      width: 100%;
    }

    .mm-modules__name {
      font-weight: 600;
    }

    .mm-modules__description {
      color: var(--mm-text-muted);
      font-size: var(--mm-text-sm);
    }
  `,
})
export class ModulesPage {
  private readonly client = inject(ModulesClient);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly columns = ['name', 'kind', 'isDefault'];
  protected readonly modules = signal<readonly ModuleDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal<string | null>(null);

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    this.modules.set(null);
    this.client.getModules().subscribe({
      next: (modules) => this.modules.set(modules),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected setDefault(module: ModuleDto, event: MatSlideToggleChange): void {
    this.saving.set(module.key);
    this.client.updateModuleDefault(module.key, new UpdateModuleDefaultCommand({ isDefault: event.checked })).subscribe({
      next: () => {
        this.saving.set(null);
        this.modules.update((list) => list?.map((m) => (m.key === module.key ? new ModuleDto({ ...m, isDefault: event.checked }) : m)) ?? null);
        this.snackBar.open($localize`:@@modules.saved:${module.name}:MODULE: saved`, undefined, { duration: 3000 });
      },
      error: (error: unknown) => {
        this.saving.set(null);
        event.source.checked = module.isDefault;
        this.snackBar.open(problemMessages(error)[0], undefined, { duration: 5000 });
      },
    });
  }
}
