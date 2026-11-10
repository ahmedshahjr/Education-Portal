import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { LocaleService } from '../../../core/i18n/locale.service';
import { LayoutService } from '../../../core/layout/layout.service';
import { AppIconComponent } from '../app-icon/app-icon.component';

/**
 * The theme's settings drawer. These are each user's own display preferences; the accent colour, sidebar colour,
 * logo and favicon belong to the school and are set on the branding page. The theme's LTR/RTL switch is replaced by
 * the language: Arabic and Urdu are laid out right to left.
 */
@Component({
  selector: 'app-theme-customizer',
  imports: [MatButtonModule, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mm-customizer">
      <div class="mm-customizer__head">
        <h2 class="mm-customizer__title" i18n="@@customizer.title">Settings</h2>
        <button
          mat-icon-button
          type="button"
          aria-label="Close display settings"
          i18n-aria-label="@@customizer.close"
          (click)="layout.toggleCustomizer(false)"
        >
          <app-icon name="x" [size]="18" />
        </button>
      </div>

      <div class="mm-customizer__body">
        <section class="mm-customizer__section">
          <h3 class="mm-customizer__label" i18n="@@customizer.theme">Theme option</h3>
          <div class="mm-customizer__pair">
            <button
              type="button"
              class="mm-opt-card"
              [class.is-active]="layout.themeMode() === 'light'"
              (click)="layout.setThemeMode('light')"
            >
              <app-icon name="sun" [size]="18" />
              <span i18n="@@customizer.light">Light</span>
            </button>
            <button
              type="button"
              class="mm-opt-card"
              [class.is-active]="layout.themeMode() === 'dark'"
              (click)="layout.setThemeMode('dark')"
            >
              <app-icon name="moon" [size]="18" />
              <span i18n="@@customizer.dark">Dark</span>
            </button>
          </div>
        </section>

        @if (locale.languages.length > 1) {
          <section class="mm-customizer__section">
            <h3 class="mm-customizer__label" i18n="@@customizer.language">Language</h3>
            <div class="mm-customizer__pair">
              @for (language of locale.languages; track language.code) {
                <button
                  type="button"
                  class="mm-opt-card"
                  [class.is-active]="language.code === locale.language"
                  [attr.aria-pressed]="language.code === locale.language"
                  [attr.lang]="language.code"
                  [disabled]="locale.changing()"
                  (click)="chooseLanguage(language.code)"
                >
                  <span>{{ language.nativeName }}</span>
                </button>
              }
            </div>
          </section>
        }

        <section class="mm-customizer__section">
          <h3 class="mm-customizer__label" i18n="@@customizer.sidebar">Sidebar type</h3>
          <div class="mm-customizer__pair">
            <button
              type="button"
              class="mm-opt-card"
              [class.is-active]="layout.sidebarMode() === 'full'"
              (click)="layout.setSidebarMode('full')"
            >
              <span i18n="@@customizer.full">Full</span>
            </button>
            <button
              type="button"
              class="mm-opt-card"
              [class.is-active]="layout.sidebarMode() === 'mini'"
              (click)="layout.setSidebarMode('mini')"
            >
              <span i18n="@@customizer.mini">Mini sidebar</span>
            </button>
          </div>
        </section>

        <section class="mm-customizer__section">
          <h3 class="mm-customizer__label" i18n="@@customizer.card">Card surface</h3>
          <div class="mm-customizer__pair">
            <button
              type="button"
              class="mm-opt-card"
              [class.is-active]="layout.cardStyle() === 'shadow'"
              (click)="layout.setCardStyle('shadow')"
            >
              <span i18n="@@customizer.shadow">Shadow</span>
            </button>
            <button
              type="button"
              class="mm-opt-card"
              [class.is-active]="layout.cardStyle() === 'border'"
              (click)="layout.setCardStyle('border')"
            >
              <span i18n="@@customizer.border">Border</span>
            </button>
          </div>
        </section>

        <button
          mat-stroked-button
          type="button"
          class="mm-reset-btn"
          (click)="layout.resetPreferences()"
        >
          <ng-container i18n="@@customizer.reset">Reset display defaults</ng-container>
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
  protected readonly locale = inject(LocaleService);

  /** The language replaces the theme's LTR/RTL switch: the direction follows the language. */
  protected chooseLanguage(code: string): void {
    if (code !== this.locale.language) {
      this.locale.changeLanguage(code).subscribe();
    }
  }
}
