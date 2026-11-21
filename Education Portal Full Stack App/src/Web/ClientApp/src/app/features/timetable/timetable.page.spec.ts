import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';
import { API_BASE_URL } from '../../web-api-client';
import { TimetablePage } from './timetable.page';

const blob = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const COURSE = '0190f3a0-0000-7000-8000-000000000001';
const SLOT = '0190f3a0-0000-7000-8000-000000000002';

interface Host {
  form: { setValue(value: unknown): void };
  busy(): boolean;
}

async function render() {
  TestBed.configureTestingModule({
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
  });
  const fixture = TestBed.createComponent(TimetablePage);
  const http = TestBed.inject(HttpTestingController);
  fixture.detectChanges();
  http.expectOne((r) => r.url.startsWith('/api/Courses')).flush(blob([{ id: COURSE, name: 'Math', classOfferingId: 'o', teacherId: 't' }]));
  http.expectOne('/api/Campuses').flush(blob([]));
  http
    .expectOne((r) => r.url.startsWith('/api/TimetableSlots') && r.method === 'GET')
    .flush(
      blob([
        { id: SLOT, courseId: COURSE, courseName: 'Math', day: 'Tuesday', start: '09:00:00', end: '10:00:00', room: 'Room 2' },
        { id: 'other', courseId: COURSE, courseName: 'Math', day: 'Monday', start: '11:00:00', end: '12:00:00' },
      ])
    );
  const element = fixture.nativeElement as HTMLElement;
  await vi.waitFor(async () => {
    await fixture.whenStable();
    fixture.detectChanges();
    expect(element.textContent).toContain('Room 2');
  });
  return { fixture, http, element, host: fixture.componentInstance as unknown as Host };
}

async function settled(host: Host): Promise<void> {
  await vi.waitFor(() => expect(host.busy()).toBe(false));
}

function open(element: HTMLElement, fixture: { detectChanges(): void }, label: string) {
  [...element.querySelectorAll('button')].find((b) => b.textContent?.includes(label))!.click();
  fixture.detectChanges();
}

describe('TimetablePage', () => {
  it('lists the lessons from Monday on, with their times and rooms', async () => {
    const { element } = await render();

    const text = element.textContent ?? '';
    expect(text.indexOf('Monday')).toBeLessThan(text.indexOf('Tuesday'));
    expect(text).toContain('09:00 - 10:00');
    expect(text).toContain('Whole school');
  });

  it('refuses a lesson that ends before it starts', async () => {
    const { fixture, http, element, host } = await render();
    open(element, fixture, 'Add lesson');
    host.form.setValue({ courseId: COURSE, day: 'Monday', start: '10:00', end: '09:00', campusId: null, room: '' });
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('form button[type="submit"]')!.click();
    fixture.detectChanges();

    http.expectNone((r) => r.method === 'POST');
    expect(element.textContent).toContain('The end time must be after the start time.');
  });

  it('adds a lesson with times the server understands', async () => {
    const { fixture, http, element, host } = await render();
    open(element, fixture, 'Add lesson');
    host.form.setValue({ courseId: COURSE, day: 'Wednesday', start: '08:30', end: '09:15', campusId: null, room: ' Lab 1 ' });
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('form button[type="submit"]')!.click();

    const request = http.expectOne((r) => r.url === '/api/TimetableSlots' && r.method === 'POST');
    expect(JSON.parse(request.request.body as string)).toMatchObject({ courseId: COURSE, day: 'Wednesday', start: '08:30:00', end: '09:15:00', room: 'Lab 1' });
    request.flush(blob('s3'), { status: 200, statusText: 'OK' });
    await settled(host);
    http.expectOne((r) => r.url.startsWith('/api/TimetableSlots') && r.method === 'GET').flush(blob([]));
  });

  it('tells which kind of clash a lesson would have, without saving it', async () => {
    const { fixture, http, element, host } = await render();
    open(element, fixture, 'Add lesson');
    host.form.setValue({ courseId: COURSE, day: 'Tuesday', start: '09:30', end: '10:30', campusId: null, room: 'Room 2' });
    fixture.detectChanges();

    open(element, fixture, 'Check for clashes');
    const request = http.expectOne((r) => r.url === '/api/TimetableSlots/conflicts');
    expect(JSON.parse(request.request.body as string)).toMatchObject({ courseId: COURSE, day: 'Tuesday', room: 'Room 2' });
    request.flush(blob([{ slotId: SLOT, kind: 'Room' }]));

    await vi.waitFor(async () => {
      await fixture.whenStable();
      fixture.detectChanges();
      expect(element.textContent).toContain('The room is already booked at this time.');
    });
    http.expectNone((r) => r.method === 'POST');
  });

  it('says when a lesson fits the timetable', async () => {
    const { fixture, http, element, host } = await render();
    open(element, fixture, 'Add lesson');
    host.form.setValue({ courseId: COURSE, day: 'Friday', start: '09:00', end: '10:00', campusId: null, room: '' });
    fixture.detectChanges();

    open(element, fixture, 'Check for clashes');
    http.expectOne((r) => r.url === '/api/TimetableSlots/conflicts').flush(blob([]));

    await vi.waitFor(async () => {
      await fixture.whenStable();
      fixture.detectChanges();
      expect(element.textContent).toContain('No clashes');
    });
  });

  it('moves a lesson through its id, keeping its course', async () => {
    const { fixture, http, element, host } = await render();
    const edit = element.querySelector<HTMLButtonElement>('button[aria-label="Edit this lesson"]')!;
    edit.click();
    fixture.detectChanges();
    host.form.setValue({ courseId: COURSE, day: 'Thursday', start: '10:00', end: '11:00', campusId: null, room: '' });
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('form button[type="submit"]')!.click();

    const request = http.expectOne((r) => r.method === 'PUT' && r.url.startsWith('/api/TimetableSlots/'));
    expect(request.request.url).toMatch(/0190f3a0-0000-7000-8000-00000000000[0-9]|other/);
    expect(JSON.parse(request.request.body as string)).toMatchObject({ day: 'Thursday', start: '10:00:00', end: '11:00:00' });
    request.flush(null, { status: 204, statusText: 'No Content' });
    await settled(host);
    http.expectOne((r) => r.url.startsWith('/api/TimetableSlots') && r.method === 'GET').flush(blob([]));
  });
});
