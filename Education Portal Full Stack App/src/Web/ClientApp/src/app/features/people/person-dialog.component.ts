import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { CampusDto, SchoolRoleDto, UserDetailsDto, UserType } from '../../web-api-client';
import { buildPersonChanges, needsAdmissionDetails, PersonChanges } from './person-changes';

export interface PersonDialogData {
  /** The person being changed, or null when a person is added. */
  readonly person: UserDetailsDto | null;
  /** The school's students, to choose a parent's children from. */
  readonly students: readonly UserDetailsDto[];
  /** The school's roles, to choose the account's roles from. */
  readonly roles: readonly SchoolRoleDto[];
  /** The students the parent is linked to now (empty for a new person). */
  readonly currentChildIds: readonly string[];
  /** The roles the person's account holds now (empty for a new person). */
  readonly currentRoleIds: readonly string[];
  /** The school's campuses, to choose where the person works from. */
  readonly campuses: readonly CampusDto[];
  /** The campuses the person is assigned to now (empty for a new person). */
  readonly currentCampusIds: readonly string[];
}

/** What the modal returns: the person's fields, the children a parent should have, the roles of the account and the campuses. */
export interface PersonDialogResult {
  readonly fields: PersonChanges;
  readonly childIds: readonly string[];
  readonly roleIds: readonly string[];
  readonly campusIds: readonly string[];
}

/**
 * Adds a person or changes one. Adding also creates the person's login: an email and a one-time password, which the
 * person replaces on first sign-in. Every type gets a login, as the maintainer decided for the people plan. A parent's
 * children, every account's roles and the campuses the person works at are chosen here too; the page saves them after
 * the person. Someone who is not an administrator acts only on their campuses.
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
  private readonly dialogRef = inject<MatDialogRef<PersonDialogComponent, PersonDialogResult>>(MatDialogRef);

  protected readonly person = this.data.person;
  protected readonly adding = this.person === null;
  protected readonly students = this.data.students;
  protected readonly roles = this.data.roles;
  protected readonly campuses = this.data.campuses;
  protected readonly types = [UserType.Principal, UserType.Teacher, UserType.Staff, UserType.Student, UserType.Parent];

  protected readonly form = inject(FormBuilder).nonNullable.group({
    firstName: [this.person?.firstName ?? '', [Validators.required, Validators.maxLength(100)]],
    middleName: [this.person?.middleName ?? '', [Validators.maxLength(100)]],
    lastName: [this.person?.lastName ?? '', [Validators.required, Validators.maxLength(100)]],
    userType: [this.person?.userType ?? UserType.Teacher, [Validators.required]],
    phone: [this.person?.phone ?? '', [Validators.maxLength(30)]],
    email: ['', [Validators.maxLength(256)]],
    oneTimePassword: ['', [Validators.maxLength(128)]],
    children: [[...this.data.currentChildIds] as string[]],
    roles: [[...this.data.currentRoleIds] as string[]],
    campuses: [[...this.data.currentCampusIds] as string[]],
  });

  /** The type chosen, so the student note and the children picker follow the selection. */
  protected readonly selectedType = signal<UserType>(this.form.controls.userType.value);
  protected readonly showAdmissionNote = computed(() => needsAdmissionDetails(this.selectedType()));
  protected readonly showChildren = computed(() => this.selectedType() === UserType.Parent);

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

    const value = this.form.getRawValue();
    this.dialogRef.close({
      fields: buildPersonChanges(value, this.adding),
      childIds: value.userType === UserType.Parent ? value.children : [],
      roleIds: value.roles,
      campusIds: value.campuses,
    });
  }
}
