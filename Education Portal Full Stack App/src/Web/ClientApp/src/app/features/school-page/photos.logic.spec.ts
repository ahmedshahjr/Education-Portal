import { describe, expect, it } from 'vitest';
import { checkPhoto, PHOTO_MAX_BYTES } from './photos.logic';

describe('checkPhoto', () => {
  it.each(['image/png', 'image/jpeg', 'image/webp'])('accepts a %s photo', (type) => {
    expect(checkPhoto({ type, size: 1000 })).toBeNull();
  });

  it('refuses any other type', () => {
    expect(checkPhoto({ type: 'image/gif', size: 1000 })).not.toBeNull();
    expect(checkPhoto({ type: 'application/pdf', size: 1000 })).not.toBeNull();
    expect(checkPhoto({ type: '', size: 1000 })).not.toBeNull();
  });

  it('accepts a photo of exactly 2 MB and refuses one byte more', () => {
    expect(checkPhoto({ type: 'image/png', size: PHOTO_MAX_BYTES })).toBeNull();
    expect(checkPhoto({ type: 'image/png', size: PHOTO_MAX_BYTES + 1 })).not.toBeNull();
  });

  it('refuses an empty file', () => {
    expect(checkPhoto({ type: 'image/png', size: 0 })).not.toBeNull();
  });
});
