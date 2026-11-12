/** The CSS custom properties a school's sidebar colour sets, read by the sidebar and the login page. */
export interface SidebarPalette {
  readonly '--mm-sidebar-bg': string;
  readonly '--mm-sidebar-text': string;
  readonly '--mm-sidebar-muted': string;
  readonly '--mm-sidebar-hover-bg': string;
  readonly '--mm-sidebar-active-bg': string;
  readonly '--mm-sidebar-active-text': string;
  readonly '--mm-auth-aside-bg': string;
  readonly '--mm-auth-aside-text': string;
}

export const SIDEBAR_PALETTE_PROPERTIES: readonly (keyof SidebarPalette)[] = [
  '--mm-sidebar-bg',
  '--mm-sidebar-text',
  '--mm-sidebar-muted',
  '--mm-sidebar-hover-bg',
  '--mm-sidebar-active-bg',
  '--mm-sidebar-active-text',
  '--mm-auth-aside-bg',
  '--mm-auth-aside-text',
];

const HEX_COLOR = /^#[0-9a-f]{6}$/i;
const DARK_TEXT = '#111c2d';
const BLACK_TEXT = '#000000';
const LIGHT_TEXT = '#ffffff';
const MIN_CONTRAST = 4.5;

/**
 * Builds the sidebar colours from one background colour. The text is light or dark, whichever contrasts more with the
 * background (WCAG 2.2 contrast ratio). Black or white always reaches 4.5:1 against any colour, so the theme's dark
 * text is used only when it reaches that too; any colour a school picks stays readable. Returns null for anything that is
 * not #rrggbb, so no other value can ever reach a CSS property.
 */
export function sidebarPalette(background: string): SidebarPalette | null {
  if (!HEX_COLOR.test(background)) {
    return null;
  }

  const useDarkText = contrastRatio(background, BLACK_TEXT) > contrastRatio(background, LIGHT_TEXT);
  const text = !useDarkText
    ? LIGHT_TEXT
    : contrastRatio(background, DARK_TEXT) >= MIN_CONTRAST
      ? DARK_TEXT
      : BLACK_TEXT;
  const overlay = useDarkText ? '0, 0, 0' : '255, 255, 255';

  return {
    '--mm-sidebar-bg': background,
    '--mm-sidebar-text': text,
    '--mm-sidebar-muted': `rgba(${useDarkText ? '17, 28, 45' : '255, 255, 255'}, 0.72)`,
    '--mm-sidebar-hover-bg': `rgba(${overlay}, 0.08)`,
    '--mm-sidebar-active-bg': `rgba(${overlay}, 0.16)`,
    '--mm-sidebar-active-text': text,
    '--mm-auth-aside-bg': background,
    '--mm-auth-aside-text': text,
  };
}

export function contrastRatio(first: string, second: string): number {
  const [lighter, darker] = [relativeLuminance(first), relativeLuminance(second)].sort((a, b) => b - a);
  return (lighter + 0.05) / (darker + 0.05);
}

function relativeLuminance(hex: string): number {
  const channels = [1, 3, 5].map((start) => parseInt(hex.slice(start, start + 2), 16) / 255);
  const [r, g, b] = channels.map((c) => (c <= 0.04045 ? c / 12.92 : ((c + 0.055) / 1.055) ** 2.4));
  return 0.2126 * r + 0.7152 * g + 0.0722 * b;
}
