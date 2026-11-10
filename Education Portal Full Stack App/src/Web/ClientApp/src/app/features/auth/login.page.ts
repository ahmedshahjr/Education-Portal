import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { BrandingService } from '../../core/branding/branding.service';
import { FormInputComponent } from '../../shared/components/form-input/form-input.component';
import { LanguageMenuComponent } from '../../shared/components/language-menu/language-menu.component';

/**
 * The theme's sign-in page, themed for the school that owns this domain before anyone signs in. Only that school's
 * accounts can sign in here; the server refuses the rest with the same message as a wrong password.
 */
@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule, MatCardModule, MatButtonModule, FormInputComponent, LanguageMenuComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-auth-shell">
      <section class="mm-auth-aside">
        <div class="mm-auth-brand">
          @if (branding.logoUrl(); as logoUrl) {
            <img class="mm-auth-logo" [src]="logoUrl" [alt]="branding.schoolName()" />
          } @else {
            <span>{{ branding.schoolName() }}</span>
          }
        </div>

        <h1 class="mm-auth-hero" i18n="@@login.welcome">Welcome to {{ branding.schoolName() }}</h1>
        <p class="mm-auth-copy" i18n="@@login.intro">Sign in with the account your school gave you.</p>
      </section>

      <main class="mm-auth-main">
        <div class="mm-auth-language">
          <app-language-menu />
        </div>
        <mat-card class="mm-card mm-elevated mm-auth-card">
          <mat-card-content>
            <h2 class="mm-auth-title" i18n="@@login.title">Sign in</h2>
            <p class="mm-auth-sub">{{ branding.schoolName() }}</p>

            <form [formGroup]="form" (ngSubmit)="submit()" class="mm-auth-form" novalidate>
              <app-form-input
                label="Email"
                i18n-label="@@login.email"
                type="email"
                fieldId="email"
                autocomplete="username"
                [control]="form.controls.email"
              />
              <app-form-input
                label="Password"
                i18n-label="@@login.password"
                type="password"
                fieldId="password"
                autocomplete="current-password"
                [control]="form.controls.password"
              />

              @if (error(); as message) {
                <p id="login-error" class="mm-auth-error" role="alert">{{ message }}</p>
              }

              <button mat-flat-button type="submit" class="mm-auth-submit" [disabled]="busy()" i18n="@@login.submit">Sign in</button>
            </form>
          </mat-card-content>
        </mat-card>
      </main>
    </div>
  `,
  styles: `
    .mm-auth-shell {
      min-height: 100vh;
      display: grid;
      grid-template-columns: 1.05fr 1fr;
      background-color: var(--mm-bg-canvas);
    }

    .mm-auth-aside {
      background-color: var(--mm-auth-aside-bg);
      color: var(--mm-auth-aside-text);
      padding: 48px;
      display: flex;
      flex-direction: column;
      justify-content: center;
      gap: 18px;
    }

    .mm-auth-brand {
      display: inline-flex;
      align-items: center;
      gap: 10px;
      font-size: 1.4rem;
      font-weight: 700;
      margin-bottom: 12px;
    }

    .mm-auth-logo {
      max-height: 64px;
      max-width: 240px;
      object-fit: contain;
    }

    .mm-auth-hero {
      margin: 0;
      font-size: 2rem;
      font-weight: 600;
      line-height: 1.25;
      max-width: 440px;
    }

    .mm-auth-copy {
      margin: 0;
      font-size: var(--mm-text-sm);
      line-height: 1.6;
      max-width: 420px;
      opacity: 0.8;
    }

    .mm-auth-main {
      position: relative;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 32px 20px;
    }

    .mm-auth-language {
      position: absolute;
      top: 16px;
      inset-inline-end: 16px;
    }

    .mm-auth-card {
      width: 100%;
      max-width: 440px;
    }

    .mm-auth-title {
      margin: 0 0 6px;
      font-size: var(--mm-text-2xl);
      font-weight: 600;
      color: var(--mm-text-primary);
    }

    .mm-auth-sub {
      margin: 0 0 22px;
      font-size: var(--mm-text-sm);
      color: var(--mm-text-muted);
    }

    .mm-auth-form {
      display: flex;
      flex-direction: column;
      gap: 10px;
    }

    .mm-auth-error {
      margin: 0;
      font-size: var(--mm-text-sm);
      color: var(--mat-sys-error);
    }

    .mm-auth-submit {
      height: 48px;
      margin-top: 8px;
    }

    @media (max-width: 900px) {
      .mm-auth-shell {
        grid-template-columns: 1fr;
      }

      .mm-auth-aside {
        padding: 32px 24px;
      }
    }
  `,
})
export class LoginPage {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  protected readonly branding = inject(BrandingService);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    email: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
    password: ['', [Validators.required]],
  });

  protected readonly error = signal<string | null>(null);
  protected readonly busy = signal(false);

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { email, password } = this.form.getRawValue();
    this.error.set(null);
    this.busy.set(true);

    this.auth.signIn(email, password).subscribe({
      next: () => void this.router.navigateByUrl(localReturnUrl(this.route.snapshot.queryParamMap.get('returnUrl'))),
      error: (error: unknown) => {
        this.busy.set(false);
        this.error.set(messageFor(statusOf(error)));
      },
    });
  }
}

/** Only paths inside this app are followed after sign-in, never another site (SEC-INPUT-07). */
export function localReturnUrl(returnUrl: string | null): string {
  return returnUrl && returnUrl.startsWith('/') && !returnUrl.startsWith('//') && !returnUrl.startsWith('/\\')
    ? returnUrl
    : '/';
}

/** A wrong password, an unknown email, another school's account and a locked account all get the same message. */
function messageFor(status: number | undefined): string {
  switch (status) {
    case 401:
      return $localize`:@@login.invalid:Invalid email or password.`;
    case 429:
      return $localize`:@@login.tooMany:Too many sign-in attempts. Wait a minute and try again.`;
    default:
      return $localize`:@@login.failed:Sign-in failed. Reload the page and try again.`;
  }
}

function statusOf(error: unknown): number | undefined {
  return typeof error === 'object' && error !== null && 'status' in error && typeof error.status === 'number'
    ? error.status
    : undefined;
}
