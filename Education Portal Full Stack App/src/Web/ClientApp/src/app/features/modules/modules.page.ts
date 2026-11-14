import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { problemMessages } from '../../core/http/problem-details';
import { moduleText, ModuleTextPipe } from '../../core/i18n/module-text.pipe';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import { ModuleDto, ModulesClient, UpdateModuleCommand } from '../../web-api-client';

/**
 * Master portal: the module catalogue, what the platform sells. Core modules are in every subscription and always on;
 * paid modules are on only for the schools that get them. The Active switch turns a paid module off for every school.
 * Give paid modules to a school on the Schools page. Master tenant administrators only; the server enforces it.
 */
@Component({
  selector: 'app-modules-page',
  imports: [
    ModuleTextPipe,
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
      subtitle="Master portal: the modules the platform sells. Core modules are in every subscription and always on; paid modules are on only for the schools that get them. An inactive module is off for every school."
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
                <div class="mm-modules__name">{{ module | moduleText }} <code class="mm-modules__code">{{ module.code }}</code></div>
                <div class="mm-modules__description">{{ module | moduleText: 'description' }}</div>
              </td>
            </ng-container>

            <ng-container matColumnDef="kind">
              <th mat-header-cell *matHeaderCellDef i18n="@@modules.kind">Kind</th>
              <td mat-cell *matCellDef="let module">
                @if (module.isCore) {
                  <app-status-badge tone="success" i18n="@@modules.core">Core</app-status-badge>
                } @else {
                  <app-status-badge tone="warning" i18n="@@modules.paid">Paid</app-status-badge>
                }
              </td>
            </ng-container>

            <ng-container matColumnDef="requires">
              <th mat-header-cell *matHeaderCellDef i18n="@@modules.requires">Requires</th>
              <td mat-cell *matCellDef="let module">
                @for (code of module.requires; track code; let last = $last) {
                  {{ nameOf(code) }}@if (!last) {, }
                } @empty {
                  <span class="mm-modules__description" i18n="@@modules.requiresNothing">Nothing</span>
                }
              </td>
            </ng-container>

            <ng-container matColumnDef="isActive">
              <th mat-header-cell *matHeaderCellDef i18n="@@modules.active">Active</th>
              <td mat-cell *matCellDef="let module">
                <mat-slide-toggle
                  [checked]="module.isActive"
                  [disabled]="module.isCore || saving() === module.code"
                  (change)="setActive(module, $event.checked)"
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

    .mm-modules__code {
      margin-inline-start: var(--mm-space-2);
      color: var(--mm-text-muted);
      font-size: var(--mm-text-xs);
      font-weight: 400;
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

  protected readonly columns = ['name', 'kind', 'requires', 'isActive'];
  protected readonly modules = signal<readonly ModuleDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly saving = signal<string | null>(null);

  private readonly byCode = computed(() => new Map((this.modules() ?? []).map((m) => [m.code, m])));

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

  /** A required module's name in the page's language. */
  protected nameOf(code: string): string {
    return moduleText(this.byCode().get(code) ?? { code, name: code });
  }

  /** Saves the Active switch with the module's other fields unchanged; on an error the table shows the stored value again. */
  protected setActive(module: ModuleDto, isActive: boolean): void {
    this.saving.set(module.code);
    const command = new UpdateModuleCommand({
      name: module.name,
      description: module.description,
      isCore: module.isCore,
      sortOrder: module.sortOrder,
      isActive,
    });
    this.client.updateModule(module.code, command).subscribe({
      next: () => {
        this.saving.set(null);
        this.replace(new ModuleDto({ ...module, isActive }));
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
    this.modules.update((list) => list?.map((m) => (m.code === updated.code ? updated : m)) ?? null);
  }
}
