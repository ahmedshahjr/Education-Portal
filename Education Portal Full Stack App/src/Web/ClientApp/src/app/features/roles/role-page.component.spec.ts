import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { API_BASE_URL, CampusDto, ModuleCode } from '../../web-api-client';
import { RolePageComponent } from './role-page.component';

const MAIN = '0190f3a0-0000-7000-8000-000000000001';
const NORTH = '0190f3a0-0000-7000-8000-000000000002';

const RIGHT_LABELS = ['View', 'Create', 'Edit', 'Delete', 'Approve', 'Export'];

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });

const campus = (id: string, name: string) =>
  ({ id, name, code: '0001', displayCode: name, kind: 'Main', genderPolicy: 'CoEducation', details: {} }) as unknown as CampusDto;

/**
 * Opens the add-role page against the real clients and HttpClient. The only fake is the HTTP backend: the test answers
 * the modules and campuses requests, and reads the role the page sends.
 */
async function openPage(options: { campuses: CampusDto[]; modules?: ModuleCode[] }) {
  TestBed.configureTestingModule({
    imports: [RolePageComponent],
    providers: [provideRouter([{ path: '**', children: [] }]), provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
  });
  const fixture = TestBed.createComponent(RolePageComponent);
  const http = TestBed.inject(HttpTestingController);
  fixture.detectChanges();

  // The generated client reads each body as a Blob on a later macrotask, so each answer is awaited before the next request.
  http.expectOne((r) => r.url.startsWith('/api/Modules/enabled')).flush(json(options.modules ?? [ModuleCode.School, ModuleCode.Students, ModuleCode.Fees]));
  await new Promise((resolve) => setTimeout(resolve));
  http.expectOne((r) => r.url.startsWith('/api/Campuses')).flush(json(options.campuses));
  for (let i = 0; i < 5; i++) {
    await new Promise((resolve) => setTimeout(resolve));
    fixture.detectChanges();
  }
  return { fixture, http, element: fixture.nativeElement as HTMLElement };
}

/** Types into the form field whose label is `label`, as a user does. */
function typeInto(element: HTMLElement, label: string, value: string) {
  const field = [...element.querySelectorAll('mat-form-field')].find((f) => f.querySelector('mat-label')?.textContent?.includes(label));
  const input = field?.querySelector<HTMLInputElement | HTMLTextAreaElement>('input, textarea');
  if (!input) {
    throw new Error(`No field labelled ${label}.`);
  }
  input.value = value;
  input.dispatchEvent(new Event('input'));
}

/** Clicks the checkbox inside the label that contains the text, as a user does. */
function clickBox(element: HTMLElement, text: string) {
  const label = [...element.querySelectorAll('label')].find((l) => l.textContent?.includes(text));
  const box = label?.querySelector('input');
  if (!box) {
    throw new Error(`No checkbox labelled ${text}.`);
  }
  box.click();
}

/** The checkbox for a right of a module, found by the aria-label the page gives it. */
function rightBox(element: HTMLElement, moduleText: string, rightLabel: string): HTMLInputElement {
  const box = element.querySelector<HTMLInputElement>(`input[aria-label="${moduleText} ${rightLabel}"]`);
  if (!box) {
    throw new Error(`No box for ${moduleText} ${rightLabel}.`);
  }
  return box;
}

/** Lets the generated client finish reading a response body, and the page react to it, over several macrotasks. */
async function settle() {
  for (let i = 0; i < 5; i++) {
    await new Promise((resolve) => setTimeout(resolve));
  }
}

/** The role the page sent: the generated client serialises the command to JSON. */
function sentBody(request: TestRequest): { permissions: { module: ModuleCode; canView: boolean; allCampuses: boolean; campusIds: string[] }[] } {
  return JSON.parse(request.request.body as string);
}

function save(element: HTMLElement, fixture: ComponentFixture<RolePageComponent>, http: HttpTestingController): TestRequest {
  element.querySelector<HTMLButtonElement>('button[type="submit"]')!.click();
  fixture.detectChanges();
  return http.expectOne({ method: 'POST', url: '/api/SchoolRoles' });
}

describe('RolePageComponent (integration)', () => {
  beforeEach(() => TestBed.resetTestingModule());
  afterEach(() => {
    vi.restoreAllMocks();
    TestBed.resetTestingModule();
  });

  it('keeps the single rights grid, with no campus choice, when the school has one campus', async () => {
    const { element } = await openPage({ campuses: [campus(MAIN, 'Main')], modules: [ModuleCode.Students] });

    expect(element.querySelector('.mm-roles__matrix')).not.toBeNull();
    expect(element.querySelector('.mm-roles__campuses')).toBeNull();
    expect(element.textContent).not.toContain('All campuses');
  });

  it('shows an "All campuses" box per module, and one box per campus once "All campuses" is cleared', async () => {
    const { element, fixture } = await openPage({ campuses: [campus(MAIN, 'Main'), campus(NORTH, 'North')], modules: [ModuleCode.Students] });

    expect(element.textContent).toContain('All campuses');
    expect(element.textContent).not.toContain('North');

    clickBox(element, 'All campuses');
    fixture.detectChanges();

    expect(element.textContent).toContain('Main');
    expect(element.textContent).toContain('North');
  });

  it('sends all campuses and no campus ids for a role on a one campus school', async () => {
    const { element, fixture, http } = await openPage({ campuses: [campus(MAIN, 'Main')], modules: [ModuleCode.Students] });
    vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    typeInto(element, 'Role name', 'Registrar');
    rightBox(element, 'Students', 'View').click();
    fixture.detectChanges();

    const request = save(element, fixture, http);

    expect(sentBody(request).permissions).toHaveLength(1);
    expect(sentBody(request).permissions[0]).toMatchObject({ module: ModuleCode.Students, canView: true, allCampuses: true, campusIds: [] });
    request.flush(json(MAIN));
    // The saved role shows a toast and leaves the page once its body is read: finish that before the test ends.
    await settle();
  });

  it('sends only the campuses ticked for a module that is not for all campuses', async () => {
    const { element, fixture, http } = await openPage({
      campuses: [campus(MAIN, 'Main'), campus(NORTH, 'North')],
      modules: [ModuleCode.Students],
    });
    vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    typeInto(element, 'Role name', 'Registrar');
    rightBox(element, 'Students', 'View').click();
    clickBox(element, 'All campuses');
    fixture.detectChanges();
    clickBox(element, 'North');
    fixture.detectChanges();

    const request = save(element, fixture, http);

    expect(sentBody(request).permissions[0]).toMatchObject({ module: ModuleCode.Students, canView: true, allCampuses: false, campusIds: [NORTH] });
    request.flush(json(MAIN));
    // The saved role shows a toast and leaves the page once its body is read: finish that before the test ends.
    await settle();
  });

  it('does not show the School module, which every school has and a role does not set', async () => {
    const { element } = await openPage({ campuses: [campus(MAIN, 'Main')] });

    expect(element.querySelectorAll('tbody tr')).toHaveLength(2);
    expect(element.querySelector('tbody')?.textContent).not.toContain('School');
  });

  it('ticks every right of every module with "Select all", and clears them again', async () => {
    const { element, fixture } = await openPage({ campuses: [campus(MAIN, 'Main')], modules: [ModuleCode.Students, ModuleCode.Fees] });
    const selectAll = element.querySelector<HTMLInputElement>('.mm-roles__select-all input');
    expect(selectAll).not.toBeNull();

    selectAll?.click();
    fixture.detectChanges();

    for (const right of RIGHT_LABELS) {
      expect(rightBox(element, 'Students', right).checked).toBe(true);
      expect(rightBox(element, 'Fees', right).checked).toBe(true);
    }

    selectAll?.click();
    fixture.detectChanges();
    expect(rightBox(element, 'Students', 'View').checked).toBe(false);
  });
});
