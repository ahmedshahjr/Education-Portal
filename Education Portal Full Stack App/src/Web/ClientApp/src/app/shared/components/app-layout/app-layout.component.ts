import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterOutlet } from '@angular/router';
import { LayoutService } from '../../../core/layout/layout.service';
import { AppIconComponent } from '../app-icon/app-icon.component';
import { ThemeCustomizerComponent } from '../customizer-drawer/customizer-drawer.component';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { TopbarComponent } from '../topbar/topbar.component';

@Component({
  selector: 'app-layout',
  imports: [
    RouterOutlet,
    MatSidenavModule,
    MatButtonModule,
    MatTooltipModule,
    SidebarComponent,
    TopbarComponent,
    ThemeCustomizerComponent,
    AppIconComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <mat-sidenav-container class="mm-shell" [dir]="layout.direction" autosize>
      <mat-sidenav
        class="mm-shell__sidenav"
        [style.width.px]="layout.effectiveSidebarWidth()"
        [mode]="layout.isMobile() ? 'over' : 'side'"
        [opened]="layout.isMobile() ? layout.mobileDrawerOpen() : true"
        (closedStart)="layout.closeMobileDrawer()"
      >
        <app-sidebar />
      </mat-sidenav>

      <mat-sidenav
        position="end"
        mode="over"
        class="mm-shell__customizer"
        [opened]="layout.customizerOpen()"
        (closedStart)="layout.toggleCustomizer(false)"
      >
        <app-theme-customizer />
      </mat-sidenav>

      <mat-sidenav-content class="mm-shell__content">
        <app-topbar />
        <main class="mm-shell__main">
          <router-outlet />
        </main>
      </mat-sidenav-content>
    </mat-sidenav-container>

    <button
      mat-fab
      type="button"
      class="mm-customizer-fab"
      aria-label="Open display settings"
      i18n-aria-label="@@layout.openSettings"
      matTooltip="Display settings"
      i18n-matTooltip="@@layout.displaySettings"
      matTooltipPosition="before"
      (click)="layout.toggleCustomizer(true)"
    >
      <app-icon name="settings" [size]="22" />
    </button>
  `,
  styles: `
    .mm-shell {
      min-height: 100vh;
      background-color: var(--mm-bg-canvas);
    }

    .mm-shell__sidenav {
      background-color: var(--mm-sidebar-bg);
      border-inline-end: 1px solid var(--mm-border-color);
      box-shadow: none !important;
      overflow-x: hidden;
      transition: width var(--mm-duration) var(--mm-ease);
    }

    .mm-shell__customizer {
      background-color: var(--mm-bg-surface);
      border-inline-start: 1px solid var(--mm-border-color);
    }

    .mm-shell__content {
      display: flex;
      flex-direction: column;
      min-height: 100vh;
      background-color: var(--mm-bg-canvas);
      overflow-x: hidden;
    }

    .mm-shell__main {
      flex: 1;
      padding: 20px 16px 32px;
      max-width: 1600px;
      width: 100%;
      margin: 0 auto;
    }

    .mm-customizer-fab {
      position: fixed;
      inset-inline-end: 24px;
      bottom: 24px;
      z-index: 30;
      background-color: var(--mm-success) !important;
      color: var(--mm-text-inverse) !important;
      box-shadow: 0 8px 20px rgba(0, 206, 182, 0.35) !important;
    }

    @media (max-width: 768px) {
      .mm-shell__main {
        padding: 16px 14px 28px;
      }

      .mm-customizer-fab {
        inset-inline-end: 16px;
        bottom: 16px;
      }
    }
  `,
})
export class AppLayoutComponent {
  protected readonly layout = inject(LayoutService);
}
