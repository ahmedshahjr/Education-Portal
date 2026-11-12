import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { FormInputComponent } from '../../shared/components/form-input/form-input.component';

@Component({
  selector: 'app-auth-page',
  standalone: true,
  imports: [RouterLink, ReactiveFormsModule, MatCardModule, MatButtonModule, FormInputComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-auth-shell">
      <div class="mm-auth-aside">
        <div class="mm-auth-brand">
          <svg width="36" height="28" viewBox="0 0 36 28" fill="none" aria-hidden="true">
            <path
              d="M3.5 23.5L9.2 7.2C10.1 4.6 13.7 4.4 14.9 6.9L18.2 13.8L21.5 6.9C22.7 4.4 26.3 4.6 27.2 7.2L32.8 23.5"
              stroke="#00a1ff"
              stroke-width="5.2"
              stroke-linecap="round"
              stroke-linejoin="round"
            />
          </svg>
          <span>MaterialM</span>
        </div>

        <h1 class="mm-auth-hero">Welcome to MaterialM Dashboard 3</h1>
        <p class="mm-auth-copy">
          Monitor revenue velocity, ROI performance, payment settlements, and delivery telemetry in one cohesive Angular 22 workspace.
        </p>
      </div>

      <div class="mm-auth-main">
        <mat-card class="mm-card mm-elevated mm-auth-card">
          <mat-card-content>
            <h2 class="mm-auth-title">{{ heading() }}</h2>
            <p class="mm-auth-sub">{{ subheading() }}</p>

            <form [formGroup]="form" (ngSubmit)="submit()" class="mm-auth-form">
              @if (mode() === 'register') {
                <app-form-input label="Full Name" [control]="form.controls.name" />
              }

              <app-form-input label="Email Address" type="email" [control]="form.controls.email" />

              @if (mode() !== 'forgot') {
                <app-form-input label="Password" type="password" [control]="form.controls.password" />
              }

              <button mat-flat-button color="primary" type="submit" class="mm-auth-submit">
                {{ submitLabel() }}
              </button>
            </form>

            <div class="mm-auth-links">
              @if (mode() === 'login') {
                <a routerLink="/auth/forgot-password">Forgot Password?</a>
                <a routerLink="/auth/register">Create an Account</a>
              } @else {
                <a routerLink="/auth/login">Back to Sign In</a>
              }
              <a routerLink="/dashboards/dashboard3">Continue to Dashboard 3</a>
            </div>
          </mat-card-content>
        </mat-card>
      </div>
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
      background-color: #111c2d;
      color: #ffffff;
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
      color: rgba(255, 255, 255, 0.72);
      line-height: 1.6;
      max-width: 420px;
    }

    .mm-auth-main {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 32px 20px;
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

    .mm-auth-submit {
      height: 44px;
      margin-top: 8px;
    }

    .mm-auth-links {
      margin-top: 20px;
      display: flex;
      align-items: center;
      justify-content: space-between;
      flex-wrap: wrap;
      gap: 12px;
      font-size: var(--mm-text-xs);
      font-weight: 600;
      color: var(--mm-primary);
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
export class AuthPageComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  readonly mode = input<'login' | 'register' | 'forgot'>('login');

  protected readonly heading = computed(() => {
    switch (this.mode()) {
      case 'register':
        return 'Sign Up to MaterialM';
      case 'forgot':
        return 'Reset Your Password';
      default:
        return 'Sign In to MaterialM';
    }
  });

  protected readonly subheading = computed(() => {
    switch (this.mode()) {
      case 'register':
        return 'Create your admin analytics account';
      case 'forgot':
        return 'Enter your workspace email to receive reset instructions';
      default:
        return 'Your Admin Dashboard';
    }
  });

  protected readonly submitLabel = computed(() => {
    switch (this.mode()) {
      case 'register':
        return 'Create Account';
      case 'forgot':
        return 'Send Reset Link';
      default:
        return 'Sign In';
    }
  });

  protected readonly form = this.fb.nonNullable.group({
    name: ['Mathew Anderson'],
    email: ['info@materialm.io', [Validators.required, Validators.email]],
    password: ['materialm-demo-pass', [Validators.required, Validators.minLength(6)]],
  });

  protected submit(): void {
    if (this.mode() === 'forgot') {
      if (this.form.controls.email.invalid) {
        this.form.controls.email.markAsTouched();
        return;
      }
      this.notifications.notify('Password reset link sent to your email', 'success');
      void this.router.navigateByUrl('/auth/login');
      return;
    }

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { email, password } = this.form.getRawValue();
    this.auth.login(email, password).subscribe(() => {
      this.notifications.notify('Signed in to MaterialM Dashboard 3', 'success');
      void this.router.navigateByUrl('/dashboards/dashboard3');
    });
  }
}
