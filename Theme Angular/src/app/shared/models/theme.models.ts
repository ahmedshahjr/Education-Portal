export type ThemeMode = 'light' | 'dark';
export type ThemeColorPreset = 'cyan' | 'blue' | 'purple' | 'ocean' | 'teal' | 'coral';
export type SidebarMode = 'full' | 'mini';
export type CardSurfaceStyle = 'shadow' | 'border';
export type LayoutDirection = 'ltr' | 'rtl';

export interface ThemeColorOption {
  readonly id: ThemeColorPreset;
  readonly label: string;
  readonly hex: string;
}

export interface LayoutPreferences {
  readonly themeMode: ThemeMode;
  readonly colorPreset: ThemeColorPreset;
  readonly sidebarMode: SidebarMode;
  readonly cardStyle: CardSurfaceStyle;
  readonly direction: LayoutDirection;
}
