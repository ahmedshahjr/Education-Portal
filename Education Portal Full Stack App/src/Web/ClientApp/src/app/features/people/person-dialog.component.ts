import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { UserDetailsDto, UserType } from '../../web-api-client';
import { buildPersonChanges, needsAdmissionDetails, PersonChanges } from './person-changes';

export interface PersonDialogData {
  /** The person being changed, or null when a person is added. */
  readonly person: UserDetailsDto | null;
}

/**
 * Adds a person or changes one. Adding also creates the person's login: an email and a one-time password, which the
 * person replaces on first sign-in. Every type gets a login, as the maintainer decided for the people plan.
 */
@Component({
  selector: 'app-person-dialog',
  imports: [ReactiveFormsModule, MatDialogModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './person-dialog.component.html',
  styleUrl: './person-dialog.component.css',
})
export class PersonDialogComponent {
  private readonly data = inject<PersonDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject<MatDialogRef<PersonDialogComponent, PersonChanges>>(MatDialogRef);

  protected readonly person = this.data.person;
  protected readonly adding = this.person === null;
  protected readonly types = [UserType.Principal, UserType.Teacher, UserType.Staff, UserType.Student];

  protected readonly form = inject(FormBuilder).nonNullable.group({
    firstName: [this.person?.firstName ?? '', [Validators.required, Validators.maxLength(100)]],
    middleName: [this.person?.middleName ?? '', [Validators.maxLength(100)]],
    lastName: [this.person?.lastName ?? '', [Validators.required, Validators.maxLength(100)]],
    userType: [this.person?.userType ?? UserType.Teacher, [Validators.required]],
    phone: [this.person?.phone ?? '', [Validators.maxLength(30)]],
    email: ['', [Validators.maxLength(256)]],
    oneTimePassword: ['', [Validators.maxLength(128)]],
  });

  /** The type chosen, so the student note follows the selection. */
  protected readonly selectedType = signal<UserType>(this.form.controls.userType.value);
  protected readonly showAdmissionNote = computed(() => needsAdmissionDetails(this.selectedType()));

  constructor() {
    if (this.adding) {
      // Adding a person creates their login, so the email and the one-time password are required.
      this.form.controls.email.setValidators([Validators.required, Validators.email, Validators.maxLength(256)]);
      this.form.controls.oneTimePassword.setValidators([Validators.required, Validators.minLength(15), Validators.maxLength(128)]);
      this.form.controls.email.updateValueAndValidity();
      this.form.controls.oneTimePassword.updateValueAndValidity();
    }

    this.form.controls.userType.valueChanges.subscribe((type) => this.selectedType.set(type));
  }

  protected save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.dialogRef.close(buildPersonChanges(this.form.getRawValue(), this.adding));
  }
}
