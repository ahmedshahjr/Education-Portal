import { describe, expect, it } from 'vitest';
import { accentPalette } from './accent-palette';

describe('accentPalette', () => {
  it('builds the primary colour, its RGB triple and a darker hover colour', () => {
    expect(accentPalette('#00A1FF')).toEqual({
      '--mm-primary': '#00a1ff',
      '--mm-primary-rgb': '0, 161, 255',
      '--mm-primary-hover': '#008ee0',
    });
  });

  it('refuses anything that is not a #rrggbb colour', () => {
    expect(accentPalette('orange')).toBeNull();
    expect(accentPalette('#123456; background: url(x)')).toBeNull();
  });
});
