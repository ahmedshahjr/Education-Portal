import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { RouterLink } from '@angular/router';
import { problemMessages } from '../../core/http/problem-details';
import { FormInputComponent } from '../../shared/components/form-input/form-input.component';
import { LanguageMenuComponent } from '../../shared/components/language-menu/language-menu.component';
import { RegistrationCountryDto, SchoolRequestsClient, SubmitSchoolRequestCommand } from '../../web-api-client';

/** Limits shared with the server (SchoolRegistration.PasswordMinLength and PasswordMaxLength); the server checks again. */
const PASSWORD_MIN_LENGTH = 15;
const PASSWORD_MAX_LENGTH = 128;

/**
 * The public self-service "register a school" form. It creates the school, its 3-month free trial and its first administrator
 * at once; the visitor signs in with the email and password chosen here. The server checks every field again (SEC-INPUT-01).
 */
@Component({
  selector: 'app-request-school-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatCardModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    FormInputComponent,
    LanguageMenuComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-auth-shell">
      <main class="mm-auth-main">
        <div class="mm-auth-language">
          <app-language-menu />
        </div>
        <mat-card class="mm-card mm-elevated mm-auth-card">
          <mat-card-content>
            <h1 class="mm-auth-title" i18n="@@requestSchool.title">Register your school</h1>
            <p class="mm-auth-sub" i18n="@@requestSchool.intro">
              Create your school and its administrator account. You get a 3-month free trial straight away.
            </p>

            @if (sent()) {
              <p class="mm-auth-success" role="status" i18n="@@requestSchool.sent">
                Your school is registered. Sign in with the email and password you chose.
              </p>
              <a mat-flat-button routerLink="/login" i18n="@@requestSchool.signIn">Sign in</a>
            } @else {
              <form [formGroup]="form" (ngSubmit)="submit()" class="mm-auth-form" novalidate>
                <app-form-input
                  label="School name"
                  i18n-label="@@requestSchool.schoolName"
                  fieldId="schoolName"
                  autocomplete="organization"
                  [control]="form.controls.schoolName"
                />

                <mat-form-field appearance="outline" subscriptSizing="dynamic">
                  <mat-label i18n="@@requestSchool.country">Country</mat-label>
                  <mat-select formControlName="countryId">
                    @for (country of countries(); track country.id) {
                      <mat-option [value]="country.id">{{ country.name }}</mat-option>
                    }
                  </mat-select>
                  <mat-hint i18n="@@requestSchool.countryHint">Sets your languages, time zone, currency and data region.</mat-hint>
                </mat-form-field>

                <app-form-input
                  label="Your name"
                  i18n-label="@@requestSchool.contactName"
                  fieldId="contactName"
                  autocomplete="name"
                  [control]="form.controls.contactName"
                />
                <app-form-input
                  label="Administrator email"
                  i18n-label="@@requestSchool.contactEmail"
                  type="email"
                  fieldId="contactEmail"
                  autocomplete="email"
                  [control]="form.controls.contactEmail"
                />
                <app-form-input
                  label="Password"
                  i18n-label="@@requestSchool.password"
                  type="password"
                  fieldId="administratorPassword"
                  autocomplete="new-password"
                  [control]="form.controls.administratorPassword"
                />

                @if (error(); as message) {
                  <p id="request-error" class="mm-auth-error" role="alert">{{ message }}</p>
                }

                <button mat-flat-button type="submit" class="mm-auth-submit" [disabled]="busy()" i18n="@@requestSchool.submit">
                  Register school
                </button>
                <a mat-button routerLink="/login" i18n="@@requestSchool.backToLogin">Back to sign in</a>
              </form>
            }
          </mat-card-content>
        </mat-card>
      </main>
    </div>
  `,
  styles: `
    .mm-auth-shell {
      min-height: 100vh;
      display: flex;
      justify-content: center;
      background-color: var(--mm-bg-canvas);
    }

    .mm-auth-main {
      width: min(560px, 100%);
      padding: 24px;
    }

    .mm-auth-language {
      display: flex;
      justify-content: flex-end;
    }

    .mm-auth-form {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .mm-auth-success {
      color: var(--mm-text-primary);
    }

    .mm-auth-error {
      color: var(--mat-sys-error, #b3261e);
    }
  `,
})
export class RequestSchoolPage implements OnInit {
  private readonly client = inject(SchoolRequestsClient);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    schoolName: ['', [Validators.required, Validators.maxLength(200)]],
    countryId: ['', [Validators.required]],
    contactName: ['', [Validators.required, Validators.maxLength(100)]],
    contactEmail: ['', [Validators.required, Validators.email, Validators.maxLength(254)]],
    administratorPassword: ['', [Validators.required, Validators.minLength(PASSWORD_MIN_LENGTH), Validators.maxLength(PASSWORD_MAX_LENGTH)]],
  });

  protected readonly countries = signal<readonly RegistrationCountryDto[]>([]);
  protected readonly error = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly sent = signal(false);

  ngOnInit(): void {
    this.client.getRegistrationCountries().subscribe({
      next: (countries) => this.countries.set(countries),
      error: (error: unknown) => this.error.set(problemMessages(error)[0] ?? null),
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const command = new SubmitSchoolRequestCommand({
      schoolName: value.schoolName,
      countryId: value.countryId,
      contactName: value.contactName,
      contactEmail: value.contactEmail,
      administratorPassword: value.administratorPassword,
    });

    this.error.set(null);
    this.busy.set(true);
    this.client.submitSchoolRequest(command).subscribe({
      next: () => {
        this.busy.set(false);
        this.sent.set(true);
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.error.set(problemMessages(error)[0] ?? null);
      },
    });
  }
}
