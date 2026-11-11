/** The CSS custom properties a school's accent colour sets. They override the theme's default primary colour. */
export interface AccentPalette {
  readonly '--mm-primary': string;
  readonly '--mm-primary-rgb': string;
  readonly '--mm-primary-hover': string;
}

export const ACCENT_PALETTE_PROPERTIES: readonly (keyof AccentPalette)[] = ['--mm-primary', '--mm-primary-rgb', '--mm-primary-hover'];

const HEX_COLOR = /^#[0-9a-f]{6}$/i;
const HOVER_SHADE = 0.88; // matches the theme's own hover colours, for example #008ee0 for cyan.

/**
 * Builds the accent colours from one accent colour: the primary colour, its RGB triple (for translucent tints) and a
 * slightly darker hover colour. Returns null for anything that is not #rrggbb, so no other value reaches a CSS property.
 */
export function accentPalette(accent: string): AccentPalette | null {
  if (!HEX_COLOR.test(accent)) {
    return null;
  }

  const channels = [1, 3, 5].map((start) => parseInt(accent.slice(start, start + 2), 16));
  const hover = channels.map((channel) => Math.round(channel * HOVER_SHADE).toString(16).padStart(2, '0'));

  return {
    '--mm-primary': accent.toLowerCase(),
    '--mm-primary-rgb': channels.join(', '),
    '--mm-primary-hover': `#${hover.join('')}`,
  };
}
