import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { provideNativeDateAdapter } from '@angular/material/core';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { AcademicYearStatus } from '../../web-api-client';
import { AcademicYearsPanelComponent } from './academic-years-panel.component';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const YEAR = '0190f3a0-0000-7000-8000-000000000101';
const year = {
  id: YEAR,
  name: '2026-2027',
  startDate: '2026-08-01',
  endDate: '2027-06-30',
  isCurrent: true,
  status: 'Active',
  terms: [
    { id: 't2', name: 'Spring', sequence: 2, startDate: '2027-01-10', endDate: '2027-06-30' },
    { id: 't1', name: 'Autumn', sequence: 1, startDate: '2026-08-01', endDate: '2026-12-20' },
  ],
};

async function render() {
  TestBed.configureTestingModule({
    imports: [AcademicYearsPanelComponent],
    providers: [provideHttpClient(), provideHttpClientTesting(), provideNativeDateAdapter()],
  });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(AcademicYearsPanelComponent);
  fixture.detectChanges();
  http.expectOne('/api/AcademicYears').flush(json([year]));
  const element = fixture.nativeElement as HTMLElement;
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(element.textContent).toContain('2026-2027');
  });
  return { fixture, http, element };
}

describe('AcademicYearsPanelComponent', () => {
  beforeEach(() => TestBed.resetTestingModule());
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('lists each year with its status and keeps its dates on the same calendar day', async () => {
    const { fixture, element } = await render();
    const row = fixture.componentInstance.years()![0];

    expect(element.textContent).toContain('Current');
    expect(row.startDate.getDate()).toBe(1);
    expect(row.startDate.getMonth()).toBe(7);
  });

  it('shows the terms of the chosen year in order', async () => {
    const { fixture, element } = await render();
    const panel = fixture.componentInstance;

    panel.selectYear(panel.years()![0]);
    fixture.detectChanges();

    expect(panel.selectedYear()?.terms.map((t) => t.name)).toEqual(['Autumn', 'Spring']);
    expect(element.textContent).toContain('Terms of 2026-2027');
  });

  it('adds a year with its dates as calendar days', async () => {
    const { fixture, http } = await render();
    const panel = fixture.componentInstance;

    panel.startAddYear();
    panel.yearForm.patchValue({ name: ' 2027-2028 ', startDate: new Date(2027, 7, 1), endDate: new Date(2028, 5, 30), isCurrent: false });
    panel.saveYear();

    const request = http.expectOne({ method: 'POST', url: '/api/AcademicYears' });
    expect(JSON.parse(request.request.body as string)).toEqual({ name: '2027-2028', startDate: '2027-08-01', endDate: '2028-06-30', isCurrent: false });
    request.flush(json(YEAR));
    await vi.waitFor(() => http.expectOne('/api/AcademicYears').flush(json([year])));
    expect(panel.editingYear()).toBeNull();
  });

  it('does not send a year without dates', async () => {
    const { fixture, http } = await render();
    const panel = fixture.componentInstance;

    panel.startAddYear();
    panel.yearForm.patchValue({ name: '2027-2028' });
    panel.saveYear();

    http.expectNone({ method: 'POST', url: '/api/AcademicYears' });
  });

  it('changes the status of an existing year and shows the server message when it refuses', async () => {
    const { fixture, http, element } = await render();
    const panel = fixture.componentInstance;

    panel.startEditYear(panel.years()![0]);
    panel.yearForm.patchValue({ status: AcademicYearStatus.Closed });
    panel.saveYear();

    const request = http.expectOne({ method: 'PUT', url: `/api/AcademicYears/${YEAR}` });
    expect((JSON.parse(request.request.body as string) as { status: string }).status).toBe('Closed');
    request.flush(json({ errors: { EndDate: ['The academic year overlaps another year.'] } }), { status: 400, statusText: 'Bad Request' });

    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(element.textContent).toContain('The academic year overlaps another year.');
    });
  });

  it('accepts a year of 730 days from its first day, refuses 731, and refuses an end before the start', async () => {
    const { fixture } = await render();
    const panel = fixture.componentInstance;
    const start = new Date(2027, 0, 1);
    const after = (days: number) => {
      const d = new Date(start);
      d.setDate(d.getDate() + days);
      return d;
    };

    panel.startAddYear();
    panel.yearForm.patchValue({ name: '2027', startDate: start, endDate: after(730) });
    expect(panel.yearForm.valid).toBe(true);

    panel.yearForm.patchValue({ endDate: after(731) });
    expect(panel.yearForm.invalid).toBe(true);

    panel.yearForm.patchValue({ endDate: new Date(2026, 11, 31) });
    expect(panel.yearForm.invalid).toBe(true);
  });

  it('refuses a term whose end is before its start', async () => {
    const { fixture } = await render();
    const panel = fixture.componentInstance;

    panel.startAddYear();
    panel.termForm.patchValue({ name: 'Autumn', sequence: 1, startDate: new Date(2026, 8, 1), endDate: new Date(2026, 7, 31) });
    expect(panel.termForm.invalid).toBe(true);
    panel.termForm.patchValue({ endDate: new Date(2026, 11, 20) });
    expect(panel.termForm.valid).toBe(true);
  });
});
