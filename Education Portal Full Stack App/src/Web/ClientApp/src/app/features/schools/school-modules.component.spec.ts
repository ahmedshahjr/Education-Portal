import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { SchoolSummaryDto } from '../../web-api-client';
import { SchoolModulesComponent } from './school-modules.component';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const modules = [
  { key: 'Branches', name: 'Branches', description: '', type: 'Default', isActive: true, isEnabled: false, followsDefault: false },
  { key: 'Grades', name: 'Grades', description: '', type: 'Default', isActive: false, isEnabled: false, followsDefault: true },
  { key: 'Parents', name: 'Parents', description: '', type: 'Custom', isActive: true, isEnabled: false, followsDefault: true },
];

async function render() {
  TestBed.configureTestingModule({ imports: [SchoolModulesComponent], providers: [provideHttpClient(), provideHttpClientTesting()] });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(SchoolModulesComponent);
  fixture.componentRef.setInput('school', new SchoolSummaryDto({ id: 5, name: 'School One', domain: 'school1.com', isMaster: false, countryId: undefined }));
  fixture.detectChanges();
  http.expectOne('/api/Schools/5/modules').flush(json(modules));
  const element = fixture.nativeElement as HTMLElement;
  // The generated client reads every response as a Blob, which is read asynchronously.
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(element.querySelectorAll('mat-slide-toggle').length).toBe(3);
  });
  return { fixture, http, element };
}

describe('SchoolModulesComponent', () => {
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('shows each module type and locks only inactive modules', async () => {
    const { element } = await render();

    const switches = [...element.querySelectorAll<HTMLButtonElement>('button[role="switch"]')];
    expect(switches.map((s) => s.disabled)).toEqual([false, true, false]);
    expect(switches.map((s) => s.getAttribute('aria-checked'))).toEqual(['false', 'false', 'false']);
    expect(element.textContent).toContain('Default');
    expect(element.textContent).toContain('Custom');
    expect(element.textContent).toContain('Inactive');
  });

  it('switches a custom module on for the school', async () => {
    const { element, http } = await render();

    element.querySelectorAll<HTMLButtonElement>('button[role="switch"]')[2].click();

    const request = http.expectOne('/api/Schools/5/modules/Parents');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toContain('"isEnabled":true');
    request.flush(null, { status: 204, statusText: 'No Content' });
  });
});
