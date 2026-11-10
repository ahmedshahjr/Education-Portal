import { BreakpointObserver } from '@angular/cdk/layout';
import { DOCUMENT } from '@angular/common';
import { computed, effect, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  CardSurfaceStyle,
  LayoutDirection,
  LayoutPreferences,
  SidebarMode,
  ThemeColorPreset,
  ThemeMode,
} from '../../shared/models/theme.models';

/** Also read by assets/theme-init.js, which applies the saved mode before the first paint. */
export const LAYOUT_STORAGE_KEY = 'education_portal_layout_prefs_v1';

const DEFAULT_PREFERENCES: LayoutPreferences = {
  themeMode: 'light',
  sidebarMode: 'full',
  cardStyle: 'shadow',
  direction: 'ltr',
};

export const THEME_COLOR_PRESETS: readonly ThemeColorPreset[] = ['cyan', 'blue', 'purple', 'ocean', 'teal', 'coral'];

/**
 * The theme's layout state (from the MaterialM theme in `Theme Angular`). Light or dark mode, sidebar type, direction
 * and card style are the user's own preferences and are kept in local storage. The accent colour preset is the school's
 * and is set by BrandingService, so it is not stored here.
 */
@Injectable({ providedIn: 'root' })
export class LayoutService {
  private readonly document = inject(DOCUMENT);
  private readonly breakpointObserver = inject(BreakpointObserver);

  private readonly preferences = signal<LayoutPreferences>(this.loadPreferences());
  private readonly preset = signal<ThemeColorPreset>('cyan');

  readonly isMobile = signal(false);
  readonly isCompactViewport = signal(false);
  readonly mobileDrawerOpen = signal(false);
  readonly customizerOpen = signal(false);

  readonly themeMode = computed(() => this.preferences().themeMode);
  readonly colorPreset = this.preset.asReadonly();
  readonly sidebarMode = computed(() => this.preferences().sidebarMode);
  readonly cardStyle = computed(() => this.preferences().cardStyle);
  readonly direction = computed(() => this.preferences().direction);
  readonly isDark = computed(() => this.themeMode() === 'dark');

  /** Effective desktop mini-rail state (manual mini OR automatic compact <=1180px) */
  readonly isMiniSidebar = computed(
    () => !this.isMobile() && (this.sidebarMode() === 'mini' || this.isCompactViewport())
  );

  readonly effectiveSidebarWidth = computed(() => {
    if (this.isMobile()) return 265;
    return this.isMiniSidebar() ? 80 : 265;
  });

  constructor() {
    this.breakpointObserver
      .observe(['(max-width: 768px)', '(max-width: 1180px)'])
      .pipe(takeUntilDestroyed())
      .subscribe((result) => {
        const mobile = result.breakpoints['(max-width: 768px)'] ?? false;
        const compact = result.breakpoints['(max-width: 1180px)'] ?? false;
        this.isMobile.set(mobile);
        this.isCompactViewport.set(compact);
        if (!mobile) {
          this.mobileDrawerOpen.set(false);
        }
      });

    effect(() => {
      const prefs = this.preferences();
      this.applyDocumentTheme(prefs);
      this.savePreferences(prefs);
    });

    effect(() => {
      const preset = this.preset();
      const html = this.document.documentElement;
      for (const option of THEME_COLOR_PRESETS) {
        html.classList.toggle(`theme-${option}`, preset === option);
      }
    });
  }

  toggleThemeMode(): void {
    this.setThemeMode(this.isDark() ? 'light' : 'dark');
  }

  setThemeMode(mode: ThemeMode): void {
    this.preferences.update((prev) => ({ ...prev, themeMode: mode }));
  }

  /** Called by BrandingService with the school's preset. */
  setColorPreset(preset: ThemeColorPreset): void {
    this.preset.set(preset);
  }

  toggleSidebar(): void {
    if (this.isMobile()) {
      this.mobileDrawerOpen.update((open) => !open);
      return;
    }
    const next: SidebarMode = this.isMiniSidebar() ? 'full' : 'mini';
    if (this.isCompactViewport() && next === 'full') {
      // Allow desktop users on ~1024px to explicitly expand if toggled
      this.isCompactViewport.set(false);
    }
    this.preferences.update((prev) => ({ ...prev, sidebarMode: next }));
  }

  setSidebarMode(mode: SidebarMode): void {
    if (mode === 'full') {
      this.isCompactViewport.set(false);
    }
    this.preferences.update((prev) => ({ ...prev, sidebarMode: mode }));
  }

  setCardStyle(cardStyle: CardSurfaceStyle): void {
    this.preferences.update((prev) => ({ ...prev, cardStyle }));
  }

  setDirection(direction: LayoutDirection): void {
    this.preferences.update((prev) => ({ ...prev, direction }));
  }

  closeMobileDrawer(): void {
    this.mobileDrawerOpen.set(false);
  }

  toggleCustomizer(open?: boolean): void {
    this.customizerOpen.update((curr) => (open !== undefined ? open : !curr));
  }

  resetPreferences(): void {
    this.preferences.set(DEFAULT_PREFERENCES);
  }

  private applyDocumentTheme(prefs: LayoutPreferences): void {
    const html = this.document.documentElement;
    html.classList.toggle('dark-theme', prefs.themeMode === 'dark');
    html.classList.toggle('card-bordered', prefs.cardStyle === 'border');
    html.setAttribute('dir', prefs.direction);
  }

  private loadPreferences(): LayoutPreferences {
    try {
      const raw = globalThis.localStorage?.getItem(LAYOUT_STORAGE_KEY);
      if (!raw) return DEFAULT_PREFERENCES;
      const parsed = JSON.parse(raw) as Partial<LayoutPreferences>;
      return { ...DEFAULT_PREFERENCES, ...parsed };
    } catch {
      // Storage can be unavailable (private mode) or hold an old value; the defaults then apply.
      return DEFAULT_PREFERENCES;
    }
  }

  private savePreferences(prefs: LayoutPreferences): void {
    try {
      globalThis.localStorage?.setItem(LAYOUT_STORAGE_KEY, JSON.stringify(prefs));
    } catch {
      // Storage can be unavailable (private mode); the preferences then last for this visit only.
    }
  }
}
