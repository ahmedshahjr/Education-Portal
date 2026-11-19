import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { Router } from '@angular/router';
import { problemMessages } from '../../core/http/problem-details';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { SchoolRoleDto, SchoolRolesClient } from '../../web-api-client';
import { modulesWithRights } from './role-rights';

/**
 * The school's roles: each gives View, Create, Edit and Delete rights per module. School administrators only, and only
 * while the school has the Users & Roles module (the server checks both).
 */
@Component({
  selector: 'app-roles-page',
  imports: [
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    AppIconComponent,
    PageHeaderComponent,
    LoadingStateComponent,
    EmptyStateComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './roles.page.html',
  styleUrl: './roles.page.css',
})
export class RolesPage {
  private readonly rolesClient = inject(SchoolRolesClient);
  private readonly router = inject(Router);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  protected readonly columns = ['name', 'modules', 'actions'];
  protected readonly roles = signal<readonly SchoolRoleDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);

  constructor() {
    this.load();
  }

  protected load(): void {
    this.loadError.set(null);
    this.rolesClient.getRoles().subscribe({
      next: (roles) => this.roles.set(roles),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected modulesCount(role: SchoolRoleDto): number {
    return modulesWithRights(role);
  }

  protected add(): void {
    void this.router.navigate(['/roles', 'new']);
  }

  protected edit(role: SchoolRoleDto): void {
    void this.router.navigate(['/roles', role.id]);
  }

  protected remove(role: SchoolRoleDto): void {
    const data: ConfirmDialogData = {
      title: $localize`:@@roles.removeTitle:Delete this role?`,
      message: $localize`:@@roles.removeMessage:Staff who have only this role lose the rights it gives.`,
      confirmLabel: $localize`:@@roles.remove:Delete role`,
    };
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed !== true) {
          return;
        }

        this.busy.set(true);
        this.rolesClient.deleteRole(role.id).subscribe({
          next: () => {
            this.busy.set(false);
            this.snackBar.open($localize`:@@roles.removed:Role deleted.`, undefined, { duration: 3000 });
            this.load();
          },
          error: (error: unknown) => {
            this.busy.set(false);
            this.snackBar.open(problemMessages(error)[0], undefined, { duration: 5000 });
          },
        });
      });
  }
}
