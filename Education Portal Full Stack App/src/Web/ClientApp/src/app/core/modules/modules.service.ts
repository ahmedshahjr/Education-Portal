import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { ModulesClient } from '../../web-api-client';

/** Keys of the portal's modules, as the server names them (`ModuleKeys` in the Domain project). */
export const MODULE_KEYS = {
  branches: 'Branches',
  usersAndRoles: 'UsersAndRoles',
  students: 'Students',
  parents: 'Parents',
  teachers: 'Teachers',
  courses: 'Courses',
  enrolments: 'Enrolments',
  grades: 'Grades',
  dashboard: 'Dashboard',
} as const;

/**
 * The modules switched on for the current school, so the app shows only those. The server checks the module again on
 * every request (SEC-AUTHZ-01); this list is only for the menu and routes.
 */
@Injectable({ providedIn: 'root' })
export class ModulesService {
  private readonly client = inject(ModulesClient);
  private readonly keys = signal<ReadonlySet<string>>(new Set());

  readonly enabled = this.keys.asReadonly();
  readonly count = computed(() => this.keys().size);

  isEnabled(key: string): boolean {
    return this.keys().has(key);
  }

  /** Loads the list for the signed-in user. On an error nothing is shown, since the server would refuse anyway. */
  load(): Observable<void> {
    return this.client.getEnabledModules().pipe(
      tap((keys) => this.keys.set(new Set(keys))),
      catchError(() => {
        this.clear();
        return of(undefined);
      }),
      map(() => undefined)
    );
  }

  clear(): void {
    this.keys.set(new Set());
  }
}
