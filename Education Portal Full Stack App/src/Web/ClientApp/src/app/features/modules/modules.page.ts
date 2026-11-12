import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { problemMessages } from '../../core/http/problem-details';
import { moduleText, ModuleTextPipe } from '../../core/i18n/module-text.pipe';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { ModuleDto, ModulesClient, ModuleType, UpdateModuleCommand } from '../../web-api-client';

/**
 * Master portal: the portal's modules. Each has a type, Default (on for every school unless switched off for one) or
 * Custom (off until switched on for a school), and an Active switch: an inactive module is off for every school.
 * Switch modules per school on the Schools page. Master tenant administrators only; the server enforces it.
 */
@Component({
  selector: 'app-modules-page',
  imports: [
    ModuleTextPipe,
    MatCardModule,
    MatTableModule,
    MatFormFieldModule,
    MatSelectModule,
    MatSlideToggleModule,
    PageHeaderComponent,
    LoadingStateComponent,
    EmptyStateComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Modules"
      i18n-title="@@modules.title"
      subtitle="Master portal: Default modules are on for every school unless you switch them off for one; Custom modules are off until you switch them on for a school on the Schools page. An inactive module is off for every school."
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
                <div class="mm-modules__name">{{ module | moduleText }}</div>
                <div class="mm-modules__description">{{ module | moduleText: 'description' }}</div>
              </td>
            </ng-container>

            <ng-container matColumnDef="type">
              <th mat-header-cell *matHeaderCellDef i18n="@@modules.kind">Type</th>
              <td mat-cell *matCellDef="let module">
                <mat-form-field appearance="outline" subscriptSizing="dynamic" class="mm-modules__type">
                  <mat-select
                    aria-label="Type of {{ module | moduleText }}"
                    i18n-aria-label="@@modules.typeOf"
                    [value]="module.type"
                    [disabled]="saving() === module.key"
                    (selectionChange)="save(module, $event.value, module.isActive)"
                  >
                    <mat-option [value]="types.Default" i18n="@@modules.default">Default</mat-option>
                    <mat-option [value]="types.Custom" i18n="@@modules.custom">Custom</mat-option>
                  </mat-select>
                </mat-form-field>
              </td>
            </ng-container>

            <ng-container matColumnDef="isActive">
              <th mat-header-cell *matHeaderCellDef i18n="@@modules.active">Active</th>
              <td mat-cell *matCellDef="let module">
                <mat-slide-toggle
                  [checked]="module.isActive"
                  [disabled]="saving() === module.key"
                  (change)="save(module, module.type, $event.checked)"
                  aria-label="{{ module | moduleText }} is active"
                  i18n-aria-label="@@modules.activeOf"
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

    .mm-modules__type {
      min-width: 160px;
      padding-block: 4px;
    }
  `,
})
export class ModulesPage {
  private readonly client = inject(ModulesClient);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly types = ModuleType;
  protected readonly columns = ['name', 'type', 'isActive'];
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

  /** Saves the module's type and active switch together; on an error the table shows the stored values again. */
  protected save(module: ModuleDto, type: ModuleType, isActive: boolean): void {
    this.saving.set(module.key);
    this.client.updateModule(module.key, new UpdateModuleCommand({ type, isActive })).subscribe({
      next: () => {
        this.saving.set(null);
        this.replace(new ModuleDto({ ...module, type, isActive }));
        this.snackBar.open($localize`:@@modules.saved:${moduleText(module)}:MODULE: saved`, undefined, { duration: 3000 });
      },
      error: (error: unknown) => {
        this.saving.set(null);
        this.replace(new ModuleDto({ ...module }));
        this.snackBar.open(problemMessages(error)[0], undefined, { duration: 5000 });
      },
    });
  }

  private replace(updated: ModuleDto): void {
    this.modules.update((list) => list?.map((m) => (m.key === updated.key ? updated : m)) ?? null);
  }
}
