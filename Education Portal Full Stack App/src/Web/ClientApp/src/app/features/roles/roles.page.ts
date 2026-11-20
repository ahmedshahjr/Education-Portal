import { ChangeDetectionStrategy, Component, computed, inject, signal, TemplateRef, viewChild } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
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
  RolePermissionDto,
  SchoolRoleDto,
  SchoolRolesClient,
  UpdateRoleCommand,
} from '../../web-api-client';
import { RIGHT_KEYS, Right, rolePermissions } from './role-rights';

/**
 * The school's roles and what each one may do in each of its modules (View, Create, Edit, Delete, Approve, Export), as a
 * table with one column per module. Only the modules the school has are offered; the server refuses a right in a module
 * that is off. The form below the table adds a role or changes the one being edited. A built-in role keeps its name and
 * cannot be deleted, so its name field is locked and it has no delete button.
 */
@Component({
  selector: 'app-roles-page',
  imports: [
    ReactiveFormsModule,
    ModuleTextPipe,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
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
  private readonly formBuilder = inject(FormBuilder);
  private readonly toast = inject(ToastService);
  private readonly dialog = inject(MatDialog);

  protected readonly roles = signal<readonly SchoolRoleDto[] | null>(null);
  protected readonly modules = signal<readonly ModuleDto[]>([]);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  /** The role whose rights are in the form, or null while a new role is being added. */
  protected readonly editing = signal<SchoolRoleDto | null>(null);

  private readonly rightLabels: Record<Right, string> = {
    canView: $localize`:@@roles.right.view:View`,
    canCreate: $localize`:@@roles.right.create:Create`,
    canEdit: $localize`:@@roles.right.edit:Edit`,
    canDelete: $localize`:@@roles.right.delete:Delete`,
    canApprove: $localize`:@@roles.right.approve:Approve`,
    canExport: $localize`:@@roles.right.export:Export`,
  };

  protected readonly rights: readonly { key: Right; label: string }[] = RIGHT_KEYS.map((key) => ({ key, label: this.rightLabels[key] }));

  /** One checkbox per (module, right) of the new role. */
  protected readonly grants = new Map<string, FormControl<boolean>>();

  protected readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.maxLength(500)]],
  });

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
            this.resetGrants();
          },
        });
      },
    });
  }

  protected grant(module: string, right: Right): FormControl<boolean> {
    const key = `${module}:${right}`;
    let control = this.grants.get(key);
    if (!control) {
      control = this.formBuilder.nonNullable.control(false);
      this.grants.set(key, control);
    }
    return control;
  }

  /** Adds the role in the form, or saves the role being edited. */
  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const permissions = rolePermissions(
      this.modules().map((m) => m.code),
      (module, right) => this.grant(module, right).value
    ).map((p) => new RolePermissionDto(p));
    const fields = { name: value.name.trim(), description: value.description.trim() || undefined, permissions };
    const editing = this.editing();

    this.run(
      editing
        ? this.rolesClient.updateRole(editing.id, new UpdateRoleCommand({ ...fields, id: editing.id }))
        : this.rolesClient.createRole(new CreateRoleCommand(fields)),
      editing ? $localize`:@@roles.saved:Role saved.` : $localize`:@@roles.added:Role added.`,
      () => this.cancelEdit()
    );
  }

  /** Puts a role's name, description and rights into the form to change them. A built-in role's name is locked. */
  protected startEdit(role: SchoolRoleDto): void {
    this.editing.set(role);
    this.errors.set([]);
    this.form.reset({ name: role.name, description: role.description ?? '' });
    if (role.isSystem) {
      this.form.controls.name.disable();
    } else {
      this.form.controls.name.enable();
    }
    for (const module of this.modules()) {
      const permission = role.permissions.find((p) => p.module === module.code);
      for (const right of RIGHT_KEYS) {
        this.grant(module.code, right).setValue(permission?.[right] ?? false);
      }
    }
  }

  /** Leaves editing: the form adds a new role again. */
  protected cancelEdit(): void {
    this.editing.set(null);
    this.form.controls.name.enable();
    this.form.reset({ name: '', description: '' });
    this.resetGrants();
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

  private resetGrants(): void {
    this.grants.clear();
    for (const module of this.modules()) {
      for (const right of this.rights) {
        this.grant(module.code, right.key);
      }
    }
  }

  private run(request: Observable<unknown>, done: string, onSuccess?: () => void): void {
    this.busy.set(true);
    this.errors.set([]);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        onSuccess?.();
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
