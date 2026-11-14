import { ChangeDetectionStrategy, Component, effect, inject, input, output, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { problemMessages } from '../../core/http/problem-details';
import { SchoolsClient, SchoolSummaryDto, UpdateSchoolCommand } from '../../web-api-client';

/** A host name such as school1.com or localhost, the same shape the server accepts (School.IsValidDomain). */
const HOST_NAME = /^[a-z0-9]([a-z0-9-]*[a-z0-9])?(\.[a-z0-9]([a-z0-9-]*[a-z0-9])?)*$/i;

/**
 * Master portal: a school's name and the domain it is reached on. The domain of the master tenant cannot change, so its
 * field is locked. The server checks the domain is free and refuses a bad one; its messages are shown here.
 */
@Component({
  selector: 'app-school-profile',
  imports: [ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './school-profile.component.html',
  styleUrl: './school-profile.component.css',
})
export class SchoolProfileComponent {
  private readonly schools = inject(SchoolsClient);
  private readonly formBuilder = inject(FormBuilder);
  private readonly snackBar = inject(MatSnackBar);

  readonly school = input.required<SchoolSummaryDto>();

  /** Raised with the school as the server now holds it, after a save. */
  readonly saved = output<SchoolSummaryDto>();

  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  protected readonly form = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(200)]],
    domain: ['', [Validators.required, Validators.maxLength(253), Validators.pattern(HOST_NAME)]],
  });

  constructor() {
    effect(() => {
      const school = this.school();
      this.reset(school);
      if (school.isMaster) {
        this.form.controls.domain.disable();
      } else {
        this.form.controls.domain.enable();
      }
    });
  }

  protected save(): void {
    if (this.form.invalid) {
      return;
    }
    const value = this.form.getRawValue();
    const name = value.name.trim();
    const domain = value.domain.trim().toLowerCase();

    this.busy.set(true);
    this.errors.set([]);
    this.schools.updateSchool(this.school().id, new UpdateSchoolCommand({ name, domain })).subscribe({
      next: () => {
        this.busy.set(false);
        const updated = new SchoolSummaryDto({ ...this.school(), name, domain });
        this.snackBar.open($localize`:@@profile.saved:School saved.`, undefined, { duration: 3000 });
        this.saved.emit(updated);
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }

  /** Puts back the values the school has now. */
  protected cancel(): void {
    this.errors.set([]);
    this.reset(this.school());
  }

  private reset(school: SchoolSummaryDto): void {
    this.form.reset({ name: school.name, domain: school.domain });
  }
}
