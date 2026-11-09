import { TestBed } from '@angular/core/testing';
import { HTTP_INTERCEPTORS, HttpClient, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { AntiforgeryInterceptor, readCookie } from './antiforgery.interceptor';

describe('readCookie', () => {
  it('finds the named cookie among others', () => {
    expect(readCookie('a=1; __Host-XSRF-TOKEN=abc; b=2', '__Host-XSRF-TOKEN')).toBe('abc');
  });

  it('returns undefined when the cookie is absent', () => {
    expect(readCookie('a=1', '__Host-XSRF-TOKEN')).toBeUndefined();
  });

  it('does not match a cookie whose name only starts with the same text', () => {
    expect(readCookie('__Host-XSRF-TOKEN2=abc', '__Host-XSRF-TOKEN')).toBeUndefined();
  });
});

describe('AntiforgeryInterceptor', () => {
  const url = 'https://localhost/api/Users/logout';
  let http: HttpClient;
  let testing: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting(),
        { provide: HTTP_INTERCEPTORS, useClass: AntiforgeryInterceptor, multi: true }
      ]
    });
    http = TestBed.inject(HttpClient);
    testing = TestBed.inject(HttpTestingController);
    vi.spyOn(document, 'cookie', 'get').mockReturnValue('__Host-XSRF-TOKEN=abc');
  });

  afterEach(() => {
    vi.restoreAllMocks();
    testing.verify();
  });

  it('sends the token in the header of an unsafe request', () => {
    http.post(url, {}).subscribe();

    const request = testing.expectOne(url);
    expect(request.request.headers.get('X-XSRF-TOKEN')).toBe('abc');
    request.flush({});
  });

  it('does not send the token with a safe request', () => {
    http.get(url).subscribe();

    const request = testing.expectOne(url);
    expect(request.request.headers.has('X-XSRF-TOKEN')).toBe(false);
    request.flush({});
  });
});
