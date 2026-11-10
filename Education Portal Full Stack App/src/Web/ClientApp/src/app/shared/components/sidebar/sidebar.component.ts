import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { BrandingService } from '../../../core/branding/branding.service';
import { LayoutService } from '../../../core/layout/layout.service';
import { APP_NAV_SECTIONS, visibleNavSections } from '../../../core/layout/navigation';
import { NavItem } from '../../models/navigation.models';
import { AppIconComponent } from '../app-icon/app-icon.component';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive, MatTooltipModule, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent {
  protected readonly layout = inject(LayoutService);
  protected readonly branding = inject(BrandingService);
  private readonly auth = inject(AuthService);

  protected readonly sections = computed(() =>
    visibleNavSections(APP_NAV_SECTIONS, this.auth.isAdministrator(), this.branding.isMaster())
  );

  private readonly expandedGroups = signal<Record<string, boolean>>({});

  protected isExpanded(itemId: string): boolean {
    return !!this.expandedGroups()[itemId];
  }

  protected toggleGroup(item: NavItem): void {
    if (this.layout.isMiniSidebar()) {
      this.layout.setSidebarMode('full');
      this.expandedGroups.update((curr) => ({ ...curr, [item.id]: true }));
      return;
    }
    this.expandedGroups.update((curr) => ({ ...curr, [item.id]: !curr[item.id] }));
  }

  protected onNavigate(): void {
    if (this.layout.isMobile()) {
      this.layout.closeMobileDrawer();
    }
  }
}
