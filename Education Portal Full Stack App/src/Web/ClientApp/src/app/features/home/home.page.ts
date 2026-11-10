import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { AuthService } from '../../core/auth/auth.service';
import { BrandingService } from '../../core/branding/branding.service';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';

/** The start page after sign-in. Real dashboards come with the academic modules (MVP step 3). */
@Component({
  selector: 'app-home-page',
  imports: [MatCardModule, PageHeaderComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <app-page-header [title]="branding.schoolName()" subtitle="Dashboard" />

    <mat-card class="mm-card mm-elevated">
      <mat-card-content>
        <h2 class="mm-home-title">Welcome back</h2>
        <p class="mm-home-text">You are signed in as {{ auth.currentUser()?.email }}.</p>
      </mat-card-content>
    </mat-card>
  `,
  styles: `
    .mm-home-title {
      margin: 0 0 8px;
      font-size: var(--mm-text-xl);
      font-weight: 600;
    }

    .mm-home-text {
      margin: 0;
      color: var(--mm-text-muted);
    }
  `,
})
export class HomePage {
  protected readonly auth = inject(AuthService);
  protected readonly branding = inject(BrandingService);
}
