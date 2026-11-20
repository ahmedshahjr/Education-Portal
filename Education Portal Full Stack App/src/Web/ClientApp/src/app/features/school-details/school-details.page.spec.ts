import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { SchoolDetailsPage } from './school-details.page';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });
const STATE = '0190f3a0-0000-7000-8000-00000000000a';
const CITY = '0190f3a0-0000-7000-8000-00000000000b';

async function render() {
  TestBed.configureTestingModule({
    imports: [SchoolDetailsPage],
    providers: [provideHttpClient(), provideHttpClientTesting()],
  });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(SchoolDetailsPage);
  fixture.detectChanges();
  http.expectOne('/api/SchoolDetails').flush(
    json({ localName: 'مدرسہ', registrationNumber: 'REG-1', stateId: STATE, cityId: CITY, academicYearStartMonth: 4 })
  );
  http.expectOne('/api/Campuses/locations').flush(
    json({ countryName: 'Pakistan', states: [{ id: STATE, name: 'Sindh', cities: [{ id: CITY, name: 'Karachi' }] }] })
  );
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(fixture.componentInstance.form.controls.registrationNumber.value).toBe('REG-1');
  });
  return { fixture, http };
}

describe('SchoolDetailsPage', () => {
  beforeEach(() => TestBed.resetTestingModule());
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('fills the form with the saved details', async () => {
    const { fixture } = await render();
    const value = fixture.componentInstance.form.getRawValue();
    expect(value.localName).toBe('مدرسہ');
    expect(value.cityId).toBe(CITY);
    expect(value.academicYearStartMonth).toBe(4);
  });

  it('saves trimmed values and leaves blank ones out', async () => {
    const { fixture, http } = await render();
    const page = fixture.componentInstance;

    page.form.patchValue({ taxNumber: '  NTN-9 ', registrationNumber: '   ', email: 'office@example.edu' });
    page.save();

    const request = http.expectOne({ method: 'PUT', url: '/api/SchoolDetails' });
    expect(JSON.parse(request.request.body as string)).toEqual({
      localName: 'مدرسہ',
      taxNumber: 'NTN-9',
      cityId: CITY,
      email: 'office@example.edu',
      academicYearStartMonth: 4,
    });
    request.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('does not send an invalid email address', async () => {
    const { fixture, http } = await render();
    const page = fixture.componentInstance;

    page.form.patchValue({ email: 'not-an-email' });
    page.save();

    http.expectNone({ method: 'PUT', url: '/api/SchoolDetails' });
  });
});
