/** What the branding editor asks to save. Files are checked again by the server, by content (SEC-INPUT-04). */
export interface BrandingChanges {
  readonly accentColor: string;
  readonly sidebarColor: string | null;
  readonly logo: File | null;
  readonly removeLogo: boolean;
  readonly favicon: File | null;
  readonly removeFavicon: boolean;
}

export const MAX_LOGO_BYTES = 512 * 1024;
export const MAX_FAVICON_BYTES = 100 * 1024;
export const LOGO_TYPES = ['image/png', 'image/jpeg', 'image/webp'];
export const FAVICON_TYPES = ['image/png', 'image/x-icon', 'image/vnd.microsoft.icon'];

/** A quick check before upload, so the user hears about a wrong file at once. The server's check is the real one. */
export function fileProblem(file: File, kind: 'logo' | 'favicon'): string | null {
  const [types, maxBytes] = kind === 'logo' ? [LOGO_TYPES, MAX_LOGO_BYTES] : [FAVICON_TYPES, MAX_FAVICON_BYTES];
  const maxKb = maxBytes / 1024;

  // One message per kind, so each language can phrase it in its own grammar.
  if (!types.includes(file.type)) {
    return kind === 'logo'
      ? $localize`:@@branding.logoType:The logo must be a PNG, JPEG or WebP image.`
      : $localize`:@@branding.faviconType:The favicon must be an ICO or PNG image.`;
  }

  if (file.size > maxBytes) {
    return kind === 'logo'
      ? $localize`:@@branding.logoSize:The logo must be at most ${maxKb}:MAX_KB: KB.`
      : $localize`:@@branding.faviconSize:The favicon must be at most ${maxKb}:MAX_KB: KB.`;
  }

  return null;
}

/** Turns the changes into the generated client's file parameters. */
export function toFileParameter(file: File | null): FileParameter | null {
  return file ? { data: file, fileName: file.name } : null;
}
