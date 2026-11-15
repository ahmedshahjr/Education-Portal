import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { CampusesPage } from './campuses.page';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const STATE = '0190f3a0-0000-7000-8000-00000000000a';
const CITY = '0190f3a0-0000-7000-8000-00000000000b';

const campus = {
  id: '0190f3a0-0000-7000-8000-000000000001',
  name: 'Karachi',
  code: '0002',
  displayCode: 'KHI0002',
  kind: 'Other',
  details: {
    address: '12 Main Road',
    stateId: STATE,
    stateName: 'Sindh',
    cityId: CITY,
    cityName: 'Karachi',
    postalCode: '75500',
    phones: ['+92 21 1234567', '0300-1234567'],
    website: undefined,
    schoolNumber: 'SN-1',
    shortName: 'KHI',
    principalId: undefined,
    principalName: undefined,
  },
};

const locations = {
  states: [{ id: STATE, name: 'Sindh', cities: [{ id: CITY, name: 'Karachi' }] }],
};

async function render() {
  TestBed.configureTestingModule({
    imports: [CampusesPage],
    providers: [provideHttpClient(), provideHttpClientTesting()],
  });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(CampusesPage);
  fixture.detectChanges();

  // The page loads its campuses, code settings, locations and principals when it opens.
  http.expectOne('/api/Campuses').flush(json([campus]));
  http.expectOne('/api/Campuses/settings').flush(json({ prefixEnabled: false, prefix: undefined }));
  http.expectOne('/api/Campuses/locations').flush(json(locations));
  http.match((request) => request.url.startsWith('/api/UserDetails')).forEach((request) => request.flush(json([])));

  const element = fixture.nativeElement as HTMLElement;
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(element.textContent).toContain('Karachi');
  });
  return { fixture, http, element };
}

describe('CampusesPage', () => {
  beforeEach(() => TestBed.resetTestingModule());
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('lists each campus with its address, state and city, and its phone numbers', async () => {
    const { element } = await render();

    expect(element.textContent).toContain('12 Main Road');
    expect(element.textContent).toContain('Sindh');
    expect(element.textContent).toContain('+92 21 1234567 · 0300-1234567');
  });

  it('does not send a campus that is missing its required details', async () => {
    const { fixture, http } = await render();
    const page = fixture.componentInstance;

    page.startAdd();
    fixture.detectChanges();
    page.save();

    http.expectNone({ method: 'POST', url: '/api/Campuses' });
  });

  it('adds a campus with its full details, the phone numbers in order', async () => {
    const { fixture, http } = await render();
    const page = fixture.componentInstance;

    page.startAdd();
    page.form.patchValue({
      name: 'Hyderabad',
      address: '4 Lake Road',
      stateId: STATE,
      cityId: CITY,
      postalCode: '71000',
      website: '',
      schoolNumber: 'SN-2',
      shortName: 'HYD',
    });
    page.phones.controls[0].setValue('+92 22 7654321');
    page.save();

    const request = http.expectOne({ method: 'POST', url: '/api/Campuses' });
    expect(JSON.parse(request.request.body as string)).toEqual({
      name: 'Hyderabad',
      details: {
        address: '4 Lake Road',
        stateId: STATE,
        cityId: CITY,
        postalCode: '71000',
        phones: ['+92 22 7654321'],
        schoolNumber: 'SN-2',
        shortName: 'HYD',
      },
    });
    request.flush(json(CITY), { status: 200, statusText: 'OK' });

    // After a save the page reloads the campus list (the generated client reads the response asynchronously).
    await vi.waitFor(() => http.expectOne('/api/Campuses').flush(json([campus])));
  });
});
