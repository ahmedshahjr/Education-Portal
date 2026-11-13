import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { ModulesPage } from './modules.page';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

async function render() {
  TestBed.configureTestingModule({ imports: [ModulesPage], providers: [provideHttpClient(), provideHttpClientTesting()] });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(ModulesPage);
  fixture.detectChanges();
  http.expectOne('/api/Modules').flush(
    json([
      { code: 'STUDENTS', name: 'Students', description: '', isCore: true, isActive: true, sortOrder: 50, requires: [] },
      { code: 'FEES', name: 'Fees', description: 'Fee collection.', isCore: false, isActive: false, sortOrder: 120, requires: ['STUDENTS'] },
    ])
  );
  const element = fixture.nativeElement as HTMLElement;
  // The generated client reads every response as a Blob, which is read asynchronously.
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(element.querySelectorAll('mat-slide-toggle').length).toBe(2);
  });
  return { fixture, http, element };
}

describe('ModulesPage', () => {
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('shows core and paid modules, what they require, and locks core modules on', async () => {
    const { element } = await render();

    const switches = [...element.querySelectorAll<HTMLButtonElement>('button[role="switch"]')];
    expect(switches.map((s) => s.getAttribute('aria-checked'))).toEqual(['true', 'false']);
    expect(switches.map((s) => s.disabled)).toEqual([true, false]);
    expect([...element.querySelectorAll('app-status-badge')].map((b) => b.textContent?.trim())).toEqual(['Core', 'Paid']);
    const rows = [...element.querySelectorAll('tr.mat-mdc-row')];
    expect(rows[0].textContent).toContain('Nothing');
    expect(rows[1].textContent).toContain('Students');
  });

  it('activates a paid module and keeps its other fields', async () => {
    const { element, http } = await render();

    element.querySelectorAll<HTMLButtonElement>('button[role="switch"]')[1].click();

    const request = http.expectOne('/api/Modules/FEES');
    expect(request.request.method).toBe('PUT');
    expect(JSON.parse(request.request.body as string)).toEqual({
      name: 'Fees',
      description: 'Fee collection.',
      isCore: false,
      sortOrder: 120,
      isActive: true,
    });
    request.flush(null, { status: 204, statusText: 'No Content' });
  });
});
