import { ChangeDetectionStrategy, Component, computed, inject, input, linkedSignal, output, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { brandingImageUrl } from '../../core/branding/branding.service';
import { accentPalette, AccentPalette } from '../../core/branding/accent-palette';
import { contrastRatio, sidebarPalette } from '../../core/branding/sidebar-palette';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { BrandingDto, BrandingImageKind } from '../../web-api-client';
import { BrandingChanges, FAVICON_TYPES, fileProblem, LOGO_TYPES } from './branding-changes';
import { ImageViewerComponent, ImageViewerData } from './image-viewer.component';

const VIEW_LOGO = $localize`:@@branding.viewLogo:View logo`;
const VIEW_FAVICON = $localize`:@@branding.viewFavicon:View favicon`;
const DEFAULT_ACCENT_COLOR = '#00a1ff';
const DEFAULT_SIDEBAR_COLOR = '#1f2a3d';
const HEX_COLOR = /^#[0-9a-f]{6}$/i;

/**
 * Edits one school's branding: accent colour, sidebar colour (with a colour picker), logo and favicon. Used for a
 * school's own branding and, in the master portal, for any school. It only collects the changes; the page saves them.
 */
@Component({
  selector: 'app-branding-editor',
  imports: [MatCardModule, MatButtonModule, MatFormFieldModule, MatInputModule, MatSlideToggleModule, AppIconComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: './branding-editor.component.html',
  styleUrl: './branding-editor.component.scss',
})
export class BrandingEditorComponent {
  private readonly dialog = inject(MatDialog);

  readonly branding = input.required<BrandingDto>();
  /** Where the current logo and favicon are read from: this school's own, or any school's in the master portal. */
  readonly imagesBase = input('api/Branding/images');
  readonly saving = input(false);
  readonly errors = input<readonly string[]>([]);

  readonly save = output<BrandingChanges>();

  protected readonly logoAccept = LOGO_TYPES.join(',');
  protected readonly faviconAccept = ['.ico', ...FAVICON_TYPES].join(',');

  protected readonly accentColor = linkedSignal(() => this.branding().accentColor || DEFAULT_ACCENT_COLOR);
  protected readonly customSidebar = linkedSignal(() => !!this.branding().sidebarColor);
  protected readonly sidebarColor = linkedSignal(() => this.branding().sidebarColor ?? DEFAULT_SIDEBAR_COLOR);
  protected readonly logo = linkedSignal<BrandingDto, File | null>({ source: this.branding, computation: () => null });
  protected readonly logoPreview = linkedSignal<BrandingDto, string | null>({ source: this.branding, computation: () => null });
  protected readonly removeLogo = linkedSignal({ source: this.branding, computation: () => false });
  protected readonly favicon = linkedSignal<BrandingDto, File | null>({ source: this.branding, computation: () => null });
  protected readonly removeFavicon = linkedSignal({ source: this.branding, computation: () => false });
  protected readonly fileError = signal<string | null>(null);

  protected readonly accentValid = computed(() => HEX_COLOR.test(this.accentColor()));
  protected readonly sidebarValid = computed(() => !this.customSidebar() || HEX_COLOR.test(this.sidebarColor()));

  protected readonly currentLogoUrl = computed(() =>
    this.logo() || this.removeLogo() ? null : brandingImageUrl(BrandingImageKind.Logo, this.branding().logoVersion, this.imagesBase())
  );
  protected readonly currentFaviconUrl = computed(() =>
    this.favicon() || this.removeFavicon()
      ? null
      : brandingImageUrl(BrandingImageKind.Favicon, this.branding().faviconVersion, this.imagesBase())
  );

  /** The accent colour as CSS properties for the preview, so it follows the picker as the user changes it. */
  protected readonly previewAccent = computed<Partial<AccentPalette> & Record<string, string>>(() => {
    const palette = this.accentValid() ? accentPalette(this.accentColor()) : null;
    if (!palette) {
      return {};
    }

    // Text on the accent is black or white, whichever reads better on it (WCAG 2.2 contrast ratio).
    const text = contrastRatio(this.accentColor(), '#ffffff') >= 4.5 ? '#ffffff' : '#000000';
    return { ...palette, '--mm-branding-preview-text': text };
  });
  protected readonly hasLogo = computed(() => !!this.logo() || (!!this.branding().logoVersion && !this.removeLogo()));
  protected readonly hasFavicon = computed(
    () => !!this.favicon() || (!!this.branding().faviconVersion && !this.removeFavicon())
  );

  /** A live preview of the sidebar with the chosen colour; the text colour is picked for contrast. */
  protected readonly preview = computed(() => {
    const palette = this.customSidebar() && this.sidebarValid() ? sidebarPalette(this.sidebarColor()) : null;
    return {
      background: palette?.['--mm-sidebar-bg'] ?? 'var(--mm-bg-surface)',
      text: palette?.['--mm-sidebar-text'] ?? 'var(--mm-text-primary)',
      logo: this.logoPreview() ?? this.currentLogoUrl(),
    };
  });

  protected setAccentColor(event: Event): void {
    this.accentColor.set((event.target as HTMLInputElement).value.trim());
  }

  protected setSidebarColor(event: Event): void {
    this.sidebarColor.set((event.target as HTMLInputElement).value.trim());
  }

  /** Opens the logo or favicon at full size: the one chosen now, or else the school's current one. */
  protected viewImage(kind: 'logo' | 'favicon'): void {
    const src = kind === 'logo' ? (this.logoPreview() ?? this.currentLogoUrl()) : this.currentFaviconUrl();
    if (!src) {
      return;
    }

    const data: ImageViewerData = { title: kind === 'logo' ? VIEW_LOGO : VIEW_FAVICON, src };
    this.dialog.open(ImageViewerComponent, { data, maxWidth: '90vw' });
  }

  protected chooseFile(event: Event, kind: 'logo' | 'favicon'): void {
    const inputElement = event.target as HTMLInputElement;
    const file = inputElement.files?.item(0) ?? null;
    inputElement.value = '';

    if (!file) {
      return;
    }

    const problem = fileProblem(file, kind);
    this.fileError.set(problem);
    if (problem) {
      return;
    }

    if (kind === 'logo') {
      this.logo.set(file);
      this.removeLogo.set(false);
      this.readPreview(file);
    } else {
      this.favicon.set(file);
      this.removeFavicon.set(false);
    }
  }

  protected remove(kind: 'logo' | 'favicon'): void {
    if (kind === 'logo') {
      this.logo.set(null);
      this.logoPreview.set(null);
      this.removeLogo.set(!!this.branding().logoVersion);
    } else {
      this.favicon.set(null);
      this.removeFavicon.set(!!this.branding().faviconVersion);
    }
  }

  protected submit(): void {
    if (!this.accentValid() || !this.sidebarValid()) {
      return;
    }

    this.save.emit({
      accentColor: this.accentColor().toLowerCase(),
      sidebarColor: this.customSidebar() ? this.sidebarColor().toLowerCase() : null,
      logo: this.logo(),
      removeLogo: this.removeLogo(),
      favicon: this.favicon(),
      removeFavicon: this.removeFavicon(),
    });
  }

  /** Reads the chosen logo as a data URL; the Content-Security-Policy allows data: images but not blob: ones. */
  private readPreview(file: File): void {
    const reader = new FileReader();
    reader.onload = () => this.logoPreview.set(typeof reader.result === 'string' ? reader.result : null);
    reader.readAsDataURL(file);
  }
}
