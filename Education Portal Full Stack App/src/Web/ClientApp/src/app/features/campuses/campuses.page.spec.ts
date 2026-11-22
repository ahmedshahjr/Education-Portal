import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { CampusesPage } from './campuses.page';
import { SchoolGenderPolicy } from '../../web-api-client';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const STATE = '0190f3a0-0000-7000-8000-00000000000a';
const CITY = '0190f3a0-0000-7000-8000-00000000000b';
const OTHER_STATE = '0190f3a0-0000-7000-8000-00000000000c';
const OTHER_CITY = '0190f3a0-0000-7000-8000-00000000000d';

const campus = {
  id: '0190f3a0-0000-7000-8000-000000000001',
  name: 'Karachi',
  code: '0002',
  displayCode: 'KHI0002',
  kind: 'Other',
  localName: undefined,
  genderPolicy: 'CoEducation',
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
    latitude: 24.8607,
    longitude: 67.0011,
  },
};

const lahore = {
  id: '0190f3a0-0000-7000-8000-000000000002',
  name: 'Lahore',
  code: '0003',
  displayCode: 'LHR0003',
  kind: 'Other',
  localName: undefined,
  genderPolicy: 'CoEducation',
  details: {
    address: '9 Canal Road',
    stateId: OTHER_STATE,
    stateName: 'Punjab',
    cityId: OTHER_CITY,
    cityName: 'Lahore',
    postalCode: '54000',
    phones: ['+92 42 7654321'],
    website: undefined,
    schoolNumber: 'SN-3',
    shortName: 'LHR',
    principalId: undefined,
    principalName: undefined,
    latitude: 31.5204,
    longitude: 74.3587,
  },
};

const locations = {
  countryName: 'Pakistan',
  states: [
    { id: STATE, name: 'Sindh', cities: [{ id: CITY, name: 'Karachi' }] },
    { id: OTHER_STATE, name: 'Punjab', cities: [{ id: OTHER_CITY, name: 'Lahore' }] },
  ],
};

/** The location picker asks for the map settings when the form opens; no key is set in these tests. */
function answerMapSettings(http: HttpTestingController) {
  http.expectOne('/api/Campuses/map-settings').flush(json({}));
}

async function render(list: unknown[] = [campus]) {
  TestBed.configureTestingModule({
    imports: [CampusesPage],
    providers: [provideHttpClient(), provideHttpClientTesting()],
  });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(CampusesPage);
  fixture.detectChanges();

  // The page loads its campuses, locations and principals when it opens.
  http.expectOne('/api/Campuses').flush(json(list));
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

  it('filters the list by state, and then by city within that state', async () => {
    const { fixture, element } = await render([campus, lahore]);
    const page = fixture.componentInstance;

    page.setStateFilter(OTHER_STATE);
    fixture.detectChanges();
    expect(page.visibleCampuses().map((c) => c.name)).toEqual(['Lahore']);
    expect(element.textContent).not.toContain('12 Main Road');

    page.cityFilter.set(OTHER_CITY);
    expect(page.visibleCampuses().map((c) => c.name)).toEqual(['Lahore']);

    page.setStateFilter(STATE);
    expect(page.cityFilter()).toBe('');
    expect(page.visibleCampuses().map((c) => c.name)).toEqual(['Karachi']);
  });

  it('offers only the cities of the chosen state, and all cities when no state is chosen', async () => {
    const { fixture } = await render([campus, lahore]);
    const page = fixture.componentInstance;

    expect(page.cityFilterOptions().map((c) => c.name)).toEqual(['Karachi', 'Lahore']);
    page.setStateFilter(OTHER_STATE);
    expect(page.cityFilterOptions().map((c) => c.name)).toEqual(['Lahore']);
  });

  it('clears the filters back to every campus', async () => {
    const { fixture } = await render([campus, lahore]);
    const page = fixture.componentInstance;

    page.setStateFilter(OTHER_STATE);
    page.clearFilters();

    expect(page.visibleCampuses().length).toBe(2);
  });

  it('sends the local name and the gender policy chosen for the campus', async () => {
    const { fixture, http } = await render();
    const page = fixture.componentInstance;

    page.startEdit(campus as unknown as Parameters<typeof page.startEdit>[0]);
    fixture.detectChanges();
    answerMapSettings(http);
    page['form'].patchValue({ localName: '  کراچی  ', genderPolicy: SchoolGenderPolicy.GirlsOnly });
    page.save();

    const request = http.expectOne({ method: 'PUT', url: `/api/Campuses/${campus.id}` });
    const body = JSON.parse(request.request.body as string) as Record<string, unknown>;
    expect(body['localName']).toBe('کراچی');
    expect(body['genderPolicy']).toBe('GirlsOnly');
    request.flush(null, { status: 204, statusText: 'No Content' });
    await vi.waitFor(() => http.expectOne('/api/Campuses').flush(json([campus])));
  });
});
