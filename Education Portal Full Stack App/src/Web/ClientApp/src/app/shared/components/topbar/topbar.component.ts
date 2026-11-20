import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { BrandingService } from '../../../core/branding/branding.service';
import { CampusScopeService } from '../../../core/campus/campus-scope.service';
import { LayoutService } from '../../../core/layout/layout.service';
import { AppIconComponent } from '../app-icon/app-icon.component';
import { LanguageMenuComponent } from '../language-menu/language-menu.component';

/**
 * The theme's top bar, reduced to what the portal uses today: sidebar toggle, language, light/dark mode and the account
 * menu.
 */
@Component({
  selector: 'app-topbar',
  imports: [MatToolbarModule, MatButtonModule, MatMenuModule, MatTooltipModule, MatFormFieldModule, MatSelectModule, AppIconComponent, LanguageMenuComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.scss',
})
export class TopbarComponent {
  protected readonly layout = inject(LayoutService);
  protected readonly auth = inject(AuthService);
  protected readonly branding = inject(BrandingService);
  protected readonly campusScope = inject(CampusScopeService);
  private readonly router = inject(Router);

  constructor() {
    this.campusScope.load();
  }

  protected readonly initials = computed(() => (this.auth.currentUser()?.email ?? '?').slice(0, 2).toUpperCase());
  protected readonly roles = computed(() => this.auth.currentUser()?.roles?.join(', ') || $localize`:@@topbar.member:Member`);

  protected readonly text = {
    switchToLight: $localize`:@@topbar.switchToLight:Switch to light mode`,
    switchToDark: $localize`:@@topbar.switchToDark:Switch to dark mode`,
    lightMode: $localize`:@@topbar.lightMode:Light mode`,
    darkMode: $localize`:@@topbar.darkMode:Dark mode`,
  };

  protected signOut(): void {
    this.auth.signOut().subscribe({
      next: () => void this.router.navigate(['/login']),
    });
  }
}
