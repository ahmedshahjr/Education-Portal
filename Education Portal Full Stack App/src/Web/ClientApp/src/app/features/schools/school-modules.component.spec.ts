import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { SchoolSummaryDto } from '../../web-api-client';
import { SchoolModulesComponent } from './school-modules.component';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const modules = [
  { code: 'STUDENTS', name: 'Students', description: '', isCore: true, isActive: true, isEnabled: true },
  { code: 'EXAMS', name: 'Exams', description: '', isCore: false, isActive: false, isEnabled: false },
  { code: 'FEES', name: 'Fees', description: '', isCore: false, isActive: true, isEnabled: false },
];

async function render() {
  TestBed.configureTestingModule({ imports: [SchoolModulesComponent], providers: [provideHttpClient(), provideHttpClientTesting()] });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(SchoolModulesComponent);
  fixture.componentRef.setInput('school', new SchoolSummaryDto({ id: '0190f3a0-0000-7000-8000-000000000005', name: 'School One', domain: 'school1.com', isMaster: false, countryId: undefined }));
  fixture.detectChanges();
  http.expectOne('/api/Schools/0190f3a0-0000-7000-8000-000000000005/modules').flush(json(modules));
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

  it('locks core and inactive modules and leaves active paid modules switchable', async () => {
    const { element } = await render();

    const switches = [...element.querySelectorAll<HTMLButtonElement>('button[role="switch"]')];
    expect(switches.map((s) => s.disabled)).toEqual([true, true, false]);
    expect(switches.map((s) => s.getAttribute('aria-checked'))).toEqual(['true', 'false', 'false']);
    expect(element.textContent).toContain('Core');
    expect(element.textContent).toContain('Paid');
    expect(element.textContent).toContain('Inactive');
  });

  it('switches a paid module on for the school', async () => {
    const { element, http } = await render();

    element.querySelectorAll<HTMLButtonElement>('button[role="switch"]')[2].click();

    const request = http.expectOne('/api/Schools/0190f3a0-0000-7000-8000-000000000005/modules/FEES');
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toContain('"isEnabled":true');
    request.flush(null, { status: 204, statusText: 'No Content' });
  });
});
