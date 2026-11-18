import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { API_BASE_URL } from '../../web-api-client';
import { DevelopmentDataPage } from './development-data.page';

const blob = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

function render() {
  TestBed.configureTestingModule({
    imports: [DevelopmentDataPage],
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
  });
  const fixture = TestBed.createComponent(DevelopmentDataPage);
  return { fixture, http: TestBed.inject(HttpTestingController), page: fixture.componentInstance };
}

describe('DevelopmentDataPage', () => {
  afterEach(() => {
    TestBed.resetTestingModule();
  });

  it('sends the form values to the generate endpoint', async () => {
    const { fixture, http, page } = render();
    page.form.setValue({ schools: 2, campusesPerSchool: 3, peoplePerCampus: 4, countryCode: 'ae', seed: 7 });
    fixture.detectChanges();

    page.generate();

    const request = http.expectOne((r) => r.url === '/api/DevelopmentData/generate');
    expect(request.request.method).toBe('POST');
    expect(JSON.parse(request.request.body as string)).toMatchObject({ schools: 2, campusesPerSchool: 3, peoplePerCampus: 4, countryCode: 'AE', seed: 7 });
    request.flush(blob({ schools: 2, campuses: 6, people: 24 }));

    // The generated client reads the response asynchronously; wait so it finishes inside this test.
    await vi.waitFor(() => expect(page.summary()?.people).toBe(24));
  });

  it('does not send a request when the total is above the server limit', () => {
    const { fixture, http, page } = render();
    page.form.setValue({ schools: 500, campusesPerSchool: 50, peoplePerCampus: 100, countryCode: 'PK', seed: null });
    fixture.detectChanges();

    expect(page.withinLimit()).toBe(false);
    expect(page.cannotSubmit()).toBe(true);

    page.generate();
    http.expectNone((r) => r.url === '/api/DevelopmentData/generate');
  });

  it('does not send a request when the country code is not two letters', () => {
    const { http, page } = render();
    page.form.patchValue({ countryCode: 'PAK' });

    expect(page.cannotSubmit()).toBe(true);

    page.generate();
    http.expectNone((r) => r.url === '/api/DevelopmentData/generate');
  });

  it('allows the largest request that is still within the limit', () => {
    const { page } = render();
    page.form.setValue({ schools: 100, campusesPerSchool: 50, peoplePerCampus: 200, countryCode: 'PK', seed: null });

    expect(page.totalPeople()).toBe(1_000_000);
    expect(page.cannotSubmit()).toBe(false);
  });

  it('does not allow a negative seed', () => {
    const { page } = render();
    page.form.patchValue({ seed: -1 });

    expect(page.cannotSubmit()).toBe(true);
  });

  it('shows the summary after the server creates the data', async () => {
    const { http, page } = render();
    page.generate();

    http.expectOne((r) => r.url === '/api/DevelopmentData/generate').flush(blob({ schools: 3, campuses: 6, people: 60 }));

    await vi.waitFor(() => expect(page.summary()?.people).toBe(60));
    expect(page.saving()).toBe(false);
  });
});
