import { Component, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../auth.service';
import { firstValueFrom } from 'rxjs';

@Component({
  standalone: false,
  selector: 'app-login',
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './login.component.html'
})
export class LoginComponent {
  email = '';
  password = '';
  invalid = false;
  message = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {}

  async login() {
    this.invalid = false;
    try {
      await firstValueFrom(this.authService.signIn(this.email, this.password));
      const returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
      await this.router.navigateByUrl(returnUrl);
    } catch (error) {
      this.message = messageFor(statusOf(error));
      this.invalid = true;
      this.cdr.detectChanges();
    }
  }
}

/** A wrong password, an unknown email and a locked account all get the same 401, so they get the same message. */
function messageFor(status: number | undefined): string {
  switch (status) {
    case 401:
      return 'Invalid email or password.';
    case 429:
      return 'Too many sign-in attempts. Wait a minute and try again.';
    default:
      return 'Sign-in failed. Reload the page and try again.';
  }
}

function statusOf(error: unknown): number | undefined {
  return typeof error === 'object' && error !== null && 'status' in error && typeof error.status === 'number'
    ? error.status
    : undefined;
}
