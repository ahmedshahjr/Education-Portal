import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { ChangePasswordCommand, CurrentUserDto, SignInCommand, UsersClient } from '../../web-api-client';
import { ModulesService } from '../modules/modules.service';

export const ADMINISTRATOR_ROLE = 'Administrator';

/** The signed-in user, as the server reports it. Hiding things in the UI is only a convenience; the server decides. */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly users = inject(UsersClient);
  private readonly modules = inject(ModulesService);
  private readonly user = signal<CurrentUserDto | null>(null);

  readonly currentUser = this.user.asReadonly();
  readonly isAuthenticated = computed(() => this.user() !== null);
  readonly isAdministrator = computed(() => this.user()?.roles?.includes(ADMINISTRATOR_ROLE) ?? false);

  /** True while the account still has its one-time password: the app then shows only the change-password page. */
  readonly mustChangePassword = computed(() => this.user()?.mustChangePassword ?? false);

  /**
   * Asks the server who is signed in. The answer also gives a fresh antiforgery token for this visitor, because a token
   * is bound to the identity it was issued for (SEC-WEB-03).
   */
  initialize(): Observable<boolean> {
    return this.users.getCurrentUser().pipe(
      tap((user) => this.user.set(user)),
      // The modules switched on for the school decide what the menu shows. An account that must change its one-time
      // password can't read them yet (the server refuses), and sees no menu until it has.
      switchMap((user) => {
        if (user.mustChangePassword) {
          this.modules.clear();
          return of(undefined);
        }
        return this.modules.load();
      }),
      map(() => true),
      catchError(() => {
        this.user.set(null);
        this.modules.clear();
        return of(false);
      })
    );
  }

  signIn(email: string, password: string): Observable<void> {
    return this.users.signIn(new SignInCommand({ email, password })).pipe(
      switchMap(() => this.initialize()),
      map(() => undefined)
    );
  }

  /** Replaces the password, then reloads the account, which ends the one-time-password block. */
  changePassword(currentPassword: string, newPassword: string): Observable<void> {
    return this.users.changePassword(new ChangePasswordCommand({ currentPassword, newPassword })).pipe(
      switchMap(() => this.initialize()),
      map(() => undefined)
    );
  }

  signOut(): Observable<void> {
    return this.users.signOut().pipe(
      switchMap(() => this.initialize()),
      map(() => undefined)
    );
  }
}
