import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Observable } from 'rxjs';
import { problemMessages } from '../../core/http/problem-details';
import { ConfirmDialogComponent, ConfirmDialogData } from '../../shared/components/confirm-dialog/confirm-dialog.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { StatusBadgeComponent } from '../../shared/components/status-badge/status-badge.component';
import {
  BranchDto,
  BranchesClient,
  BranchKind,
  CampusSettingsDto,
  CreateBranchCommand,
  UpdateBranchCommand,
  UpdateCampusSettingsCommand,
} from '../../web-api-client';

/**
 * The school's campuses, for its administrators (the Branches module). Each campus gets a four-digit code when it is added,
 * unique in the school. The school may show a prefix in front of every code. The main campus is the school's registration
 * campus: it cannot be deleted. The server refuses to delete a campus that still has module grants.
 */
@Component({
  selector: 'app-branches-page',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatSlideToggleModule,
    PageHeaderComponent,
    EmptyStateComponent,
    LoadingStateComponent,
    StatusBadgeComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './branches.page.html',
  styleUrl: './branches.page.css',
})
export class BranchesPage {
  private readonly branchesClient = inject(BranchesClient);
  private readonly formBuilder = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);
  private readonly dialog = inject(MatDialog);

  protected readonly branches = signal<readonly BranchDto[] | null>(null);
  protected readonly loadError = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  /** The campus whose row is being renamed, or null. */
  protected readonly editingId = signal<string | null>(null);

  /** The code settings as the server holds them, or null while they load. */
  protected readonly settings = signal<CampusSettingsDto | null>(null);
  protected readonly settingsErrors = signal<readonly string[]>([]);

  protected readonly kinds = BranchKind;

  protected readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
  });

  protected readonly settingsForm = this.formBuilder.nonNullable.group({
    prefixEnabled: [false],
    prefix: ['', [Validators.maxLength(6), Validators.pattern(/^[a-z0-9]*$/i)]],
  });

  constructor() {
    this.load();
    this.loadSettings();
  }

  protected load(): void {
    this.loadError.set(null);
    this.branches.set(null);
    this.editingId.set(null);
    this.branchesClient.getBranches().subscribe({
      next: (branches) => this.branches.set(branches),
      error: (error: unknown) => this.loadError.set(problemMessages(error)[0]),
    });
  }

  protected loadSettings(): void {
    this.branchesClient.getCampusSettings().subscribe({
      next: (settings) => {
        this.settings.set(settings);
        this.settingsForm.reset({ prefixEnabled: settings.prefixEnabled, prefix: settings.prefix ?? '' });
        this.updatePrefixValidation();
      },
      error: (error: unknown) => this.settingsErrors.set(problemMessages(error)),
    });
  }

  /** The prefix is required only while it is switched on. */
  protected updatePrefixValidation(): void {
    const prefix = this.settingsForm.controls.prefix;
    if (this.settingsForm.controls.prefixEnabled.value) {
      prefix.setValidators([Validators.required, Validators.maxLength(6), Validators.pattern(/^[a-z0-9]*$/i)]);
    } else {
      prefix.setValidators([Validators.maxLength(6), Validators.pattern(/^[a-z0-9]*$/i)]);
    }
    prefix.updateValueAndValidity();
  }

  protected saveSettings(): void {
    if (this.settingsForm.invalid) {
      return;
    }
    const value = this.settingsForm.getRawValue();
    this.busy.set(true);
    this.settingsErrors.set([]);
    this.branchesClient
      .updateCampusSettings(new UpdateCampusSettingsCommand({ prefixEnabled: value.prefixEnabled, prefix: value.prefix.trim().toUpperCase() || undefined }))
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.snackBar.open($localize`:@@branches.settingsSaved:Campus codes saved.`, undefined, { duration: 3000 });
          this.loadSettings();
          this.load();
        },
        error: (error: unknown) => {
          this.busy.set(false);
          this.settingsErrors.set(problemMessages(error));
        },
      });
  }

  protected add(): void {
    if (this.form.invalid) {
      return;
    }
    const value = this.form.getRawValue();
    this.busy.set(true);
    this.errors.set([]);
    this.branchesClient.createBranch(new CreateBranchCommand({ name: value.name.trim() })).subscribe({
      next: () => {
        this.busy.set(false);
        this.form.reset({ name: '' });
        this.snackBar.open($localize`:@@branches.added:Campus added.`, undefined, { duration: 3000 });
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }

  protected startEdit(branch: BranchDto): void {
    this.errors.set([]);
    this.editingId.set(branch.id);
    this.form.reset({ name: branch.name });
  }

  protected cancelEdit(): void {
    this.errors.set([]);
    this.editingId.set(null);
  }

  protected saveEdit(branch: BranchDto): void {
    if (this.form.invalid) {
      return;
    }
    const value = this.form.getRawValue();
    this.busy.set(true);
    this.errors.set([]);
    this.branchesClient.updateBranch(branch.id, new UpdateBranchCommand({ name: value.name.trim() })).subscribe({
      next: () => {
        this.busy.set(false);
        this.snackBar.open($localize`:@@branches.updated:Campus saved.`, undefined, { duration: 3000 });
        this.load();
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }

  /** Asks first: a deleted campus disappears for everyone; its data is kept. */
  protected confirmDelete(branch: BranchDto): void {
    const data: ConfirmDialogData = {
      title: $localize`:@@branches.deleteTitle:Delete this campus?`,
      message: $localize`:@@branches.deleteMessage:The campus disappears from the school. Its data is kept. A campus with module grants must have them turned off first.`,
      confirmLabel: $localize`:@@branches.delete:Delete`,
    };
    this.dialog
      .open(ConfirmDialogComponent, { data, autoFocus: 'dialog' })
      .afterClosed()
      .subscribe((confirmed: boolean | undefined) => {
        if (confirmed === true) {
          this.run(this.branchesClient.deleteBranch(branch.id), $localize`:@@branches.deleted:Campus deleted.`);
        }
      });
  }

  private run(request: Observable<unknown>, done: string): void {
    this.busy.set(true);
    this.errors.set([]);
    request.subscribe({
      next: () => {
        this.busy.set(false);
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
