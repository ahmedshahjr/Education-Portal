/** The image types the gallery accepts (the server reads the type from the bytes and refuses any other). */
export const PHOTO_TYPES: readonly string[] = ['image/png', 'image/jpeg', 'image/webp'];

/** The largest photo, in bytes (2 MB). */
export const PHOTO_MAX_BYTES = 2 * 1024 * 1024;

export const PHOTO_CAPTION_MAX_LENGTH = 200;

/** Why a file cannot be added to the gallery, in the page's language, or `null` when it can. The server checks again. */
export function checkPhoto(file: Pick<File, 'type' | 'size'>): string | null {
  if (!PHOTO_TYPES.includes(file.type)) {
    return $localize`:@@schoolPage.photoType:The photo must be a PNG, JPEG or WebP image.`;
  }

  if (file.size > PHOTO_MAX_BYTES) {
    return $localize`:@@schoolPage.photoSize:The photo must be at most 2 MB.`;
  }

  if (file.size === 0) {
    return $localize`:@@schoolPage.photoEmpty:The photo is empty.`;
  }

  return null;
}
