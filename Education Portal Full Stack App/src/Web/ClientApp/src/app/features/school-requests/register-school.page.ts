import { ChangeDetectionStrategy, Component, inject, OnInit, signal } from '@angular/core';
import { FormBuilder, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { Router, RouterLink } from '@angular/router';
import { problemMessages } from '../../core/http/problem-details';
import { FormInputComponent } from '../../shared/components/form-input/form-input.component';
import { LanguageMenuComponent } from '../../shared/components/language-menu/language-menu.component';
import { ToastService } from '../../shared/services/toast.service';
import { RegisterSchoolCommand, RegistrationCountryDto, SchoolRegistrationsClient } from '../../web-api-client';
import { passwordsMatch } from '../account/change-password.page';
import { PASSWORD_MAX_LENGTH, PASSWORD_MIN_LENGTH, SCHOOL_NAME_MAX_LENGTH } from '../schools/school-form.component';

/**
 * The public "Register school" form on the master tenant's domain. It creates the school and its administrator at once, with
 * the password the administrator chose and a free 3-month trial, so there is nothing to wait for. The server checks every
 * field again, including the full password policy (SEC-INPUT-01, SEC-AUTHN-03). The administrator signs in on this domain until
 * the platform team gives the school its own.
 */
@Component({
  selector: 'app-register-school-page',
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
  templateUrl: './register-school.page.html',
  styleUrl: './register-school.page.scss',
})
export class RegisterSchoolPage implements OnInit {
  private readonly client = inject(SchoolRegistrationsClient);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  protected readonly countryPlaceholder = $localize`:@@registerSchool.countryPlaceholder:Choose your country`;

  protected readonly form = inject(FormBuilder).nonNullable.group(
    {
      schoolName: ['', [Validators.required, Validators.maxLength(SCHOOL_NAME_MAX_LENGTH)]],
      countryId: new FormControl<string>('', { nonNullable: true, validators: [Validators.required] }),
      administratorEmail: ['', [Validators.required, Validators.email, Validators.maxLength(256)]],
      // The field names match what `passwordsMatch` checks on the change-password page.
      newPassword: ['', [Validators.required, Validators.minLength(PASSWORD_MIN_LENGTH), Validators.maxLength(PASSWORD_MAX_LENGTH)]],
      repeatPassword: ['', [Validators.required]],
    },
    { validators: passwordsMatch }
  );

  protected readonly countries = signal<readonly RegistrationCountryDto[]>([]);
  protected readonly error = signal<string | null>(null);
  protected readonly busy = signal(false);

  ngOnInit(): void {
    this.client.getRegistrationCountries().subscribe({
      next: (countries) => this.countries.set(countries),
      error: (error: unknown) => this.toast.error(error),
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const command = new RegisterSchoolCommand({
      schoolName: value.schoolName.trim(),
      countryId: value.countryId,
      administratorEmail: value.administratorEmail.trim(),
      password: value.newPassword,
      confirmPassword: value.repeatPassword,
    });

    this.error.set(null);
    this.busy.set(true);
    this.client.registerSchool(command).subscribe({
      next: () => {
        this.toast.success($localize`:@@registerSchool.created:Your school is ready. Sign in with your email and password.`);
        void this.router.navigateByUrl('/login');
      },
      error: (error: unknown) => {
        this.busy.set(false);
        this.error.set(problemMessages(error)[0] ?? null);
      },
    });
  }
}
