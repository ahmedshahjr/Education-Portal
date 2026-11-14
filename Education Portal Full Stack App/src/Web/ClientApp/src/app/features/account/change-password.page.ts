import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { problemMessages } from '../../core/http/problem-details';
import { FormInputComponent } from '../../shared/components/form-input/form-input.component';
import { LanguageMenuComponent } from '../../shared/components/language-menu/language-menu.component';
import { PASSWORD_MAX_LENGTH, PASSWORD_MIN_LENGTH } from '../schools/school-form.component';

/**
 * Lets the signed-in user replace their password. An account created with a one-time password is sent here first and
 * can do nothing else until it has its own password; the server enforces that on every request. The new password
 * follows the password policy, which the server checks in full (SEC-AUTHN-03).
 */
@Component({
  selector: 'app-change-password-page',
  imports: [ReactiveFormsModule, MatCardModule, MatButtonModule, FormInputComponent, LanguageMenuComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-auth-shell">
      <main class="mm-auth-main">
        <div class="mm-auth-language">
          <app-language-menu />
        </div>
        <mat-card class="mm-card mm-elevated mm-auth-card">
          <mat-card-content>
            <h1 class="mm-auth-title" i18n="@@changePassword.title">Choose your password</h1>
            @if (required()) {
              <p class="mm-auth-sub" i18n="@@changePassword.requiredIntro">
                You signed in with a one-time password. Choose your own password to continue.
              </p>
            } @else {
              <p class="mm-auth-sub" i18n="@@changePassword.intro">Replace your password. Your other sessions will be signed out.</p>
            }

            <form [formGroup]="form" (ngSubmit)="submit()" class="mm-auth-form" novalidate>
              <app-form-input
                label="Current password"
                i18n-label="@@changePassword.current"
                type="password"
                fieldId="currentPassword"
                autocomplete="current-password"
                [control]="form.controls.currentPassword"
              />
              <app-form-input
                label="New password"
                i18n-label="@@changePassword.new"
                type="password"
                fieldId="newPassword"
                autocomplete="new-password"
                hint="At least 15 characters. A few unrelated words make a strong, easy to remember password."
                i18n-hint="@@changePassword.newHint"
                [control]="form.controls.newPassword"
              />
              <app-form-input
                label="Repeat the new password"
                i18n-label="@@changePassword.repeat"
                type="password"
                fieldId="repeatPassword"
                autocomplete="new-password"
                [control]="form.controls.repeatPassword"
              />

              @if (form.hasError('mismatch') && form.controls.repeatPassword.touched) {
                <p class="mm-auth-error" role="alert" i18n="@@changePassword.mismatch">The new passwords do not match.</p>
              }
              @for (message of errors(); track message) {
                <p class="mm-auth-error" role="alert">{{ message }}</p>
              }

              <button mat-flat-button type="submit" class="mm-auth-submit" [disabled]="busy()" i18n="@@changePassword.submit">
                Save password
              </button>
              <button mat-button type="button" (click)="signOut()" i18n="@@changePassword.signOut">Sign out</button>
            </form>
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

    .mm-auth-error {
      margin: 0;
      color: var(--mat-sys-error, #b3261e);
    }
  `,
})
export class ChangePasswordPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  /** True when the account still has its one-time password, so this page is the only one it can use. */
  protected readonly required = computed(() => this.auth.mustChangePassword());

  protected readonly form = inject(FormBuilder).nonNullable.group(
    {
      currentPassword: ['', [Validators.required, Validators.maxLength(PASSWORD_MAX_LENGTH)]],
      newPassword: [
        '',
        [Validators.required, Validators.minLength(PASSWORD_MIN_LENGTH), Validators.maxLength(PASSWORD_MAX_LENGTH)],
      ],
      repeatPassword: ['', [Validators.required]],
    },
    { validators: passwordsMatch }
  );

  protected readonly busy = signal(false);
  protected readonly errors = signal<readonly string[]>([]);

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { currentPassword, newPassword } = this.form.getRawValue();
    this.errors.set([]);
    this.busy.set(true);

    this.auth.changePassword(currentPassword, newPassword).subscribe({
      next: () => void this.router.navigateByUrl('/'),
      error: (error: unknown) => {
        this.busy.set(false);
        this.errors.set(problemMessages(error));
      },
    });
  }

  protected signOut(): void {
    this.auth.signOut().subscribe({ next: () => void this.router.navigateByUrl('/login') });
  }
}

/** The two new-password fields must hold the same text. */
export function passwordsMatch(group: AbstractControl): ValidationErrors | null {
  const value = group.value as { newPassword?: string; repeatPassword?: string };
  return value.newPassword === value.repeatPassword ? null : { mismatch: true };
}
