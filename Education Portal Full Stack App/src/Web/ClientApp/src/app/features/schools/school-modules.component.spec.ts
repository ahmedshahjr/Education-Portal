import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { of } from 'rxjs';
import { SchoolSummaryDto } from '../../web-api-client';
import { SchoolModulesComponent } from './school-modules.component';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });
const SCHOOL = '0190f3a0-0000-7000-8000-000000000005';
const url = (path: string) => `/api/Schools/${SCHOOL}/${path}`;

const modules = [
  { code: 'STUDENTS', name: 'Students', description: '', isCore: true, isActive: true, isDefaultEnabled: true, isEnabled: true },
  { code: 'EXAMS', name: 'Exams', description: '', isCore: false, isActive: true, isDefaultEnabled: false, isEnabled: true },
  { code: 'LIBRARY', name: 'Library', description: '', isCore: false, isActive: true, isDefaultEnabled: false, isEnabled: false },
  { code: 'HOSTEL', name: 'Hostel', description: '', isCore: false, isActive: false, isDefaultEnabled: false, isEnabled: false },
];

const openDialog = vi.fn();

async function render() {
  TestBed.configureTestingModule({
    imports: [SchoolModulesComponent],
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: MatDialog, useValue: { open: openDialog } }],
  });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(SchoolModulesComponent);
  fixture.componentRef.setInput('school', new SchoolSummaryDto({ id: SCHOOL, name: 'School One', domain: 'school1.com', isMaster: false, countryId: undefined }));
  fixture.detectChanges();
  http.expectOne(url('modules')).flush(json(modules));
  const element = fixture.nativeElement as HTMLElement;
  // The generated client reads every response as a Blob, which is read asynchronously.
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(element.querySelectorAll('mat-slide-toggle').length).toBe(modules.length);
  });
  return { fixture, http, element };
}

/** After a change the component reloads the module list the server holds. */
async function reloadAfterChange(http: HttpTestingController) {
  await vi.waitFor(() => http.expectOne(url('modules')).flush(json(modules)));
}

/** The switch for one module, by its row order in the list. */
const switchInput = (element: HTMLElement, index: number) => element.querySelectorAll<HTMLButtonElement>('[role="switch"]')[index];

describe('SchoolModulesComponent', () => {
  beforeEach(() => openDialog.mockReset());
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('lets the master switch every module, core and default ones included', async () => {
    const { element } = await render();

    expect(switchInput(element, 0).disabled).toBe(false); // STUDENTS: core, on by default
    expect(switchInput(element, 1).disabled).toBe(false); // EXAMS: paid, on
    expect(switchInput(element, 2).disabled).toBe(false); // LIBRARY: paid, off
  });

  it('locks a module the master switched off for every school', async () => {
    const { element } = await render();

    expect(switchInput(element, 3).disabled).toBe(true); // HOSTEL: inactive
  });

  it('turns a module on for the school without asking first', async () => {
    const { element, http } = await render();

    switchInput(element, 2).click();

    const request = http.expectOne({ method: 'PUT', url: url('modules/LIBRARY') });
    expect(JSON.parse(request.request.body as string)).toEqual({ enabled: true });
    request.flush(null, { status: 204, statusText: 'No Content' });
    await reloadAfterChange(http);
  });

  it('turns a module off only after the master confirms', async () => {
    openDialog.mockReturnValue({ afterClosed: () => of(true) });
    const { element, http } = await render();

    switchInput(element, 1).click();

    const request = http.expectOne({ method: 'PUT', url: url('modules/EXAMS') });
    expect(JSON.parse(request.request.body as string)).toEqual({ enabled: false });
    request.flush(null, { status: 204, statusText: 'No Content' });
    await reloadAfterChange(http);
  });

  it('sends nothing when the master declines the confirm step', async () => {
    openDialog.mockReturnValue({ afterClosed: () => of(false) });
    const { element, http } = await render();

    switchInput(element, 1).click();

    http.expectNone({ method: 'PUT', url: url('modules/EXAMS') });
  });
});
