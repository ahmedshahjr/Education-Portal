import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { CampusPageComponent } from './campus-page.component';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const STATE = '0190f3a0-0000-7000-8000-00000000000a';
const CITY = '0190f3a0-0000-7000-8000-00000000000b';

const locations = {
  countryName: 'Pakistan',
  states: [{ id: STATE, name: 'Sindh', cities: [{ id: CITY, name: 'Karachi' }] }],
};

/** Opens the add-campus page and answers the requests it makes when it opens: locations, principals and map settings. */
async function render() {
  TestBed.configureTestingModule({
    imports: [CampusPageComponent],
    providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
  });
  const http = TestBed.inject(HttpTestingController);
  const router = TestBed.inject(Router);
  const fixture = TestBed.createComponent(CampusPageComponent);
  fixture.detectChanges();

  http.expectOne('/api/Campuses/locations').flush(json(locations));
  http.match((request) => request.url.startsWith('/api/UserDetails')).forEach((request) => request.flush(json([])));
  http.match('/api/Campuses/map-settings').forEach((request) => request.flush(json({})));

  const element = fixture.nativeElement as HTMLElement;
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(element.querySelector('form')).not.toBeNull();
  });
  return { fixture, http, router, element, page: fixture.componentInstance };
}

/** Fills every required field with values the server accepts. */
function fillValidCampus(page: CampusPageComponent) {
  page['form'].patchValue({
    name: 'Karachi North',
    address: '12 Main Road',
    stateId: STATE,
    cityId: CITY,
    postalCode: '75500',
    schoolNumber: 'SN-2',
    shortName: 'KHN',
    latitude: 24.8607,
    longitude: 67.0011,
  });
  page['form'].controls.phones.controls[0].setValue('+92 21 1234567');
}

describe('CampusPageComponent', () => {
  beforeEach(() => TestBed.resetTestingModule());
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('sends nothing when the form is invalid, and marks the missing fields', async () => {
    const { page, http, router, fixture } = await render();
    const navigate = vi.spyOn(router, 'navigate');

    expect(page['form'].invalid).toBe(true);
    page['save']();
    fixture.detectChanges();

    http.expectNone({ method: 'POST', url: '/api/Campuses' });
    expect(navigate).not.toHaveBeenCalled();
    expect(page['form'].controls.name.touched).toBe(true);
  });

  it('shows the phone error and sends nothing for a phone number the server would refuse', async () => {
    const { page, http, element, fixture } = await render();
    fillValidCampus(page);
    page['form'].controls.phones.controls[0].setValue('call me');
    fixture.detectChanges();

    page['save']();
    fixture.detectChanges();

    expect(element.textContent).toContain('Use 7 to 15 digits');
    http.expectNone({ method: 'POST', url: '/api/Campuses' });
  });

  it('adds a valid campus and returns to the campus list', async () => {
    const { page, http, router } = await render();
    const navigate = vi.spyOn(router, 'navigate').mockResolvedValue(true);
    fillValidCampus(page);

    page['save']();

    const request = http.expectOne({ method: 'POST', url: '/api/Campuses' });
    const body = JSON.parse(request.request.body as string) as { name: string; details: { stateId: string; phones: string[] } };
    expect(body.name).toBe('Karachi North');
    expect(body.details.stateId).toBe(STATE);
    expect(body.details.phones).toEqual(['+92 21 1234567']);
    request.flush(json('0190f3a0-0000-7000-8000-000000000009'));

    // The generated client reads the response blob asynchronously, so the navigation happens after a tick.
    await vi.waitFor(() => expect(navigate).toHaveBeenCalledWith(['/campuses']));
  });
});
