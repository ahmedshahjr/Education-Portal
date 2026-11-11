import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { RouterLink } from '@angular/router';
import { problemMessages } from '../../core/http/problem-details';
import { FormInputComponent } from '../../shared/components/form-input/form-input.component';
import { LanguageMenuComponent } from '../../shared/components/language-menu/language-menu.component';
import { SchoolRequestsClient, SubmitSchoolRequestCommand } from '../../web-api-client';

/**
 * The public "request a school" form. It only sends a request: the master tenant reviews it, and nothing is created
 * for the school until then. The server checks every field again (SEC-INPUT-01).
 */
@Component({
  selector: 'app-request-school-page',
  imports: [ReactiveFormsModule, RouterLink, MatCardModule, MatButtonModule, FormInputComponent, LanguageMenuComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-auth-shell">
      <main class="mm-auth-main">
        <div class="mm-auth-language">
          <app-language-menu />
        </div>
        <mat-card class="mm-card mm-elevated mm-auth-card">
          <mat-card-content>
            <h1 class="mm-auth-title" i18n="@@requestSchool.title">Request a school</h1>
            <p class="mm-auth-sub" i18n="@@requestSchool.intro">
              Tell us about your school. The platform team reviews each request before anything is set up.
            </p>

            @if (sent()) {
              <p class="mm-auth-success" role="status" i18n="@@requestSchool.sent">
                Thank you. We have your request and will contact you after we review it.
              </p>
              <a mat-button routerLink="/login" i18n="@@requestSchool.backToLogin">Back to sign in</a>
            } @else {
              <form [formGroup]="form" (ngSubmit)="submit()" class="mm-auth-form" novalidate>
                <app-form-input
                  label="School name"
                  i18n-label="@@requestSchool.schoolName"
                  fieldId="schoolName"
                  autocomplete="organization"
                  [control]="form.controls.schoolName"
                />
                <app-form-input
                  label="Your name"
                  i18n-label="@@requestSchool.contactName"
                  fieldId="contactName"
                  autocomplete="name"
                  [control]="form.controls.contactName"
                />
                <app-form-input
                  label="Your email"
                  i18n-label="@@requestSchool.contactEmail"
                  type="email"
                  fieldId="contactEmail"
                  autocomplete="email"
                  [control]="form.controls.contactEmail"
                />

                @if (error(); as message) {
                  <p id="request-error" class="mm-auth-error" role="alert">{{ message }}</p>
                }

                <button mat-flat-button type="submit" class="mm-auth-submit" [disabled]="busy()" i18n="@@requestSchool.submit">
                  Send request
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
export class RequestSchoolPage {
  private readonly client = inject(SchoolRequestsClient);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    schoolName: ['', [Validators.required, Validators.maxLength(200)]],
    contactName: ['', [Validators.required, Validators.maxLength(100)]],
    contactEmail: ['', [Validators.required, Validators.email, Validators.maxLength(254)]],
  });

  protected readonly error = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected readonly sent = signal(false);

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const command = new SubmitSchoolRequestCommand({
      schoolName: value.schoolName,
      contactName: value.contactName,
      contactEmail: value.contactEmail,
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
