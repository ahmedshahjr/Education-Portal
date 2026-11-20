import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideNativeDateAdapter } from '@angular/material/core';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { HolidayAudience, HolidayType } from '../../web-api-client';
import { SchoolCalendarPanelComponent } from './school-calendar-panel.component';

const today = new Date();
const pad = (n: number) => String(n).padStart(2, '0');
const day = (d: number) => `${today.getFullYear()}-${pad(today.getMonth() + 1)}-${pad(d)}`;

async function render() {
  TestBed.configureTestingModule({
    imports: [SchoolCalendarPanelComponent],
    providers: [provideHttpClient(), provideHttpClientTesting(), provideNativeDateAdapter()],
  });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(SchoolCalendarPanelComponent);
  fixture.componentRef.setInput('campuses', [{ id: 'campus-1', name: 'North' }]);
  fixture.detectChanges();
  await fixture.whenStable();
  const request = http.expectOne((r) => r.url === '/api/SchoolCalendar');
  request.flush({
    holidays: [{ id: 'h1', campusId: undefined, title: 'Spring break', startDate: day(1), endDate: day(3), type: 'Vacation', appliesTo: 'Both', isTentative: false }],
    events: [{ id: 'e1', campusId: 'campus-1', title: 'Sports day', eventDate: day(2), startTime: '09:00:00', endTime: '12:00:00', type: 'Sports', audience: 'All' }],
  });
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(fixture.componentInstance.entries()).not.toBeNull();
  });
  return { fixture, http, element: fixture.nativeElement as HTMLElement };
}

describe('SchoolCalendarPanelComponent', () => {
  beforeEach(() => TestBed.resetTestingModule());
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('lists the month and marks what happens on the chosen day', async () => {
    const { fixture, element } = await render();
    const panel = fixture.componentInstance;

    expect(element.textContent).toContain('Spring break');
    expect(element.textContent).toContain('09:00 – 12:00');
    panel.selectDay(new Date(today.getFullYear(), today.getMonth(), 2));
    expect(panel.dayEntries().map((e) => e.title)).toEqual(['Spring break', 'Sports day']);
    panel.selectDay(new Date(today.getFullYear(), today.getMonth(), 4));
    expect(panel.dayEntries()).toEqual([]);
  });

  it('adds a holiday for the whole school on the chosen day', async () => {
    const { fixture, http } = await render();
    const panel = fixture.componentInstance;

    panel.selectDay(new Date(today.getFullYear(), today.getMonth(), 5));
    panel.startAddHoliday();
    panel.holidayForm.patchValue({ title: '  Founders day ', type: HolidayType.School, appliesTo: HolidayAudience.Students });
    panel.saveHoliday();

    const request = http.expectOne({ method: 'POST', url: '/api/SchoolCalendar/holidays' });
    const body = JSON.parse(request.request.body as string) as Record<string, unknown>;
    expect(body).toEqual({ title: 'Founders day', startDate: day(5), endDate: day(5), type: 'School', appliesTo: 'Students', isTentative: false });
    request.flush(new Blob([JSON.stringify('id')], { type: 'application/json' }));

    await vi.waitFor(() => http.expectOne((r) => r.url === '/api/SchoolCalendar').flush({ holidays: [], events: [] }));
    expect(panel.editing()).toBeNull();
  });

  it('does not send an event whose end time comes without a start time', async () => {
    const { fixture, http } = await render();
    const panel = fixture.componentInstance;

    panel.startAddEvent();
    panel.eventForm.patchValue({ title: 'Assembly', endTime: '10:00' });
    panel.saveEvent();

    expect(panel.eventForm.hasError('endNeedsStart')).toBe(true);
    http.expectNone({ method: 'POST', url: '/api/SchoolCalendar/events' });
  });
});
