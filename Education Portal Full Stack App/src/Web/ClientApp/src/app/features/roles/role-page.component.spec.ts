import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, provideRouter, Router } from '@angular/router';
import { of } from 'rxjs';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { CampusDto, CampusesClient, CreateRoleCommand, ModuleCode, ModulesClient, SchoolRolesClient } from '../../web-api-client';
import { RolePageComponent } from './role-page.component';
import { RIGHT_KEYS } from './role-rights';

const MAIN = '0190f3a0-0000-7000-8000-000000000001';
const NORTH = '0190f3a0-0000-7000-8000-000000000002';

const campus = (id: string, name: string) =>
  ({ id, name, code: '0001', displayCode: name, kind: 'Main', genderPolicy: 'CoEducation', details: {} }) as unknown as CampusDto;

interface SetupOptions {
  readonly campuses: CampusDto[];
  readonly modules?: ModuleCode[];
}

/** Opens the add-role page. Every client is mocked, so the page's grid is built from these answers only. */
function setup({ campuses, modules = [ModuleCode.School, ModuleCode.Students, ModuleCode.Fees] }: SetupOptions) {
  const rolesClient = {
    getRoles: vi.fn(() => of([])),
    createRole: vi.fn((_command: CreateRoleCommand) => of('0190f3a0-0000-7000-8000-0000000000aa')),
    updateRole: vi.fn(() => of(undefined)),
  };

  TestBed.configureTestingModule({
    imports: [RolePageComponent],
    providers: [
      provideRouter([]),
      { provide: ModulesClient, useValue: { getEnabledModules: () => of(modules) } },
      { provide: CampusesClient, useValue: { getCampuses: () => of(campuses) } },
      { provide: SchoolRolesClient, useValue: rolesClient },
      { provide: ActivatedRoute, useValue: { snapshot: { paramMap: convertToParamMap({}) } } },
    ],
  });

  const fixture = TestBed.createComponent(RolePageComponent);
  fixture.detectChanges();
  return { fixture, page: fixture.componentInstance, element: fixture.nativeElement as HTMLElement, rolesClient };
}

/** Clicks the checkbox inside the label that contains the text, as a user does (this runs the form's change handling). */
function clickBox(element: HTMLElement, text: string) {
  const label = [...element.querySelectorAll('label')].find((l) => l.textContent?.includes(text));
  const box = label?.querySelector('input');
  if (!box) {
    throw new Error(`No checkbox labelled ${text}.`);
  }
  box.click();
}

describe('RolePageComponent', () => {
  beforeEach(() => TestBed.resetTestingModule());
  afterEach(() => TestBed.resetTestingModule());

  it('keeps the single rights grid, with no campus choice, when the school has one campus', () => {
    const { element } = setup({ campuses: [campus(MAIN, 'Main')], modules: [ModuleCode.Students] });

    expect(element.querySelector('.mm-roles__matrix')).not.toBeNull();
    expect(element.querySelector('.mm-roles__campuses')).toBeNull();
    expect(element.textContent).not.toContain('All campuses');
  });

  it('shows an "All campuses" box per module, and one box per campus once "All campuses" is cleared', () => {
    const { element, fixture } = setup({ campuses: [campus(MAIN, 'Main'), campus(NORTH, 'North')], modules: [ModuleCode.Students] });

    expect(element.textContent).toContain('All campuses');
    expect(element.textContent).not.toContain('North');

    clickBox(element, 'All campuses');
    fixture.detectChanges();

    expect(element.textContent).toContain('Main');
    expect(element.textContent).toContain('North');
  });

  it('sends all campuses and no campus ids for a role on a one campus school', () => {
    const { element, fixture, page, rolesClient } = setup({ campuses: [campus(MAIN, 'Main')], modules: [ModuleCode.Students] });
    vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    page['form'].controls.name.setValue('Registrar');
    page['grant'](ModuleCode.Students, 'canView').setValue(true);
    fixture.detectChanges();
    expect(element.querySelector('.mm-roles__campuses')).toBeNull();

    page['save']();

    const sent = rolesClient.createRole.mock.calls[0][0];
    expect(sent.permissions).toHaveLength(1);
    expect(sent.permissions?.[0]).toMatchObject({ module: ModuleCode.Students, canView: true, allCampuses: true, campusIds: [] });
  });

  it('sends the campuses ticked for a module that is not for all campuses', () => {
    const { element, fixture, page, rolesClient } = setup({
      campuses: [campus(MAIN, 'Main'), campus(NORTH, 'North')],
      modules: [ModuleCode.Students],
    });
    vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
    page['form'].controls.name.setValue('Registrar');
    page['grant'](ModuleCode.Students, 'canView').setValue(true);
    clickBox(element, 'All campuses');
    fixture.detectChanges();
    clickBox(element, 'North');
    fixture.detectChanges();

    page['save']();

    const sent = rolesClient.createRole.mock.calls[0][0];
    expect(sent.permissions?.[0]).toMatchObject({ module: ModuleCode.Students, canView: true, allCampuses: false, campusIds: [NORTH] });
  });

  it('does not show the School module, which every school has and a role does not set', () => {
    const { element, page } = setup({ campuses: [campus(MAIN, 'Main')] });

    expect(page['modules'].map((m) => m.code)).toEqual([ModuleCode.Students, ModuleCode.Fees]);
    expect(element.querySelectorAll('tbody tr')).toHaveLength(2);
  });

  it('ticks every right of every module with "Select all", and clears them again', () => {
    const { element, fixture, page } = setup({ campuses: [campus(MAIN, 'Main')], modules: [ModuleCode.Students, ModuleCode.Fees] });
    const selectAll = element.querySelector<HTMLInputElement>('.mm-roles__select-all input');
    expect(selectAll).not.toBeNull();

    selectAll?.click();
    fixture.detectChanges();

    for (const module of [ModuleCode.Students, ModuleCode.Fees]) {
      for (const right of RIGHT_KEYS) {
        expect(page['grant'](module, right).value).toBe(true);
      }
    }
    expect(page['allSelected']()).toBe(true);

    selectAll?.click();
    fixture.detectChanges();
    expect(page['grant'](ModuleCode.Students, 'canView').value).toBe(false);
  });
});
