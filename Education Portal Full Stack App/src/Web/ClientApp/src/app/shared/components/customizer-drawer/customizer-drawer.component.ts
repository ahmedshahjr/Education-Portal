import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { LayoutService } from '../../../core/layout/layout.service';
import { AppIconComponent } from '../app-icon/app-icon.component';

/**
 * The theme's settings drawer. These are each user's own display preferences; the accent colour, sidebar colour,
 * logo and favicon belong to the school and are set on the branding page.
 */
@Component({
  selector: 'app-theme-customizer',
  imports: [MatButtonModule, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-customizer">
      <div class="mm-customizer__head">
        <h2 class="mm-customizer__title">Settings</h2>
        <button
          mat-icon-button
          type="button"
          aria-label="Close theme customizer"
          (click)="layout.toggleCustomizer(false)"
        >
          <app-icon name="x" [size]="18" />
        </button>
      </div>

      <div class="mm-customizer__body">
        <section class="mm-customizer__section">
          <h3 class="mm-customizer__label">Theme Option</h3>
          <div class="mm-customizer__pair">
            <button
              type="button"
              class="mm-opt-card"
              [class.is-active]="layout.themeMode() === 'light'"
              (click)="layout.setThemeMode('light')"
            >
              <app-icon name="sun" [size]="18" />
              <span>Light</span>
            </button>
            <button
              type="button"
              class="mm-opt-card"
              [class.is-active]="layout.themeMode() === 'dark'"
              (click)="layout.setThemeMode('dark')"
            >
              <app-icon name="moon" [size]="18" />
              <span>Dark</span>
            </button>
          </div>
        </section>

        <section class="mm-customizer__section">
          <h3 class="mm-customizer__label">Theme Direction</h3>
          <div class="mm-customizer__pair">
            <button
              type="button"
              class="mm-opt-card"
              [class.is-active]="layout.direction() === 'ltr'"
              (click)="layout.setDirection('ltr')"
            >
              <span>LTR</span>
            </button>
            <button
              type="button"
              class="mm-opt-card"
              [class.is-active]="layout.direction() === 'rtl'"
              (click)="layout.setDirection('rtl')"
            >
              <span>RTL</span>
            </button>
          </div>
        </section>

        <section class="mm-customizer__section">
          <h3 class="mm-customizer__label">Sidebar Type</h3>
          <div class="mm-customizer__pair">
            <button
              type="button"
              class="mm-opt-card"
              [class.is-active]="layout.sidebarMode() === 'full'"
              (click)="layout.setSidebarMode('full')"
            >
              <span>Full</span>
            </button>
            <button
              type="button"
              class="mm-opt-card"
              [class.is-active]="layout.sidebarMode() === 'mini'"
              (click)="layout.setSidebarMode('mini')"
            >
              <span>Minisidebar</span>
            </button>
          </div>
        </section>

        <section class="mm-customizer__section">
          <h3 class="mm-customizer__label">Card Surface</h3>
          <div class="mm-customizer__pair">
            <button
              type="button"
              class="mm-opt-card"
              [class.is-active]="layout.cardStyle() === 'shadow'"
              (click)="layout.setCardStyle('shadow')"
            >
              <span>Shadow</span>
            </button>
            <button
              type="button"
              class="mm-opt-card"
              [class.is-active]="layout.cardStyle() === 'border'"
              (click)="layout.setCardStyle('border')"
            >
              <span>Border</span>
            </button>
          </div>
        </section>

        <button
          mat-stroked-button
          type="button"
          class="mm-reset-btn"
          (click)="layout.resetPreferences()"
        >
          Reset Theme Defaults
        </button>
      </div>
    </div>
  `,
  styles: `
    .mm-customizer {
      display: flex;
      flex-direction: column;
      height: 100%;
      width: 310px;
      background-color: var(--mm-bg-surface);
      color: var(--mm-text-primary);
    }

    .mm-customizer__head {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 18px 20px;
      border-bottom: 1px solid var(--mm-border-color);
    }

    .mm-customizer__title {
      margin: 0;
      font-size: var(--mm-text-lg);
      font-weight: 600;
    }

    .mm-customizer__body {
      flex: 1;
      overflow-y: auto;
      padding: 20px;
      display: flex;
      flex-direction: column;
      gap: 22px;
    }

    .mm-customizer__section {
      display: flex;
      flex-direction: column;
      gap: 10px;
    }

    .mm-customizer__label {
      margin: 0;
      font-size: var(--mm-text-sm);
      font-weight: 600;
      color: var(--mm-text-primary);
    }

    .mm-customizer__pair {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 10px;
    }

    .mm-opt-card {
      appearance: none;
      border: 1px solid var(--mm-border-color);
      background-color: var(--mm-bg-surface);
      color: var(--mm-text-primary);
      border-radius: var(--mm-radius-md);
      padding: 12px;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      font-size: var(--mm-text-xs);
      font-weight: 600;
      cursor: pointer;
      transition: all var(--mm-duration) var(--mm-ease);

      &.is-active {
        background-color: var(--mm-light-primary);
        border-color: var(--mm-primary);
        color: var(--mm-primary);
      }
    }

    .mm-reset-btn {
      margin-top: auto;
      width: 100%;
    }
  `,
})
export class ThemeCustomizerComponent {
  protected readonly layout = inject(LayoutService);
}
