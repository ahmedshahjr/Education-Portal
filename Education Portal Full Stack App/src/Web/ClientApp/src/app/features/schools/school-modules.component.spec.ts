import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { SchoolSummaryDto } from '../../web-api-client';
import { SchoolModulesComponent } from './school-modules.component';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const modules = [
  { key: 'Branches', name: 'Branches', description: '', isCore: true, isDefault: true, isEnabled: true, followsDefault: true },
  { key: 'Grades', name: 'Grades', description: '', isCore: false, isDefault: true, isEnabled: true, followsDefault: true },
  { key: 'Parents', name: 'Parents', description: '', isCore: false, isDefault: false, isEnabled: false, followsDefault: true },
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

  it('locks core modules on and shows which are default or custom', async () => {
    const { element } = await render();

    const switches = [...element.querySelectorAll<HTMLButtonElement>('button[role="switch"]')];
    expect(switches.map((s) => s.disabled)).toEqual([true, false, false]);
    expect(switches.map((s) => s.getAttribute('aria-checked'))).toEqual(['true', 'true', 'false']);
    expect(element.textContent).toContain('Always on');
    expect(element.textContent).toContain('Custom');
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
