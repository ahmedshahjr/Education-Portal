import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { API_BASE_URL, UserDetailsDto, UserType } from '../../web-api-client';
import { PersonFormData, PersonFormResult } from './person-form.models';
import { PersonSaverService } from './person-saver.service';

const json = (body: unknown) => new Blob([JSON.stringify(body)], { type: 'application/json' });
const PERSON = '0190f3a0-0000-7000-8000-000000000021';
const NEW_PERSON = '0190f3a0-0000-7000-8000-000000000022';
const ROLE_A = '0190f3a0-0000-7000-8000-000000000031';
const ROLE_B = '0190f3a0-0000-7000-8000-000000000032';
const CAMPUS = '0190f3a0-0000-7000-8000-000000000041';
const CHILD_A = '0190f3a0-0000-7000-8000-000000000051';
const CHILD_B = '0190f3a0-0000-7000-8000-000000000052';
const CHILD_C = '0190f3a0-0000-7000-8000-000000000053';

const fields = { firstName: 'Ayesha', lastName: 'Khan', userType: UserType.Parent };
const person = { id: PERSON, firstName: 'Ayesha', lastName: 'Khan', userType: UserType.Parent } as unknown as UserDetailsDto;

function start(overrides: Partial<PersonFormData> = {}): PersonFormData {
  return {
    person,
    students: [],
    roles: [],
    campuses: [],
    currentChildIds: [],
    currentRoleIds: [ROLE_A],
    currentCampusIds: [CAMPUS],
    ...overrides,
  };
}

function result(overrides: Partial<PersonFormResult> = {}): PersonFormResult {
  return { fields: { ...fields }, childIds: [], roleIds: [ROLE_A], campusIds: [CAMPUS], ...overrides } as PersonFormResult;
}

describe('PersonSaverService', () => {
  let service: PersonSaverService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: API_BASE_URL, useValue: '' }],
    });
    service = TestBed.inject(PersonSaverService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('creates a new person, then saves their roles and campuses in one call', async () => {
    const created = vi.fn();
    service.save(null, result(), start({ person: null, currentRoleIds: [], currentCampusIds: [] })).subscribe(created);

    http.expectOne({ method: 'POST', url: '/api/UserDetails' }).flush(json(NEW_PERSON));

    await vi.waitFor(() => {
      const access = http.expectOne({ method: 'PUT', url: `/api/UserDetails/${NEW_PERSON}/access` });
      expect(JSON.parse(access.request.body as string)).toEqual({ roleIds: [ROLE_A], campusIds: [CAMPUS] });
      access.flush(null, { status: 204, statusText: 'No Content' });
    });
    await vi.waitFor(() => expect(created).toHaveBeenCalledWith(NEW_PERSON));
  });

  it('updates the fields of a person and sends no access change when roles and campuses are the same', async () => {
    service.save(person, result({ roleIds: [ROLE_A], campusIds: [CAMPUS] }), start()).subscribe();

    http.expectOne({ method: 'PUT', url: `/api/UserDetails/${PERSON}` }).flush(null, { status: 204, statusText: 'No Content' });
    expect(http.match((r) => r.url.endsWith('/access'))).toEqual([]);
  });

  it('saves the roles and campuses only when the ids differ, in any order', async () => {
    service.save(person, result({ roleIds: [ROLE_B, ROLE_A], campusIds: [CAMPUS] }), start({ currentRoleIds: [ROLE_A, ROLE_B] })).subscribe();
    http.expectOne({ method: 'PUT', url: `/api/UserDetails/${PERSON}` }).flush(null, { status: 204, statusText: 'No Content' });
    expect(http.match((r) => r.url.endsWith('/access'))).toEqual([]);

    service.save(person, result({ roleIds: [ROLE_A, ROLE_B], campusIds: [CAMPUS] }), start({ currentRoleIds: [ROLE_A] })).subscribe();
    http.expectOne({ method: 'PUT', url: `/api/UserDetails/${PERSON}` }).flush(null, { status: 204, statusText: 'No Content' });
    await vi.waitFor(() => {
      const access = http.expectOne({ method: 'PUT', url: `/api/UserDetails/${PERSON}/access` });
      expect(JSON.parse(access.request.body as string)).toEqual({ roleIds: [ROLE_A, ROLE_B], campusIds: [CAMPUS] });
      access.flush(null, { status: 204, statusText: 'No Content' });
    });
  });

  it('links a child that is new to the parent and unlinks one that is no longer wanted', async () => {
    service
      .save(person, result({ childIds: [CHILD_B, CHILD_C] }), start({ currentChildIds: [CHILD_A, CHILD_B] }))
      .subscribe();
    http.expectOne({ method: 'PUT', url: `/api/UserDetails/${PERSON}` }).flush(null, { status: 204, statusText: 'No Content' });

    await vi.waitFor(() => {
      const link = http.expectOne({ method: 'POST', url: `/api/UserDetails/${PERSON}/children` });
      expect(JSON.parse(link.request.body as string)).toEqual({ studentId: CHILD_C });
      link.flush(null, { status: 204, statusText: 'No Content' });
      http.expectOne({ method: 'DELETE', url: `/api/UserDetails/${PERSON}/children/${CHILD_A}` }).flush(null, { status: 204, statusText: 'No Content' });
    });
  });
});
