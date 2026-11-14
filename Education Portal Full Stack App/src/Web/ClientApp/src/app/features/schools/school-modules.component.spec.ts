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
  { code: 'STUDENTS', name: 'Students', description: '', isCore: true, isActive: true, isEnabled: true },
  { code: 'EXAMS', name: 'Exams', description: '', isCore: false, isActive: true, isEnabled: true },
  { code: 'LIBRARY', name: 'Library', description: '', isCore: false, isActive: true, isEnabled: true },
  { code: 'FEES', name: 'Fees', description: '', isCore: false, isActive: true, isEnabled: false },
  { code: 'HOSTEL', name: 'Hostel', description: '', isCore: false, isActive: false, isEnabled: false },
];
const subscription = { planCode: 'BASIC', status: 'Active', modules: [] };
const plans = [{ id: 'p1', code: 'BASIC', name: 'Basic', description: '', isActive: true, modules: ['EXAMS'] }];

const openDialog = vi.fn();

async function render(subscriptionBody: unknown | null = subscription) {
  TestBed.configureTestingModule({
    imports: [SchoolModulesComponent],
    providers: [provideHttpClient(), provideHttpClientTesting(), { provide: MatDialog, useValue: { open: openDialog } }],
  });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(SchoolModulesComponent);
  fixture.componentRef.setInput('school', new SchoolSummaryDto({ id: SCHOOL, name: 'School One', domain: 'school1.com', isMaster: false, countryId: undefined }));
  fixture.detectChanges();
  http.expectOne('/api/Plans').flush(json(plans));
  if (subscriptionBody === null) {
    http.expectOne(url('subscription')).flush(json({}), { status: 404, statusText: 'Not Found' });
  } else {
    http.expectOne(url('subscription')).flush(json(subscriptionBody));
  }
  http.expectOne(url('modules')).flush(json(modules));
  const element = fixture.nativeElement as HTMLElement;
  // The generated client reads every response as a Blob, which is read asynchronously.
  await vi.waitFor(() => {
    fixture.detectChanges();
    expect(element.querySelectorAll('mat-slide-toggle').length).toBe(modules.length);
  });
  return { fixture, http, element };
}

/** After a change the component reloads what the server holds: the subscription and the module list. */
async function reloadAfterChange(http: HttpTestingController) {
  await vi.waitFor(() => http.expectOne(url('subscription')).flush(json(subscription)));
  await vi.waitFor(() => http.expectOne(url('modules')).flush(json(modules)));
}

/** The switch for one module, by its row order in the list. */
const switchInput = (element: HTMLElement, index: number) => element.querySelectorAll<HTMLButtonElement>('[role="switch"]')[index];

describe('SchoolModulesComponent', () => {
  beforeEach(() => openDialog.mockReset());
  afterEach(() => TestBed.inject(HttpTestingController).verify());

  it('locks core, inactive and plan modules, and leaves add-ons switchable', async () => {
    const { element } = await render();

    expect(switchInput(element, 0).disabled).toBe(true); // STUDENTS: core
    expect(switchInput(element, 1).disabled).toBe(true); // EXAMS: in the plan
    expect(switchInput(element, 2).disabled).toBe(false); // LIBRARY: add-on that is on
    expect(switchInput(element, 3).disabled).toBe(false); // FEES: add-on that is off
    expect(switchInput(element, 4).disabled).toBe(true); // HOSTEL: made inactive by the master
  });

  it('keeps add-on switches off when the school has no subscription yet', async () => {
    const { element } = await render(null);

    expect(switchInput(element, 3).disabled).toBe(true);
    expect(element.textContent).toContain('Start the school');
  });

  it('adds a paid module to the subscription when its switch is turned on', async () => {
    const { http, element } = await render();

    switchInput(element, 3).click();

    const request = http.expectOne(url('subscription/modules'));
    expect(request.request.method).toBe('POST');
    expect(JSON.parse(request.request.body as string)).toMatchObject({ moduleCode: 'FEES' });
    request.flush(null, { status: 204, statusText: 'No Content' });
    await reloadAfterChange(http);
  });

  it('removes an add-on only after the master confirms', async () => {
    openDialog.mockReturnValue({ afterClosed: () => of(true) });
    const { http, element } = await render();

    switchInput(element, 2).click();

    expect(openDialog).toHaveBeenCalledOnce();
    const request = http.expectOne(url('subscription/modules/LIBRARY'));
    expect(request.request.method).toBe('DELETE');
    request.flush(null, { status: 204, statusText: 'No Content' });
    await reloadAfterChange(http);
  });

  it('keeps the module on when the master declines the confirm step', async () => {
    openDialog.mockReturnValue({ afterClosed: () => of(false) });
    const { http, element } = await render();

    switchInput(element, 2).click();

    http.expectNone(url('subscription/modules/LIBRARY'));
    expect(switchInput(element, 2).getAttribute('aria-checked')).toBe('true');
  });
});
