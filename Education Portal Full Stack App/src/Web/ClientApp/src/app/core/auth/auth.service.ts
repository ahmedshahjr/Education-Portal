import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { CurrentUserDto, SignInCommand, UsersClient } from '../../web-api-client';

export const ADMINISTRATOR_ROLE = 'Administrator';

/** The signed-in user, as the server reports it. Hiding things in the UI is only a convenience; the server decides. */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly users = inject(UsersClient);
  private readonly user = signal<CurrentUserDto | null>(null);

  readonly currentUser = this.user.asReadonly();
  readonly isAuthenticated = computed(() => this.user() !== null);
  readonly isAdministrator = computed(() => this.user()?.roles?.includes(ADMINISTRATOR_ROLE) ?? false);

  /**
   * Asks the server who is signed in. The answer also gives a fresh antiforgery token for this visitor, because a token
   * is bound to the identity it was issued for (SEC-WEB-03).
   */
  initialize(): Observable<boolean> {
    return this.users.getCurrentUser().pipe(
      tap((user) => this.user.set(user)),
      map(() => true),
      catchError(() => {
        this.user.set(null);
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

  signOut(): Observable<void> {
    return this.users.signOut().pipe(
      switchMap(() => this.initialize()),
      map(() => undefined)
    );
  }
}
