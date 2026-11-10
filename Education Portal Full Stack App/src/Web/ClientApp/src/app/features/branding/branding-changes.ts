import { ThemePreset } from '../../web-api-client';

/** What the branding editor asks to save. Files are checked again by the server, by content (SEC-INPUT-04). */
export interface BrandingChanges {
  readonly themePreset: ThemePreset;
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
  const [types, maxBytes, formats] =
    kind === 'logo' ? [LOGO_TYPES, MAX_LOGO_BYTES, 'PNG, JPEG or WebP'] : [FAVICON_TYPES, MAX_FAVICON_BYTES, 'ICO or PNG'];

  if (!types.includes(file.type)) {
    return `The ${kind} must be a ${formats} image.`;
  }

  if (file.size > maxBytes) {
    return `The ${kind} must be at most ${maxBytes / 1024} KB.`;
  }

  return null;
}

/** Turns the changes into the generated client's file parameters. */
export function toFileParameter(file: File | null): FileParameter | null {
  return file ? { data: file, fileName: file.name } : null;
}
