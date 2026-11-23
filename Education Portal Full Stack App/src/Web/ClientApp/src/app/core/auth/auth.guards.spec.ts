import { Component } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router, Routes } from '@angular/router';
import { describe, expect, it } from 'vitest';
import { API_BASE_URL } from '../../web-api-client';
import { authGuard, signedInGuard } from './auth.guards';
import { AuthService } from './auth.service';

@Component({ selector: 'app-stub', template: '' })
class StubPage {}

const routes: Routes = [
  { path: 'login', component: StubPage },
  { path: 'change-password', component: StubPage, canActivate: [signedInGuard] },
  { path: 'schools', component: StubPage, canActivate: [authGuard] },
];

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const settle = async () => {
  for (let i = 0; i < 5; i++) {
    await new Promise((resolve) => setTimeout(resolve));
  }
};

/**
 * Starts the real AuthService, which asks the real API who is signed in, and answers those requests over
 * HttpTestingController. Routes and guards are the real ones, so the redirects are checked by navigating.
 */
async function visitAs(user: { authenticated: boolean; mustChangePassword: boolean }): Promise<Router> {
  TestBed.configureTestingModule({
    providers: [provideRouter(routes), provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
  });
  const auth = TestBed.inject(AuthService);
  const http = TestBed.inject(HttpTestingController);
  if (user.authenticated) {
    auth.initialize().subscribe();
    http.expectOne('/api/Users/me').flush(json({ userName: 'teacher@demo.localhost', roles: [], mustChangePassword: user.mustChangePassword }));
    // The generated client reads each body as a Blob, which completes over several macrotasks, before the next request.
    await settle();
    if (!user.mustChangePassword) {
      http.expectOne((r) => r.url.startsWith('/api/Modules/enabled')).flush(json([]));
      await settle();
      http.expectOne('/api/Users/me/rights').flush(json({ modules: [] }));
      await settle();
    }
  }
  const router = TestBed.inject(Router);
  await router.navigateByUrl(user.mustChangePassword ? '/change-password' : '/schools');
  return router;
}

describe('route guards with the real sign-in state (integration)', () => {
  it('lets a signed-in user reach a protected page', async () => {
    const router = await visitAs({ authenticated: true, mustChangePassword: false });

    expect(router.url).toBe('/schools');
  });

  it('sends a visitor to the login page and back afterwards', async () => {
    const router = await visitAs({ authenticated: false, mustChangePassword: false });

    expect(router.url).toBe('/login?returnUrl=%2Fschools');
  });

  it('sends an account with a one-time password to the change-password page', async () => {
    const router = await visitAs({ authenticated: true, mustChangePassword: true });

    expect(router.url).toBe('/change-password');
  });

  it('sends a visitor away from the change-password page to the login page', async () => {
    const router = await visitAs({ authenticated: false, mustChangePassword: false });
    await router.navigateByUrl('/change-password');

    expect(router.url).toBe('/login?returnUrl=%2Fchange-password');
  });
});
