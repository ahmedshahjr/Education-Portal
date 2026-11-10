import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { BrandingService } from '../../../core/branding/branding.service';
import { LayoutService } from '../../../core/layout/layout.service';
import { AppIconComponent } from '../app-icon/app-icon.component';

/** The theme's top bar, reduced to what the portal uses today: sidebar toggle, light/dark mode and the account menu. */
@Component({
  selector: 'app-topbar',
  imports: [MatToolbarModule, MatButtonModule, MatMenuModule, MatTooltipModule, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.scss',
})
export class TopbarComponent {
  protected readonly layout = inject(LayoutService);
  protected readonly auth = inject(AuthService);
  protected readonly branding = inject(BrandingService);
  private readonly router = inject(Router);

  protected readonly initials = computed(() => (this.auth.currentUser()?.email ?? '?').slice(0, 2).toUpperCase());
  protected readonly roles = computed(() => this.auth.currentUser()?.roles?.join(', ') || 'Member');

  protected signOut(): void {
    this.auth.signOut().subscribe({
      next: () => void this.router.navigate(['/login']),
    });
  }
}
