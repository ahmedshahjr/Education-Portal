import { computed, inject, Injectable, signal } from '@angular/core';
import { AuthService } from '../auth/auth.service';
import { BrandingService } from '../branding/branding.service';
import { CampusDto, CampusesClient } from '../../web-api-client';

/** The session key that keeps the chosen campus across page loads of the same tab. */
const STORAGE_KEY = 'mm-selected-campus';

/**
 * The campus the school's administrator is working on. The top bar shows the switcher when the school has more than one campus;
 * lists that belong to a campus follow the choice. An empty choice means every campus. The server still checks every request
 * (SEC-AUTHZ-01); this only narrows what is shown.
 */
@Injectable({ providedIn: 'root' })
export class CampusScopeService {
  private readonly campusesClient = inject(CampusesClient);
  private readonly auth = inject(AuthService);
  private readonly branding = inject(BrandingService);

  /** The school's campuses, the main campus first. Empty until loaded, or when the Campuses module is off. */
  readonly campuses = signal<readonly CampusDto[]>([]);

  /** The chosen campus's public id, or '' for every campus. */
  readonly selectedId = signal<string>(this.readStored());

  /** The switcher is shown only to a school's administrator, when the school has more than one campus. */
  readonly showSwitcher = computed(
    () => this.auth.isAdministrator() && !this.branding.isMaster() && this.campuses().length > 1
  );

  /** Loads the school's campuses for the switcher. A school without the Campuses module simply has none. */
  load(): void {
    if (!this.auth.isAdministrator() || this.branding.isMaster()) {
      return;
    }
    this.campusesClient.getCampuses().subscribe({
      next: (campuses) => {
        this.campuses.set(campuses);
        // A stored choice of a campus that no longer exists is dropped.
        if (this.selectedId() !== '' && !campuses.some((c) => c.id === this.selectedId())) {
          this.select('');
        }
      },
      error: () => this.campuses.set([]),
    });
  }

  select(campusId: string): void {
    this.selectedId.set(campusId);
    try {
      sessionStorage.setItem(STORAGE_KEY, campusId);
    } catch {
      // Storage can be unavailable (private browsing); the choice then lasts for this page only.
    }
  }

  /** Whether a campus is shown under the current choice. */
  shows(campusId: string): boolean {
    const chosen = this.selectedId();
    return chosen === '' || chosen === campusId;
  }

  private readStored(): string {
    try {
      return sessionStorage.getItem(STORAGE_KEY) ?? '';
    } catch {
      return '';
    }
  }
}
