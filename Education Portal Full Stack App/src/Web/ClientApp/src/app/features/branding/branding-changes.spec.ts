import { describe, expect, it } from 'vitest';
import { fileProblem, MAX_FAVICON_BYTES } from './branding-changes';

const file = (type: string, size: number) => new File([new Uint8Array(size)], 'x', { type });

describe('fileProblem', () => {
  it('accepts a small PNG logo', () => {
    expect(fileProblem(file('image/png', 1000), 'logo')).toBeNull();
  });

  it('refuses an SVG logo', () => {
    expect(fileProblem(file('image/svg+xml', 1000), 'logo')).toContain('PNG, JPEG or WebP');
  });

  it('refuses a favicon over the size limit', () => {
    expect(fileProblem(file('image/png', MAX_FAVICON_BYTES + 1), 'favicon')).toContain('100 KB');
  });
});
