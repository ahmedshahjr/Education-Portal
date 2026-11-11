import { DOCUMENT } from '@angular/common';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { BrandingClient, BrandingDto, BrandingImageKind } from '../../web-api-client';
import { ACCENT_PALETTE_PROPERTIES, accentPalette } from './accent-palette';
import { SIDEBAR_PALETTE_PROPERTIES, sidebarPalette } from './sidebar-palette';

const DEFAULT_SCHOOL_NAME = 'Education Portal';
const DEFAULT_FAVICON = 'favicon.png';

/** The URL of a school's logo or favicon. The version changes with each upload, so a new image is never served stale. */
export function brandingImageUrl(kind: BrandingImageKind, version: string | null | undefined): string | null {
  return version ? `api/Branding/images/${kind}?v=${encodeURIComponent(version)}` : null;
}

/**
 * The branding of the school that owns this domain. It is loaded before the app starts (the server picks the school
 * from the host), so the login page already shows the school's name, logo, favicon, accent colour and sidebar colour.
 */
@Injectable({ providedIn: 'root' })
export class BrandingService {
  private readonly client = inject(BrandingClient);
  private readonly document = inject(DOCUMENT);
  private readonly title = inject(Title);

  private readonly state = signal<BrandingDto | null>(null);

  readonly branding = this.state.asReadonly();
  readonly schoolName = computed(() => this.state()?.schoolName ?? DEFAULT_SCHOOL_NAME);
  readonly isMaster = computed(() => this.state()?.isMaster ?? false);
  readonly logoUrl = computed(() => brandingImageUrl(BrandingImageKind.Logo, this.state()?.logoVersion));

  /** Loads and applies the branding. A failure keeps the theme's defaults, so the app still starts. */
  load(): Observable<void> {
    return this.client.getBranding().pipe(
      tap((branding) => this.apply(branding)),
      map(() => undefined),
      catchError(() => of(undefined))
    );
  }

  apply(branding: BrandingDto): void {
    this.state.set(branding);
    this.applyAccentColor(branding.accentColor);
    this.applySidebarColor(branding.sidebarColor ?? null);
    this.applyFavicon(brandingImageUrl(BrandingImageKind.Favicon, branding.faviconVersion));
    this.title.setTitle(branding.schoolName);
  }

  private applyAccentColor(color: string): void {
    const style = this.document.documentElement.style;
    const palette = accentPalette(color);

    for (const property of ACCENT_PALETTE_PROPERTIES) {
      if (palette) {
        style.setProperty(property, palette[property]);
      } else {
        style.removeProperty(property);
      }
    }
  }

  private applySidebarColor(color: string | null): void {
    const style = this.document.documentElement.style;
    const palette = color ? sidebarPalette(color) : null;

    for (const property of SIDEBAR_PALETTE_PROPERTIES) {
      if (palette) {
        style.setProperty(property, palette[property]);
      } else {
        style.removeProperty(property);
      }
    }
  }

  private applyFavicon(url: string | null): void {
    let link = this.document.querySelector<HTMLLinkElement>('link[rel="icon"]');
    if (!link) {
      link = this.document.createElement('link');
      link.rel = 'icon';
      this.document.head.appendChild(link);
    }

    link.removeAttribute('type');
    link.href = url ?? DEFAULT_FAVICON;
  }
}
