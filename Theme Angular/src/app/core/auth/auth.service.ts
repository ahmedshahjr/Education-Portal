import { computed, Injectable, signal } from '@angular/core';
import { delay, Observable, of, tap } from 'rxjs';
import { UserProfile } from '../../shared/models/dashboard.models';

const DEFAULT_USER: UserProfile = {
  id: 'usr-101',
  name: 'Mathew Anderson',
  email: 'info@materialm.io',
  role: 'Product Designer & Admin',
  company: 'MaterialM Labs',
  location: 'San Francisco, CA',
  phone: '+1 (415) 890-4312',
  bio: 'Leading SaaS product analytics, conversion tracking, and design systems at MaterialM.',
  avatarInitials: 'MA',
};

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly userSignal = signal<UserProfile>(DEFAULT_USER);
  private readonly authenticatedSignal = signal(true);
  private readonly tokenSignal = signal<string>('demo-materialm-session-token');

  readonly currentUser = this.userSignal.asReadonly();
  readonly isAuthenticated = this.authenticatedSignal.asReadonly();
  readonly token = this.tokenSignal.asReadonly();
  readonly userFirstName = computed(() => this.userSignal().name.split(' ')[0] ?? 'User');

  login(email: string, _password: string): Observable<UserProfile> {
    const cleanEmail = email.trim() || DEFAULT_USER.email;
    const updated: UserProfile = {
      ...this.userSignal(),
      email: cleanEmail,
    };
    return of(updated).pipe(
      delay(350),
      tap((user) => {
        this.userSignal.set(user);
        this.authenticatedSignal.set(true);
        this.tokenSignal.set('demo-materialm-session-token');
      })
    );
  }

  logout(): void {
    this.authenticatedSignal.set(false);
    this.tokenSignal.set('');
  }

  restoreDemoSession(): void {
    this.authenticatedSignal.set(true);
    this.tokenSignal.set('demo-materialm-session-token');
  }

  updateProfile(patch: Partial<UserProfile>): Observable<UserProfile> {
    const next: UserProfile = {
      ...this.userSignal(),
      ...patch,
    };
    return of(next).pipe(
      delay(250),
      tap((saved) => this.userSignal.set(saved))
    );
  }
}
