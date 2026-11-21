import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { problemMessages } from '../../core/http/problem-details';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { ToastService } from '../../shared/services/toast.service';
import { CreateStudentAdmissionCommand, Gender, StudentAdmissionsClient } from '../../web-api-client';

/** The limits the server checks (StudentProfile), repeated here so the form says what is wrong before it is sent. */
export const NAME_MAX_LENGTH = 100;
export const FATHER_NAME_MAX_LENGTH = 100;
export const NATIONALITY_MAX_LENGTH = 60;
export const ADDRESS_MAX_LENGTH = 300;
export const CITY_MAX_LENGTH = 100;
export const CLASS_MAX_LENGTH = 50;
export const PREVIOUS_SCHOOL_MAX_LENGTH = 200;

/** The label of a gender, in the page's language. */
export function genderLabel(gender: Gender): string {
  switch (gender) {
    case Gender.Male:
      return $localize`:@@admission.genderMale:Male`;
    case Gender.Female:
      return $localize`:@@admission.genderFemale:Female`;
    case Gender.Other:
      return $localize`:@@admission.genderOther:Other`;
  }
}

/**
 * Admits a new student (Students module): the child's details, the guardian's phone and the class applied for. Saving creates the
 * student's record; the server checks every field again, including the child's age on the admission date.
 */
@Component({
  selector: 'app-student-admission-page',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    PageHeaderComponent,
  ],
  providers: [provideNativeDateAdapter()],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './student-admission.page.html',
  styleUrl: './academics.css',
})
export class StudentAdmissionPage {
  private readonly client = inject(StudentAdmissionsClient);
  private readonly toast = inject(ToastService);

  protected readonly genders = Object.values(Gender);
  protected readonly genderLabel = genderLabel;
  protected readonly today = new Date();

  protected readonly form = inject(FormBuilder).group({
    firstName: ['', [Validators.required, Validators.maxLength(NAME_MAX_LENGTH)]],
    middleName: ['', Validators.maxLength(NAME_MAX_LENGTH)],
    lastName: ['', [Validators.required, Validators.maxLength(NAME_MAX_LENGTH)]],
    gender: [null as Gender | null, Validators.required],
    dateOfBirth: [null as Date | null, Validators.required],
    phone: [''],
    fatherName: ['', [Validators.required, Validators.maxLength(FATHER_NAME_MAX_LENGTH)]],
    guardianPhone: ['', Validators.required],
    nationality: ['', [Validators.required, Validators.maxLength(NATIONALITY_MAX_LENGTH)]],
    address: ['', [Validators.required, Validators.maxLength(ADDRESS_MAX_LENGTH)]],
    city: ['', [Validators.required, Validators.maxLength(CITY_MAX_LENGTH)]],
    classApplied: ['', [Validators.required, Validators.maxLength(CLASS_MAX_LENGTH)]],
    previousSchool: ['', Validators.maxLength(PREVIOUS_SCHOOL_MAX_LENGTH)],
    admissionDate: [new Date() as Date | null, Validators.required],
  });

  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const command = new CreateStudentAdmissionCommand({
      firstName: value.firstName?.trim() ?? '',
      middleName: value.middleName?.trim() || undefined,
      lastName: value.lastName?.trim() ?? '',
      phone: value.phone?.trim() || undefined,
      dateOfBirth: value.dateOfBirth ?? undefined,
      gender: value.gender ?? undefined,
      fatherName: value.fatherName?.trim() ?? '',
      nationality: value.nationality?.trim() ?? '',
      address: value.address?.trim() ?? '',
      city: value.city?.trim() ?? '',
      classApplied: value.classApplied?.trim() ?? '',
      previousSchool: value.previousSchool?.trim() || undefined,
      guardianPhone: value.guardianPhone?.trim() ?? '',
      admissionDate: value.admissionDate ?? undefined,
    });

    this.busy.set(true);
    this.errors.set([]);
    this.client.createStudentAdmission(command).subscribe({
      next: () => {
        this.busy.set(false);
        this.toast.success($localize`:@@admission.saved:Student admitted.`);
        this.form.reset({ admissionDate: new Date() });
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }
}
