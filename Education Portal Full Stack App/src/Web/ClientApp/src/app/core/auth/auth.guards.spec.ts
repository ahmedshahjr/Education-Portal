import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, UrlTree } from '@angular/router';
import { describe, expect, it } from 'vitest';
import { authGuard, signedInGuard } from './auth.guards';
import { AuthService } from './auth.service';

function withUser(user: { authenticated: boolean; mustChangePassword: boolean }) {
  TestBed.configureTestingModule({
    providers: [
      provideRouter([]),
      { provide: AuthService, useValue: { isAuthenticated: () => user.authenticated, mustChangePassword: () => user.mustChangePassword } },
    ],
  });
}

const run = (guard: typeof authGuard) =>
  TestBed.runInInjectionContext(() => guard({} as never, { url: '/schools' } as never)) as boolean | UrlTree;

const path = (result: boolean | UrlTree) => (result instanceof UrlTree ? TestBed.inject(Router).serializeUrl(result) : result);

describe('authGuard', () => {
  it('lets a signed-in user in', () => {
    withUser({ authenticated: true, mustChangePassword: false });
    expect(run(authGuard)).toBe(true);
  });

  it('sends a visitor to the login page and back afterwards', () => {
    withUser({ authenticated: false, mustChangePassword: false });
    expect(path(run(authGuard))).toBe('/login?returnUrl=%2Fschools');
  });

  it('sends an account with a one-time password to the change-password page', () => {
    withUser({ authenticated: true, mustChangePassword: true });
    expect(path(run(authGuard))).toBe('/change-password');
  });
});

describe('signedInGuard', () => {
  it('lets an account with a one-time password reach the change-password page', () => {
    withUser({ authenticated: true, mustChangePassword: true });
    expect(run(signedInGuard)).toBe(true);
  });

  it('sends a visitor to the login page', () => {
    withUser({ authenticated: false, mustChangePassword: false });
    expect(path(run(signedInGuard))).toBe('/login?returnUrl=%2Fschools');
  });
});
