import { ChangeDetectionStrategy, Component, computed, inject, signal, TemplateRef, viewChild } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { ToastService } from '../../shared/services/toast.service';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { moduleText, ModuleTextPipe } from '../../core/i18n/module-text.pipe';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { DataTableCellContext, DataTableColumn, DataTableComponent } from '../../shared/components/data-table/data-table.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import {
  CreateRoleCommand,
  ModuleDto,
  ModulesClient,
  SchoolRoleDto,
  SchoolRolesClient,
  UpdateRoleCommand,
} from '../../web-api-client';
import { Right, RIGHT_KEYS } from './role-rights';
import { RoleDialogComponent, RoleDialogData, RoleDialogResult } from './role-dialog.component';

/**
 * The school's roles and what each one may do in each of its modules, as a table with one column per module. Adding and
 * changing a role open a dialog (small entities, ADR-024). A built-in role keeps its name and cannot be deleted, so it has
 * no delete button. Only the modules the school has are offered; the server refuses a right in a module that is off.
 */
@Component({
  selector: 'app-roles-page',
  imports: [
    ModuleTextPipe,
    MatButtonModule,
    MatCardModule,
    PageHeaderComponent,
    EmptyStateComponent,
    AppIconComponent,
    DataTableComponent,
    StatusBadgeComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './roles.page.html',
  styleUrl: './roles.page.css',
})
export class RolesPage {
  private readonly rolesClient = inject(SchoolRolesClient);
  private readonly modulesClient = inject(ModulesClient);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);

  protected readonly roles = signal<readonly SchoolRoleDto[] | null>(null);
  protected readonly modules = signal<readonly ModuleDto[]>([]);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  private readonly rightLabels: Record<Right, string> = {
    canView: $localize`:@@roles.right.view:View`,
    canCreate: $localize`:@@roles.right.create:Create`,
    canEdit: $localize`:@@roles.right.edit:Edit`,
    canDelete: $localize`:@@roles.right.delete:Delete`,
    canApprove: $localize`:@@roles.right.approve:Approve`,
    canExport: $localize`:@@roles.right.export:Export`,
  };

  protected readonly rights: readonly { key: Right; label: string }[] = RIGHT_KEYS.map((key) => ({ key, label: this.rightLabels[key] }));

  private readonly actionsCell = viewChild.required<TemplateRef<DataTableCellContext<SchoolRoleDto>>>('actionsCell');
  private readonly nameCell = viewChild.required<TemplateRef<DataTableCellContext<SchoolRoleDto>>>('nameCell');

  /** The name column, then one column per module of the school, then the actions. */
  protected readonly columns = computed<DataTableColumn<SchoolRoleDto>[]>(() => [
    { key: 'name', header: $localize`:@@roles.colName:Role`, valueFn: (r) => r.name, cell: this.nameCell() },
    ...this.modules().map<DataTableColumn<SchoolRoleDto>>((module) => ({
      key: `module:${module.code}`,
      header: moduleText(module),
      valueFn: (role) => this.rightsOf(role, module.code),
    })),
    { key: 'actions', header: $localize`:@@common.actions:Actions`, sortable: false, align: 'right', cell: this.actionsCell() },
  ]);

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    this.roles.set(null);
    this.rolesClient.getRoles().subscribe({
      next: (roles) => this.roles.set(roles),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
    this.modulesClient.getEnabledModules().subscribe({
      next: (codes) => {
        this.modulesClient.getModules().subscribe({
          next: (modules) => {
            const enabled = new Set(codes);
            this.modules.set(modules.filter((m) => enabled.has(m.code)));
          },
        });
      },
    });
  }

  /** Opens the role dialog: a new role, or the role in the row. */
  protected openDialog(role: SchoolRoleDto | null): void {
    this.errors.set([]);
    const data: RoleDialogData = { role, modules: this.modules() };
    this.dialog
      .open<RoleDialogComponent, RoleDialogData, RoleDialogResult>(RoleDialogComponent, { data, width: '720px', autoFocus: 'first-tabbable' })
      .afterClosed()
      .subscribe((result) => {
        if (result) {
          this.saveRole(role, result);
        }
      });
  }

  /** Adds the role, or saves the changes to the role it was opened for. */
  private saveRole(role: SchoolRoleDto | null, result: RoleDialogResult): void {
    const fields = { name: result.name, description: result.description, permissions: result.permissions };

    this.run(
      role
        ? this.rolesClient.updateRole(role.id, new UpdateRoleCommand({ ...fields, id: role.id }))
        : this.rolesClient.createRole(new CreateRoleCommand(fields)),
      role ? $localize`:@@roles.saved:Role saved.` : $localize`:@@roles.added:Role added.`
    );
  }

  protected confirmDelete(role: SchoolRoleDto): void {
    const data: ConfirmDialogData = {
      title: $localize`:@@roles.deleteTitle:Delete this role?`,
      message: $localize`:@@roles.deleteMessage:The staff who had it lose its rights at once.`,
      confirmLabel: $localize`:@@roles.delete:Delete`,
    };
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed === true) {
          this.run(this.rolesClient.deleteRole(role.id), $localize`:@@roles.deleted:Role deleted.`);
        }
      });
  }

  /** The rights of a role in one module, as text for the table. */
  protected rightsOf(role: SchoolRoleDto, module: string): string {
    const permission = role.permissions.find((p) => p.module === module);
    if (!permission) {
      return '—';
    }
    const names = this.rights.filter((r) => permission[r.key]).map((r) => r.label);
    return names.length > 0 ? names.join(', ') : '—';
  }

  private run(request: Observable<unknown>, done: string): void {
    this.busy.set(true);
    this.errors.set([]);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success(done);
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
