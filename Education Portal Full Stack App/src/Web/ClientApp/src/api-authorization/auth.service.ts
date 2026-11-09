import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { catchError, map, switchMap, tap } from 'rxjs/operators';
import { SignInCommand, UsersClient } from '../app/web-api-client';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private _isAuthenticated = new BehaviorSubject<boolean>(false);
  isAuthenticated$ = this._isAuthenticated.asObservable();

  constructor(private usersClient: UsersClient) {}

  /**
   * Asks the server who is signed in and updates the signed-in state. The answer also gives a fresh antiforgery token
   * for this visitor, because a token is bound to the identity it was issued for (SEC-WEB-03).
   */
  initialize(): Observable<boolean> {
    return this.usersClient.getCurrentUser().pipe(
      map(() => true),
      catchError(() => of(false)),
      tap(isAuth => this._isAuthenticated.next(isAuth))
    );
  }

  signIn(email: string, password: string): Observable<void> {
    return this.usersClient.signIn(new SignInCommand({ email, password })).pipe(
      switchMap(() => this.initialize()),
      map(() => void 0)
    );
  }

  signOut(): Observable<void> {
    return this.usersClient.signOut().pipe(
      switchMap(() => this.initialize()),
      map(() => void 0)
    );
  }
}
