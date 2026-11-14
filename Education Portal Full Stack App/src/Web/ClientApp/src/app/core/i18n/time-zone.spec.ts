import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { browserTimeZone, TIME_ZONE_HEADER, timeZoneInterceptor } from './time-zone';

describe('timeZoneInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;

  beforeEach(() => {
    vi.spyOn(Intl.DateTimeFormat.prototype, 'resolvedOptions').mockReturnValue({
      ...new Intl.DateTimeFormat('en').resolvedOptions(),
      timeZone: 'Asia/Dubai',
    });
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptors([timeZoneInterceptor])), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
  });

  afterEach(() => vi.restoreAllMocks());

  it('sends the device time zone to this app’s own server', () => {
    http.get(`${document.location.origin}/api/Localization`).subscribe();
    http.get('api/Countries').subscribe();

    const requests = backend.match(() => true);
    expect(requests.map((r) => r.request.headers.get(TIME_ZONE_HEADER))).toEqual(['Asia/Dubai', 'Asia/Dubai']);
  });

  it('never sends it to another site', () => {
    http.get('https://example.com/data').subscribe();

    expect(backend.expectOne('https://example.com/data').request.headers.has(TIME_ZONE_HEADER)).toBe(false);
  });

  it('sends nothing when the browser does not know its time zone', () => {
    vi.spyOn(Intl.DateTimeFormat.prototype, 'resolvedOptions').mockImplementation(() => {
      throw new Error('not supported');
    });
    expect(browserTimeZone()).toBeNull();

    http.get('api/Countries').subscribe();

    expect(backend.expectOne('api/Countries').request.headers.has(TIME_ZONE_HEADER)).toBe(false);
  });
});
