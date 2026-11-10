import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { LayoutService } from '../../../core/layout/layout.service';
import { MATERIALM_NAV_SECTIONS } from '../../../core/layout/navigation.data';
import { NavItem } from '../../models/navigation.models';
import { AppIconComponent } from '../app-icon/app-icon.component';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatTooltipModule, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
})
export class SidebarComponent {
  protected readonly layout = inject(LayoutService);
  protected readonly sections = MATERIALM_NAV_SECTIONS;

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
