import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { ModuleTextPipe } from '../../core/i18n/module-text.pipe';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import {
  CreateRoleCommand,
  ModuleDto,
  ModulesClient,
  RolePermissionDto,
  SchoolRoleDto,
  SchoolRolesClient,
} from '../../web-api-client';

/** The four rights a role gives per module. */
type Right = 'canView' | 'canCreate' | 'canEdit' | 'canDelete';

/**
 * The school's roles and what each one may do in each of its modules (View, Create, Edit, Delete). Only the modules the school
 * has are offered; the server refuses a right in a module that is off.
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
    LoadingStateComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './roles.page.html',
  styleUrl: './roles.page.css',
})
export class RolesPage {
  private readonly rolesClient = inject(SchoolRolesClient);
  private readonly modulesClient = inject(ModulesClient);
  private readonly formBuilder = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  protected readonly roles = signal<readonly SchoolRoleDto[] | null>(null);
  protected readonly modules = signal<readonly ModuleDto[]>([]);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  protected readonly rights: readonly { key: Right; label: string }[] = [
    { key: 'canView', label: $localize`:@@roles.right.view:View` },
    { key: 'canCreate', label: $localize`:@@roles.right.create:Create` },
    { key: 'canEdit', label: $localize`:@@roles.right.edit:Edit` },
    { key: 'canDelete', label: $localize`:@@roles.right.delete:Delete` },
  ];

  /** One checkbox per (module, right) of the new role. */
  protected readonly grants = new Map<string, FormControl<boolean>>();

  protected readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
  });

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

  protected add(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const permissions = this.modules()
      .map((m) =>
        new RolePermissionDto({
          module: m.code,
          canView: this.grant(m.code, 'canView').value,
          canCreate: this.grant(m.code, 'canCreate').value,
          canEdit: this.grant(m.code, 'canEdit').value,
          canDelete: this.grant(m.code, 'canDelete').value,
        })
      )
      .filter((p) => p.canView || p.canCreate || p.canEdit || p.canDelete);

    this.run(
      this.rolesClient.createRole(new CreateRoleCommand({ name: this.form.getRawValue().name.trim(), permissions })),
      $localize`:@@roles.added:Role added.`,
      () => {
        this.form.reset({ name: '' });
        this.resetGrants();
      }
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

  /** The rights of a role in one module, as text for the list. */
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
        this.snackBar.open(done, undefined, { duration: 3000 });
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
