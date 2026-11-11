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
      { key: 'Grades', name: 'Grades', description: '', type: 'Default', isActive: true },
      { key: 'Parents', name: 'Parents', description: '', type: 'Custom', isActive: false },
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

  it('shows each module type and whether it is active', async () => {
    const { element } = await render();

    const switches = [...element.querySelectorAll<HTMLButtonElement>('button[role="switch"]')];
    expect(switches.map((s) => s.getAttribute('aria-checked'))).toEqual(['true', 'false']);
    expect([...element.querySelectorAll('mat-select')].map((s) => s.textContent?.trim())).toEqual(['Default', 'Custom']);
    expect(element.textContent).not.toContain('Always on');
  });

  it('deactivates a module and keeps its type', async () => {
    const { element, http } = await render();

    element.querySelectorAll<HTMLButtonElement>('button[role="switch"]')[0].click();

    const request = http.expectOne('/api/Modules/Grades');
    expect(request.request.method).toBe('PUT');
    expect(JSON.parse(request.request.body as string)).toEqual({ type: 'Default', isActive: false });
    request.flush(null, { status: 204, statusText: 'No Content' });
  });
});
