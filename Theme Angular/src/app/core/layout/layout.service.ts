import { BreakpointObserver } from '@angular/cdk/layout';
import { DOCUMENT } from '@angular/common';
import { computed, effect, inject, Injectable, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  CardSurfaceStyle,
  LayoutDirection,
  LayoutPreferences,
  SidebarMode,
  ThemeColorOption,
  ThemeColorPreset,
  ThemeMode,
} from '../../shared/models/theme.models';

const STORAGE_KEY = 'materialm_layout_prefs_v1';

const DEFAULT_PREFERENCES: LayoutPreferences = {
  themeMode: 'light',
  colorPreset: 'cyan',
  sidebarMode: 'full',
  cardStyle: 'shadow',
  direction: 'ltr',
};

export const THEME_COLOR_OPTIONS: readonly ThemeColorOption[] = [
  { id: 'cyan', label: 'Aqua Cyan', hex: '#00a1ff' },
  { id: 'blue', label: 'Cobalt Blue', hex: '#0074ba' },
  { id: 'purple', label: 'Royal Violet', hex: '#763ebd' },
  { id: 'ocean', label: 'Ocean Teal', hex: '#0a7ea4' },
  { id: 'teal', label: 'Mint Green', hex: '#00ceb6' },
  { id: 'coral', label: 'Warm Coral', hex: '#fa896b' },
];

@Injectable({ providedIn: 'root' })
export class LayoutService {
  private readonly document = inject(DOCUMENT);
  private readonly breakpointObserver = inject(BreakpointObserver);

  private readonly preferences = signal<LayoutPreferences>(this.loadPreferences());

  readonly isMobile = signal(false);
  readonly isCompactViewport = signal(false);
  readonly mobileDrawerOpen = signal(false);
  readonly customizerOpen = signal(false);
  readonly searchOpen = signal(false);

  readonly themeMode = computed(() => this.preferences().themeMode);
  readonly colorPreset = computed(() => this.preferences().colorPreset);
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
  }

  toggleThemeMode(): void {
    this.setThemeMode(this.isDark() ? 'light' : 'dark');
  }

  setThemeMode(mode: ThemeMode): void {
    this.preferences.update((prev) => ({ ...prev, themeMode: mode }));
  }

  setColorPreset(preset: ThemeColorPreset): void {
    this.preferences.update((prev) => ({ ...prev, colorPreset: preset }));
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

  toggleSearch(open?: boolean): void {
    this.searchOpen.update((curr) => (open !== undefined ? open : !curr));
  }

  resetPreferences(): void {
    this.preferences.set(DEFAULT_PREFERENCES);
  }

  private applyDocumentTheme(prefs: LayoutPreferences): void {
    const html = this.document.documentElement;
    if (!html) return;

    html.classList.toggle('dark-theme', prefs.themeMode === 'dark');
    html.classList.toggle('card-bordered', prefs.cardStyle === 'border');
    html.setAttribute('dir', prefs.direction);

    for (const opt of THEME_COLOR_OPTIONS) {
      html.classList.toggle(`theme-${opt.id}`, prefs.colorPreset === opt.id);
    }
  }

  private loadPreferences(): LayoutPreferences {
    try {
      const raw = globalThis.localStorage?.getItem(STORAGE_KEY);
      if (!raw) return DEFAULT_PREFERENCES;
      const parsed = JSON.parse(raw) as Partial<LayoutPreferences>;
      return { ...DEFAULT_PREFERENCES, ...parsed };
    } catch {
      return DEFAULT_PREFERENCES;
    }
  }

  private savePreferences(prefs: LayoutPreferences): void {
    try {
      globalThis.localStorage?.setItem(STORAGE_KEY, JSON.stringify(prefs));
    } catch {
      // Ignore storage failures in private browsing or restricted test environments
    }
  }
}
