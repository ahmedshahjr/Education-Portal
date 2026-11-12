import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { AuthService } from '../../core/auth/auth.service';
import { LayoutService } from '../../core/layout/layout.service';
import { NotificationService } from '../../core/services/notification.service';
import { FormInputComponent } from '../../shared/components/form-input/form-input.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { BreadcrumbItem } from '../../shared/models/navigation.models';

@Component({
  selector: 'app-account-settings-page',
  standalone: true,
  imports: [ReactiveFormsModule, MatCardModule, MatButtonModule, PageHeaderComponent, FormInputComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header
      title="Account Settings"
      subtitle="Update administrator profile details, workspace preferences, and theme configuration"
      [breadcrumbs]="breadcrumbs"
    >
      <button mat-stroked-button type="button" (click)="layout.toggleCustomizer(true)">
        Open Theme Customizer
      </button>
    </app-page-header>

    <div class="mm-grid">
      <div class="mm-col-8">
        <mat-card class="mm-card mm-elevated">
          <mat-card-content>
            <h3 class="mm-set-heading">Personal Details</h3>
            <form [formGroup]="form" (ngSubmit)="saveProfile()" class="mm-set-form">
              <div class="mm-set-row">
                <app-form-input label="Full Name" [control]="form.controls.name" />
                <app-form-input label="Email Address" type="email" [control]="form.controls.email" />
              </div>
              <div class="mm-set-row">
                <app-form-input label="Role Title" [control]="form.controls.role" />
                <app-form-input label="Company" [control]="form.controls.company" />
              </div>
              <div class="mm-set-row">
                <app-form-input label="Location" [control]="form.controls.location" />
                <app-form-input label="Phone" type="tel" [control]="form.controls.phone" />
              </div>
              <div class="mm-set-actions">
                <button mat-flat-button color="primary" type="submit">Save Profile</button>
              </div>
            </form>
          </mat-card-content>
        </mat-card>
      </div>

      <div class="mm-col-4">
        <mat-card class="mm-card mm-elevated">
          <mat-card-content class="mm-set-card-preview">
            <div class="mm-set-avatar">{{ auth.currentUser().avatarInitials }}</div>
            <h4 class="mm-set-name">{{ auth.currentUser().name }}</h4>
            <p class="mm-set-role">{{ auth.currentUser().role }}</p>
            <p class="mm-set-bio">{{ auth.currentUser().bio }}</p>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: `
    .mm-set-heading {
      margin: 0 0 18px;
      font-size: var(--mm-text-lg);
      font-weight: 600;
      color: var(--mm-text-primary);
    }

    .mm-set-form {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .mm-set-row {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 16px;
    }

    .mm-set-actions {
      display: flex;
      justify-content: flex-end;
      margin-top: 8px;
    }

    .mm-set-card-preview {
      display: flex;
      flex-direction: column;
      align-items: center;
      text-align: center;
      gap: 8px;
    }

    .mm-set-avatar {
      width: 76px;
      height: 76px;
      border-radius: var(--mm-radius-pill);
      background-color: var(--mm-light-primary);
      color: var(--mm-primary);
      font-size: var(--mm-text-2xl);
      font-weight: 700;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      margin-bottom: 6px;
    }

    .mm-set-name {
      margin: 0;
      font-size: var(--mm-text-lg);
      font-weight: 600;
      color: var(--mm-text-primary);
    }

    .mm-set-role {
      margin: 0;
      font-size: var(--mm-text-sm);
      color: var(--mm-primary);
      font-weight: 500;
    }

    .mm-set-bio {
      margin: 6px 0 0;
      font-size: var(--mm-text-xs);
      color: var(--mm-text-muted);
      line-height: 1.5;
    }

    @media (max-width: 640px) {
      .mm-set-row {
        grid-template-columns: 1fr;
      }
    }
  `,
})
export class AccountSettingsPageComponent {
  protected readonly auth = inject(AuthService);
  protected readonly layout = inject(LayoutService);
  private readonly notifications = inject(NotificationService);
  private readonly fb = inject(FormBuilder);

  protected readonly breadcrumbs: readonly BreadcrumbItem[] = [
    { label: 'Dashboard 3', route: '/dashboards/dashboard3' },
    { label: 'Account Settings' },
  ];

  private readonly user = this.auth.currentUser();

  protected readonly form = this.fb.nonNullable.group({
    name: [this.user.name, [Validators.required, Validators.minLength(2)]],
    email: [this.user.email, [Validators.required, Validators.email]],
    role: [this.user.role, [Validators.required]],
    company: [this.user.company, [Validators.required]],
    location: [this.user.location, [Validators.required]],
    phone: [this.user.phone, [Validators.required]],
  });

  protected saveProfile(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.auth.updateProfile(this.form.getRawValue()).subscribe(() => {
      this.notifications.notify('Profile settings saved', 'success');
    });
  }
}
