import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, map, tap } from 'rxjs/operators';
import { ModuleCode, ModulesClient } from '../../web-api-client';

/** Codes of the portal's modules: the generated `ModuleCode` enum, which holds the names the server sends. */
export const MODULE_CODES = {
  school: ModuleCode.School,
  campus: ModuleCode.Campus,
  academicSetup: ModuleCode.AcademicSetup,
  usersRoles: ModuleCode.UsersRoles,
  students: ModuleCode.Students,
  staff: ModuleCode.Staff,
  admissions: ModuleCode.Admissions,
  attendance: ModuleCode.Attendance,
  fees: ModuleCode.Fees,
  parentCommunication: ModuleCode.ParentCommunication,
  exams: ModuleCode.Exams,
  timetable: ModuleCode.Timetable,
  hrPayroll: ModuleCode.HrPayroll,
  transport: ModuleCode.Transport,
  library: ModuleCode.Library,
  lms: ModuleCode.Lms,
  hostel: ModuleCode.Hostel,
} as const;

/**
 * The modules switched on for the current school, so the app shows only those. The server checks the module again on
 * every request (SEC-AUTHZ-01); this list is only for the menu and routes.
 */
@Injectable({ providedIn: 'root' })
export class ModulesService {
  private readonly client = inject(ModulesClient);
  private readonly keys = signal<ReadonlySet<ModuleCode>>(new Set());

  readonly enabled = this.keys.asReadonly();
  readonly count = computed(() => this.keys().size);

  isEnabled(code: ModuleCode): boolean {
    return this.keys().has(code);
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
