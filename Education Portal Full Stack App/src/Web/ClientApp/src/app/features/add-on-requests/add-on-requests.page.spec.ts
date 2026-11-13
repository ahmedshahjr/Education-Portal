import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { AddOnRequestsPage } from './add-on-requests.page';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });
const pending = {
  id: '0190f3a0-0000-7000-8000-000000000001',
  schoolId: '0190f3a0-0000-7000-8000-000000000005',
  schoolName: 'School One',
  moduleCode: 'LIBRARY',
  moduleName: 'Library',
  status: 'Pending',
  created: '2026-11-13T09:00:00Z',
};

async function render(body: unknown[]) {
  TestBed.configureTestingModule({ imports: [AddOnRequestsPage], providers: [provideHttpClient(), provideHttpClientTesting()] });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(AddOnRequestsPage);
  fixture.detectChanges();
  http.expectOne('/api/AddOnRequests?status=Pending').flush(json(body));
  const element = fixture.nativeElement as HTMLElement;
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(element.querySelector('app-loading-state')).toBeNull();
  });
  return { fixture, http, element };
}

describe('AddOnRequestsPage', () => {
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('lists pending requests with the school and the module', async () => {
    const { element } = await render([pending]);

    expect(element.querySelectorAll('li').length).toBe(1);
    expect(element.textContent).toContain('School One');
    expect(element.textContent).toContain('Library');
  });

  it('shows an empty state when nothing is waiting', async () => {
    const { element } = await render([]);

    expect(element.querySelectorAll('li').length).toBe(0);
    expect(element.querySelector('app-empty-state')).not.toBeNull();
  });

  it('approves a request and reloads the list', async () => {
    const { fixture, http, element } = await render([pending]);

    element.querySelector<HTMLButtonElement>('li button')!.click();

    const approve = http.expectOne(`/api/AddOnRequests/${pending.id}/approve`);
    expect(approve.request.method).toBe('POST');
    approve.flush(null, { status: 204, statusText: 'No Content' });
    await vi.waitFor(() => http.expectOne('/api/AddOnRequests?status=Pending').flush(json([])));
    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(element.querySelectorAll('li').length).toBe(0);
    });
  });

  it('rejects a request', async () => {
    const { http, element } = await render([pending]);

    element.querySelectorAll<HTMLButtonElement>('li button')[1].click();

    const reject = http.expectOne(`/api/AddOnRequests/${pending.id}/reject`);
    expect(reject.request.method).toBe('POST');
    reject.flush(null, { status: 204, statusText: 'No Content' });
    await vi.waitFor(() => http.expectOne('/api/AddOnRequests?status=Pending').flush(json([pending])));
  });
});
