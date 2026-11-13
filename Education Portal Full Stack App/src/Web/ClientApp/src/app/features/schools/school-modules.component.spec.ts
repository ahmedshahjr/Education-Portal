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
    expect(element.querySelectorAll('li').length).toBe(3);
  });
  return { fixture, http, element };
}

describe('SchoolModulesComponent', () => {
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('shows which modules are on and has no switch, because modules change through the subscription', async () => {
    const { element } = await render();

    expect(element.querySelector('mat-slide-toggle')).toBeNull();
    const rows = [...element.querySelectorAll('li')].map((row) => row.textContent ?? '');
    expect(rows[0]).toContain('Core');
    expect(rows[0]).toContain('On');
    expect(rows[1]).toContain('Inactive');
    expect(rows[1]).toContain('Off');
    expect(rows[2]).toContain('Paid');
    expect(rows[2]).toContain('Off');
  });
});
