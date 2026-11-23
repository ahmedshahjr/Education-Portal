import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { FormControl } from '@angular/forms';
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

  it('shows the postal code error for a code with symbols, and accepts letters, digits, spaces and hyphens', async () => {
    const { page, element, fixture } = await render();
    page['form'].controls.postalCode.setValue('AB#1');
    page['form'].controls.postalCode.markAsTouched();
    fixture.detectChanges();
    expect(element.textContent).toContain('Use up to 10 letters, digits, spaces or hyphens.');

    for (const code of ['SW1A 1AA', '75008', 'AB-12']) {
      page['form'].controls.postalCode.setValue(code);
      expect(page['form'].controls.postalCode.valid).toBe(true);
    }
  });

  it('accepts a postal code of exactly 10 characters and refuses 11', async () => {
    const { page } = await render();
    page['form'].controls.postalCode.setValue('A'.repeat(10));
    expect(page['form'].controls.postalCode.valid).toBe(true);
    page['form'].controls.postalCode.setValue('A'.repeat(11));
    expect(page['form'].controls.postalCode.valid).toBe(false);
  });

  it('refuses a phone number with fewer than seven digits, and accepts seven', async () => {
    const { page, element, fixture } = await render();
    page['form'].controls.phones.controls[0].setValue('123456');
    page['form'].controls.phones.controls[0].markAsTouched();
    fixture.detectChanges();
    expect(element.textContent).toContain('Use 7 to 15 digits');

    page['form'].controls.phones.controls[0].setValue('1234567');
    fixture.detectChanges();
    expect(page['form'].controls.phones.controls[0].valid).toBe(true);
  });

  it('asks for at least one phone number when every number is blank', async () => {
    const { page, element, fixture } = await render();
    page['form'].controls.phones.controls[0].setValue('   ');
    fixture.detectChanges();
    expect(element.textContent).toContain('Add at least one phone number.');
  });

  it('accepts five phone numbers and shows the error for a sixth', async () => {
    const { page, element, fixture } = await render();
    const addPhone = () => (Array.from(element.querySelectorAll('button')).find((b) => b.textContent?.includes('Add a phone number')) as HTMLButtonElement).click();
    for (let i = 0; i < 4; i++) {
      addPhone();
      fixture.detectChanges();
    }
    page['form'].controls.phones.controls.forEach((c, i) => c.setValue(`+92 21 100000${i}`));
    expect(page['form'].controls.phones.valid).toBe(true);

    page['form'].controls.phones.push(new FormControl('+92 21 1000009', { nonNullable: true }));
    fixture.detectChanges();
    expect(page['form'].controls.phones.hasError('maxPhones')).toBe(true);
    expect(element.textContent).toContain('A campus lists at most five phone numbers.');
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

  it('shows the field errors the server returns for a refused campus, and a plain message for a refusal with no detail', async () => {
    const { page, http, element, fixture } = await render();
    fillValidCampus(page);
    page['save']();

    const refused = http.expectOne({ method: 'POST', url: '/api/Campuses' });
    refused.flush(new Blob([JSON.stringify({ errors: { Name: ['The name is already used.'] } })], { type: 'application/json' }), {
      status: 400,
      statusText: 'Bad Request',
    });
    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(element.textContent).toContain('The name is already used.');
    });

    page['save']();
    http.expectOne({ method: 'POST', url: '/api/Campuses' }).flush(new Blob([''], { type: 'text/plain' }), { status: 403, statusText: 'Forbidden' });
    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(element.textContent).toContain('You do not have permission to do this.');
    });
  });
});
