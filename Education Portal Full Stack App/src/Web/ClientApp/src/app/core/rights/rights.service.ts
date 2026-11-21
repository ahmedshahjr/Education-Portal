import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { AccessRight, ModuleCode, MyRightsDto, UsersClient } from '../../web-api-client';

/**
 * The signed-in user's own rights (ADR-022, R1.6): which rights their roles give in each module, and on which campuses.
 * Only for the menu and the route guards. The server checks every request again; hiding a menu item is not security.
 */
@Injectable({ providedIn: 'root' })
export class RightsService {
  private readonly users = inject(UsersClient);
  private readonly rights = signal<MyRightsDto | null>(null);

  readonly loaded = computed(() => this.rights() !== null);

  /** True when the signed-in user's person is a parent: only parents see "My children" (R5.3). */
  readonly isParent = computed(() => this.rights()?.isParent ?? false);

  /** Loads the rights after sign-in. A failure leaves the user with no rights (fails closed). */
  load(): Observable<void> {
    return this.users.getMyRights().pipe(
      tap((rights) => this.rights.set(rights)),
      map(() => undefined),
      catchError(() => {
        this.rights.set(null);
        return of(undefined);
      })
    );
  }

  clear(): void {
    this.rights.set(null);
  }

  /** True when the user has the right in at least one module (for features that span modules, such as the change history). */
  canAnywhere(right: AccessRight): boolean {
    return (this.rights()?.modules ?? []).some((m) => m.rights.some((r) => r.right === right));
  }

  /**
   * True when the user has the right in the module. Without a campus: on at least one campus (the same question the server
   * asks first). With a campus id: only where that campus is among the user's campuses for the right, or every campus is.
   * Unknown module or right: false.
   */
  can(module: ModuleCode, right: AccessRight, campusId?: string): boolean {
    const modules = this.rights()?.modules ?? [];
    const grant = modules
      .find((m) => m.module === module)
      ?.rights.find((r) => r.right === right);
    if (!grant) {
      return false;
    }
    if (campusId === undefined || grant.allCampuses) {
      return true;
    }
    return grant.campusIds.includes(campusId);
  }
}
