import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { moduleText } from '../../core/i18n/module-text.pipe';
import { ModulesService } from '../../core/modules/modules.service';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { CreateRoleCommand, SchoolRolesClient, UpdateRoleCommand } from '../../web-api-client';
import { NO_RIGHTS, orderedModuleCodes, RightFlags, rightsOf, RightName, toPermissions, withRight } from './role-rights';

/**
 * One role: its name and the View, Create, Edit and Delete rights in each module the school has. Used for a new role
 * (`/roles/new`) and for a role that exists (`/roles/:roleId`). Rights in a module the school no longer has are kept on
 * the server, so this page only sends the modules it shows.
 */
@Component({
  selector: 'app-role-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
    PageHeaderComponent,
    LoadingStateComponent,
    EmptyStateComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './role.page.html',
  styleUrl: './role.page.css',
})
export class RolePage {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly rolesClient = inject(SchoolRolesClient);
  private readonly modulesService = inject(ModulesService);
  private readonly snackBar = inject(MatSnackBar);

  /** The role's public id from the route, or null for a new role. */
  protected readonly roleId = this.route.snapshot.paramMap.get('roleId');
  protected readonly isNew = this.roleId === null;

  /** The modules the school has, in the portal's order: one row each. */
  protected readonly modules = computed(() => orderedModuleCodes(this.modulesService.enabled()));
  protected readonly rights = signal<Record<string, RightFlags>>({});
  protected readonly roleName = signal('');

  protected readonly newTitle = $localize`:@@roles.newTitle:New role`;
  protected readonly newSubtitle = $localize`:@@roles.newSubtitle:Choose what this role may view, create, edit and delete in each module.`;
  protected readonly editSubtitle = $localize`:@@roles.editSubtitle:Change the rights this role gives in each module.`;
  protected readonly rolesLabel = $localize`:@@roles.breadcrumb:Roles`;
  protected readonly ready = signal(false);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
  });

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    if (this.roleId === null) {
      this.rights.set(rightsOf(null, this.modules()));
      this.ready.set(true);
      return;
    }

    const roleId = this.roleId;
    this.rolesClient.getRoles().subscribe({
      next: (roles) => {
        const role = roles.find((r) => r.id === roleId) ?? null;
        if (role === null) {
          this.loadError.set($localize`:@@roles.notFound:This role was not found. It may have been deleted.`);
          return;
        }

        this.form.patchValue({ name: role.name });
        this.roleName.set(role.name);
        this.rights.set(rightsOf(role, this.modules()));
        this.ready.set(true);
      },
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected backToList(): void {
    void this.router.navigate(['/roles']);
  }

  /** The name of a module by its code, in the page's language. */
  protected moduleName(code: string): string {
    return moduleText({ code });
  }

  protected rightsFor(code: string): RightFlags {
    return this.rights()[code] ?? NO_RIGHTS;
  }

  protected setRight(code: string, name: RightName, value: boolean): void {
    this.rights.update((current) => ({ ...current, [code]: withRight(current[code] ?? NO_RIGHTS, name, value) }));
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const name = this.form.controls.name.value.trim();
    const permissions = toPermissions(this.rights(), this.modules());
    const request: Observable<unknown> = this.roleId === null
      ? this.rolesClient.createRole(new CreateRoleCommand({ name, permissions }))
      : this.rolesClient.updateRole(this.roleId, new UpdateRoleCommand({ name, permissions }));

    this.busy.set(true);
    this.errors.set([]);
    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.snackBar.open($localize`:@@roles.saved:Role saved.`, undefined, { duration: 3000 });
        void this.router.navigate(['/roles']);
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
