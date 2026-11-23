import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';
import { API_BASE_URL } from '../../web-api-client';
import { SchoolEventsPage } from './school-events.page';

const blob = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

interface Host {
  form: { setValue(value: unknown): void };
  range: { setValue(value: unknown): void };
  busy(): boolean;
}

async function render() {
  TestBed.configureTestingModule({
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
  });
  const fixture = TestBed.createComponent(SchoolEventsPage);
  const http = TestBed.inject(HttpTestingController);
  fixture.detectChanges();
  http
    .expectOne((r) => r.url.startsWith('/api/SchoolEvents'))
    .flush(
      blob([
        { id: 'e2', title: 'Sports day', startsAt: '2026-11-20T08:00:00Z', endsAt: '2026-11-20T12:00:00Z', audience: 'Students' },
        { id: 'e1', title: 'Parent meeting', startsAt: '2026-11-10T08:00:00Z', endsAt: '2026-11-10T09:00:00Z', audience: 'Parents' },
      ])
    );
  const element = fixture.nativeElement as HTMLElement;
  await vi.waitFor(async () => {
    await fixture.whenStable();
    fixture.detectChanges();
    expect(element.textContent).toContain('Sports day');
  });
  return { fixture, http, element, host: fixture.componentInstance as unknown as Host };
}

describe('SchoolEventsPage', () => {
  it('asks for the events of the current month to begin with', async () => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
    });
    TestBed.createComponent(SchoolEventsPage).detectChanges();

    const request = TestBed.inject(HttpTestingController).expectOne((r) => r.url.startsWith('/api/SchoolEvents'));

    // The generated client sends the range as full timestamps; the first one is the start of this month, local time.
    const from = new Date(new URL(request.request.urlWithParams, 'http://localhost').searchParams.get('From') ?? '');
    const now = new Date();
    expect(from.getTime()).toBe(new Date(now.getFullYear(), now.getMonth(), 1).getTime());
  });

  it('lists the events in time order with who they are for', async () => {
    const { element } = await render();

    const text = element.textContent ?? '';
    expect(text.indexOf('Parent meeting')).toBeLessThan(text.indexOf('Sports day'));
    expect(text).toContain('Parents');
    expect(text).toContain('Students');
  });

  it('refuses an event that ends before it starts', async () => {
    const { fixture, http, element, host } = await render();
    [...element.querySelectorAll('button')].find((b) => b.textContent?.includes('Add event'))!.click();
    fixture.detectChanges();
    host.form.setValue({ title: 'Trip', description: '', startsAt: '2026-11-25T10:00', endsAt: '2026-11-25T09:00', audience: 'All' });
    fixture.detectChanges();

    element.querySelectorAll<HTMLButtonElement>('form button[type="submit"]')[1].click();
    fixture.detectChanges();

    http.expectNone((r) => r.method === 'POST');
    expect(element.textContent).toContain('The event must end after it starts.');
  });

  it('adds an event for an audience', async () => {
    const { fixture, http, element, host } = await render();
    [...element.querySelectorAll('button')].find((b) => b.textContent?.includes('Add event'))!.click();
    fixture.detectChanges();
    host.form.setValue({ title: ' Science fair ', description: '', startsAt: '2026-11-25T09:00', endsAt: '2026-11-25T12:00', audience: 'Staff' });
    fixture.detectChanges();

    element.querySelectorAll<HTMLButtonElement>('form button[type="submit"]')[1].click();

    const request = http.expectOne((r) => r.url === '/api/SchoolEvents' && r.method === 'POST');
    const body = JSON.parse(request.request.body as string);
    expect(body).toMatchObject({ title: 'Science fair', audience: 'Staff' });
    expect(new Date(body.startsAt).getTime()).toBe(new Date('2026-11-25T09:00').getTime());
    request.flush(blob('e3'), { status: 200, statusText: 'OK' });
    await vi.waitFor(() => expect(host.busy()).toBe(false));
    http.expectOne((r) => r.url.startsWith('/api/SchoolEvents') && r.method === 'GET').flush(blob([]));
  });

  it('does not ask for events when the dates are not a range', async () => {
    const { fixture, http, element, host } = await render();
    host.range.setValue({ from: '2026-12-01', to: '2026-11-01' });
    fixture.detectChanges();

    element.querySelector<HTMLButtonElement>('form button[type="submit"]')!.click();
    fixture.detectChanges();

    http.expectNone((r) => r.url.startsWith('/api/SchoolEvents'));
    expect(element.textContent).toContain('not before it');
  });

  it('names every audience with its own label, and offers all of them', async () => {
    const { fixture } = await render();
    const page = fixture.componentInstance as unknown as { audiences: readonly string[]; audienceLabel(a: string): string };
    const labels = page.audiences.map((a) => page.audienceLabel(a));

    expect(page.audiences.length).toBeGreaterThan(0);
    expect(labels.every((text) => text.length > 0)).toBe(true);
    expect(new Set(labels).size).toBe(page.audiences.length);
  });

  it('refuses an event that ends at the moment it starts', async () => {
    const { fixture, http, element, host } = await render();
    [...element.querySelectorAll('button')].find((b) => b.textContent?.includes('Add event'))!.click();
    fixture.detectChanges();
    host.form.setValue({ title: 'Trip', description: '', startsAt: '2026-11-25T10:00', endsAt: '2026-11-25T10:00', audience: 'All' });
    fixture.detectChanges();

    element.querySelectorAll<HTMLButtonElement>('form button[type="submit"]')[1].click();
    fixture.detectChanges();

    http.expectNone((r) => r.method === 'POST');
    expect(element.textContent).toContain('The event must end after it starts.');
  });
});
