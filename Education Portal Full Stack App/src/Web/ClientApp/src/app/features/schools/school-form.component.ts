import { ChangeDetectionStrategy, Component, inject, input, output } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { CountryDto, DataRegion } from '../../web-api-client';

/** Limits shared with the server (School.NameMaxLength, PasswordRules); the server checks everything again. */
export const SCHOOL_NAME_MAX_LENGTH = 200;
export const PASSWORD_MIN_LENGTH = 15;
export const PASSWORD_MAX_LENGTH = 128;
const GENERATED_PASSWORD_LENGTH = 20;

/** A host name such as school1.com: letters, digits and hyphens in dot-separated labels, no scheme, port or path. */
const DOMAIN_PATTERN = /^(?=.{1,253}\.?$)(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*\.?$/;

/** What the form asks to create. */
export interface NewSchool {
  readonly name: string;
  readonly domain: string;
  readonly countryId: string | null;
  readonly dataRegion: DataRegion | null;
  readonly administratorEmail: string;
  readonly administratorPassword: string;
}

/**
 * Adds a school with its domain, country, data region and first administrator. The administrator gets a one-time
 * password from the master tenant and must replace it at first sign-in. The checks mirror the server's (SEC-INPUT-01),
 * which has the final say, including the full password policy.
 */
@Component({
  selector: 'app-school-form',
  imports: [ReactiveFormsModule, MatCardModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-card class="mm-card mm-elevated">
      <mat-card-content>
        <form [formGroup]="form" (ngSubmit)="submit()" class="mm-school-form" novalidate>
          <h2 class="mm-school-form__title" i18n="@@schoolForm.title">Add a school</h2>

          <h3 class="mm-school-form__section" i18n="@@schoolForm.schoolSection">School</h3>
          <div class="mm-grid mm-school-form__grid">
            <mat-form-field appearance="outline" subscriptSizing="dynamic" class="mm-col-6 mm-col-md-12">
              <mat-label i18n="@@schoolForm.name">School name</mat-label>
              <input matInput formControlName="name" [maxlength]="nameMaxLength" autocomplete="off" />
              @if (form.controls.name.invalid) {
                <mat-error i18n="@@schoolForm.nameError">Enter the school's name.</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline" subscriptSizing="dynamic" class="mm-col-6 mm-col-md-12">
              <mat-label i18n="@@schoolForm.domain">Domain</mat-label>
              <input matInput formControlName="domain" maxlength="253" autocomplete="off" dir="ltr" spellcheck="false" />
              <mat-hint i18n="@@schoolForm.domainHint">The web address of the school, for example school1.com. Locally use a name ending in .localhost.</mat-hint>
              @if (form.controls.domain.invalid) {
                <mat-error i18n="@@schoolForm.domainError">Enter a host name such as school1.com, without http:// or a path.</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline" subscriptSizing="dynamic" class="mm-col-6 mm-col-md-12">
              <mat-label i18n="@@schoolForm.country">Country</mat-label>
              <mat-select formControlName="countryId">
                <mat-option [value]="null" i18n="@@schools.noCountry">None (English only, UTC)</mat-option>
                @for (country of countries(); track country.id) {
                  <mat-option [value]="country.id">{{ country.name }}</mat-option>
                }
              </mat-select>
              <mat-hint i18n="@@schoolForm.countryHint">Sets the school's languages, time zone, currency and price book.</mat-hint>
            </mat-form-field>

            <mat-form-field appearance="outline" subscriptSizing="dynamic" class="mm-col-6 mm-col-md-12">
              <mat-label i18n="@@schoolForm.dataRegion">Data region</mat-label>
              <mat-select formControlName="dataRegion">
                <mat-option [value]="null" i18n="@@schoolForm.noDataRegion">Not decided yet</mat-option>
                <mat-option [value]="regions.SouthAsia" i18n="@@schoolForm.regionSouthAsia">South Asia</mat-option>
                <mat-option [value]="regions.MiddleEast" i18n="@@schoolForm.regionMiddleEast">Middle East</mat-option>
                <mat-option [value]="regions.NorthAmerica" i18n="@@schoolForm.regionNorthAmerica">North America</mat-option>
              </mat-select>
              <mat-hint i18n="@@schoolForm.dataRegionHint">Where the school's data is kept.</mat-hint>
            </mat-form-field>
          </div>

          <h3 class="mm-school-form__section" i18n="@@schoolForm.adminSection">First administrator</h3>
          <div class="mm-grid mm-school-form__grid">
            <mat-form-field appearance="outline" subscriptSizing="dynamic" class="mm-col-6 mm-col-md-12">
              <mat-label i18n="@@schoolForm.adminEmail">Administrator email</mat-label>
              <input matInput type="email" formControlName="administratorEmail" maxlength="256" autocomplete="off" dir="ltr" />
              <mat-hint i18n="@@schoolForm.adminEmailHint">They sign in with this email on the school's domain.</mat-hint>
              @if (form.controls.administratorEmail.invalid) {
                <mat-error i18n="@@schoolForm.adminEmailError">Enter a valid email address.</mat-error>
              }
            </mat-form-field>

            <mat-form-field appearance="outline" subscriptSizing="dynamic" class="mm-col-6 mm-col-md-12">
              <mat-label i18n="@@schoolForm.oneTimePassword">One-time password</mat-label>
              <input matInput formControlName="administratorPassword" [maxlength]="passwordMaxLength" autocomplete="new-password" dir="ltr" spellcheck="false" />
              <button mat-button matSuffix type="button" (click)="generatePassword()" i18n="@@schoolForm.generate">Generate</button>
              <mat-hint i18n="@@schoolForm.oneTimePasswordHint">At least 15 characters. Pass it on privately; they must change it when they first sign in.</mat-hint>
              @if (form.controls.administratorPassword.invalid) {
                <mat-error i18n="@@schoolForm.oneTimePasswordError">Use 15 to 128 characters.</mat-error>
              }
            </mat-form-field>
          </div>

          @for (message of errors(); track message) {
            <p class="mm-school-form__error" role="alert">{{ message }}</p>
          }

          <div class="mm-school-form__actions">
            <button mat-button type="button" (click)="cancel.emit()" i18n="@@common.cancel">Cancel</button>
            <button mat-flat-button type="submit" [disabled]="saving()" i18n="@@schoolForm.save">Add school</button>
          </div>
        </form>
      </mat-card-content>
    </mat-card>
  `,
  styles: `
    .mm-school-form__title {
      margin: 0 0 var(--mm-space-4);
      font-size: var(--mm-text-lg);
      font-weight: 600;
    }

    .mm-school-form__section {
      margin: var(--mm-space-4) 0 var(--mm-space-2);
      font-size: var(--mm-text-sm);
      font-weight: 600;
      color: var(--mm-text-secondary);
    }

    /* Theme 12-column grid; each field sizes its hint area to the text so long hints push the next row down. */
    .mm-school-form__grid {
      row-gap: var(--mm-space-4);
      align-items: start;
    }

    .mm-school-form__error {
      margin: var(--mm-space-2) 0 0;
      color: var(--mat-sys-error);
      font-size: var(--mm-text-sm);
    }

    .mm-school-form__actions {
      display: flex;
      justify-content: flex-end;
      gap: var(--mm-space-2);
      margin-top: var(--mm-space-4);
    }
  `,
})
export class SchoolFormComponent {
  readonly countries = input.required<readonly CountryDto[]>();
  readonly saving = input(false);
  readonly errors = input<readonly string[]>([]);

  readonly save = output<NewSchool>();
  readonly cancel = output<void>();

  protected readonly regions = DataRegion;
  protected readonly nameMaxLength = SCHOOL_NAME_MAX_LENGTH;
  protected readonly passwordMaxLength = PASSWORD_MAX_LENGTH;

  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(SCHOOL_NAME_MAX_LENGTH)]],
    domain: ['', [Validators.required, Validators.pattern(DOMAIN_PATTERN)]],
    countryId: [null as string | null],
    dataRegion: [null as DataRegion | null],
    administratorEmail: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    administratorPassword: [
      '',
      [Validators.required, Validators.minLength(PASSWORD_MIN_LENGTH), Validators.maxLength(PASSWORD_MAX_LENGTH)],
    ],
  });

  /** Fills in a random password from the browser's cryptographic generator, so the master need not invent one. */
  protected generatePassword(): void {
    const alphabet = 'ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789';
    const bytes = crypto.getRandomValues(new Uint32Array(GENERATED_PASSWORD_LENGTH));
    const password = Array.from(bytes, (value) => alphabet[value % alphabet.length]).join('');
    this.form.controls.administratorPassword.setValue(password);
    this.form.controls.administratorPassword.markAsTouched();
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.save.emit({
      ...value,
      name: value.name.trim(),
      domain: value.domain.trim().toLowerCase(),
      administratorEmail: value.administratorEmail.trim(),
    });
  }
}
