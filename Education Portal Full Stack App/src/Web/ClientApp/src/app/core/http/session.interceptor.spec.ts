import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { sessionInterceptor } from './session.interceptor';

describe('sessionInterceptor', () => {
  let http: HttpClient;
  let testing: HttpTestingController;
  let navigate: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), provideHttpClient(withInterceptors([sessionInterceptor])), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpClient);
    testing = TestBed.inject(HttpTestingController);
    navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
  });

  afterEach(() => testing.verify());

  it('sends the session cookie with every call', () => {
    http.get('/api/Branding').subscribe();

    const request = testing.expectOne('/api/Branding');
    expect(request.request.withCredentials).toBe(true);
    request.flush({});
  });

  it('sends the visitor to the login page when the session has ended', () => {
    http.get('/api/Schools').subscribe({ error: () => undefined });

    testing.expectOne('/api/Schools').flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(navigate).toHaveBeenCalledWith(['/login'], expect.anything());
  });

  it('treats a 401 from the current-user check as a normal answer', () => {
    http.get('/api/Users/me').subscribe({ error: () => undefined });

    testing.expectOne('/api/Users/me').flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(navigate).not.toHaveBeenCalled();
  });
});
