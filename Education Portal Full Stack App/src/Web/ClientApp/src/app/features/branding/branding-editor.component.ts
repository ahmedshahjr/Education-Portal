import { ChangeDetectionStrategy, Component, computed, input, linkedSignal, output, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { sidebarPalette } from '../../core/branding/sidebar-palette';
import { brandingImageUrl } from '../../core/branding/branding.service';
import { AppIconComponent } from '../../shared/components/app-icon/app-icon.component';
import { BrandingDto, BrandingImageKind } from '../../web-api-client';
import { BrandingChanges, FAVICON_TYPES, fileProblem, LOGO_TYPES } from './branding-changes';

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
  readonly branding = input.required<BrandingDto>();
  /** Where the current images are read from: the school's own domain, or nowhere for another school (master portal). */
  readonly showCurrentImages = input(true);
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
    this.showCurrentImages() && !this.removeLogo() ? brandingImageUrl(BrandingImageKind.Logo, this.branding().logoVersion) : null
  );
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
