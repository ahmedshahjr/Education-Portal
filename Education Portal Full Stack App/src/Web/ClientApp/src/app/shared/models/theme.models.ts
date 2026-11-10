export type ThemeMode = 'light' | 'dark';
export type ThemeColorPreset = 'cyan' | 'blue' | 'purple' | 'ocean' | 'teal' | 'coral';
export type SidebarMode = 'full' | 'mini';
export type CardSurfaceStyle = 'shadow' | 'border';
export type LayoutDirection = 'ltr' | 'rtl';

/** The user's own display preferences. The colour preset is the school's and is not part of them. */
export interface LayoutPreferences {
  readonly themeMode: ThemeMode;
  readonly sidebarMode: SidebarMode;
  readonly cardStyle: CardSurfaceStyle;
  readonly direction: LayoutDirection;
}
