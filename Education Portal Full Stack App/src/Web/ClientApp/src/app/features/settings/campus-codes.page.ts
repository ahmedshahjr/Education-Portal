import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar } from '@angular/material/snack-bar';
import { problemMessages } from '../../core/http/problem-details';
import { LoadingStateComponent } from '../../shared/components/loading-state/loading-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { CampusesClient, CampusSettingsDto, UpdateCampusSettingsCommand } from '../../web-api-client';

/** The prefix pattern: letters and digits only, the server allows up to six. */
const PREFIX_PATTERN = /^[a-z0-9]*$/i;

/**
 * Customization, Campus codes: the school's campus codes. A campus has a four-digit code; with the prefix switched on, every
 * code is shown with the prefix in front (for example KHI0002). The codes are shown in the campus table as the school sees them.
 */
@Component({
  selector: 'app-campus-codes-page',
  imports: [ReactiveFormsModule, MatButtonModule, MatCardModule, MatFormFieldModule, MatInputModule, MatSlideToggleModule, PageHeaderComponent, LoadingStateComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './campus-codes.page.html',
  styleUrl: './campus-codes.page.css',
})
export class CampusCodesPage {
  private readonly campusesClient = inject(CampusesClient);
  private readonly formBuilder = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);

  /** The codes as the server holds them, or null while they load. */
  protected readonly settings = signal<CampusSettingsDto | null>(null);
  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  protected readonly form = this.formBuilder.nonNullable.group({
    prefixEnabled: [false],
    prefix: ['', [Validators.maxLength(6), Validators.pattern(PREFIX_PATTERN)]],
  });

  constructor() {
    this.load();
  }

  protected load(): void {
    this.campusesClient.getCampusSettings().subscribe({
      next: (settings) => {
        this.settings.set(settings);
        this.form.reset({ prefixEnabled: settings.prefixEnabled, prefix: settings.prefix ?? '' });
        this.updatePrefixValidation();
      },
      error: (error: unknown) => this.errors.set(problemMessages(error)),
    });
  }

  /** The prefix is required only while it is switched on. */
  protected updatePrefixValidation(): void {
    const prefix = this.form.controls.prefix;
    prefix.setValidators(
      this.form.controls.prefixEnabled.value
        ? [Validators.required, Validators.maxLength(6), Validators.pattern(PREFIX_PATTERN)]
        : [Validators.maxLength(6), Validators.pattern(PREFIX_PATTERN)]
    );
    prefix.updateValueAndValidity();
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.busy.set(true);
    this.errors.set([]);
    this.campusesClient
      .updateCampusSettings(new UpdateCampusSettingsCommand({ prefixEnabled: value.prefixEnabled, prefix: value.prefix.trim().toUpperCase() || undefined }))
      .subscribe({
        next: () => {
          this.busy.set(false);
          this.snackBar.open($localize`:@@campuses.settingsSaved:Campus codes saved.`, undefined, { duration: 3000 });
          this.load();
        },
        error: (error: unknown) => {
          this.busy.set(false);
          this.errors.set(problemMessages(error));
        },
      });
  }
}
