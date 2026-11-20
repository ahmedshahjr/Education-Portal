import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { SchoolCalendarDto } from '../../web-api-client';
import { SchoolCalendarApi } from './school-calendar.api';

describe('SchoolCalendarApi', () => {
  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
  });
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('sends the range as plain calendar dates, without a time or a time zone shift', () => {
    let result: SchoolCalendarDto | undefined;
    TestBed.inject(SchoolCalendarApi)
      .read(new Date(2026, 8, 1), new Date(2026, 8, 30), null)
      .subscribe((calendar) => (result = calendar));

    const request = TestBed.inject(HttpTestingController).expectOne((r) => r.url === '/api/SchoolCalendar');
    expect(request.request.params.get('from')).toBe('2026-09-01');
    expect(request.request.params.get('to')).toBe('2026-09-30');
    expect(request.request.params.has('campusId')).toBe(false);
    request.flush({ holidays: [{ id: 'h', title: 'Eid', startDate: '2026-09-10', endDate: '2026-09-12', type: 'Religious', appliesTo: 'Both', isTentative: true }], events: [] });

    expect(result?.holidays[0].title).toBe('Eid');
  });

  it('names the campus when one is chosen', () => {
    TestBed.inject(SchoolCalendarApi).read(new Date(2026, 0, 1), new Date(2026, 0, 31), 'campus-1').subscribe();
    const request = TestBed.inject(HttpTestingController).expectOne((r) => r.url === '/api/SchoolCalendar');
    expect(request.request.params.get('campusId')).toBe('campus-1');
    request.flush({ holidays: [], events: [] });
  });
});
