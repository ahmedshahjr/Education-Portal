import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { problemMessages } from '../../core/http/problem-details';
import { ModuleTextPipe } from '../../core/i18n/module-text.pipe';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { ToastService } from '../../shared/services/toast.service';
import { CampusesClient, CreateRoleCommand, ModuleCode, ModulesClient, SchoolRoleDto, SchoolRolesClient, UpdateRoleCommand } from '../../web-api-client';
import { RoleFormBase } from './role-form.base';

/**
 * A role on its own page: `/roles/new` adds one, `/roles/:id` changes one. A role's rights are a grid with a row per module,
 * which is too large for a dialog (ADR-024), so both go to a page. Saving returns to the list.
 */
@Component({
  selector: 'app-role-page',
  imports: [ReactiveFormsModule, RouterLink, ModuleTextPipe, MatButtonModule, MatFormFieldModule, MatInputModule, PageHeaderComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './role-page.component.html',
  styleUrl: './role-dialog.component.css',
})
export class RolePageComponent extends RoleFormBase {
  private readonly rolesClient = inject(SchoolRolesClient);
  private readonly modulesClient = inject(ModulesClient);
  private readonly campusesClient = inject(CampusesClient);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected readonly busy = signal(false);
  protected readonly loading = signal(true);
  protected readonly errors = signal<readonly string[]>([]);
  protected readonly loadError = signal<string | null>(null);

  /** The role id from the URL, or null when adding. */
  private readonly roleId = this.route.snapshot.paramMap.get('id');

  constructor() {
    super({
      canView: $localize`:@@roles.right.view:View`,
      canCreate: $localize`:@@roles.right.create:Create`,
      canEdit: $localize`:@@roles.right.edit:Edit`,
      canDelete: $localize`:@@roles.right.delete:Delete`,
      canApprove: $localize`:@@roles.right.approve:Approve`,
      canExport: $localize`:@@roles.right.export:Export`,
    });

    const roleId = this.roleId;
    const editing = roleId !== null && roleId !== 'new';

    forkJoin({
      codes: this.modulesClient.getEnabledModules(),
      campuses: this.campusesClient.getCampuses(),
      role: editing
        ? this.rolesClient.getRoles().pipe(map((roles) => roles.find((r) => r.id === roleId) ?? null))
        : of<SchoolRoleDto | null>(null),
    })
      .pipe(catchError((error: unknown) => {
        this.loadError.set(problemMessages(error)[0]);
        this.loading.set(false);
        return of(null);
      }))
      .subscribe((loaded) => {
        if (!loaded) {
          return;
        }
        if (editing && loaded.role === null) {
          this.loadError.set($localize`:@@roles.notFound:This role was not found.`);
        }
        this.applyRole(loaded.role, loaded.codes.map((code: ModuleCode) => ({ code })), loaded.campuses);
        this.loading.set(false);
      });
  }

  /** True when a role is being changed, not added. */
  protected get editing(): boolean {
    return this.role !== null;
  }

  protected save(): void {
    const result = this.buildResult();
    if (!result) {
      return;
    }

    this.busy.set(true);
    this.errors.set([]);
    const fields = { name: result.name, description: result.description, permissions: result.permissions };
    const request: Observable<unknown> = this.role
      ? this.rolesClient.updateRole(this.role.id, new UpdateRoleCommand({ ...fields, id: this.role.id }))
      : this.rolesClient.createRole(new CreateRoleCommand(fields));

    request.subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success(this.role ? $localize`:@@roles.saved:Role saved.` : $localize`:@@roles.added:Role added.`);
        this.router.navigate(['/roles']);
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
