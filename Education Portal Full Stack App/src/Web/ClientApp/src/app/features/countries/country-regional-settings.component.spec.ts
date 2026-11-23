import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { CountryDto, DayOfWeek } from '../../web-api-client';
import { CountryRegionalSettingsComponent } from './country-regional-settings.component';

const ID = '0190f3a0-0000-7000-8000-000000000001';
const country = CountryDto.fromJS({
  id: ID,
  code: 'AE',
  name: 'United Arab Emirates',
  languages: ['en', 'ar'],
  defaultLanguage: 'ar',
  timeZoneId: 'Asia/Dubai',
  currencyCode: 'AED',
  currencyDecimals: 2,
  phonePattern: undefined,
  weekStartDay: 'Monday',
  weekendDays: ['Saturday', 'Sunday'],
});

function render() {
  TestBed.configureTestingModule({
    imports: [CountryRegionalSettingsComponent],
    providers: [provideHttpClient(), provideHttpClientTesting()],
  });
  const fixture = TestBed.createComponent(CountryRegionalSettingsComponent);
  fixture.componentRef.setInput('country', country);
  fixture.detectChanges();
  return { fixture, http: TestBed.inject(HttpTestingController) };
}

describe('CountryRegionalSettingsComponent', () => {
  beforeEach(() => TestBed.resetTestingModule());
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('starts from the country\'s saved settings', () => {
    const { fixture } = render();
    expect(fixture.componentInstance.form.getRawValue()).toEqual({
      currencyDecimals: 2,
      phonePattern: '',
      weekStartDay: DayOfWeek.Monday,
      weekendDays: [DayOfWeek.Saturday, DayOfWeek.Sunday],
    });
  });

  it('saves a Friday weekend and a phone pattern', async () => {
    const { fixture, http } = render();
    const component = fixture.componentInstance;
    let saved = false;
    component.saved.subscribe(() => (saved = true));

    component.form.patchValue({ weekStartDay: DayOfWeek.Sunday, weekendDays: [DayOfWeek.Friday, DayOfWeek.Saturday], phonePattern: ' ^\\+971\\d+$ ' });
    component.save();

    const request = http.expectOne({ method: 'PUT', url: `/api/Countries/${ID}/regional-settings` });
    expect(JSON.parse(request.request.body as string)).toEqual({
      id: ID,
      currencyDecimals: 2,
      phonePattern: '^\\+971\\d+$',
      weekStartDay: 'Sunday',
      weekendDays: ['Friday', 'Saturday'],
    });
    request.flush(null, { status: 204, statusText: 'No Content' });
    await vi.waitFor(() => expect(saved).toBe(true));
  });

  it('does not send more than three weekend days', () => {
    const { fixture, http } = render();
    const component = fixture.componentInstance;

    component.form.patchValue({ weekendDays: [DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday] });
    component.save();

    http.expectNone({ method: 'PUT', url: `/api/Countries/${ID}/regional-settings` });
  });

  it('refuses a phone pattern that does not compile, and a pattern of 201 characters', () => {
    const { fixture, http } = render();
    const component = fixture.componentInstance;

    component.form.patchValue({ phonePattern: '(' });
    expect(component.form.controls.phonePattern.hasError('phonePattern')).toBe(true);
    component.save();
    http.expectNone({ method: 'PUT', url: `/api/Countries/${ID}/regional-settings` });

    component.form.patchValue({ phonePattern: 'a'.repeat(200) });
    expect(component.form.controls.phonePattern.valid).toBe(true);
    component.form.patchValue({ phonePattern: 'a'.repeat(201) });
    expect(component.form.controls.phonePattern.valid).toBe(false);
  });

  it('accepts up to three different weekend days, and refuses the same day twice', () => {
    const { fixture, http } = render();
    const component = fixture.componentInstance;

    component.form.patchValue({ weekendDays: [DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday] });
    expect(component.form.controls.weekendDays.valid).toBe(true);

    component.form.patchValue({ weekendDays: [DayOfWeek.Saturday, DayOfWeek.Saturday] });
    expect(component.form.controls.weekendDays.hasError('duplicateDay')).toBe(true);
    component.save();
    http.expectNone({ method: 'PUT', url: `/api/Countries/${ID}/regional-settings` });
  });
});
