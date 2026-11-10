import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { LayoutService } from '../../../core/layout/layout.service';
import { NotificationService } from '../../../core/services/notification.service';
import { AppIconName } from '../../models/navigation.models';
import { AppIconComponent } from '../app-icon/app-icon.component';
import { SearchDialogComponent } from '../search-dialog/search-dialog.component';

interface QuickAppLink {
  readonly title: string;
  readonly subtitle: string;
  readonly route: string;
  readonly icon: AppIconName;
  readonly tone: 'primary' | 'secondary' | 'success' | 'warning' | 'error';
}

interface LanguageOption {
  readonly code: string;
  readonly label: string;
}

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [
    RouterLink,
    MatToolbarModule,
    MatButtonModule,
    MatMenuModule,
    MatDialogModule,
    MatTooltipModule,
    AppIconComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './topbar.component.html',
  styleUrl: './topbar.component.scss',
})
export class TopbarComponent {
  protected readonly layout = inject(LayoutService);
  protected readonly auth = inject(AuthService);
  protected readonly notifications = inject(NotificationService);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);

  protected readonly selectedLanguage = signal<LanguageOption>({ code: 'EN', label: 'English (UK)' });

  protected readonly languages: readonly LanguageOption[] = [
    { code: 'EN', label: 'English (UK)' },
    { code: 'ES', label: 'Español' },
    { code: 'FR', label: 'Français' },
    { code: 'DE', label: 'Deutsch' },
  ];

  protected readonly quickApps: readonly QuickAppLink[] = [
    {
      title: 'Dashboard 3',
      subtitle: 'Analytics & KPIs',
      route: '/dashboards/dashboard3',
      icon: 'monitor',
      tone: 'primary',
    },
    {
      title: 'Projects App',
      subtitle: 'Manage SaaS & Mobile',
      route: '/apps/projects',
      icon: 'kanban',
      tone: 'success',
    },
    {
      title: 'Transactions',
      subtitle: 'Cards & Settlements',
      route: '/apps/transactions',
      icon: 'wallet',
      tone: 'secondary',
    },
    {
      title: 'Earning Reports',
      subtitle: 'Market Breakdown',
      route: '/pages/reports',
      icon: 'trending-up',
      tone: 'warning',
    },
    {
      title: 'UI Components',
      subtitle: 'Design System Kit',
      route: '/pages/ui-components',
      icon: 'blocks',
      tone: 'error',
    },
    {
      title: 'Account Settings',
      subtitle: 'Profile & Security',
      route: '/pages/account-settings',
      icon: 'settings',
      tone: 'primary',
    },
  ];

  protected openSearch(): void {
    this.dialog.open(SearchDialogComponent, {
      width: '560px',
      maxWidth: '94vw',
      panelClass: 'mm-dialog-surface',
    });
  }

  protected setLanguage(lang: LanguageOption): void {
    this.selectedLanguage.set(lang);
    this.notifications.notify(`Language switched to ${lang.label}`, 'primary');
  }

  protected signOut(): void {
    this.auth.logout();
    void this.router.navigateByUrl('/auth/login');
  }
}
