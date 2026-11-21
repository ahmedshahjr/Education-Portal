import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { describe, expect, it, vi } from 'vitest';
import { API_BASE_URL } from '../../../web-api-client';
import { NotificationBellComponent } from './notification-bell.component';

const blob = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const notifications = (unread: number) => ({
  items: [
    { id: 'n1', title: 'Fee reminder', body: 'Due on Friday', created: '2026-11-20T08:00:00Z' },
    { id: 'n2', title: 'Old news', created: '2026-11-01T08:00:00Z', readAt: '2026-11-02T08:00:00Z' },
  ],
  unreadCount: unread,
});

async function render(response: () => { body: unknown; status?: number }, bellShows = true) {
  TestBed.configureTestingModule({
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
  });
  const fixture = TestBed.createComponent(NotificationBellComponent);
  const http = TestBed.inject(HttpTestingController);
  fixture.detectChanges();
  const { body, status } = response();
  http.expectOne('/api/Notifications').flush(blob(body), { status: status ?? 200, statusText: status ? 'Error' : 'OK' });
  // The generated client reads every response as a Blob, which is read asynchronously.
  await vi.waitFor(async () => {
    await fixture.whenStable();
    fixture.detectChanges();
    expect((fixture.nativeElement as HTMLElement).querySelector('button.mm-bell') !== null).toBe(bellShows);
  });
  return { fixture, http, element: fixture.nativeElement as HTMLElement };
}

describe('NotificationBellComponent', () => {
  it('shows how many notifications are unread', async () => {
    const { element } = await render(() => ({ body: notifications(3) }));

    expect(element.querySelector('.mm-bell__count')?.textContent?.trim()).toBe('3');
  });

  it('shows no count when everything is read', async () => {
    const { element } = await render(() => ({ body: notifications(0) }));

    expect(element.querySelector('button.mm-bell')).not.toBeNull();
    expect(element.querySelector('.mm-bell__count')).toBeNull();
  });

  it('shows 99+ for more than 99', async () => {
    const { element } = await render(() => ({ body: notifications(150) }));

    expect(element.querySelector('.mm-bell__count')?.textContent?.trim()).toBe('99+');
  });

  it('shows no bell when the notifications cannot be read', async () => {
    const { element } = await render(() => ({ body: {}, status: 403 }), false);

    expect(element.querySelector('button.mm-bell')).toBeNull();
  });
});
