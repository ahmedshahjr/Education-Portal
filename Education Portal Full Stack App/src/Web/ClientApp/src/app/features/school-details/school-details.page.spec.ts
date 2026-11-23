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

  it('accepts the email rule boundary: 254 characters yes, 255 no, and refuses consecutive or leading dots', async () => {
    const { fixture } = await render();
    const page = fixture.componentInstance;
    const suffix = '@example.com';

    page.form.controls.email.setValue('a'.repeat(254 - suffix.length) + suffix);
    expect(page.form.controls.email.valid).toBe(true);
    page.form.controls.email.setValue('a'.repeat(254 - suffix.length + 1) + suffix);
    expect(page.form.controls.email.valid).toBe(false);

    page.form.controls.email.setValue('a..b@example.com');
    expect(page.form.controls.email.hasError('email')).toBe(true);
    page.form.controls.email.setValue('.a@example.com');
    expect(page.form.controls.email.hasError('email')).toBe(true);
  });

  it('accepts a padded email address once trimmed, and shows the email error for a display name', async () => {
    const { fixture, http } = await render();
    const page = fixture.componentInstance;

    page.form.controls.email.setValue(' office@example.edu ');
    expect(page.form.controls.email.valid).toBe(true);
    page.form.controls.email.setValue('Office <office@example.edu>');
    page.form.controls.email.markAsTouched();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Enter a valid email address.');
    page.save();
    http.expectNone({ method: 'PUT', url: '/api/SchoolDetails' });
  });

  it('accepts a website of exactly 2048 characters, refuses 2049, and refuses a relative or non-web address', async () => {
    const { fixture, http } = await render();
    const page = fixture.componentInstance;
    const prefix = 'https://a.com/';

    page.form.controls.website.setValue(prefix + 'a'.repeat(2048 - prefix.length));
    expect(page.form.controls.website.valid).toBe(true);
    page.form.controls.website.setValue(prefix + 'a'.repeat(2048 - prefix.length + 1));
    expect(page.form.controls.website.valid).toBe(false);

    for (const bad of ['school1.com', '/about', 'ftp://school1.com', 'javascript:alert(1)']) {
      page.form.controls.website.setValue(bad);
      expect(page.form.controls.website.hasError('website')).toBe(true);
    }

    page.form.controls.website.setValue('javascript:alert(1)');
    page.form.controls.website.markAsTouched();
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Enter a full web address that starts with http:// or https://.');
    page.save();
    http.expectNone({ method: 'PUT', url: '/api/SchoolDetails' });
  });
});
