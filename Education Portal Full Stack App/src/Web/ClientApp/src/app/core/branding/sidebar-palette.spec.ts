import { describe, expect, it } from 'vitest';
import { contrastRatio, sidebarPalette } from './sidebar-palette';

describe('sidebarPalette', () => {
  it('uses light text on a dark sidebar', () => {
    expect(sidebarPalette('#1f2a3d')?.['--mm-sidebar-text']).toBe('#ffffff');
  });

  it('uses dark text on a light sidebar', () => {
    expect(sidebarPalette('#f5f5dc')?.['--mm-sidebar-text']).toBe('#111c2d');
  });

  it('keeps the text readable (at least 4.5:1) for any colour', () => {
    for (const colour of ['#000000', '#ffffff', '#ff0000', '#00a1ff', '#777777', '#ffae1f']) {
      const palette = sidebarPalette(colour)!;
      expect(contrastRatio(colour, palette['--mm-sidebar-text'])).toBeGreaterThanOrEqual(4.5);
    }
  });

  it('refuses anything that is not #rrggbb', () => {
    expect(sidebarPalette('red')).toBeNull();
    expect(sidebarPalette('#123456; background: url(x)')).toBeNull();
  });
});
